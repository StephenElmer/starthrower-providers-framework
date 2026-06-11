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
    public class AccountController : BaseController
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }
            base.Initialize(requestContext);
        }


        //GET: /Account/LogOn/
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LogOn()
        {
            AccountLogOnViewModel data = new AccountLogOnViewModel();
            LoadBaseData(data);

            return View(data);
        }

        //POST: /Account/LogOn/
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult LogOn(AccountLogOnViewModel data, string returnUrl)
        {
            LoadBaseData(data);

            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(data.UserName) && !String.IsNullOrEmpty(data.Password))
                {
                    if (MembershipService.ValidateUser(data.UserName, data.Password))
                    {
                        FormsService.SignIn(data.UserName, data.RememberMe);
                        if (!String.IsNullOrEmpty(returnUrl))
                        {
                            Session["isLoggedIn"] = true;
                            Session["userName"] = data.UserName;
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(data);
        }

        // GET: /Account/LogOff/
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult LogOff()
        {
            FormsService.SignOut();
            Session["isLoggedIn"] = false;
            Session["userName"] = String.Empty;
            return RedirectToAction("Index", "Home");
        }

    }
}
