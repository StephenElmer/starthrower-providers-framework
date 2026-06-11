// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.Text;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [ServiceContract]
    public interface IRoleService
    {
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/adduserstoroles", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void AddUsersToRoles(string[] userNames, string[] roleNames);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/createrole", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void CreateRole(string roleName);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/deleterole", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool DeleteRole(string roleName, bool throwOnPopulatedRole);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/findusersinrole", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string[] FindUsersInRole(string roleName, string userNameToMatch);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getallroles", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string[] GetAllRoles();

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getrolesforuser", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string[] GetRolesForUser(string userName);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getusersinrole", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        string[] GetUsersInRole(string roleName);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/isuerinrole", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool IsUserInRole(string userName, string roleName);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/removeusersfromroles", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void RemoveUsersFromRoles(string[] userNames, string[] roleNames);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/roleexists", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool RoleExists(string roleName);
    }
}
