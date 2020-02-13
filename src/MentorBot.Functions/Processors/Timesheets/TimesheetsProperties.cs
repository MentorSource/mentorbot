﻿// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

namespace MentorBot.Functions.Processors.Timesheets
{
    /// <summary>The timesheets properties keys.</summary>
    public static class TimesheetsProperties
    {
        /// <summary>The notifications group key.</summary>
        public const string NotificationsGroup = "OpenAir.Notifications";

        /// <summary>The email key.</summary>
        public const string Email = "OpenAir.Notifications.Email";

        /// <summary>The notify by email key.</summary>
        public const string NotifyByEmail = "OpenAir.Notifications.NotifyByEmail";

        /// <summary>Filter out the manager when sending notifications.</summary>
        public const string DontNotifyManager = "OpenAir.Notifications.DontNotifyManager";

        /// <summary>The filter by customer key.</summary>
        public const string FilterByCustomer = "OpenAir.Filters.Customer";

        /// <summary>The user properties key.</summary>
        public const string UserProperties = "OpenAir.UserProperties";

        /// <summary>The user maximum hours key.</summary>
        public const string UserMaxHours = "OpenAir.User.MaxHours";
    }
}
