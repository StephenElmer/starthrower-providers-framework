// Copyright © 2005-2026 Stephen Elmer. Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using StarThrower.Providers.Framework.TestWebApp.Common;
using StarThrower.Providers.Framework.TestWebApp.Models;


namespace StarThrower.Providers.Framework.TestWebApp.Controllers
{
    public class AdminController : BaseController
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }


        //
        // GET: /Admin/
        [Authorize(Roles = "Administrator, User")]
        public ActionResult Index()
        {
            AdminViewModel model = new AdminViewModel();
            LoadBaseData(model);

            return View(model);
        }

    }
}
