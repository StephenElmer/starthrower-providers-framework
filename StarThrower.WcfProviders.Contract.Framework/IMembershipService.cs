// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.Web.Security;
using System.Text;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [ServiceContract]
    public interface IMembershipService
    {
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/changepassword", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool ChangePassword(string userName, string oldPassword, string newPassword);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/changepasswordquestionandanswer", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool ChangePasswordQuestionAndAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/createuser", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        CreateUserResult CreateUser(string userName, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/deleteuser", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool DeleteUser(string userName, bool deleteAllRelatedData);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/enablepasswordreset", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool EnablePasswordReset();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/enablepasswordretrieval", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool EnablePasswordRetrieval();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/findusersbyemail", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        FindUserResult FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/findusersbyname", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        FindUserResult FindUsersByName(string userNameToMatch, int pageIndex, int pageSize);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getallusers", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        FindUserResult GetAllUsers(int pageIndex, int pageSize);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getnumberofusersonline", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int GetNumberOfUsersOnline();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getpassword", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetPassword(string userName, string answer);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getuserbyname", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        User GetUserByName(string userName, bool userIsOnline);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getuserbykey", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        User GetUserByKey(object providerUserKey, bool userIsOnline);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getusernamebyemail", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetUserNameByEmail(string email);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/maxinvalidpasswordattempts", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int MaxInvalidPasswordAttempts();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/minrequirednonalphanumericcharacters", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int MinRequiredNonAlphanumericCharacters();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/minrequiredpasswordlength", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int MinRequiredPasswordLength();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/passwordattemptwindow", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int PasswordAttemptWindow();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/passwordformat", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        MembershipPasswordFormat PasswordFormat();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/passwordstrengthregularexpression", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string PasswordStrengthRegularExpression();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/requiresquestionandanswer", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool RequiresQuestionAndAnswer();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/requiresuniqueemail", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool RequiresUniqueEmail();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/resetpassword", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string ResetPassword(string userName, string answer);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/administrativepasswordreset", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string AdministrativePasswordReset(string userName);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/unlockuser", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool UnlockUser(string userName);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/updateuser", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void UpdateUser(User user);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/validateuser", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool ValidateUser(string userName, string password);
    }
}
