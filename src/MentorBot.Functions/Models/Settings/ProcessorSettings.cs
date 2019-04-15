﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MentorBot.Functions.Models.Settings
{
    /// <summary>
    /// Contains all the settings about a Command Processor
    /// </summary>
    public class ProcessorSettings
    {
        /// <summary>
        /// Gets or sets the name of the command processor
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command processor is enabled
        /// </summary>
        public bool Enabled { get; set; }
    }
}