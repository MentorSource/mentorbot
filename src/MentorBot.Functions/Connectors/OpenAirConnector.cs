// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

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
            TimesheetStates state,
            string senderEmail,
            bool filterSender,
            string userPropertyMaxHoursKey,
            IReadOnlyList<string> filterByCustomers)
        {
            var toweek = date.AddDays(-(double)date.DayOfWeek);
            var normalizedCustomerNames = filterByCustomers?.Select(NormalizeValue).ToArray();
            var timesheets = await _client.GetTimesheetsAsync(toweek, date.AddDays(1));
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
                    (it.Department?.Owner?.Email.Equals(senderEmail, StringComparison.InvariantCultureIgnoreCase) ?? false) ||
                    IsManager(it, senderEmail, users, new List<string>()))
                .Where(it => FiterCustomersByNames(it.Customers, normalizedCustomerNames))
                .Select(user =>
                {
                    try
                    {
                        return new TimesheetExtendedData(
                            timesheetsData.FirstOrDefault(it => it.UserId == user.OpenAirUserId),
                            user,
                            user.Properties
                                .GetAllUserValues<int>(userPropertyMaxHoursKey)
                                .DefaultIfEmpty(40).First());
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message);
                        return new TimesheetExtendedData(null, user, 40);
                    }
                })
                .Where(it => it.Timesheet.Total < it.HoursPerWeek)
                .Select(it => new Timesheet
                {
                    Total = it.Timesheet.Total,
                    UtilizationInHours = it.HoursPerWeek,
                    UserName = FormatDisplayName(it.User?.Name ?? "Unknown User"),
                    UserEmail = it.User.Email,
                    DepartmentName = it.User.Department.Name,
                    ManagerName = FormatDisplayName(FindUser(it.User.Manager, users)?.Name)
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
                else if (storedUser != null && UserNeedUpdate(storedUser, user, manager, department, customers))
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

        private static bool UserNeedUpdate(User storedUser, OpenAirClient.User openAirUser, UserReference manager, Department department, Customer[] openAirCustomers) =>
            storedUser.Active != openAirUser.Active ||
            storedUser.Department?.OpenAirDepartmentId != openAirUser.DepartmentId ||
            storedUser.Department?.Owner?.OpenAirUserId != department?.Owner?.OpenAirUserId ||
            storedUser.Manager?.OpenAirUserId != manager?.OpenAirUserId ||
            storedUser.Customers?.Length != openAirCustomers.Length ||
            openAirCustomers.Any(it => !storedUser.Customers.Contains(it));

        private static bool IsManager(User user, string email, IReadOnlyList<User> users, List<string> emails) =>
            user != null && IsUserRefManager(user.Manager, email, users, emails);

        private static bool IsUserRefManager(UserReference userRef, string email, IReadOnlyList<User> users, List<string> emails)
        {
            if (userRef == null || emails.Contains(userRef.Email))
            {
                return false;
            }

            emails.Add(userRef.Email);

            return userRef.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) ||
                   IsManager(FindUser(userRef, users), email, users, emails);
        }

        private static User FindUser(UserReference userRef, IReadOnlyList<User> users) =>
            userRef == null ? null : users.FirstOrDefault(it => it.OpenAirUserId == userRef.OpenAirUserId);

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
            public TimesheetExtendedData(TimesheetBasicData timesheet, User user, int hoursPerWeek)
            {
                Timesheet = timesheet ?? new TimesheetBasicData(user.OpenAirUserId, 0.0);
                User = user;
                HoursPerWeek = hoursPerWeek;
            }

            public TimesheetBasicData Timesheet { get; set; }

            public User User { get; set; }

            public int HoursPerWeek { get; set; }
        }
    }
}
