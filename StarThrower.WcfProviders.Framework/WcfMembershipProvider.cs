// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace StarThrower.WcfProviders.Framework
{
    public class WcfMembershipProvider : MembershipProvider
    {
        private string _applicationName = "/";

        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return ServiceWrapper.Instance.ChangePassword(userName, oldPassword, newPassword);
        }

        public override bool ChangePasswordQuestionAndAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            return ServiceWrapper.Instance.ChangePasswordQuestionAndAnswer(userName, password, newPasswordQuestion, newPasswordAnswer);
        }

        public override MembershipUser CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            return ServiceWrapper.Instance.CreateUser(userName, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        public override bool DeleteUser(string userName, bool deleteAllRelatedData)
        {
            return ServiceWrapper.Instance.DeleteUser(userName, deleteAllRelatedData);
        }

        public override bool EnablePasswordReset
        {
            get { return ServiceWrapper.Instance.EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return ServiceWrapper.Instance.EnablePasswordRetrieval; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return ServiceWrapper.Instance.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByName(string userNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return ServiceWrapper.Instance.FindUsersByName(userNameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return ServiceWrapper.Instance.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            return ServiceWrapper.Instance.GetNumberOfUsersOnline();
        }

        public override string GetPassword(string userName, string answer)
        {
            return ServiceWrapper.Instance.GetPassword(userName, answer);
        }

        public override MembershipUser GetUser(string userName, bool userIsOnline)
        {
            return ServiceWrapper.Instance.GetUserByName(userName, userIsOnline);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return ServiceWrapper.Instance.GetUserByKey(providerUserKey, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            return ServiceWrapper.Instance.GetUserNameByEmail(email);
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return ServiceWrapper.Instance.MaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return ServiceWrapper.Instance.MinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return ServiceWrapper.Instance.MinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return ServiceWrapper.Instance.PasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return ServiceWrapper.Instance.PasswordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return ServiceWrapper.Instance.PasswordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return ServiceWrapper.Instance.RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return ServiceWrapper.Instance.RequiresUniqueEmail; }
        }

        public override string ResetPassword(string userName, string answer)
        {
            return ServiceWrapper.Instance.ResetPassword(userName, answer);
        }

        public string AdministrativePasswordReset(string userName)
        {
            return ServiceWrapper.Instance.AdministrativePasswordReset(userName);
        }

        public override bool UnlockUser(string userName)
        {
            return ServiceWrapper.Instance.UnlockUser(userName);
        }

        public override void UpdateUser(MembershipUser user)
        {
            ServiceWrapper.Instance.UpdateUser(user);
        }

        public override bool ValidateUser(string userName, string password)
        {
            return ServiceWrapper.Instance.ValidateUser(userName, password);
        }
    }
}
