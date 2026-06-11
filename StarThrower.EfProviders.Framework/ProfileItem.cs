// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace StarThrower.EfProviders.Framework
{
    internal class ProfileItem
    {
        public string Name { get; set; }
        public string PropertyType { get; set; }
        public object DefaultValue { get; set; }
        public SettingsSerializeAs SerializeAs { get; set; }
        public string Storage { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
    }
}
