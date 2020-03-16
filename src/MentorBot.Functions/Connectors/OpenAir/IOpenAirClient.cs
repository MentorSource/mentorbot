using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static MentorBot.Functions.Connectors.OpenAir.OpenAirClient;

namespace MentorBot.Functions.Connectors.OpenAir
{
    /// <summary>A open air client.</summary>
    public interface IOpenAirClient
    {
        /// <summary>Gets the timesheets asynchronous.</summary>
        Task<IReadOnlyList<Timesheet>> GetTimesheetsAsync(DateTime startDate, DateTime endDate);

        /// <summary>Gets all users asynchronous.</summary>
        Task<IReadOnlyList<User>> GetAllUsersAsync();

        /// <summary>Gets all departments asynchronous.</summary>
        Task<Department[]> GetAllDepartmentsAsync();

        /// <summary>Gets all active customers asynchronous.</summary>
        Task<Customer[]> GetAllActiveCustomersAsync();

        /// <summary>Gets all active bookings asynchronous.</summary>
        Task<Booking[]> GetAllActiveBookingsAsync(DateTime today);
    }
}
