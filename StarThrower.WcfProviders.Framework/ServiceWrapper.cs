// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using StarThrower.WcfProviders.Contract.Framework;
using System.Web.Profile;
using System.Web.Security;

namespace StarThrower.WcfProviders.Framework
{
    public class ServiceWrapper
    {
        #region [ Static Members ]

        #region [ Private Static Variables ]

        private static ServiceWrapper _serviceWrapper;
        private static object _serviceWrapperLockObj = new object();

        #endregion


        #region [ Public Static Methods ]

        public static ServiceWrapper Instance
        {
            get
            {
                if (_serviceWrapper == null)
                {
                    lock (_serviceWrapperLockObj)
                    {
                        if (_serviceWrapper == null)
                        {
                            string userName = ConfigurationManager.AppSettings["serviceUserName"];
                            string password = ConfigurationManager.AppSettings["servicePassword"];
                            string ignoreCertificateValidationAsString = ConfigurationManager.AppSettings["ignoreCertificateValidation"];
                            bool ignoreCertificateValidation = false;
                            if (!bool.TryParse(ignoreCertificateValidationAsString, out ignoreCertificateValidation))
                            {
                                ignoreCertificateValidation = false;
                            }
                            _serviceWrapper = new ServiceWrapper(userName, password, ignoreCertificateValidation);
                        }
                    }
                }
                return _serviceWrapper;
            }
        }

        #endregion

        #endregion


        #region [ Instance Members ]

        #region [ Private Instance Variables ]

        private string _serviceUserName;
        private string _servicePassword;

        #endregion


        #region [ Construction ]

        private ServiceWrapper(string serviceUserName, string servicePassword, bool ignoreCertificateValidation)
        {
            _serviceUserName = serviceUserName;
            _servicePassword = servicePassword;
            if (ignoreCertificateValidation)
            {
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            }
        }

        #endregion


        #region [ Private Methods ]

        private IRoleService GetRoleChannel()
        {
            WebChannelFactory<IRoleService> cf = new WebChannelFactory<IRoleService>("roleService");
            WebHttpBinding binding = (WebHttpBinding)(cf.Endpoint.Binding);
            if (binding.Security.Mode != WebHttpSecurityMode.None)
            {
                cf.Credentials.UserName.UserName = _serviceUserName;
                cf.Credentials.UserName.Password = _servicePassword;
            }
            IRoleService channel = cf.CreateChannel();
            return channel;
        }

        private IMembershipService GetMembershipChannel()
        {
            WebChannelFactory<IMembershipService> cf = new WebChannelFactory<IMembershipService>("membershipService");
            WebHttpBinding binding = (WebHttpBinding)(cf.Endpoint.Binding);
            if (binding.Security.Mode != WebHttpSecurityMode.None)
            {
                cf.Credentials.UserName.UserName = _serviceUserName;
                cf.Credentials.UserName.Password = _servicePassword;
            }
            IMembershipService channel = cf.CreateChannel();
            return channel;
        }

        private IProfileService GetProfileChannel()
        {
            WebChannelFactory<IProfileService> cf = new WebChannelFactory<IProfileService>("profileService");
            WebHttpBinding binding = (WebHttpBinding)(cf.Endpoint.Binding);
            if (binding.Security.Mode != WebHttpSecurityMode.None)
            {
                cf.Credentials.UserName.UserName = _serviceUserName;
                cf.Credentials.UserName.Password = _servicePassword;
            }
            IProfileService channel = cf.CreateChannel();
            return channel;
        }

        /// <summary>
        /// Remotes the certificate validate.
        /// </summary>
        /// <remarks>
        /// see http://www.codemeit.com/wcf/wcf-could-not-establish-trust-relationship-for-the-ssltls-secure-channel-with-authority.html 
        /// </remarks>
        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        #endregion


        #region [ Service API ]

        #region [ Role Service ]

        public void AddUsersToRoles(string[] userNames, string[] roleNames)
        {
            IRoleService channel = GetRoleChannel();
            channel.AddUsersToRoles(userNames, roleNames);
        }

        public void CreateRole(string roleName)
        {
            IRoleService channel = GetRoleChannel();
            channel.CreateRole(roleName);
        }

        public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            bool result = false;
            IRoleService channel = GetRoleChannel();
            result = channel.DeleteRole(roleName, throwOnPopulatedRole);
            return result;
        }

        public string[] FindUsersInRole(string roleName, string userNameToMatch)
        {
            string[] result = null;
            IRoleService channel = GetRoleChannel();
            result = channel.FindUsersInRole(roleName, userNameToMatch);
            return result;
        }

        public string[] GetAllRoles()
        {
            string[] result = null;
            IRoleService channel = GetRoleChannel();
            result = channel.GetAllRoles();
            return result;
        }

        public string[] GetRolesForUser(string userName)
        {
            string[] result = null;
            IRoleService channel = GetRoleChannel();
            result = channel.GetRolesForUser(userName);
            return result;
        }

        public string[] GetUsersInRole(string roleName)
        {
            string[] result = null;
            IRoleService channel = GetRoleChannel();
            result = channel.GetUsersInRole(roleName);
            return result;
        }

        public bool IsUserInRole(string userName, string roleName)
        {
            bool result = false;
            IRoleService channel = GetRoleChannel();
            result = channel.IsUserInRole(userName, roleName);
            return result;
        }

        public void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
        {
            IRoleService channel = GetRoleChannel();
            channel.RemoveUsersFromRoles(userNames, roleNames);
        }

        public bool RoleExists(string roleName)
        {
            bool result = false;
            IRoleService channel = GetRoleChannel();
            result = channel.RoleExists(roleName);
            return result;
        }

        #endregion

        #region [ Membership Service ]

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            bool result = false;
            IMembershipService channel = GetMembershipChannel();
            result = channel.ChangePassword(userName, oldPassword, newPassword);
            return result;
        }

        public bool ChangePasswordQuestionAndAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            bool result = false;
            IMembershipService channel = GetMembershipChannel();
            result = channel.ChangePasswordQuestionAndAnswer(userName, password, newPasswordQuestion, newPasswordAnswer);
            return result;
        }

        public MembershipUser CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            CreateUserResult userResult = null;
            IMembershipService channel = GetMembershipChannel();
            userResult = channel.CreateUser(userName, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey);
            status = userResult.Status;
            MembershipUser user = userResult.User.GetMembershipUser();
            return user;
        }

        public bool DeleteUser(string userName, bool deleteAllRelatedData)
        {
            bool result = false;
            IMembershipService channel = GetMembershipChannel();
            result = channel.DeleteUser(userName, deleteAllRelatedData);
            return result;
        }

        public bool EnablePasswordReset
        {
            get
            {
                bool result = false;
                IMembershipService channel = GetMembershipChannel();
                result = channel.EnablePasswordReset();
                return result;
            }
        }

        public bool EnablePasswordRetrieval
        {
            get
            {
                bool result = false;
                IMembershipService channel = GetMembershipChannel();
                result = channel.EnablePasswordRetrieval();
                return result;
            }
        }

        public MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            FindUserResult userResult = null;
            IMembershipService channel = GetMembershipChannel();
            userResult = channel.FindUsersByEmail(emailToMatch, pageIndex, pageSize);
            totalRecords = userResult.TotalRecords;
            MembershipUserCollection result = new MembershipUserCollection();
            foreach (User u in userResult.Users)
            {
                result.Add(u.GetMembershipUser());
            }
            return result;
        }

        public MembershipUserCollection FindUsersByName(string userNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            FindUserResult userResult = null;
            IMembershipService channel = GetMembershipChannel();
            userResult = channel.FindUsersByName(userNameToMatch, pageIndex, pageSize);
            totalRecords = userResult.TotalRecords;
            MembershipUserCollection result = new MembershipUserCollection();
            foreach (User u in userResult.Users)
            {
                result.Add(u.GetMembershipUser());
            }
            return result;
        }

        public MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            FindUserResult userResult = null;
            IMembershipService channel = GetMembershipChannel();
            userResult = channel.GetAllUsers(pageIndex, pageSize);
            totalRecords = userResult.TotalRecords;
            MembershipUserCollection result = new MembershipUserCollection();
            foreach (User u in userResult.Users)
            {
                result.Add(u.GetMembershipUser());
            }
            return result;
        }

        public int GetNumberOfUsersOnline()
        {
            int result = 0;
            IMembershipService channel = GetMembershipChannel();
            result = channel.GetNumberOfUsersOnline();
            return result;
        }

        public string GetPassword(string userName, string answer)
        {
            string result = String.Empty;
            IMembershipService channel = GetMembershipChannel();
            result = channel.GetPassword(userName, answer);
            return result;
        }

        public MembershipUser GetUserByName(string userName, bool userIsOnline)
        {
            User userResult = null;
            IMembershipService channel = GetMembershipChannel();
            userResult = channel.GetUserByName(userName, userIsOnline);
            MembershipUser result = userResult.GetMembershipUser();
            return result;
        }

        public MembershipUser GetUserByKey(object providerUserKey, bool userIsOnline)
        {
            User userResult = null;
            IMembershipService channel = GetMembershipChannel();
            userResult = channel.GetUserByKey(providerUserKey, userIsOnline);
            MembershipUser result = userResult.GetMembershipUser();
            return result;
        }

        public string GetUserNameByEmail(string email)
        {
            string result = String.Empty;
            IMembershipService channel = GetMembershipChannel();
            result = channel.GetUserNameByEmail(email);
            return result;
        }

        public int MaxInvalidPasswordAttempts
        {
            get
            {
                int result = 0;
                IMembershipService channel = GetMembershipChannel();
                result = channel.MaxInvalidPasswordAttempts();
                return result;
            }
        }

        public int MinRequiredNonAlphanumericCharacters
        {
            get
            {
                int result = 0;
                IMembershipService channel = GetMembershipChannel();
                result = channel.MinRequiredNonAlphanumericCharacters();
                return result;
            }
        }

        public int MinRequiredPasswordLength
        {
            get
            {
                int result = 0;
                IMembershipService channel = GetMembershipChannel();
                result = channel.MinRequiredPasswordLength();
                return result;
            }
        }

        public int PasswordAttemptWindow
        {
            get
            {
                int result = 0;
                IMembershipService channel = GetMembershipChannel();
                result = channel.PasswordAttemptWindow();
                return result;
            }
        }

        public MembershipPasswordFormat PasswordFormat
        {
            get
            {
                MembershipPasswordFormat result = MembershipPasswordFormat.Clear;
                IMembershipService channel = GetMembershipChannel();
                result = channel.PasswordFormat();
                return result;
            }
        }

        public string PasswordStrengthRegularExpression
        {
            get
            {
                string result = String.Empty;
                IMembershipService channel = GetMembershipChannel();
                result = channel.PasswordStrengthRegularExpression();
                return result;
            }
        }

        public bool RequiresQuestionAndAnswer
        {
            get
            {
                bool result = false;
                IMembershipService channel = GetMembershipChannel();
                result = channel.RequiresQuestionAndAnswer();
                return result;
            }
        }

        public bool RequiresUniqueEmail
        {
            get
            {
                bool result = false;
                IMembershipService channel = GetMembershipChannel();
                result = channel.RequiresUniqueEmail();
                return result;
            }
        }

        public string ResetPassword(string userName, string answer)
        {
            string result = String.Empty;
            IMembershipService channel = GetMembershipChannel();
            result = channel.ResetPassword(userName, answer);
            return result;
        }

        public string AdministrativePasswordReset(string userName)
        {
            string result = String.Empty;
            IMembershipService channel = GetMembershipChannel();
            result = channel.AdministrativePasswordReset(userName);
            return result;
        }

        public bool UnlockUser(string userName)
        {
            bool result = false;
            IMembershipService channel = GetMembershipChannel();
            result = channel.UnlockUser(userName);
            return result;
        }

        public void UpdateUser(MembershipUser user)
        {
            IMembershipService channel = GetMembershipChannel();
            channel.UpdateUser(new User(user));
        }

        public bool ValidateUser(string userName, string password)
        {
            bool result = false;
            IMembershipService channel = GetMembershipChannel();
            result = channel.ValidateUser(userName, password);
            return result;
        }

        #endregion

        #region [ Profile Service ]

        public int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int result = 0;
            IProfileService channel = GetProfileChannel();
            result = channel.DeleteInactiveProfiles(authenticationOption, userInactiveSinceDate);
            return result;
        }

        public int DeleteProfiles(string[] userNames)
        {
            int result = 0;
            IProfileService channel = GetProfileChannel();
            result = channel.DeleteProfilesByUserName(userNames);
            return result;
        }

        public int DeleteProfiles(ProfileInfoCollection profiles)
        {
            int result = 0;
            IProfileService channel = GetProfileChannel();
            result = channel.DeleteProfilesByProfile(profiles);
            return result;
        }

        public GetProfilesResult FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize)
        {
            GetProfilesResult result = null;
            IProfileService channel = GetProfileChannel();
            result = channel.FindInactiveProfilesByUserName(authenticationOption, userNameToMatch, userInactiveSinceDate, pageIndex, pageSize);
            return result;
        }

        public GetProfilesResult FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, int pageIndex, int pageSize)
        {
            GetProfilesResult result = null;
            IProfileService channel = GetProfileChannel();
            result = channel.FindProfilesByUserName(authenticationOption, userNameToMatch, pageIndex, pageSize);
            return result;
        }

        public GetProfilesResult GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize)
        {
            GetProfilesResult result = null;
            IProfileService channel = GetProfileChannel();
            result = channel.GetAllInactiveProfiles(authenticationOption, userInactiveSinceDate, pageIndex, pageSize);
            return result;
        }

        public GetProfilesResult GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize)
        {
            GetProfilesResult result = null;
            IProfileService channel = GetProfileChannel();
            result = channel.GetAllProfiles(authenticationOption, pageIndex, pageSize);
            return result;
        }

        public int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int result = 0;
            IProfileService channel = GetProfileChannel();
            result = channel.GetNumberOfInactiveProfiles(authenticationOption, userInactiveSinceDate);
            return result;
        }

        public Collection<ProfileItem> GetPropertyValues(SettingsContext context, Collection<ProfileItem> collection)
        {
            Collection<ProfileItem> result = null;
            IProfileService channel = GetProfileChannel();
            result = channel.GetPropertyValues(context, collection);
            return result;
        }

        public void SetPropertyValues(SettingsContext context, Collection<ProfileItem> collection)
        {
            IProfileService channel = GetProfileChannel();
            channel.SetPropertyValues(context, collection);
        }

        #endregion

        #endregion

        #endregion
    }
}
