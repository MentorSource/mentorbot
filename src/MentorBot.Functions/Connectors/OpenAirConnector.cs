﻿// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Abstract.Services;
using MentorBot.Functions.App.Extensions;
using MentorBot.Functions.Connectors.OpenAir;
using MentorBot.Functions.Models.Business;
using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Base;

using static MentorBot.Functions.Connectors.OpenAir.OpenAirFactory;

namespace MentorBot.Functions.Connectors
{
    /// <summary>Provider methods connected to OpenAir service endpoints.</summary>
    public sealed class OpenAirConnector : IOpenAirConnector
    {
        private readonly IOpenAirClient _client;
        private readonly IStorageService _storageService;

        /// <summary>Initializes a new instance of the <see cref="OpenAirConnector"/> class.</summary>
        public OpenAirConnector(IOpenAirClient client, IStorageService storageService)
        {
            _client = client;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Timesheet>> GetUnsubmittedTimesheetsAsync(
            DateTime date,
            DateTime today,
            TimesheetStates state,
            string senderEmail,
            bool filterSender,
            string userPropertyMaxHoursKey,
            IReadOnlyList<string> filterByCustomers)
        {
            var toweek = date.AddDays(-(double)date.DayOfWeek);
            var lastWeek = toweek.AddDays(-7);
            var timesheets = new List<OpenAirClient.Timesheet>();
            var normalizedCustomerNames = filterByCustomers?.Select(NormalizeValue).ToArray();
            var isEndOfMonthReport = date.Date == today && today.AddDays(1).Day == 1;
            var dayOfWeekMultiplier = today.DayOfWeek == DayOfWeek.Saturday || today.DayOfWeek == DayOfWeek.Sunday ? 5 : (int)today.DayOfWeek;

            if (state == TimesheetStates.Unsubmitted)
            {
                timesheets.AddRange(await _client.GetTimesheetsByStatusAsync(toweek, date.AddDays(1), "S"));
                timesheets.AddRange(await _client.GetTimesheetsByStatusAsync(toweek, date.AddDays(1), "A"));
            }
            else if (state == TimesheetStates.Unapproved)
            {
                timesheets.AddRange(await _client.GetTimesheetsByStatusAsync(lastWeek, toweek, "A"));
            }

            var timesheetsData = timesheets
                .GroupBy(it => it.UserId)
                .Select(it =>
                    new TimesheetBasicData(
                        it.Key.Value,
                        it.Where(sheet => sheet.StartDate.Date > toweek)
                          .Where(sheet =>
                            (state == TimesheetStates.Unapproved && sheet.Status == "A") ||
                            (state == TimesheetStates.Unsubmitted && (sheet.Status == "S" || sheet.Status == "A")))
                          .Sum(sheet => sheet.Total ?? 0)))
                .ToArray();

            var users = await _storageService.GetAllActiveUsersAsync();

            // 1. Filter out only users where the sender is line manager.
            // 2. Filter out customers.
            // 3. Select timesheet
            var result = users
                .Where(it =>
                    !filterSender ||
                    !senderEmail.Equals(it.Email, StringComparison.InvariantCultureIgnoreCase))
                .Where(it => it.Manager != null)
                .Where(it =>
                {
                    try
                    {
                        return (it.Department?.Owner?.Email.Equals(senderEmail, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                            users.IsRequestorManager(it, senderEmail);
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                        return false;
                    }
                })
                .Where(it =>
                {
                    try
                    {
                        return FiterCustomersByNames(it.Customers, normalizedCustomerNames);
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                        return false;
                    }
                })
                .Select(user =>
                {
                    try
                    {
                        return new TimesheetExtendedData(
                            timesheetsData.FirstOrDefault(it => it.UserId == user.OpenAirUserId),
                            user,
                            CalculateRequiredHours(
                                user.Properties
                                    .GetAllUserValues<int>(userPropertyMaxHoursKey)
                                    .DefaultIfEmpty(40).First(),
                                isEndOfMonthReport,
                                dayOfWeekMultiplier));
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                        return new TimesheetExtendedData(null, user, 40);
                    }
                })
                .Where(it => it.Timesheet.Total < it.RequiredHours)
                .Select(it => new Timesheet
                {
                    Total = it.Timesheet.Total,
                    UtilizationInHours = it.RequiredHours,
                    UserName = FormatDisplayName(it.User?.Name ?? "Unknown User"),
                    UserEmail = it.User.Email,
                    DepartmentName = it.User.Department.Name,
                    ManagerName = FormatDisplayName(users.FindUserByRef(it.User.Manager)?.Name)
                })
                .ToArray();

            return result;
        }

        /// <inheritdoc/>
        public async Task SyncUsersAsync()
        {
            var storedUsers = await _storageService.GetAllUsersAsync();
            var openAirModelUsers = await _client.GetAllUsersAsync();
            var openAirDepartments = await _client.GetAllDepartmentsAsync();
            var openAirCustomers = await _client.GetAllActiveCustomersAsync();
            var openAirBookings = await _client.GetAllActiveBookingsAsync(DateTime.Today);
            var usersListToUpdate = new List<User>();
            var usersListToAdd = new List<User>();
            foreach (var user in openAirModelUsers)
            {
                var storedUser = storedUsers.FirstOrDefault(it => it.OpenAirUserId == user.Id);
                var department = user.DepartmentId.HasValue ?
                    CreateDepartment(openAirDepartments.FirstOrDefault(it => it.Id == user.DepartmentId.Value), openAirModelUsers) :
                    null;

                var manager = CreateUserReferenceById(user.ManagerId, openAirModelUsers);

                var customerIdList = openAirBookings?
                    .Where(it => it.UserId == user.Id)
                    .Select(it => it.CustomerId)
                    .Distinct()
                    .ToArray() ?? new List<long?>().ToArray();

                var customers = openAirCustomers
                    .Where(it => customerIdList.Contains(it.Id))
                    .Select(it => new Customer { OpenAirId = it.Id.Value, Name = it.Name })
                    .ToArray();

                if (storedUser == null && user.Active == true)
                {
                    var createUser = CreateUser(Guid.NewGuid().ToString(null, CultureInfo.InvariantCulture), user, manager, department, customers);
                    usersListToAdd.Add(createUser);
                }
                else if (
                    storedUser != null &&
                    UserNeedUpdate(
                        storedUser,
                        storedUser.Manager,
                        storedUser.Department,
                        storedUser.Customers ?? new Customer[0],
                        user,
                        manager,
                        department,
                        customers))
                {
                    var updateUser = CreateUser(storedUser.Id, user, manager, department, customers);
                    usersListToUpdate.Add(updateUser);
                }
            }

            if (usersListToAdd.Count > 0)
            {
                await _storageService.AddUsersAsync(usersListToAdd);
            }

            if (usersListToUpdate.Count > 0)
            {
                await _storageService.UpdateUsersAsync(usersListToUpdate);
            }
        }

        private static int CalculateRequiredHours(int hoursPerWeek, bool isEndOfMonth, int dayOfWeekMultiplier) =>
            isEndOfMonth ?
            (hoursPerWeek / 5 * dayOfWeekMultiplier) :
            hoursPerWeek;

        private static bool UserNeedUpdate(
            User storedUser,
            UserReference storedManager,
            Department storedDep,
            Customer[] storedCustomers,
            OpenAirClient.User openAirUser,
            UserReference manager,
            Department department,
            Customer[] openAirCustomers) =>
            storedUser.Active != openAirUser.Active ||
            storedDep?.OpenAirDepartmentId != openAirUser.DepartmentId ||
            storedDep?.Name != department?.Name ||
            storedDep?.Owner?.OpenAirUserId != department?.Owner?.OpenAirUserId ||
            storedManager?.OpenAirUserId != manager?.OpenAirUserId ||
            storedCustomers.Length != openAirCustomers.Length ||
            openAirCustomers.Any(it => !storedCustomers.Contains(it));

        private static bool FiterCustomersByNames(Customer[] customers, string[] customerNames) =>
            customerNames == null ||
            customerNames.Length == 0 ||
            customers == null ||
            customers.Length == 0 ||
            !customers.AnyStringInCollection(customerNames, it => NormalizeValue(it.Name), StringComparison.InvariantCultureIgnoreCase);

        private static string NormalizeValue(string value) =>
            value.Replace(" ", string.Empty, StringComparison.InvariantCulture);

        private static string FormatDisplayName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var names = name.Split(',');
            return names.Length == 1 ? name : $"{names[1].Trim()} {names[0].Trim()}";
        }

        private class TimesheetBasicData
        {
            public TimesheetBasicData(long userId, double total)
            {
                UserId = userId;
                Total = total;
            }

            public long UserId { get; set; }

            public double Total { get; set; }
        }

        private class TimesheetExtendedData
        {
            /// <summary>Initializes a new instance of the <see cref="TimesheetExtendedData" /> class.</summary>
            public TimesheetExtendedData(TimesheetBasicData timesheet, User user, int requiredHours)
            {
                Timesheet = timesheet ?? new TimesheetBasicData(user.OpenAirUserId, 0.0);
                User = user;
                RequiredHours = requiredHours;
            }

            public TimesheetBasicData Timesheet { get; set; }

            public User User { get; set; }

            public int RequiredHours { get; set; }
        }
    }
}