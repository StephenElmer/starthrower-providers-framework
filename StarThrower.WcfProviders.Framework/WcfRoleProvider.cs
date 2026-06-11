// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace StarThrower.WcfProviders.Framework
{
    public class WcfRoleProvider : RoleProvider
    {
        private string _applicationName = "/";

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
        }

        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override void AddUsersToRoles(string[] userNames, string[] roleNames)
        {
            ServiceWrapper.Instance.AddUsersToRoles(userNames, roleNames);
        }

        public override void CreateRole(string roleName)
        {
            ServiceWrapper.Instance.CreateRole(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return ServiceWrapper.Instance.DeleteRole(roleName, throwOnPopulatedRole);
        }

        public override string[] FindUsersInRole(string roleName, string userNameToMatch)
        {
            return ServiceWrapper.Instance.FindUsersInRole(roleName, userNameToMatch);
        }

        public override string[] GetAllRoles()
        {
            return ServiceWrapper.Instance.GetAllRoles();
        }

        public override string[] GetRolesForUser(string userName)
        {
            return ServiceWrapper.Instance.GetRolesForUser(userName);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return ServiceWrapper.Instance.GetUsersInRole(roleName);
        }

        public override bool IsUserInRole(string userName, string roleName)
        {
            return ServiceWrapper.Instance.IsUserInRole(userName, roleName);
        }

        public override void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
        {
            ServiceWrapper.Instance.RemoveUsersFromRoles(userNames, roleNames);
        }

        public override bool RoleExists(string roleName)
        {
            return ServiceWrapper.Instance.RoleExists(roleName);
        }
    }
}
