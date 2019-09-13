using AventioErrorLogSystem.Common;
using ErrorLogBusinessLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AventioErrorLogSystem.Controllers
{
    public class LandingPageController : Controller
    {
        //
        // GET: /LandingPage/
        public string currentFile = String.Empty, methodName = String.Empty;
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;

        public ActionResult Index()
        {
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                   
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    ViewBag.ProfilePicURL = _UserDetailsBusinessLogic.GetUserDetails(ticket.Name)
                                            .FirstOrDefault().ProfilePicURL;
                    ViewBag.Username = ticket.Name;
                    ViewBag.UserType = _UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim();
                    
                    return View();

                }
                else
                {
                   
                    this.HttpContext.Session["ErrorMsg"] = "LoginErr";
                    return RedirectToAction("Index", "Login");
                }


            }
            catch (Exception ex)
            {
                currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);


                this.HttpContext.Session["ErrorMsg"] = "PageLoadError";
                return RedirectToAction("Index", "LoginError");
            }
        }

    }
}
