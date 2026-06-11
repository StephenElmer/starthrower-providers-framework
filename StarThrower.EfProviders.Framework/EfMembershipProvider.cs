// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
using System.Security.Cryptography;

namespace StarThrower.EfProviders.Framework
{
    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/f1kyba5e.aspx
    /// </summary>
    public class EfMembershipProvider : MembershipProvider
    {
        #region [ Private Instance Variables ]

        private bool _enablePasswordRetrieval = false;
        private bool _enablePasswordReset = true;
        private bool _requiresQuestionAndAnswer = true;
        private string _applicationName = "/";
        private bool _requiresUniqueEmail = true;
        private int _maxInvalidPasswordAttempts = 3;
        private int _passwordAttemptWindow = 10;
        private int _minRequiredPasswordLength = 7;
        private int _minRequiredNonalphanumericCharacters = 0;
        private string _passwordStrengthRegularExpression;
        private int _schemaVersionCheck = 0;
        private MembershipPasswordFormat _passwordFormat = MembershipPasswordFormat.Hashed;

        private const int PASSWORD_SIZE = 14;

        #endregion


        #region [ Public Properties ]

        /// <summary>
        /// The EnablePasswordRetrieval property indicates whether users can retrieve their password using the GetPassword method.
        /// </summary>
        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        /// <summary>
        /// The EnablePasswordReset property indicates whether users can use the ResetPassword method to overwrite their current password with a new, randomly generated password.
        /// </summary>
        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        /// <summary>
        /// The RequiresQuestionAndAnswer property indicates whether users must supply a password answer in order to retrieve their password using the GetPassword method, or reset their password using the ResetPassword method.
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        /// <summary>
        /// The RequiresUniqueEmail property indicates whether users must supply a unique e-mail address value when creating a user. If a user already exists in the data source for the current ApplicationName, the CreateUser method returns null and a status value of DuplicateEmail.
        /// </summary>
        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        /// <summary>
        /// The PasswordFormat property indicates the format that passwords are stored in. Passwords can be stored in Clear, Encrypted, and Hashed password formats. Clear passwords are stored in plain text, which improves the performance of password storage and retrieval but is less secure, as passwords are easily read if your data source is compromised. Encrypted passwords are encrypted when stored and can be decrypted for password comparison or password retrieval. This requires additional processing for password storage and retrieval but is more secure, as passwords are not easily determined if the data source is compromised. Hashed passwords are hashed using a one-way hash algorithm and a randomly generated salt value when stored in the database. When a password is validated, it is hashed with the salt value in the database for verification. Hashed passwords cannot be retrieved.
        ///
        /// You can use the EncryptPassword and DecryptPassword virtual methods of the MembershipProvider class to encrypt and decrypt password values, or you can supply your own encryption code. If you use the EncryptPassword and DecryptPassword virtual methods of the MembershipProvider class, Encrypted passwords are encrypted using the key information supplied in the machineKey element the configuration file
        /// </summary>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        /// <summary>
        /// The MaxInvalidPasswordAttempts works in conjunction with the PasswordAttemptWindow to guard against an unwanted source guessing the password or password answer of a membership user through repeated attempts. If the number of invalid passwords or password questions supplied for a membership user exceeds the MaxInvalidPasswordAttempts within the number of minutes identified by the PasswordAttemptWindow, then the membership user is locked out by setting the IsLockedOut property to true until the user is unlocked using the UnlockUser method. If a valid password or password answer is supplied before the MaxInvalidPasswordAttempts is reached, the counter that tracks the number of invalid attempts is reset to zero.
        ///
        /// If the RequiresQuestionAndAnswer property is set to false, invalid password answer attempts are not tracked.
        ///
        /// Invalid password and password answer attempts are tracked in the ValidateUser, ChangePassword, ChangePasswordQuestionAndAnswer, GetPassword, and ResetPassword methods
        /// </summary>
        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// For a description, see the description of the MaxInvalidPasswordAttempts property.
        /// </summary>
        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonalphanumericCharacters; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        /// <summary>
        /// The name of the application using the membership information specified in the configuration file (Web.config). The ApplicationName is stored in the data source with related user information and used when querying for that information. See the section on the ApplicationName later in this topic for more information.
        /// </summary>
        public override string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value");
                }

                if (value.Length > 256)
                {
                    throw new ProviderException(String.Format(Resources.Provider_application_name_too_long));
                }
                _applicationName = value;
            }
        }

        #endregion


        #region [ Construction & Instantiation ]

        /// <summary>
        /// Takes, as input, the name of the provider and a NameValueCollection of configuration settings. Used to set property values for the provider instance including implementation-specific values and options specified in the configuration file (Machine.config or Web.config) supplied in the configuration.
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
                name = "EfMembershipProvider";
            }
            if (String.IsNullOrWhiteSpace(config["description"]))
            {
                config.Remove("description");
                config.Add("description", String.Format(Resources.MembershipSqlProvider_description));
            }
            base.Initialize(name, config);

            _schemaVersionCheck = 0;

            _enablePasswordRetrieval = EfProviderUtil.GetBooleanValue(config, "enablePasswordRetrieval", false);
            _enablePasswordReset = EfProviderUtil.GetBooleanValue(config, "enablePasswordReset", true);
            _requiresQuestionAndAnswer = EfProviderUtil.GetBooleanValue(config, "requiresQuestionAndAnswer", true);
            _requiresUniqueEmail = EfProviderUtil.GetBooleanValue(config, "requiresUniqueEmail", true);
            _maxInvalidPasswordAttempts = EfProviderUtil.GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
            _passwordAttemptWindow = EfProviderUtil.GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            _minRequiredPasswordLength = EfProviderUtil.GetIntValue(config, "minRequiredPasswordLength", 7, false, 128);
            _minRequiredNonalphanumericCharacters = EfProviderUtil.GetIntValue(config, "minRequiredNonalphanumericCharacters", 1, true, 128);

            _passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];
            if (_passwordStrengthRegularExpression != null)
            {
                _passwordStrengthRegularExpression = _passwordStrengthRegularExpression.Trim();
                if (_passwordStrengthRegularExpression.Length != 0)
                {
                    try
                    {
                        Regex regex = new Regex(_passwordStrengthRegularExpression);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ProviderException(e.Message, e);
                    }
                }
            }
            else
            {
                _passwordStrengthRegularExpression = String.Empty;
            }
            if (_minRequiredNonalphanumericCharacters > _minRequiredPasswordLength)
            {
                throw new HttpException(String.Format(Resources.MinRequiredNonalphanumericCharacters_can_not_be_more_than_MinRequiredPasswordLength));
            }

            _applicationName = config["applicationName"];
            if (String.IsNullOrWhiteSpace(_applicationName))
            {
                _applicationName = EfProviderUtil.GetDefaultAppName();
            }

            if (_applicationName.Length > 256)
            {
                throw new ProviderException(String.Format(Resources.Provider_application_name_too_long));
            }

            string strTemp = config["passwordFormat"];
            if (String.IsNullOrWhiteSpace(strTemp))
            {
                strTemp = "Hashed";
            }

            switch (strTemp)
            {
                case "Clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;
                case "Encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;
                default:
                    throw new ProviderException(String.Format(Resources.Provider_bad_password_format));
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed && EnablePasswordRetrieval)
            {
                throw new ProviderException(String.Format(Resources.Provider_can_not_retrieve_hashed_password));
            }

            config.Remove("enablePasswordRetrieval");
            config.Remove("enablePasswordReset");
            config.Remove("requiresQuestionAndAnswer");
            config.Remove("applicationName");
            config.Remove("requiresUniqueEmail");
            config.Remove("maxInvalidPasswordAttempts");
            config.Remove("passwordAttemptWindow");
            config.Remove("passwordFormat");
            config.Remove("name");
            config.Remove("minRequiredPasswordLength");
            config.Remove("minRequiredNonalphanumericCharacters");
            config.Remove("passwordStrengthRegularExpression");
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
        /// Takes, as input, the name of a new user, a password, and an e-mail address 
        /// and inserts a new user for the application into the data source. The 
        /// CreateUser method returns a MembershipUser object that is populated with 
        /// the information for the newly created user. The CreateUser method also 
        /// defines an out parameter (in Visual Basic, you can use ByRef) that returns 
        /// a MembershipCreateStatus value that indicates whether the user was 
        /// successfully created, or a reason that the user was not successfully created.
        ///
        /// The CreateUser method raises the ValidatingPassword event if a 
        /// MembershipValidatePasswordEventHandler has been specified, and continues or 
        /// cancels the create-user action based on the results of the event. You can use 
        /// the OnValidatingPassword virtual method to execute the specified 
        /// MembershipValidatePasswordEventHandler.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="isApproved"></param>
        /// <param name="providerUserKey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public override MembershipUser CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (!EfProviderUtil.ValidateParameter(ref password, true, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            string salt = GenerateSalt();
            string pass = EncodePassword(password, (int)_passwordFormat, salt);
            if (pass.Length > 128)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            string encodedPasswordAnswer;
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }

            if (!String.IsNullOrWhiteSpace(passwordAnswer))
            {
                if (passwordAnswer.Length > 128)
                {
                    status = MembershipCreateStatus.InvalidAnswer;
                    return null;
                }
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), (int)_passwordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = passwordAnswer;
            }
            if (!EfProviderUtil.ValidateParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, true, false, 128))
            {
                status = MembershipCreateStatus.InvalidAnswer;
                return null;
            }

            if (!EfProviderUtil.ValidateParameter(ref userName, true, true, true, 256))
            {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }

            if (!EfProviderUtil.ValidateParameter(ref email, RequiresUniqueEmail, RequiresUniqueEmail, false, 256))
            {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }

            if (!EfProviderUtil.ValidateParameter(ref passwordQuestion, RequiresQuestionAndAnswer, true, false, 256))
            {
                status = MembershipCreateStatus.InvalidQuestion;
                return null;
            }

            if (providerUserKey != null)
            {
                if (!(providerUserKey is Guid))
                {
                    status = MembershipCreateStatus.InvalidProviderUserKey;
                    return null;
                }
            }

            if (password.Length < MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            int count = 0;

            for (int i = 0; i < password.Length; i++)
            {
                if (!char.IsLetterOrDigit(password, i))
                {
                    count++;
                }
            }

            if (count < MinRequiredNonAlphanumericCharacters)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(password, PasswordStrengthRegularExpression))
                {
                    status = MembershipCreateStatus.InvalidPassword;
                    return null;
                }
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(userName, password, true);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    DateTime utcNow = DateTime.UtcNow;

                    //if the application doesn't exist, create it...
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

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user != null)
                    {
                        status = MembershipCreateStatus.DuplicateUserName;
                        return null;
                    }

                    //insert user into Users (generating new UserId)
                    user = new aspnet_Users();
                    user.ApplicationId = application.ApplicationId;
                    user.UserId = Guid.NewGuid();
                    user.UserName = userName;
                    user.LoweredUserName = userName.ToLower();
                    user.IsAnonymous = false;
                    user.LastActivityDate = utcNow;
                    db.aspnet_Users.AddObject(user);
                    db.SaveChanges();

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership != null)
                    {
                        status = MembershipCreateStatus.DuplicateProviderUserKey;
                        return null;
                    }

                    if (RequiresUniqueEmail)
                    {
                        membership = (from x in db.aspnet_Membership
                                      where x.ApplicationId == application.ApplicationId && x.LoweredEmail == email.ToLower()
                                      select x).FirstOrDefault();
                        if (membership != null)
                        {
                            status = MembershipCreateStatus.DuplicateEmail;
                            return null;
                        }
                    }

                    //insert user into Membership
                    membership = new aspnet_Membership();
                    membership.ApplicationId = application.ApplicationId;
                    membership.UserId = user.UserId;
                    membership.Password = pass;
                    membership.PasswordSalt = salt;
                    membership.Email = email;
                    membership.PasswordQuestion = passwordQuestion;
                    membership.PasswordAnswer = encodedPasswordAnswer;
                    membership.IsApproved = isApproved;
                    membership.PasswordFormat = (int)PasswordFormat;
                    membership.LoweredEmail = email.ToLower();
                    membership.CreateDate = utcNow;
                    membership.IsLockedOut = false;
                    membership.LastLockoutDate = EfProviderUtil.DbNullDate;
                    membership.FailedPasswordAttemptCount = 0;
                    membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.FailedPasswordAnswerAttemptCount = 0;
                    membership.FailedPasswordAnswerAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.LastLoginDate = EfProviderUtil.DbNullDate;
                    membership.LastPasswordChangedDate = EfProviderUtil.DbNullDate;
                    db.aspnet_Membership.AddObject(membership);
                    db.SaveChanges();
                    DateTime now = utcNow.ToLocalTime();
                    status = MembershipCreateStatus.Success;

                    return new MembershipUser(this.Name, userName, providerUserKey, email, passwordQuestion, null, isApproved, false, now, now, now, now, EfProviderUtil.DbNullDate);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes, as input, a user name, a password, a password question, and a password answer, and updates the password question and answer in the data source if the supplied user name and password are valid. The ChangePasswordQuestionAndAnswer method returns true if the password question and answer are updated successfully; otherwise, false.
        ///
        /// If the supplied user name and password are not valid, false is returned
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="newPasswordQuestion"></param>
        /// <param name="newPasswordAnswer"></param>
        /// <returns></returns>
        public override bool ChangePasswordQuestionAndAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            EfProviderUtil.CheckParameter(ref userName, true, true, true, 256, "username");
            EfProviderUtil.CheckParameter(ref password, true, true, false, 128, "password");

            string salt;
            int passwordFormat;
            if (!CheckPassword(userName, password, false, false, out salt, out passwordFormat))
            {
                return false;
            }
            EfProviderUtil.CheckParameter(ref newPasswordQuestion, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 256, "newPasswordQuestion");
            string encodedPasswordAnswer;
            if (newPasswordAnswer != null)
            {
                newPasswordAnswer = newPasswordAnswer.Trim();
            }

            EfProviderUtil.CheckParameter(ref newPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");
            if (!String.IsNullOrWhiteSpace(newPasswordAnswer))
            {
                encodedPasswordAnswer = EncodePassword(newPasswordAnswer.ToLower(CultureInfo.InvariantCulture), (int)passwordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = newPasswordAnswer;
            }
            EfProviderUtil.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "newPasswordAnswer");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        throw new ProviderException(GetExceptionText(1));
                    }

                    membership.PasswordQuestion = newPasswordQuestion;
                    membership.PasswordAnswer = encodedPasswordAnswer;
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
        /// Takes, as input, a user name and a password answer and retrieves the password for that user from the data source and returns the password as a string.
        ///
        /// GetPassword ensures that the EnablePasswordRetrieval property is set to true before performing any action. If the EnablePasswordRetrieval property is false, an ProviderException is thrown.
        ///
        /// The GetPassword method also checks the value of the RequiresQuestionAndAnswer property. If the RequiresQuestionAndAnswer property is true, the GetPassword method checks the value of the supplied answer parameter against the stored password answer in the data source. If they do not match, a MembershipPasswordException is thrown
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passwordAnswer"></param>
        /// <returns></returns>
        public override string GetPassword(string userName, string passwordAnswer)
        {
            if (!EnablePasswordRetrieval)
            {
                throw new NotSupportedException(String.Format(Resources.Membership_PasswordRetrieval_not_supported));
            }

            EfProviderUtil.CheckParameter(ref userName, true, true, true, 256, "username");

            string encodedPasswordAnswer = GetEncodedPasswordAnswer(userName, passwordAnswer);
            EfProviderUtil.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");

            string errText;
            int passwordFormat = 0;
            int status = 0;

            string pass = GetPasswordFromDB(userName, encodedPasswordAnswer, RequiresQuestionAndAnswer, out passwordFormat, out status);

            if (pass == null)
            {
                errText = GetExceptionText(status);
                if (IsStatusDueToBadPassword(status))
                {
                    throw new MembershipPasswordException(errText);
                }
                else
                {
                    throw new ProviderException(errText);
                }
            }

            return UnEncodePassword(pass, passwordFormat);
        }

        /// <summary>
        /// Takes, as input, a user name, a current password, and a new password, and updates the password in the data source if the supplied user name and current password are valid. The ChangePassword method returns true if the password was updated successfully; otherwise, false.
        ///
        /// The ChangePassword method raises the ValidatingPassword event, if a MembershipValidatePasswordEventHandler has been specified, and continues or cancels the change-password action based on the results of the event. You can use the OnValidatingPassword virtual method to execute the specified MembershipValidatePasswordEventHandler
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public override bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            EfProviderUtil.CheckParameter(ref userName, true, true, true, 256, "username");
            EfProviderUtil.CheckParameter(ref oldPassword, true, true, false, 128, "oldPassword");
            EfProviderUtil.CheckParameter(ref newPassword, true, true, false, 128, "newPassword");

            string salt = null;
            int passwordFormat;

            if (!CheckPassword(userName, oldPassword, false, false, out salt, out passwordFormat))
            {
                return false;
            }

            if (newPassword.Length < MinRequiredPasswordLength)
            {
                throw new ArgumentException(String.Format(Resources.Password_too_short, "newPassword", MinRequiredPasswordLength.ToString(CultureInfo.InvariantCulture)));
            }

            int count = 0;

            for (int i = 0; i < newPassword.Length; i++)
            {
                if (!char.IsLetterOrDigit(newPassword, i))
                {
                    count++;
                }
            }

            if (count < MinRequiredNonAlphanumericCharacters)
            {
                throw new ArgumentException(String.Format(Resources.Password_need_more_non_alpha_numeric_chars, "newPassword", MinRequiredNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture)));
            }

            if (PasswordStrengthRegularExpression.Length > 0)
            {
                if (!Regex.IsMatch(newPassword, PasswordStrengthRegularExpression))
                {
                    throw new ArgumentException(String.Format(Resources.Password_does_not_match_regular_expression, "newPassword"));
                }
            }

            string pass = EncodePassword(newPassword, (int)passwordFormat, salt);
            if (pass.Length > 128)
            {
                throw new ArgumentException(String.Format(Resources.Membership_password_too_long), "newPassword");
            }

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(userName, newPassword, false);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ArgumentException(String.Format(Resources.Membership_Custom_Password_Validation_Failure), "newPassword");
                }
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

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        return false;  //throw an exception?  (errorCode = 1)
                    }

                    membership.Password = pass;
                    membership.PasswordFormat = (int)PasswordFormat;
                    membership.PasswordSalt = salt;
                    membership.LastPasswordChangedDate = DateTime.UtcNow;
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
        /// Takes, as input, a user name and a password answer and generates a new, random password for the specified user. The ResetPassword method updates the user information in the data source with the new password value and returns the new password as a string. A convenient mechanism for generating a random password is the GeneratePassword method of the Membership class.
        ///
        /// The ResetPassword method ensures that the EnablePasswordReset property is set to true before performing any action. If the EnablePasswordReset property is false, a NotSupportedException is thrown. The ResetPassword method also checks the value of the RequiresQuestionAndAnswer property. If the RequiresQuestionAndAnswer property is true, the ResetPassword method checks the value of the supplied answer parameter against the stored password answer in the data source. If they do not match, a MembershipPasswordException is thrown.
        ///
        /// The ResetPassword method raises the ValidatingPassword event, if a MembershipValidatePasswordEventHandler has been specified, to validate the newly generated password and continues or cancels the reset-password action based on the results of the event. You can use the OnValidatingPassword virtual method to execute the specified MembershipValidatePasswordEventHandler.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passwordAnswer"></param>
        /// <returns></returns>
        public override string ResetPassword(string userName, string passwordAnswer)
        {
            DateTime utcNow = DateTime.UtcNow;

            if (!EnablePasswordReset)
            {
                throw new NotSupportedException(String.Format(Resources.Not_configured_to_support_password_resets));
            }

            EfProviderUtil.CheckParameter(ref userName, true, true, true, 256, "userName");

            string salt;
            int passwordFormat;
            string passwdFromDB;
            int status;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            bool isApproved;
            DateTime lastLoginDate, lastActivityDate;

            bool result = GetPasswordWithFormat(userName, false, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (!result)
            {
                if (IsStatusDueToBadPassword(status))
                {
                    throw new MembershipPasswordException(GetExceptionText(status));
                }
                else
                {
                    throw new ProviderException(GetExceptionText(status));
                }
            }

            string encodedPasswordAnswer;
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (!String.IsNullOrWhiteSpace(passwordAnswer))
            {
                encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt);
            }
            else
            {
                encodedPasswordAnswer = passwordAnswer;
            }
            EfProviderUtil.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");
            string newPassword = GeneratePassword();

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(userName, newPassword, false);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ProviderException(String.Format(Resources.Membership_Custom_Password_Validation_Failure));
                }
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

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        throw new ProviderException(String.Format(Resources.Membership_UserNotFound));
                    }

                    if (membership.IsLockedOut)
                    {
                        throw new ProviderException(String.Format(Resources.Membership_AccountLockOut));
                    }

                    if (RequiresQuestionAndAnswer)
                    {
                        if (encodedPasswordAnswer != membership.PasswordAnswer)
                        {
                            if (utcNow > membership.FailedPasswordAnswerAttemptWindowStart.Add(new TimeSpan(0, PasswordAttemptWindow, 0)))
                            {
                                membership.FailedPasswordAnswerAttemptCount = 1;
                                membership.FailedPasswordAnswerAttemptWindowStart = utcNow;
                            }
                            else
                            {
                                membership.FailedPasswordAnswerAttemptCount += 1;
                                membership.FailedPasswordAnswerAttemptWindowStart = utcNow;
                            }

                            if (membership.FailedPasswordAnswerAttemptCount >= MaxInvalidPasswordAttempts)
                            {
                                membership.IsLockedOut = true;
                                membership.LastLockoutDate = utcNow;
                            }

                            throw new ProviderException(String.Format(Resources.Membership_InvalidAnswer));
                        }
                        else
                        {
                            membership.FailedPasswordAttemptCount = 0;
                            membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                        }
                    }

                    //if got to here everything is okay so reset some things
                    membership.Password = EncodePassword(newPassword, (int)passwordFormat, salt);
                    membership.IsLockedOut = false;
                    membership.FailedPasswordAttemptCount = 0;
                    membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.FailedPasswordAnswerAttemptCount = 0;
                    membership.FailedPasswordAnswerAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.PasswordFormat = (int)passwordFormat;
                    membership.PasswordSalt = salt;

                    db.SaveChanges();

                    return newPassword;
                }
            }
            catch
            {
                throw;
            }
        }

        public string AdministrativePasswordReset(string userName)
        {
            DateTime utcNow = DateTime.UtcNow;

            if (!EnablePasswordReset)
            {
                throw new NotSupportedException(String.Format(Resources.Not_configured_to_support_password_resets));
            }

            EfProviderUtil.CheckParameter(ref userName, true, true, true, 256, "userName");

            string salt;
            int passwordFormat;
            string passwdFromDB;
            int status;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            bool isApproved;
            DateTime lastLoginDate, lastActivityDate;

            bool result = GetPasswordWithFormat(userName, false, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (!result)
            {
                if (IsStatusDueToBadPassword(status))
                {
                    throw new MembershipPasswordException(GetExceptionText(status));
                }
                else
                {
                    throw new ProviderException(GetExceptionText(status));
                }
            }

            //string encodedPasswordAnswer;
            //if (passwordAnswer != null)
            //{
            //    passwordAnswer = passwordAnswer.Trim();
            //}
            //if (!String.IsNullOrWhiteSpace(passwordAnswer))
            //{
            //    encodedPasswordAnswer = EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, salt);
            //}
            //else
            //{
            //    encodedPasswordAnswer = passwordAnswer;
            //}
            //EfProviderUtil.CheckParameter(ref encodedPasswordAnswer, RequiresQuestionAndAnswer, RequiresQuestionAndAnswer, false, 128, "passwordAnswer");
            
            string newPassword = GeneratePassword();

            ValidatePasswordEventArgs e = new ValidatePasswordEventArgs(userName, newPassword, false);
            OnValidatingPassword(e);

            if (e.Cancel)
            {
                if (e.FailureInformation != null)
                {
                    throw e.FailureInformation;
                }
                else
                {
                    throw new ProviderException(String.Format(Resources.Membership_Custom_Password_Validation_Failure));
                }
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

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        throw new ProviderException(String.Format(Resources.Membership_UserNotFound));
                    }

                    if (membership.IsLockedOut)
                    {
                        throw new ProviderException(String.Format(Resources.Membership_AccountLockOut));
                    }

                    //if (RequiresQuestionAndAnswer)
                    //{
                    //    if (encodedPasswordAnswer != membership.PasswordAnswer)
                    //    {
                    //        if (utcNow > membership.FailedPasswordAnswerAttemptWindowStart.Add(new TimeSpan(0, PasswordAttemptWindow, 0)))
                    //        {
                    //            membership.FailedPasswordAnswerAttemptCount = 1;
                    //            membership.FailedPasswordAnswerAttemptWindowStart = utcNow;
                    //        }
                    //        else
                    //        {
                    //            membership.FailedPasswordAnswerAttemptCount += 1;
                    //            membership.FailedPasswordAnswerAttemptWindowStart = utcNow;
                    //        }
                    //        if (membership.FailedPasswordAnswerAttemptCount >= MaxInvalidPasswordAttempts)
                    //        {
                    //            membership.IsLockedOut = true;
                    //            membership.LastLockoutDate = utcNow;
                    //        }
                    //        throw new ProviderException(String.Format(Resources.Membership_InvalidAnswer));
                    //    }
                    //    else
                    //    {
                    //        membership.FailedPasswordAttemptCount = 0;
                    //        membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                    //    }
                    //}

                    //if got to here everything is okay so reset some things
                    membership.Password = EncodePassword(newPassword, (int)passwordFormat, salt);
                    membership.IsLockedOut = false;
                    membership.FailedPasswordAttemptCount = 0;
                    membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.FailedPasswordAnswerAttemptCount = 0;
                    membership.FailedPasswordAnswerAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.PasswordFormat = (int)passwordFormat;
                    membership.PasswordSalt = salt;

                    db.SaveChanges();

                    return newPassword;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes, as input, a MembershipUser object populated with user information and updates the data source with the supplied values.
        /// </summary>
        /// <param name="user"></param>
        public override void UpdateUser(MembershipUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            string temp = user.UserName;
            EfProviderUtil.CheckParameter(ref temp, true, true, true, 256, "UserName");
            temp = user.Email;
            EfProviderUtil.CheckParameter(ref temp, RequiresUniqueEmail, RequiresUniqueEmail, false, 256, "Email");
            user.Email = temp;
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    aspnet_Users usr = (from x in db.aspnet_Users
                                        where x.ApplicationId == application.ApplicationId && x.UserId == (Guid)user.ProviderUserKey
                                        select x).FirstOrDefault();
                    if (usr == null)
                    {
                        throw new ProviderException();
                    }

                    if (RequiresUniqueEmail)
                    {
                        var u = (from x in db.aspnet_Membership
                                 where x.ApplicationId == application.ApplicationId && x.UserId != usr.UserId && x.LoweredEmail == user.Email.ToLower()
                                 select x).FirstOrDefault();
                        if (u != null)
                        {
                            throw new ProviderException();
                        }
                    }

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == usr.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        throw new ProviderException();
                    }
                    membership.Email = user.Email;
                    membership.Comment = user.Comment;
                    membership.IsApproved = user.IsApproved;
                    membership.LastLoginDate = user.LastLoginDate.ToUniversalTime();

                    user.LastActivityDate = user.LastActivityDate.ToUniversalTime();
                    db.SaveChanges();
                    return;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes, as input, a user name and a password and verifies that the values match those in the data source. The ValidateUser method returns true for a successful user name and password match; otherwise, false.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override bool ValidateUser(string userName, string password)
        {
            EfProviderUtil.ValidateParameter(ref userName, true, true, true, 256);
            EfProviderUtil.ValidateParameter(ref password, true, true, false, 128);

            if (CheckPassword(userName, password, true, true))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Takes, as input, a user name, and updates the field in the data source that stores the IsLockedOut property to false. The UnlockUser method returns true if the record for the membership user is updated successfully; otherwise false.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public override bool UnlockUser(string userName)
        {
            EfProviderUtil.CheckParameter(ref userName, true, true, true, 256, "username");
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) return false;

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null) return false;

                    membership.IsLockedOut = false;
                    membership.FailedPasswordAttemptCount = 0;
                    membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.FailedPasswordAnswerAttemptCount = 0;
                    membership.FailedPasswordAnswerAttemptWindowStart = EfProviderUtil.DbNullDate;
                    membership.LastLockoutDate = EfProviderUtil.DbNullDate;
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
        /// Takes, as input, a unique user identifier and a Boolean value indicating whether to update the LastActivityDate value for the user to show that the user is currently online. The GetUser method returns a MembershipUser object populated with current values from the data source for the specified user. If the user name is not found in the data source, the GetUser method returns null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="providerUserKey"></param>
        /// <param name="userIsOnline"></param>
        /// <returns></returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey == null)
            {
                throw new ArgumentNullException("providerUserKey");
            }

            if (!(providerUserKey is Guid))
            {
                throw new ArgumentException(String.Format(Resources.Membership_InvalidProviderUserKey), "providerUserKey");
            }

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) return null;

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.UserId == (Guid)providerUserKey
                                         select x).FirstOrDefault();
                    if (user == null) return null;

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null) return null;

                    string email = membership.Email;
                    string passwordQuestion = membership.PasswordQuestion;
                    string comment = membership.Comment;
                    bool isApproved = membership.IsApproved;
                    DateTime dtCreate = membership.CreateDate.ToLocalTime();
                    DateTime dtLastLogin = membership.LastLoginDate.ToLocalTime();
                    DateTime dtLastActivity = user.LastActivityDate.ToLocalTime();
                    DateTime dtLastPassChange = membership.LastPasswordChangedDate.ToLocalTime();
                    string userName = user.UserName;
                    bool isLockedOut = membership.IsLockedOut;
                    DateTime dtLastLockoutDate = membership.LastLockoutDate.ToLocalTime();
                    return new MembershipUser(this.Name, userName, providerUserKey, email, passwordQuestion, comment, isApproved, isLockedOut, dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange, dtLastLockoutDate);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes, as input, a user name and a Boolean value indicating whether to update the LastActivityDate value for the user to show that the user is currently online. The GetUser method returns a MembershipUser object populated with current values from the data source for the specified user. If the user name is not found in the data source, the GetUser method returns null (Nothing in Visual Basic).
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="userIsOnline"></param>
        /// <returns></returns>
        public override MembershipUser GetUser(string userName, bool userIsOnline)
        {
            EfProviderUtil.CheckParameter(ref userName, true, false, true, 256, "username");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) return null;

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) return null;

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null) return null;

                    string email = membership.Email;
                    string passwordQuestion = membership.PasswordQuestion;
                    string comment = membership.Comment;
                    bool isApproved = membership.IsApproved;
                    DateTime dtCreate = membership.CreateDate.ToLocalTime();
                    DateTime dtLastLogin = membership.LastLoginDate.ToLocalTime();
                    DateTime dtLastActivity = user.LastActivityDate.ToLocalTime();
                    DateTime dtLastPassChange = membership.LastPasswordChangedDate.ToLocalTime();
                    Guid userId = user.UserId;
                    bool isLockedOut = membership.IsLockedOut;
                    DateTime dtLastLockoutDate = membership.LastLockoutDate.ToLocalTime();
                    return new MembershipUser(this.Name, userName, userId, email, passwordQuestion, comment, isApproved, isLockedOut, dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange, dtLastLockoutDate);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes, as input, an e-mail address and returns the first user name from the data source where the e-mail address matches the supplied email parameter value.
        ///
        /// If no user name is found with a matching e-mail address, an empty string is returned.
        ///
        /// If multiple user names are found that match a particular e-mail address, only the first user name found is returned
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public override string GetUserNameByEmail(string email)
        {
            EfProviderUtil.CheckParameter(ref email, false, false, false, 256, "email");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) return null;

                    var membership = (from x in db.aspnet_Membership
                                      join y in db.aspnet_Users
                                      on x.UserId equals y.UserId
                                      where x.ApplicationId == application.ApplicationId && x.LoweredEmail == email.ToLower()
                                      orderby y.UserName
                                      select y.UserName);
                    int count = membership.Count();
                    if (count == 1) return membership.FirstOrDefault();
                    if (count == 0) return null;
                    if (count > 1)
                    {
                        if (RequiresUniqueEmail) throw new ProviderException(String.Format(Resources.Membership_more_than_one_user_with_email));
                        return membership.LastOrDefault();
                    }
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Takes, as input, the name of a user and deletes that user's information from the data source. 
        /// The DeleteUser method returns true if the user was successfully deleted; otherwise, false. An 
        /// additional Boolean parameter is included to indicate whether related information for the user, 
        /// such as role or profile information is also deleted.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="deleteAllRelatedData"></param>
        /// <returns></returns>
        public override bool DeleteUser(string userName, bool deleteAllRelatedData)
        {
            EfProviderUtil.CheckParameter(ref userName, true, true, true, 256, "username");

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) return false;

                    var user = (from x in db.aspnet_Users
                                where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                select x).FirstOrDefault();
                    if (user != null)
                    {
                        if (deleteAllRelatedData)
                        {
                            aspnet_Membership membership = (from x in db.aspnet_Membership
                                                            where x.UserId == user.UserId
                                                            select x).FirstOrDefault();
                            if (membership != null)
                            {
                                db.DeleteObject(membership);
                            }

                            user.aspnet_Roles.Clear();

                            aspnet_Profile profile = (from x in db.aspnet_Profile
                                                      where x.UserId == user.UserId
                                                      select x).FirstOrDefault();
                            if (profile != null)
                            {
                                db.DeleteObject(profile);
                            }

                            user.aspnet_PersonalizationPerUser.Clear();
                        }

                        db.DeleteObject(user);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a MembershipUserCollection populated with MembershipUser objects for all of the users in the data source.
        ///
        /// The results returned by GetAllUsers are constrained by the pageIndex and pageSize parameters. The pageSize parameter identifies the maximum number of MembershipUser objects to return in the MembershipUserCollection. The pageIndex parameter identifies which page of results to return, where 0 identifies the first page. The totalRecords parameter is an out parameter that is set to the total number of membership users. For example, if 13 users were in the database for the application, and the pageIndex value was 1 with a pageSize of 5, the MembershipUserCollection returned would contain the sixth through the tenth users returned. totalRecords would be set to 13
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
            {
                throw new ArgumentException(String.Format(Resources.PageIndex_bad), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(String.Format(Resources.PageSize_bad), "pageSize");
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException(String.Format(Resources.PageIndex_PageSize_bad), "pageIndex and pageSize");
            }

            MembershipUserCollection result = new MembershipUserCollection();
            totalRecords = 0;
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null)
                    {
                        totalRecords = 0;
                        return new MembershipUserCollection();
                    }

                    var membership = (from x in db.aspnet_Membership
                                      join y in db.aspnet_Users
                                      on x.UserId equals y.UserId
                                      where x.ApplicationId == application.ApplicationId
                                      orderby y.UserName
                                      select new
                                      {
                                          UserName = y.UserName,
                                          Email = x.Email,
                                          PasswordQuestion = x.PasswordQuestion,
                                          Comment = x.Comment,
                                          IsApproved = x.IsApproved,
                                          CreateDate = x.CreateDate,
                                          LastLoginDate = x.LastLoginDate,
                                          LastActivityDate = y.LastActivityDate,
                                          LastPasswordChangedDate = x.LastPasswordChangedDate,
                                          UserId = y.UserId,
                                          IsLockedOut = x.IsLockedOut,
                                          LastLockoutDate = x.LastLockoutDate
                                      });
                    foreach (var user in membership) //TODO: paging stuff here!
                    {
                        string username;
                        string email;
                        string passwordQuestion;
                        string comment;
                        bool isApproved;
                        DateTime dtCreate;
                        DateTime dtLastLogin;
                        DateTime dtLastActivity;
                        DateTime dtLastPassChange;
                        Guid userId;
                        bool isLockedOut;
                        DateTime dtLastLockoutDate;
                        username = user.UserName;
                        email = user.Email;
                        passwordQuestion = user.PasswordQuestion;
                        comment = user.Comment;
                        isApproved = user.IsApproved;
                        dtCreate = user.CreateDate.ToLocalTime();
                        dtLastLogin = user.LastLoginDate.ToLocalTime();
                        dtLastActivity = user.LastActivityDate.ToLocalTime();
                        dtLastPassChange = user.LastPasswordChangedDate.ToLocalTime();
                        userId = user.UserId;
                        isLockedOut = user.IsLockedOut;
                        dtLastLockoutDate = user.LastLockoutDate.ToLocalTime();
                        result.Add(new MembershipUser(this.Name, username, userId, email, passwordQuestion, comment, isApproved, isLockedOut, dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange, dtLastLockoutDate));
                    }
                }
            }
            catch
            {
                throw;
            }
            return result;
        }

        /// <summary>
        /// Returns an integer value that is the count of all the users in the data source where the LastActivityDate is greater than the current date and time minus the UserIsOnlineTimeWindow property. The UserIsOnlineTimeWindow property is an integer value specifying the number of minutes to use when determining whether a user is online.
        /// </summary>
        /// <returns></returns>
        public override int GetNumberOfUsersOnline()
        {
            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) return 0;

                    DateTime dateActive = DateTime.UtcNow.Subtract(new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0));
                    var result = (from x in db.aspnet_Users
                                  where x.ApplicationId == application.ApplicationId && x.LastActivityDate > dateActive
                                  select x).Count();
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a list of membership users where the user name contains a match of the supplied usernameToMatch for the configured ApplicationName. For example, if the usernameToMatch parameter is set to "user," then the users "user1," "user2," "user3," and so on are returned. Wildcard support is included based on the data source. Users are returned in alphabetical order by user name.
        ///
        /// The results returned by FindUsersByName are constrained by the pageIndex and pageSize parameters. The pageSize parameter identifies the number of MembershipUser objects to return in the MembershipUserCollection. The pageIndex parameter identifies which page of results to return, where 1 identifies the first page. The totalRecords parameter is an out parameter that is set to the total number of membership users that matched the usernameToMatch value. For example, if 13 users were found where usernameToMatch matched part of or the entire user name, and the pageIndex value was 2 with a pageSize of 5, then the MembershipUserCollection would contain the sixth through the tenth users returned. totalRecords would be set to 13.
        /// </summary>
        /// <param name="userNameToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByName(string userNameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            EfProviderUtil.CheckParameter(ref userNameToMatch, true, true, false, 256, "usernameToMatch");

            if (pageIndex < 0)
            {
                throw new ArgumentException(String.Format(Resources.PageIndex_bad), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(String.Format(Resources.PageSize_bad), "pageSize");
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException(String.Format(Resources.PageIndex_PageSize_bad), "pageIndex and pageSize");
            }

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null)
                    {
                        totalRecords = 0;
                        return new MembershipUserCollection();
                    }

                    var users = from x in db.aspnet_Membership
                                join y in db.aspnet_Users
                                on x.UserId equals y.UserId
                                where x.ApplicationId == application.ApplicationId && y.LoweredUserName == userNameToMatch.ToLower()
                                orderby y.UserName
                                select new
                                {
                                    UserName = y.UserName,
                                    Email = x.Email,
                                    PasswordQuestion = x.PasswordQuestion,
                                    Comment = x.Comment,
                                    IsApproved = x.IsApproved,
                                    CreateDate = x.CreateDate,
                                    LastLoginDate = x.LastLoginDate,
                                    LastActivityDate = y.LastActivityDate,
                                    LastPasswordChangedDate = x.LastPasswordChangedDate,
                                    UserId = y.UserId,
                                    IsLockedOut = x.IsLockedOut,
                                    LastLockoutDate = x.LastLockoutDate
                                };
                    MembershipUserCollection result = new MembershipUserCollection();
                    foreach (var user in users) //TODO: paging here!
                    {
                        string username;
                        string email;
                        string passwordQuestion;
                        string comment;
                        bool isApproved;
                        DateTime dtCreate;
                        DateTime dtLastLogin;
                        DateTime dtLastActivity;
                        DateTime dtLastPassChange;
                        Guid userId;
                        bool isLockedOut;
                        DateTime dtLastLockoutDate;
                        username = user.UserName;
                        email = user.Email;
                        passwordQuestion = user.PasswordQuestion;
                        comment = user.Comment;
                        isApproved = user.IsApproved;
                        dtCreate = user.CreateDate.ToLocalTime();
                        dtLastLogin = user.LastLoginDate.ToLocalTime();
                        dtLastActivity = user.LastActivityDate.ToLocalTime();
                        dtLastPassChange = user.LastPasswordChangedDate.ToLocalTime();
                        userId = user.UserId;
                        isLockedOut = user.IsLockedOut;
                        dtLastLockoutDate = user.LastLockoutDate.ToLocalTime();
                        result.Add(new MembershipUser(this.Name, username, userId, email, passwordQuestion, comment, isApproved, isLockedOut, dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange, dtLastLockoutDate));
                    }
                    totalRecords = users.Count();
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a list of membership users where the user name contains a match of the supplied emailToMatch for the 
        /// configured ApplicationName. For example, if the emailToMatch parameter is set to "address@example.com," then 
        /// users with the e-mail addresses "address1@example.com," "address2@example.com," and so on are returned. 
        /// Wildcard support is included based on the data source. Users are returned in alphabetical order by user name.
        ///
        /// The results returned by FindUsersByEmail are constrained by the pageIndex and pageSize parameters. The pageSize 
        /// parameter identifies the number of MembershipUser objects to return in the MembershipUserCollection collection. 
        /// The pageIndex parameter identifies which page of results to return, where 1 identifies the first page. The 
        /// totalRecords parameter is an out parameter that is set to the total number of membership users that matched 
        /// the emailToMatch value. For example, if 13 users were found where emailToMatch matched part of or the entire 
        /// user name, and the pageIndex value was 2 with a pageSize of 5, then the MembershipUserCollection would contain 
        /// the sixth through the tenth users returned. totalRecords would be set to 13.
        /// </summary>
        /// <param name="emailToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            EfProviderUtil.CheckParameter(ref emailToMatch, false, false, false, 256, "emailToMatch");

            if (pageIndex < 0)
            {
                throw new ArgumentException(String.Format(Resources.PageIndex_bad), "pageIndex");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException(String.Format(Resources.PageSize_bad), "pageSize");
            }

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
            {
                throw new ArgumentException(String.Format(Resources.PageIndex_PageSize_bad), "pageIndex and pageSize");
            }

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null)
                    {
                        totalRecords = 0;
                        return new MembershipUserCollection();
                    }

                    var users = from x in db.aspnet_Membership
                                join y in db.aspnet_Users
                                on x.UserId equals y.UserId
                                where x.ApplicationId == application.ApplicationId && x.LoweredEmail == emailToMatch.ToLower()
                                orderby y.UserName
                                select new
                                {
                                    UserName = y.UserName,
                                    Email = x.Email,
                                    PasswordQuestion = x.PasswordQuestion,
                                    Comment = x.Comment,
                                    IsApproved = x.IsApproved,
                                    CreateDate = x.CreateDate,
                                    LastLoginDate = x.LastLoginDate,
                                    LastActivityDate = y.LastActivityDate,
                                    LastPasswordChangedDate = x.LastPasswordChangedDate,
                                    UserId = y.UserId,
                                    IsLockedOut = x.IsLockedOut,
                                    LastLockoutDate = x.LastLockoutDate
                                };
                    MembershipUserCollection result = new MembershipUserCollection();
                    foreach (var user in users) //TODO: paging here!
                    {
                        string username;
                        string email;
                        string passwordQuestion;
                        string comment;
                        bool isApproved;
                        DateTime dtCreate;
                        DateTime dtLastLogin;
                        DateTime dtLastActivity;
                        DateTime dtLastPassChange;
                        Guid userId;
                        bool isLockedOut;
                        DateTime dtLastLockoutDate;
                        username = user.UserName;
                        email = user.Email;
                        passwordQuestion = user.PasswordQuestion;
                        comment = user.Comment;
                        isApproved = user.IsApproved;
                        dtCreate = user.CreateDate.ToLocalTime();
                        dtLastLogin = user.LastLoginDate.ToLocalTime();
                        dtLastActivity = user.LastActivityDate.ToLocalTime();
                        dtLastPassChange = user.LastPasswordChangedDate.ToLocalTime();
                        userId = user.UserId;
                        isLockedOut = user.IsLockedOut;
                        dtLastLockoutDate = user.LastLockoutDate.ToLocalTime();
                        result.Add(new MembershipUser(this.Name, username, userId, email, passwordQuestion, comment, isApproved, isLockedOut, dtCreate, dtLastLogin, dtLastActivity, dtLastPassChange, dtLastLockoutDate));
                    }
                    totalRecords = users.Count();
                    return result;
                }
            }
            catch
            {
                throw;
            }
        }

        public virtual string GeneratePassword()
        {
            return Membership.GeneratePassword(MinRequiredPasswordLength < PASSWORD_SIZE ? PASSWORD_SIZE : MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);
        }

        #endregion


        #region [ Private Methods ]

        private void CheckSchemaVersion(MembershipDB connection)
        {
            string[] features = { "Common", "Membership" };
            string version = "1";
            EfProviderUtil.CheckSchemaVersion(this, connection, features, version, ref _schemaVersionCheck);
        }

        private bool CheckPassword(string userName, string password, bool updateLastLoginActivityDate, bool failIfNotApproved)
        {
            string salt;
            int passwordFormat;
            return CheckPassword(userName, password, updateLastLoginActivityDate, failIfNotApproved, out salt, out passwordFormat);
        }

        private bool CheckPassword(string userName, string password, bool updateLastLoginActivityDate, bool failIfNotApproved, out string salt, out int passwordFormat)
        {
            DateTime utcNow = DateTime.UtcNow;
            string passwdFromDB;
            int status;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            bool isPasswordCorrect;
            bool isApproved;
            DateTime lastLoginDate, lastActivityDate;

            bool result = GetPasswordWithFormat(userName, updateLastLoginActivityDate, out status, out passwdFromDB, out passwordFormat, out salt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (!result)
            {
                return false;
            }
            if (!isApproved && failIfNotApproved)
            {
                return false;
            }

            string encodedPasswd = EncodePassword(password, passwordFormat, salt);

            isPasswordCorrect = passwdFromDB.Equals(encodedPasswd);

            if (isPasswordCorrect && failedPasswordAttemptCount == 0 && failedPasswordAnswerAttemptCount == 0)
            {
                return true;
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

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        return false;  //throw an exception?  (errorCode = 1)
                    }

                    if (membership.IsLockedOut)
                    {
                        return false; //throw exception?  (errorCode = 99)
                    }

                    if (!isPasswordCorrect)
                    {
                        if (utcNow > membership.FailedPasswordAttemptWindowStart.Add(new TimeSpan(0, PasswordAttemptWindow, 0)))
                        {
                            membership.FailedPasswordAttemptCount = 1;
                            membership.FailedPasswordAttemptWindowStart = utcNow;
                        }
                        else
                        {
                            membership.FailedPasswordAttemptCount += 1;
                            membership.FailedPasswordAttemptWindowStart = utcNow;
                        }

                        if (membership.FailedPasswordAttemptCount >= MaxInvalidPasswordAttempts)
                        {
                            membership.IsLockedOut = true;
                            membership.LastLockoutDate = utcNow;
                        }
                    }
                    else
                    {
                        if (membership.FailedPasswordAttemptCount > 0 || membership.FailedPasswordAnswerAttemptCount > 0)
                        {
                            membership.FailedPasswordAttemptCount = 0;
                            membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                            membership.FailedPasswordAnswerAttemptCount = 0;
                            membership.FailedPasswordAnswerAttemptWindowStart = EfProviderUtil.DbNullDate;
                            membership.LastLockoutDate = EfProviderUtil.DbNullDate;
                        }
                    }

                    if (updateLastLoginActivityDate)
                    {
                        user.LastActivityDate = DateTime.UtcNow;
                        membership.LastLoginDate = DateTime.UtcNow;
                    }

                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }

            return isPasswordCorrect;
        }

        private bool GetPasswordWithFormat(string userName, bool updateLastLoginActivityDate, out int status, out string password, out int passwordFormat, out string passwordSalt, out int failedPasswordAttemptCount, out int failedPasswordAnswerAttemptCount, out bool isApproved, out DateTime lastLoginDate, out DateTime lastActivityDate)
        {
            try
            {
                status = -1;
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null)
                    {
                        status = 1;
                        throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));
                    }

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null)
                    {
                        status = 1;
                        throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));
                    }

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        status = 1;
                        throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));
                    }

                    if (membership.IsLockedOut)
                    {
                        status = 99;
                        throw new ProviderException(String.Format(Resources.Membership_AccountLockOut));
                    }

                    if (status == -1)
                    {
                        password = membership.Password;
                        passwordFormat = membership.PasswordFormat;
                        passwordSalt = membership.PasswordSalt;
                        failedPasswordAttemptCount = membership.FailedPasswordAttemptCount;
                        failedPasswordAnswerAttemptCount = membership.FailedPasswordAnswerAttemptCount;
                        isApproved = membership.IsApproved;
                        lastLoginDate = membership.LastLoginDate;
                        lastActivityDate = user.LastActivityDate;
                        status = -1;
                    }
                    else
                    {
                        password = null;
                        passwordFormat = 0;
                        passwordSalt = null;
                        failedPasswordAttemptCount = 0;
                        failedPasswordAnswerAttemptCount = 0;
                        isApproved = false;
                        lastLoginDate = DateTime.UtcNow;
                        lastActivityDate = DateTime.UtcNow;
                    }
                    return status == -1;
                }
            }
            catch
            {
                throw;
            }

        }

        private string GetPasswordFromDB(string userName, string encodedPasswordAnswer, bool requiresQuestionAndAnswer, out int passwordFormat, out int status)
        {
            DateTime utcNow = DateTime.UtcNow;

            try
            {
                using (MembershipDB db = new MembershipDB())
                {
                    CheckSchemaVersion(db);

                    var application = (from x in db.aspnet_Applications
                                       where x.LoweredApplicationName == ApplicationName.ToLower()
                                       select x).FirstOrDefault();
                    if (application == null) throw new ProviderException(String.Format(Resources.Provider_application_not_found, ApplicationName));

                    aspnet_Users user = (from x in db.aspnet_Users
                                         where x.ApplicationId == application.ApplicationId && x.LoweredUserName == userName.ToLower()
                                         select x).FirstOrDefault();
                    if (user == null) throw new ProviderException(String.Format(Resources.Provider_user_not_found, userName));

                    aspnet_Membership membership = (from x in db.aspnet_Membership
                                                    where x.ApplicationId == application.ApplicationId && x.UserId == user.UserId
                                                    select x).FirstOrDefault();
                    if (membership == null)
                    {
                        throw new ProviderException(String.Format(Resources.Membership_UserNotFound));
                    }

                    if (membership.IsLockedOut)
                    {
                        throw new ProviderException(String.Format(Resources.Membership_AccountLockOut));
                    }

                    if (RequiresQuestionAndAnswer)
                    {
                        if (encodedPasswordAnswer != membership.PasswordAnswer)
                        {
                            if (utcNow > membership.FailedPasswordAttemptWindowStart.Add(new TimeSpan(0, PasswordAttemptWindow, 0)))
                            {
                                membership.FailedPasswordAttemptCount = 1;
                                membership.FailedPasswordAttemptWindowStart = utcNow;
                            }
                            else
                            {
                                membership.FailedPasswordAttemptCount += 1;
                                membership.FailedPasswordAttemptWindowStart = utcNow;
                            }


                            if (membership.FailedPasswordAttemptCount >= MaxInvalidPasswordAttempts)
                            {
                                membership.IsLockedOut = true;
                                membership.LastLockoutDate = utcNow;
                            }

                            throw new ProviderException(String.Format(Resources.Membership_WrongAnswer));
                        }
                        else
                        {
                            membership.FailedPasswordAttemptCount = 0;
                            membership.FailedPasswordAttemptWindowStart = EfProviderUtil.DbNullDate;
                        }
                    }

                    //if got to here everything is okay so reset some things
                    passwordFormat = membership.PasswordFormat;
                    status = -1;
                    string password = membership.Password;
                    return password;
                }
            }
            catch
            {
                throw;
            }
        }

        private string GetEncodedPasswordAnswer(string userName, string passwordAnswer)
        {
            if (passwordAnswer != null)
            {
                passwordAnswer = passwordAnswer.Trim();
            }
            if (String.IsNullOrWhiteSpace(passwordAnswer))
            {
                return passwordAnswer;
            }
            int status;
            int passwordFormat;
            int failedPasswordAttemptCount;
            int failedPasswordAnswerAttemptCount;
            string password;
            string passwordSalt;
            bool isApproved;
            DateTime lastLoginDate;
            DateTime lastActivityDate;
            bool result = GetPasswordWithFormat(userName, false, out status, out password, out passwordFormat, out passwordSalt, out failedPasswordAttemptCount, out failedPasswordAnswerAttemptCount, out isApproved, out lastLoginDate, out lastActivityDate);
            if (result)
            {
                return EncodePassword(passwordAnswer.ToLower(CultureInfo.InvariantCulture), passwordFormat, passwordSalt);
            }
            else
            {
                throw new ProviderException(GetExceptionText(status));
            }
        }

        private string GetExceptionText(int status)
        {
            string key;
            switch (status)
            {
                case 0:
                    return String.Empty;
                case 1:
                    key = Resources.Membership_UserNotFound;
                    break;
                case 2:
                    key = Resources.Membership_WrongPassword;
                    break;
                case 3:
                    key = Resources.Membership_WrongAnswer;
                    break;
                case 4:
                    key = Resources.Membership_InvalidPassword;
                    break;
                case 5:
                    key = Resources.Membership_InvalidQuestion;
                    break;
                case 6:
                    key = Resources.Membership_InvalidAnswer;
                    break;
                case 7:
                    key = Resources.Membership_InvalidEmail;
                    break;
                case 99:
                    key = Resources.Membership_AccountLockOut;
                    break;
                default:
                    key = Resources.Provider_Error;
                    break;
            }
            return String.Format(key);
        }

        private bool IsStatusDueToBadPassword(int status)
        {
            return (status >= 2 && status <= 6 || status == 99);
        }

        private DateTime RoundToSeconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        private string GenerateSalt()
        {
            byte[] buf = new byte[16];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        private string EncodePassword(string pass, int passwordFormat, string salt)
        {
            if (passwordFormat == 0) // MembershipPasswordFormat.Clear
            {
                return pass;
            }

            byte[] bIn = Encoding.Unicode.GetBytes(pass);
            byte[] bSalt = Convert.FromBase64String(salt);
            byte[] bAll = new byte[bSalt.Length + bIn.Length];
            byte[] bRet = null;

            Buffer.BlockCopy(bSalt, 0, bAll, 0, bSalt.Length);
            Buffer.BlockCopy(bIn, 0, bAll, bSalt.Length, bIn.Length);
            if (passwordFormat == 1)
            {
                // MembershipPasswordFormat.Hashed
                //HashAlgorithm s = HashAlgorithm.Create(Membership.HashAlgorithmType);
                //bRet = s.ComputeHash(bAll);

                //SE - 8/2/2013: modified to use local config file rather than the hosted config file
                //Configuration configuration = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
                //MachineKeySection machineKey = (MachineKeySection)configuration.GetSection("system.web/machineKey");
                MachineKeySection machineKey = (MachineKeySection)ConfigurationManager.GetSection("system.web/machineKey");

                HMACSHA1 hash = new HMACSHA1 { Key = HexToByte(machineKey.ValidationKey) };
                bRet = hash.ComputeHash(bAll);
            }
            else
            {
                bRet = EncryptPassword(bAll);
            }

            return Convert.ToBase64String(bRet);
        }

        private string UnEncodePassword(string pass, int passwordFormat)
        {
            switch (passwordFormat)
            {
                case 0: // MembershipPasswordFormat.Clear:
                    return pass;
                case 1: // MembershipPasswordFormat.Hashed:
                    throw new ProviderException(String.Format(Resources.Provider_can_not_decode_hashed_password));
                default:
                    byte[] bIn = Convert.FromBase64String(pass);
                    byte[] bRet = DecryptPassword(bIn);
                    if (bRet == null)
                    {
                        return null;
                    }
                    return Encoding.Unicode.GetString(bRet, 16, bRet.Length - 16);
            }
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array. Used to convert encryption key values from the configuration.
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        #endregion
    }
}
