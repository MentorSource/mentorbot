﻿using MentorBot.Functions.Models.Domains.Plugins;
using MentorBot.Functions.Processors.BuildInfo;
using MentorBot.Functions.Processors.Issues;
using MentorBot.Functions.Processors.Timesheets;

namespace MentorBot.Functions.App
{
    /// <summary>A default in-system plugins.</summary>
    public static class SystemPlugins
    {
        /// <summary>Gets the system plugins.</summary>
        public static Plugin[] GetSystemPlugins() =>
            new[]
                {
                    new Plugin
                    {
                        Id = "31235913-2cd7-4bb1-9af8-35efa521bb1d",
                        Name = "Issues/Ticketing",
                        ProcessorTypeName = "MentorBot.Functions.Processors.Issues.IssuesProcessor",
                        Enabled = true,
                        Groups = new[]
                        {
                            new PluginPropertyGroup
                            {
                                Name = "Jira Hosts",
                                UniqueName = IssuesProperties.HostsGroup,
                                Multi = false,
                                ObjectType = PropertyObjectTypes.Settings,
                                Properties = new[]
                                {
                                    new PluginProperty
                                    {
                                        Name = "Host",
                                        UniqueName = IssuesProperties.Host,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Username",
                                        UniqueName = IssuesProperties.User,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Token",
                                        UniqueName = IssuesProperties.Token,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                },
                            },
                        },
                    },
                    new Plugin
                    {
                        Id = "7239ed4d-5b95-4bdd-be2c-007c281e87e6",
                        Name = "Jenkins Build Info",
                        ProcessorTypeName = "MentorBot.Functions.Processors.BuildInfo.BuildInfoProcessor",
                        Enabled = true,
                        Groups = new[]
                        {
                            new PluginPropertyGroup
                            {
                                Name = "Jenkins Hosts",
                                UniqueName = BuildInfoProperties.HostsGroup,
                                Multi = false,
                                ObjectType = PropertyObjectTypes.Settings,
                                Properties = new[]
                                {
                                    new PluginProperty
                                    {
                                        Name = "Host",
                                        UniqueName = BuildInfoProperties.Host,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Username",
                                        UniqueName = BuildInfoProperties.User,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Token",
                                        UniqueName = BuildInfoProperties.Token,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                },
                            },
                            new PluginPropertyGroup
                            {
                                Name = "Jenkins Jobs",
                                UniqueName = "Jenkins.Jobs",
                                Multi = true,
                                ObjectType = PropertyObjectTypes.User,
                                Properties = new[]
                                {
                                    new PluginProperty
                                    {
                                        Name = "Job",
                                        UniqueName = BuildInfoProperties.JobName,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                },
                            },
                        },
                    },
                    new Plugin
                    {
                        Id = "1a90977d-208a-4d85-a2ba-6366532a4225",
                        Name = "Google Calendar",
                        ProcessorTypeName = "MentorBot.Functions.Processors.CalendarProcessor",
                        Enabled = true,
                    },
                    new Plugin
                    {
                        Id = "0930a559-9e53-44d0-8c27-56030a6d4940",
                        Name = "Greetings",
                        ProcessorTypeName = "MentorBot.Functions.Processors.HelloProcessor",
                        Enabled = true,
                    },
                    new Plugin
                    {
                        Id = "acdf97a9-467a-43fa-b4b2-35dd933870ad",
                        Name = "Help",
                        ProcessorTypeName = "MentorBot.Functions.Processors.HelpProcessor",
                        Enabled = true,
                    },
                    new Plugin
                    {
                        Id = "fdc902a9-7cbc-45b4-9670-5785fa9e9a25",
                        Name = "Time",
                        ProcessorTypeName = "MentorBot.Functions.Processors.LocalTimeProcessor",
                        Enabled = true,
                    },
                    new Plugin
                    {
                        Id = "1e2d563b-88f4-4ab4-896f-89fe5c6a0236",
                        Name = "OpenAir",
                        ProcessorTypeName = "MentorBot.Functions.Processors.Timesheets.OpenAirProcessor",
                        Enabled = true,
                        Groups = new[]
                        {
                            new PluginPropertyGroup
                            {
                                Name = "OpenAir Timesheets Scheduled Notifications",
                                UniqueName = TimesheetsProperties.NotificationsGroup,
                                Multi = true,
                                ObjectType = PropertyObjectTypes.Settings,
                                Properties = new[]
                                {
                                    new PluginProperty
                                    {
                                        Name = "Manager Email",
                                        UniqueName = TimesheetsProperties.Email,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Notify By Email",
                                        UniqueName = TimesheetsProperties.NotifyByEmail,
                                        ValueType = PropertyValueTypes.Boolean,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Don't Notify Manager",
                                        UniqueName = TimesheetsProperties.DontNotifyManager,
                                        ValueType = PropertyValueTypes.Boolean,
                                    },
                                },
                            },
                            new PluginPropertyGroup
                            {
                                Name = "OpenAir Filter Customers",
                                UniqueName = "OpenAir.Filters.Customers",
                                Multi = true,
                                ObjectType = PropertyObjectTypes.Settings,
                                Properties = new[]
                                {
                                    new PluginProperty
                                    {
                                        Name = "Customer Name",
                                        UniqueName = TimesheetsProperties.FilterByCustomer,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                },
                            },
                            new PluginPropertyGroup
                            {
                                Name = "OpenAir Properties",
                                UniqueName = TimesheetsProperties.UserProperties,
                                Multi = false,
                                ObjectType = PropertyObjectTypes.User,
                                Properties = new[]
                                {
                                    new PluginProperty
                                    {
                                        Name = "Max Hours Per Week",
                                        UniqueName = TimesheetsProperties.UserMaxHours,
                                        ValueType = PropertyValueTypes.Number,
                                    },
                                },
                            },
                            new PluginPropertyGroup
                            {
                                Name = "Timesheets Auto Scheduling",
                                UniqueName = TimesheetsProperties.AutoNotificationsGroup,
                                Multi = true,
                                ObjectType = PropertyObjectTypes.Settings,
                                Properties = new[]
                                {
                                    new PluginProperty
                                    {
                                        Name = "Start Hour (ex: '16 Fri' or '18 *' for every day at 18:00)",
                                        UniqueName = TimesheetsProperties.AutoNotificationsCron,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Report Name (unsubmitted, unapproved, etc)",
                                        UniqueName = TimesheetsProperties.AutoNotificationsReportName,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Space (comma separated list)",
                                        UniqueName = TimesheetsProperties.AutoNotificationsSpaces,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Email",
                                        UniqueName = TimesheetsProperties.AutoNotificationsManagerEmail,
                                        ValueType = PropertyValueTypes.String,
                                    },
                                    new PluginProperty
                                    {
                                        Name = "Notify Users",
                                        UniqueName = TimesheetsProperties.AutoNotificationsNotify,
                                        ValueType = PropertyValueTypes.Boolean,
                                    },
                                },
                            },
                        },
                    },
                    new Plugin
                    {
                        Id = "720f5079-6677-4e31-a953-a2c6dc7ce637",
                        Name = "Repeater",
                        ProcessorTypeName = "MentorBot.Functions.Processors.RepeatProcessor",
                        Enabled = true,
                    },
                    new Plugin
                    {
                        Id = "bb1c66bf-6627-4498-9118-1ac6f46ee22d",
                        Name = "Wikipedia",
                        ProcessorTypeName = "MentorBot.Functions.Processors.WikipediaProcessor",
                        Enabled = true,
                    },
                };
    }
}