// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StarThrower.Providers.Framework.TestWebApp.Common;

namespace StarThrower.Providers.Framework.TestWebApp.Models
{
    public class AccountLogOnViewModel : BaseViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}