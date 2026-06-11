// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace StarThrower.WcfProviders.Contract.Framework
{
    [DataContract]
    public class GenericFault
    {
        private string _message = String.Empty;

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public GenericFault() { }

        public GenericFault(string message)
        {
            _message = message;
        }
    }
}
