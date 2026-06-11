// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [DataContract]
    [KnownType(typeof(TextProfileItem))]
    [KnownType(typeof(BinaryProfileItem))]
    public abstract class ProfileItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string PropertyType { get; set; }

        [DataMember]
        public object DefaultValue { get; set; }

        [DataMember]
        public SettingsSerializeAs SerializeAs { get; set; }
    }
}
