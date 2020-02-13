﻿// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MentorBot.Functions.Abstract.Connectors;
using MentorBot.Functions.Models.Business;

namespace MentorBot.Functions.Abstract.Processor
{
    /// <summary>Timesheet processor actions.</summary>
    public interface ITimesheetProcessor
    {
        /// <summary>Notifies the asynchronous.</summary>
        /// <param name="date">The current date.</param>
        /// <param name="state">The timesheet state.</param>
        /// <param name="email">The requester email.</param>
        /// <param name="customersToExclude">The customers to exclude.</param>
        /// <param name="department">The department.</param>
        /// <param name="notify">if set to <c>true</c> [notify].</param>
        /// <param name="notifyByEmail">if set to <c>true</c> [notify by email].</param>
        /// <param name="filterOutSender">if set to <c>true</c> [do not send norification to sender email].</param>
        /// <param name="address">The chat address.</param>
        /// <param name="connector">The chat connector.</param>
        Task NotifyAsync(
            DateTime date,
            TimesheetStates state,
            string email,
            IReadOnlyList<string> customersToExclude,
            string department,
            bool notify,
            bool notifyByEmail,
            bool filterOutSender,
            GoogleChatAddress address,
            IHangoutsChatConnector connector);
    }
}
