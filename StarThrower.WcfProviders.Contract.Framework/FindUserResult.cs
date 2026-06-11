// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [DataContract]
    public class FindUserResult
    {
        [DataMember]
        public Collection<User> Users { get; set; }

        [DataMember]
        public int TotalRecords { get; set; }
    }
}
