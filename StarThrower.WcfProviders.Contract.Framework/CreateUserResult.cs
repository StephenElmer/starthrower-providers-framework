// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Runtime.Serialization;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [DataContract]
    public class CreateUserResult
    {
        [DataMember]
        public User User { get; set; }

        [DataMember]
        public MembershipCreateStatus Status { get; set; }
    }
}
