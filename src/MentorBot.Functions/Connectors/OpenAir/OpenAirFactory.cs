// Copyright (c) 2018. Licensed under the MIT License. See https://www.opensource.org/licenses/mit-license.php for full license information.

using System.Collections.Generic;
using System.Linq;

using MentorBot.Functions.Models.Domains;
using MentorBot.Functions.Models.Domains.Base;

namespace MentorBot.Functions.Connectors.OpenAir
{
    /// <summary>A domain creation method factory.</summary>
    public static class OpenAirFactory
    {
        /// <summary>Creates a <see cref="User"/> model.</summary>
        public static User CreateUser(string id, OpenAirClient.User user, UserReference manager, Department department, Customer[] customers) =>
           new User
           {
               Id = id,
               OpenAirUserId = user.Id.Value,
               Name = user.Name,
               Email = user.Address.FirstOrDefault()?.Email,
               Active = user.Active ?? false,
               Department = department,
               Manager = manager,
               Customers = customers
           };

        /// <summary>Creates a <see cref="UserReference"/> model.</summary>
        public static UserReference CreateUserReferenceById(long? userId, IEnumerable<OpenAirClient.User> users) =>
            userId.HasValue ? CreateUserReference(users.FirstOrDefault(it => it.Id == userId.Value)) : null;

        /// <summary>Creates a <see cref="UserReference"/> model.</summary>
        public static UserReference CreateUserReference(OpenAirClient.User user) =>
            user == null ?
            null :
            new UserReference
            {
                OpenAirUserId = user.Id.Value,
                Email = user.Address.FirstOrDefault()?.Email
            };

        /// <summary>Creates a <see cref="Department"/> model.</summary>
        public static Department CreateDepartment(OpenAirClient.Department department, IEnumerable<OpenAirClient.User> users) =>
            department == null ? null :
            new Department
            {
                OpenAirDepartmentId = department.Id,
                Name = department.Name,
                Owner = CreateUserReferenceById(department.UserId, users)
            };
    }
}
