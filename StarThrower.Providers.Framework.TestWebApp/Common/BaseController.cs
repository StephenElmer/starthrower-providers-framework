// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StarThrower.Providers.Framework.TestWebApp.Common
{
    public class BaseController : Controller
    {
        #region [ Private Instance Variables ]

        private string _moniker = String.Empty;

        #endregion


        #region [ Public Properties ]

        public string Moniker
        {
            get
            {
                if (String.IsNullOrEmpty(_moniker))
                {
                    _moniker = this.RouteData.GetRequiredString("controller");
                }
                return _moniker;
            }
        }

        #endregion


        #region [ Protected Methods ]

        protected void LoadBaseData(BaseViewModel data)
        {
            data.IsLoggedIn = (Session["isLoggedIn"] == null ? false : (bool)(Session["isLoggedIn"]));
        }

        #endregion
    }
}