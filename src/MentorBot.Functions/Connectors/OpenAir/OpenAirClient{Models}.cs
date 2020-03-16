// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MentorBot.Functions.Connectors.OpenAir
{
    /// <summary>The model needed for OpenAir client to use.</summary>
    public sealed partial class OpenAirClient
    {
        /// <summary>OpenAir date type.</summary>
        public enum DateType
        {
            /// <summary>The timesheet date type.</summary>
            Timesheet = 1,

            /// <summary>The user date type.</summary>
            User = 2,

            /// <summary>The department date type.</summary>
            Department = 3,

            /// <summary>The project data type.</summary>
            Project,

            /// <summary>The project task data type.</summary>
            Projecttask,

            /// <summary>The project task assign data type.</summary>
            Projecttaskassign,

            /// <summary>The customer data type.</summary>
            Customer,

            /// <summary>The booking data type.</summary>
            Booking
        }

        private static long? ParseStringToLong(string value) =>
            string.IsNullOrEmpty(value) ? (long?)null : long.Parse(value, CultureInfo.InvariantCulture);

        private static string ParseBoolToString(bool value) =>
            value ? "1" : "0";

        private static bool? ParseStringToBool(string value) =>
            value == null ? (bool?)null : value == "1";

        /// <summary>The open air request model.</summary>
        [Serializable]
        [XmlRoot("request", Namespace = "")]
        public sealed class Request
        {
            /// <summary>Gets or sets the API version.</summary>
            [XmlAttribute("API_version")]
            public string ApiVersion { get; set; } = "1.0";

            /// <summary>Gets or sets the client name.</summary>
            [XmlAttribute("client")]
            public string Client { get; set; }

            /// <summary>Gets or sets the client version.</summary>
            [XmlAttribute("client_ver")]
            public string ClientVersion { get; set; } = "1.0";

            /// <summary>Gets or sets the namespace.</summary>
            [XmlAttribute("namespace")]
            public string Namespace { get; set; } = "default";

            /// <summary>Gets or sets the key.</summary>
            [XmlAttribute("key")]
            public string Key { get; set; }

            /// <summary>Gets or sets the authentication.</summary>
            [XmlElement]
            public Auth Auth { get; set; }

            /// <summary>Gets or sets the read.</summary>
            [XmlElement]
            public Read Read { get; set; }
        }

        /// <summary>The open air auth section.</summary>
        [Serializable]
        public sealed class Auth
        {
            /// <summary>Gets or sets the login.</summary>
            [XmlElement]
            public Login Login { get; set; }
        }

        /// <summary>The open air login model.</summary>
        [Serializable]
        public sealed class Login
        {
            /// <summary>Gets or sets the company.</summary>
            [XmlElement("company")]
            public string Company { get; set; }

            /// <summary>Gets or sets the user.</summary>
            [XmlElement("user")]
            public string User { get; set; }

            /// <summary>Gets or sets the password.</summary>
            [XmlElement("password")]
            public string Password { get; set; }
        }

        /// <summary>The open air read request.</summary>
        [Serializable]
        public sealed class Read
        {
            /// <summary>Gets or sets the type.</summary>
            [XmlAttribute("type")]
            public DateType Type { get; set; }

            /// <summary>Gets or sets the filter.</summary>
            [XmlAttribute("filter")]
            public string Filter { get; set; }

            /// <summary>Gets or sets the field.</summary>
            [XmlAttribute("field")]
            public string Field { get; set; }

            /// <summary>Gets or sets the method.</summary>
            [XmlAttribute("method")]
            public string Method { get; set; } = "all";

            /// <summary>Gets or sets the limit as text.</summary>
            [XmlAttribute("limit")]
            public string LimitAsText { get; set; } = "1000";

            /// <summary>Gets or sets the limit.</summary>
            [XmlIgnore]
            public int Limit
            {
                get => int.Parse(LimitAsText, CultureInfo.InvariantCulture);
                set => LimitAsText = value.ToString(CultureInfo.InvariantCulture);
            }

            /// <summary>Gets or sets the dates.</summary>
            [XmlElement("Date", Order = 1)]
            public Date[] Date { get; set; }

            /// <summary>Gets or sets the users.</summary>
            [XmlElement("User", Order = 2)]
            public User[] User { get; set; }

            /// <summary>Gets or sets the departments.</summary>
            [XmlElement("Department", Order = 3)]
            public Department[] Department { get; set; }

            /// <summary>Gets or sets the timesheets.</summary>
            [XmlElement("Timesheet", Order = 4)]
            public Timesheet[] Timesheet { get; set; }

            /// <summary>Gets or sets the customers.</summary>
            [XmlElement("Customer", Order = 8)]
            public Customer[] Customer { get; set; }

            /// <summary>Gets or sets the bookings.</summary>
            [XmlElement("Booking", Order = 9)]
            public Booking[] Booking { get; set; }

            /// <summary>Gets or sets the return.</summary>
            [XmlElement("_Return", Order = 100)]
            public RaedReturn Return { get; set; }
        }

        /// <summary>The open air read request return collection.</summary>
        [Serializable]
        public sealed class RaedReturn : IXmlSerializable
        {
            /// <summary>Gets or sets the content.</summary>
            public string Content { get; set; }

            /// <inheritdoc/>
            public XmlSchema GetSchema() => new XmlSchema();

            /// <inheritdoc/>
            public void ReadXml(XmlReader reader) =>
                Content = reader.ReadContentAsString();

            /// <inheritdoc/>
            public void WriteXml(XmlWriter writer) =>
                writer.WriteRaw(Content);
        }

        /// <summary>The open air date model.</summary>
        [Serializable]
        public sealed class Date : IComparable<DateTime>
        {
            /// <summary>Gets or sets the month.</summary>
            [XmlElement("month")]
            public int Month { get; set; }

            /// <summary>Gets or sets the day.</summary>
            [XmlElement("day")]
            public int Day { get; set; }

            /// <summary>Gets or sets the year.</summary>
            [XmlElement("year")]
            public int Year { get; set; }

            /// <summary>Implements the operator >.</summary>
            public static bool operator >(Date date, DateTime dateTime) =>
                date.ToDateTime() > dateTime;

            /// <summary>Implements the operator >.</summary>
            public static bool operator <(Date date, DateTime dateTime) =>
               date.ToDateTime() < dateTime;

            /// <summary>Implements the operator less or equal.</summary>
            public static bool operator <=(Date date, DateTime dateTime) =>
               date.ToDateTime() <= dateTime;

            /// <summary>Implements the operator less or equal.</summary>
            public static bool operator >=(Date date, DateTime dateTime) =>
               date.ToDateTime() >= dateTime;

            /// <summary>Implements the operator equal.</summary>
            public static bool operator ==(Date date, DateTime dateTime) =>
               date.ToDateTime() == dateTime;

            /// <summary>Implements the operator no equal.</summary>
            public static bool operator !=(Date date, DateTime dateTime) =>
               date.ToDateTime() != dateTime;

            /// <summary>Creates the specified date time.</summary>
            public static Date Create(DateTime dateTime) =>
                new Date
                {
                    Year = dateTime.Year,
                    Month = dateTime.Month,
                    Day = dateTime.Day
                };

            /// <summary>Convert to <see cref="DateTime"/>.</summary>
            public DateTime ToDateTime() =>
                new DateTime(Year, Month, Day);

            /// <inheritdoc/>
            public int CompareTo(DateTime other) =>
                ToDateTime().CompareTo(other);

            /// <inheritdoc/>
            public override bool Equals(object obj) =>
                obj is DateTime dateTime && CompareTo(dateTime) == 0;

            /// <inheritdoc/>
            public override int GetHashCode() =>
                (Year * 10000) + (Month * 100) + Day;
        }

        /// <summary>The open air user model.</summary>
        [Serializable]
        public sealed class User
        {
            /// <summary>Gets or sets the identifier.</summary>
            [XmlIgnore]
            public long? Id { get; set; }

            /// <summary>Gets or sets the name.</summary>
            [XmlElement("name")]
            public string Name { get; set; }

            /// <summary>Gets or sets the department identifier.</summary>
            [XmlIgnore]
            public long? DepartmentId { get; set; }

            /// <summary>Gets or sets the manager identifier.</summary>
            [XmlIgnore]
            public long? ManagerId { get; set; }

            /// <summary>Gets or sets the location identifier.</summary>
            [XmlIgnore]
            public long? LocationId { get; set; }

            /// <summary>Gets or sets a value indicating whether user is active.</summary>
            [XmlIgnore]
            public bool? Active { get; set; }

            /// <summary>Gets or sets the identifier.</summary>
            [XmlElement("id")]
            public string IdAsText
            {
                get => Id?.ToString(CultureInfo.InvariantCulture);
                set => Id = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the active as text.</summary>
            [XmlElement("active")]
            public string ActiveAsText
            {
                get => Active.HasValue ? ParseBoolToString(Active.Value) : null;
                set => Active = ParseStringToBool(value);
            }

            /// <summary>Gets or sets the department identifier as text.</summary>
            [XmlElement("departmentid")]
            public string DepartmentIdAsText
            {
                get => DepartmentId.HasValue ? DepartmentId.ToString() : null;
                set => DepartmentId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the manager identifier as text.</summary>
            [XmlElement("line_managerid")]
            public string ManagerIdAsText
            {
                get => ManagerId.HasValue ? ManagerId.ToString() : null;
                set => ManagerId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the location identifier as text.</summary>
            [XmlElement("user_locationid")]
            public string LocationIdAsText
            {
                get => LocationId.HasValue ? LocationId.ToString() : null;
                set => LocationId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the addresses.</summary>
            [XmlArray("addr")]
            [XmlArrayItem("Address")]
            public Address[] Address { get; set; }
        }

        /// <summary>The open air department model.</summary>
        [Serializable]
        public sealed class Department
        {
            /// <summary>Gets or sets the identifier.</summary>
            [XmlElement("id")]
            public long Id { get; set; }

            /// <summary>Gets or sets the user identifier of the head of the department.</summary>
            [XmlIgnore]
            public long? UserId { get; set; }

            /// <summary>Gets or sets the user identifier as text.</summary>
            [XmlElement("userid")]
            public string UserIdAsText
            {
                get => UserId.HasValue ? UserId.ToString() : null;
                set => UserId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the name.</summary>
            [XmlElement("name")]
            public string Name { get; set; }
        }

        /// <summary>A open air customer model.</summary>
        public sealed class Customer
        {
            /// <summary>Gets or sets the identifier.</summary>
            [XmlIgnore]
            public long? Id { get; set; }

            /// <summary>Gets or sets a value indicating whether customer is active.</summary>
            [XmlIgnore]
            public bool? Active { get; set; }

            /// <summary>Gets or sets the name.</summary>
            [XmlElement("name")]
            public string Name { get; set; }

            /// <summary>Gets or sets the identifier.</summary>
            [XmlElement("id")]
            public string IdAsText
            {
                get => Id?.ToString(CultureInfo.InvariantCulture);
                set => Id = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the active as text.</summary>
            [XmlElement("active")]
            public string ActiveAsText
            {
                get => Active.HasValue ? ParseBoolToString(Active.Value) : null;
                set => Active = ParseStringToBool(value);
            }
        }

        /// <summary>A open air booking model.</summary>
        public sealed class Booking
        {
            /// <summary>Gets or sets the identifier.</summary>
            [XmlIgnore]
            public long? Id { get; set; }

            /// <summary>Gets or sets the user identifier.</summary>
            [XmlIgnore]
            public long? UserId { get; set; }

            /// <summary>Gets or sets the owner user identifier.</summary>
            [XmlIgnore]
            public long? OwnerId { get; set; }

            /// <summary>Gets or sets the project identifier.</summary>
            [XmlIgnore]
            public long? ProjectId { get; set; }

            /// <summary>Gets or sets the customer identifier.</summary>
            [XmlIgnore]
            public long? CustomerId { get; set; }

            /// <summary>Gets or sets the booking type identifier.</summary>
            [XmlIgnore]
            public long? BookingTypeId { get; set; }

            /// <summary>Gets or sets the approval status.</summary>
            [XmlElement("approval_status")]
            public string ApprovalStatus { get; set; }

            /// <summary>Gets or sets the identifier.</summary>
            [XmlElement("id")]
            public string IdAsText
            {
                get => Id?.ToString(CultureInfo.InvariantCulture);
                set => Id = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the user identifier as text.</summary>
            [XmlElement("userid")]
            public string UserIdAsText
            {
                get => UserId?.ToString(CultureInfo.InvariantCulture);
                set => UserId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the owner identifier as text.</summary>
            [XmlElement("ownerid")]
            public string OwnerIdAsText
            {
                get => OwnerId?.ToString(CultureInfo.InvariantCulture);
                set => OwnerId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the project identifier as text.</summary>
            [XmlElement("projectid")]
            public string ProjectIdAsText
            {
                get => ProjectId?.ToString(CultureInfo.InvariantCulture);
                set => ProjectId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the customer identifier as text.</summary>
            [XmlElement("customerid")]
            public string CustomerIdAsText
            {
                get => CustomerId?.ToString(CultureInfo.InvariantCulture);
                set => CustomerId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the booking type identifier as text.</summary>
            [XmlElement("booking_typeid")]
            public string BookingTypeIdAsText
            {
                get => BookingTypeId?.ToString(CultureInfo.InvariantCulture);
                set => BookingTypeId = ParseStringToLong(value);
            }
        }

        /// <summary>The open air address class.</summary>
        [Serializable]
        public sealed class Address
        {
            /// <summary>Gets or sets the email.</summary>
            [XmlElement("email")]
            public string Email { get; set; }
        }

        /// <summary>The open air response.</summary>
        [XmlRoot("response", Namespace = "")]
        public sealed class Response
        {
            /// <summary>Gets or sets the read result.</summary>
            [XmlElement]
            public Read Read { get; set; }
        }

        /// <summary>The openair timesheet model.</summary>
        [Serializable]
        public sealed class Timesheet
        {
            /// <summary>Gets or sets the user identifier.</summary>
            [XmlIgnore]
            public long? UserId { get; set; }

            /// <summary>Gets or sets the user identifier as text.</summary>
            [XmlElement("userid")]
            public string UserIdAsText
            {
                get => UserId?.ToString(CultureInfo.InvariantCulture);
                set => UserId = ParseStringToLong(value);
            }

            /// <summary>Gets or sets the status.</summary>
            [XmlElement("status")]
            public string Status { get; set; }

            /// <summary>Gets or sets the name.</summary>
            [XmlElement("name")]
            public string Name { get; set; }

            /// <summary>Gets or sets the total hours.</summary>
            [XmlIgnore]
            public double? Total { get; set; }

            /// <summary>Gets or sets the total hours as test.</summary>
            [XmlElement("total")]
            public string TotalAsText
            {
                get => Total == null ? null : UserId.ToString();
                set => Total = string.IsNullOrEmpty(value) ? (double?)null : double.Parse(value, CultureInfo.InvariantCulture);
            }

            /// <summary>Gets or sets the notes.</summary>
            [XmlElement("notes")]
            public string Notes { get; set; }

            /// <summary>Gets or sets the start date.</summary>
            [XmlElement("starts")]
            public DataContainer StartDate { get; set; }
        }

        /// <summary>A field that contains date.</summary>
        [Serializable]
        public sealed class DataContainer
        {
            /// <summary>Gets or sets the date.</summary>
            [XmlElement("Date")]
            public Date Date { get; set; }
        }
    }
}
