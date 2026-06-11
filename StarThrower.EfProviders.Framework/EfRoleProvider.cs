// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace StarThrower.EfProviders.Framework
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/8fw7xh74.aspx
    /// </summary>
    public class EfRoleProvider : RoleProvider
    {
        #region [ Private Instance Variables

        private string _applicationName = "/";
        private int _schemaVersionCheck = 0;

        #endregion


        #region [ Public Properties ]

        /// <summary>
        /// The name of the application using the role information specified in the configuration file (Web.config). The ApplicationName is stored in the data source with related user information and used when querying for user information. See the section on the ApplicationName later in this topic for more information.
        ///
        /// This property is read-write and defaults to the ApplicationPath if not specified explicitly
        /// </summary>
        public override string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                _applicationName = value;

                if (_applicationName.Length > 256)
                {
                    throw new ProviderException(String.Format(Resources.Provider_application_name_too_long));
                }
            }
        }

        #endregion


        #region [ Construction & Instantiation ]

        /// <summary>
        /// Takes as input the name of the provider and a NameValueCollection of configuration settings. Used to set property values for the provider instance including implementation-specific values and options specified in the configuration file (Machine.config or Web.config).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                name = "EfRoleProvider";
            }
            if (String.IsNullOrWhiteSpace(config["description"]))
            {
                config.Remove("description");
                config.Add("description", String.Format(Resources.RoleProvider_description));
            }
            base.Initialize(name, config);

            _schemaVersionCheck = 0;

            _applicationName = config["applicationName"];
            if (String.IsNullOrWhiteSpace(_applicationName))
            {
                _applicationName = EfProviderUtil.GetDefaultAppName();
            }

            if (_applicationName.Length > 256)
            {
                throw new ProviderException(String.Format(Resources.Provider_application_name_too_long));
            }

            config.Remove("applicationName");
            if (config.Count > 0)
            {
                string attribUnrecognized = config.GetKey(0);
                if (!String.IsNullOrWhiteSpace(attribUnrecognized))
                {
                    throw new ProviderException(String.Format(Resources.Provider_unrecognized_attribute, attribUnrecognized));
                }
            }
        }

        #endregion


        #region [ Public Methods ]

        /// <summary>
        /// Takes as input a user name and a role name and determines whether the specified user 
        /// is associated with a role from the data source for the configured ApplicationName.
        ///
        /// You should throw a ProviderException if the role name or user name specified does 
        /// not exist for the configured ApplicationName.
        ///
        /// You should throw an ArgumentException if the specified user name or role name is an 
        /// empty string and an ArgumentNullException if the specified user name or role name 
        /// is null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool IsUserInRole(string userName, string roleName)
        {
            EfProviderUtil.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            EfProviderUtil.CheckParameter(ref userName, true, false, true, 256, "userName");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    var user = (from x in db.aspnet_Users
                                where x.LoweredUserName == userName.ToLower() && x.ApplicationId == application.ApplicationId
                                select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    var role = (from x in db.aspnet_Roles
                                where x.LoweredRoleName == roleName.ToLower() && x.ApplicationId == application.ApplicationId
                                select x).FirstOrDefault();
                    if (role == null) throw new ProviderException(String.Format(Resources.Provider_role_not_found, roleName));

                    return user.aspnet_Roles.Contains(role);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes as input a user name and returns the role names that the specified user is 
        /// associated with, from the data source. Only the roles for the configured 
        /// ApplicationName are retrieved.
        ///
        /// If no roles exist for the specified user for the configured ApplicationName, you 
        /// should return a string array with no elements.
        ///
        /// You should throw an ArgumentException if the specified user name is an empty string. 
        /// You should throw an ArgumentNullException if the specified user name is 
        /// null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public override string[] GetRolesForUser(string userName)
        {
            EfProviderUtil.CheckParameter(ref userName, true, false, true, 256, "username");
            if (userName.Length < 1)
            {
                return new string[0];
            }
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    var user = (from x in db.aspnet_Users
                                where x.LoweredUserName == userName.ToLower() && x.ApplicationId == application.ApplicationId
                                select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    string[] result = user.aspnet_Roles.Where(x => x.ApplicationId == application.ApplicationId).Select(x => x.RoleName).ToArray();

                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes as input the name of a role and adds the specified role to the data source for the configured ApplicationName.
        ///
        /// You should throw a ProviderException if the specified role name already exists for the configured ApplicationName.
        ///
        /// You should throw an ArgumentException if the specified role name is an empty string, contains a comma, or exceeds the maximum length allowed by the data source, and an ArgumentNullException if the specified role name is null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="roleName"></param>
        public override void CreateRole(string roleName)
        {
            EfProviderUtil.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    if (RoleExists(roleName)) throw new ProviderException(String.Format(Resources.Provider_role_already_exists, roleName));

                    aspnet_Applications application = (from x in db.aspnet_Applications
                                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                                       select x).FirstOrDefault();
                    if (application == null)
                    {
                        application = new aspnet_Applications();
                        application.ApplicationId = Guid.NewGuid();
                        application.ApplicationName = ApplicationName;
                        application.LoweredApplicationName = ApplicationName.ToLower();
                        db.aspnet_Applications.AddObject(application);
                        db.SaveChanges();
                    }

                    aspnet_Roles role = (from x in db.aspnet_Roles
                                         where x.ApplicationId == application.ApplicationId && x.LoweredRoleName == roleName.ToLower()
                                         select x).FirstOrDefault();
                    if (role != null)
                    {
                        throw new ProviderException(String.Format(Resources.Provider_role_already_exists, roleName));
                    }

                    role = new aspnet_Roles();
                    role.RoleId = Guid.NewGuid();
                    role.RoleName = roleName;
                    role.ApplicationId = application.ApplicationId;
                    role.LoweredRoleName = roleName.ToLower();
                    db.AddToaspnet_Roles(role);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes as input the name of a role and a Boolean value that indicates whether to throw an exception if there are still users associated with the role. The DeleteRole deletes the specified role from the data source for the configured ApplicationName.
        ///
        /// If the throwOnPopulatedRole parameter is true, and the role identified by the role name parameter has one or more members, throw a ProviderException and do not delete the role. If the throwOnPopulatedRole parameter is false, then delete the role whether it is empty or not.
        ///
        /// When you delete a role from the data source, ensure that you also delete any associations between a user name and the deleted role for the configured ApplicationName.
        ///
        /// You should throw an ArgumentException if the specified role name does not exist, or is an empty string. You should throw an ArgumentNullException if the specified role name is null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="throwOnPopulatedRole"></param>
        /// <returns></returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            EfProviderUtil.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    var role = (from x in db.aspnet_Roles
                                where x.ApplicationId == application.ApplicationId
                                select x).FirstOrDefault();
                    if (role == null) throw new ProviderException(String.Format(Resources.Provider_role_not_found, roleName));

                    if (throwOnPopulatedRole && role.aspnet_Users.Count > 0) throw new ProviderException(String.Format(Resources.Role_is_not_empty));

                    db.aspnet_Roles.DeleteObject(role);
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes as input a role name and determines whether the role name exists in the 
        /// data source for the configured ApplicationName.
        ///
        /// You should throw an ArgumentException if the specified role name is an empty string. 
        /// It is recommended that you throw an ArgumentNullException if the specified role 
        /// name is null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool RoleExists(string roleName)
        {
            EfProviderUtil.CheckParameter(ref roleName, true, true, true, 256, "roleName");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) return false;

                    var role = (from x in db.aspnet_Roles
                                where x.LoweredRoleName == roleName.ToLower() && x.ApplicationId == application.ApplicationId
                                select x).FirstOrDefault();

                    return (role != null);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes as input a list of user names and a list of role names, and associates the specified users with the specified roles at the data source for the configured ApplicationName.
        ///
        /// You should throw a ProviderException if any of the role names or user names specified do not exist for the configured ApplicationName.
        ///
        /// You should throw an ArgumentException if any of the specified user names or role names is an empty string and an ArgumentNullException if any of the specified user names or role names is null (Nothing in Visual Basic).
        ///
        /// If your data source supports transactions, you should include each add operation in a transaction and roll back the transaction and throw an exception if any add operation fails.
        /// </summary>
        /// <param name="userNames"></param>
        /// <param name="roleNames"></param>
        public override void AddUsersToRoles(string[] userNames, string[] roleNames)
        {
            EfProviderUtil.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            EfProviderUtil.CheckArrayParameter(ref userNames, true, true, true, 256, "userNames");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    bool added = false;
                    foreach (string userName in userNames)
                    {
                        var user = (from x in db.aspnet_Users
                                    where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                    select x).FirstOrDefault();
                        if (user != null)
                        {
                            foreach (string roleName in roleNames)
                            {
                                var role = (from x in db.aspnet_Roles
                                            where x.ApplicationId == application.ApplicationId && x.LoweredRoleName == roleName.ToLower()
                                            select x).FirstOrDefault();
                                if (role != null)
                                {
                                    if (!role.aspnet_Users.Contains(user))
                                    {
                                        role.aspnet_Users.Add(user);
                                        added = true;
                                    }
                                }
                                else
                                {
                                    throw new ProviderException(String.Format(Resources.Provider_role_not_found, roleName));
                                }
                            }
                        }
                        else
                        {
                            throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));
                        }
                    }
                    if (added)
                    {
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes as input a list of user names and a list of role names and removes the association for the specified users from the specified roles at the data source for the configured ApplicationName.
        ///
        /// You should throw a ProviderException if any of the role names or user names specified does not exist for the configured ApplicationName.
        ///
        /// You should throw an ArgumentException if any of the specified user names or role names is an empty string and an ArgumentNullException if any of the specified user names or role names is null (Nothing in Visual Basic).
        ///
        /// If your data source supports transactions, you should include each remove operation in a transaction and roll back the transaction and throw an exception if any remove operation fails
        /// </summary>
        /// <param name="userNames"></param>
        /// <param name="roleNames"></param>
        public override void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
        {
            EfProviderUtil.CheckArrayParameter(ref roleNames, true, true, true, 256, "roleNames");
            EfProviderUtil.CheckArrayParameter(ref userNames, true, true, true, 256, "usernames");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    bool removed = false;
                    foreach (string userName in userNames)
                    {
                        var user = (from x in db.aspnet_Users
                                    where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                    select x).FirstOrDefault();
                        if (user != null)
                        {
                            foreach (string roleName in roleNames)
                            {
                                var role = (from x in db.aspnet_Roles
                                            where x.ApplicationId == application.ApplicationId && x.LoweredRoleName == roleName.ToLower()
                                            select x).FirstOrDefault();
                                if (role != null)
                                {
                                    user.aspnet_Roles.Remove(role);
                                    removed = true;
                                }
                                else
                                {
                                    throw new ProviderException(String.Format(Resources.Provider_role_not_found, roleName));
                                }
                            }
                        }
                        else
                        {
                            throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));
                        }
                    }
                    if (removed)
                    {
                        db.SaveChanges();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes as input a role name and returns the user names associated with a 
        /// role from the data source. Only the roles for the configured ApplicationName 
        /// are retrieved.
        ///
        /// If the specified role name does not exist for the configured ApplicationName, 
        /// you should throw a ProviderException.
        ///
        /// If no users are associated with the specified role for the configured 
        /// ApplicationName, you should return a string array with no elements.
        ///
        /// You should throw an ArgumentException if the specified role name is an empty 
        /// string, contains a comma, or exceeds the maximum length for a role name allowed 
        /// by your data source. You should throw an ArgumentNullException if the specified 
        /// role name is null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override string[] GetUsersInRole(string roleName)
        {
            EfProviderUtil.CheckParameter(ref roleName, true, true, true, 256, "roleName");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    var role = (from x in db.aspnet_Roles
                                where x.LoweredRoleName == roleName.ToLower() && x.ApplicationId == application.ApplicationId
                                select x).FirstOrDefault();
                    if (role == null) throw new ProviderException(String.Format(Resources.Provider_role_not_found, roleName));

                    string[] result = role.aspnet_Users.Where(x => x.ApplicationId == application.ApplicationId).Select(x => x.UserName).ToArray();

                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a list of role names from the data source. Only the roles for the 
        /// specified ApplicationName are retrieved.
        ///
        /// If no roles exist for the configured ApplicationName, you should return 
        /// a string array with no elements.
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllRoles()
        {
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    string[] result = application.aspnet_Roles.Select(x => x.RoleName).ToArray();

                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes a role name and a string value and returns a collection of user names 
        /// in the role that contain the provided string value. Wildcard support is included 
        /// based on the data source. Users are returned in alphabetical order by user name.
        ///
        /// It is recommended that you throw a ProviderException if the role name specified 
        /// does not exist in the data source.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userNameToMatch"></param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string roleName, string userNameToMatch)
        {
            EfProviderUtil.CheckParameter(ref roleName, true, true, true, 256, "roleName");
            EfProviderUtil.CheckParameter(ref userNameToMatch, true, true, false, 256, "userNameToMatch");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    var role = (from x in db.aspnet_Roles
                                where x.LoweredRoleName == roleName.ToLower() && x.ApplicationId == application.ApplicationId
                                select x).FirstOrDefault();
                    if (role == null) throw new ProviderException(String.Format(Resources.Provider_role_not_found, roleName));

                    string[] result = role.aspnet_Users.Where(x => x.ApplicationId == application.ApplicationId && x.LoweredUserName == userNameToMatch.ToLower()).Select(x => x.UserName).ToArray();

                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion


        #region [ Private Methods ]

        private void CheckSchemaVersion(MembershipDB connection)
        {
            string[] features = { "Role Manager" };
            string version = "1";
            EfProviderUtil.CheckSchemaVersion(this, connection, features, version, ref _schemaVersionCheck);
        }

        #endregion
    }
}
