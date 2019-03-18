﻿// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;

using MentorBot.Functions.Models.Domains.Base;

using Newtonsoft.Json;

namespace MentorBot.Functions.Models.Domains
{
    /// <summary>The user domain model.</summary>
    public sealed class User
    {
        /// <summary>Gets or sets the identifier.</summary>
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        /// <summary>Gets or sets the email.</summary>
        public string Email { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the open air user identifier.</summary>
        public long OpenAirUserId { get; set; }

        /// <summary>Gets or sets the user manager.</summary>
        public UserReference Manager { get; set; }

        /// <summary>Gets or sets a value indicating whether this <see cref="User"/> is active.</summary>
        public bool Active { get; set; }

        /// <summary>Gets or sets the department.</summary>
        public Department Department { get; set; }

        /// <summary>Gets or sets the customers.</summary>
        public Customer[] Customers { get; set; }
    }
}