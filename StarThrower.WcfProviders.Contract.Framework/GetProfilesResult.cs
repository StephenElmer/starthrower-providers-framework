// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Web.Profile;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [DataContract]
    public class GetProfilesResult
    {
        [DataMember]
        public ProfileInfoCollection Profiles { get; set; }

        [DataMember]
        public int TotalRecords { get; set; }
    }
}
