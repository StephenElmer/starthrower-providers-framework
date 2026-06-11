// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Runtime.Serialization;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string ProviderName { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public object ProviderUserKey { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string PasswordQuestion { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public bool IsApproved { get; set; }

        [DataMember]
        public bool IsLockedOut { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public DateTime LastLoginDate { get; set; }

        [DataMember]
        public DateTime LastActivityDate { get; set; }

        [DataMember]
        public DateTime LastPasswordChangeDate { get; set; }

        [DataMember]
        public DateTime LastLockoutDate { get; set; }

        public User()
        { }

        public User(string providerName, string userName, object providerUserKey, string email, string passwordQuestion, string comment, bool isApproved, bool isLockedOut, DateTime creationDate, DateTime lastLoginDate, DateTime lastActivityDate, DateTime lastPasswordChangeDate, DateTime lastLockoutDate)
        {
            this.ProviderName = providerName;
            this.UserName = userName;
            this.ProviderUserKey = providerUserKey;
            this.Email = email;
            this.PasswordQuestion = passwordQuestion;
            this.Comment = comment;
            this.IsApproved = isApproved;
            this.IsLockedOut = isLockedOut;
            this.CreationDate = creationDate;
            this.LastLoginDate = lastLoginDate;
            this.LastActivityDate = lastActivityDate;
            this.LastPasswordChangeDate = lastPasswordChangeDate;
            this.LastLockoutDate = lastLockoutDate;
        }

        public User(MembershipUser user)
        {
            this.ProviderName = user.ProviderName;
            this.UserName = user.UserName;
            this.ProviderUserKey = user.ProviderUserKey;
            this.Email = user.Email;
            this.PasswordQuestion = user.PasswordQuestion;
            this.Comment = user.Comment;
            this.IsApproved = user.IsApproved;
            this.IsLockedOut = user.IsLockedOut;
            this.CreationDate = user.CreationDate;
            this.LastLoginDate = user.LastLoginDate;
            this.LastActivityDate = user.LastActivityDate;
            this.LastPasswordChangeDate = user.LastPasswordChangedDate;
            this.LastLockoutDate = user.LastLockoutDate;
        }

        public MembershipUser GetMembershipUser()
        {
            MembershipUser result = new MembershipUser(this.ProviderName, this.UserName, this.ProviderUserKey, this.Email, this.PasswordQuestion, this.Comment, this.IsApproved, this.IsLockedOut, this.CreationDate, this.LastLoginDate, this.LastActivityDate, this.LastPasswordChangeDate, this.LastLockoutDate);
            return result;
        }
    }
}
