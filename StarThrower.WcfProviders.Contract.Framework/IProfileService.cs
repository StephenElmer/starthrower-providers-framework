// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.Web.Profile;
using System.Web.Security;
using System.Text;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [ServiceContract]
    public interface IProfileService
    {
        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/deleteinactiveprofiles", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/deleteprofilesbyusername", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int DeleteProfilesByUserName(string[] userNames);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/deleteprofilesbyprofile", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int DeleteProfilesByProfile(ProfileInfoCollection profiles);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/findinactiveprofilesbyusername", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetProfilesResult FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/findprofilesbyusername", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetProfilesResult FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string userNameToMatch, int pageIndex, int pageSize);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getallinactiveprofiles", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetProfilesResult GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getallprofiles", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        GetProfilesResult GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getnumberofinactiveprofiles", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/getpropertyvalues", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        Collection<ProfileItem> GetPropertyValues(SettingsContext context, Collection<ProfileItem> collection);

        [OperationContract]
        [FaultContract(typeof(GenericFault))]
        [WebInvoke(UriTemplate = "/setpropertyvalues", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SetPropertyValues(SettingsContext context, Collection<ProfileItem> collection);
    }
}
