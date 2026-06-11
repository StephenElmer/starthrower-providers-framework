// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StarThrower.Providers.Framework.TestWebApp.Common;
using StarThrower.Providers.Framework.TestWebApp.Models;

namespace StarThrower.Providers.Framework.TestWebApp.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();
            LoadBaseData(model);

            return View(model);
        }

    }
}
