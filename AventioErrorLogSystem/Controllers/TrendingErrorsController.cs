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
    public class TrendingErrorsController : Controller
    {
        //
        // GET: /TrendingErrors/ 
        public string currentFile = String.Empty, methodName = String.Empty;
        private UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        private TrendingErrorsBusinessLogic _TrendingErrorsBusinessLogic;

        public ActionResult Index()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                // string LoginUserType = System.Web.HttpContext.Current.Session["LoginUserType"].ToString();
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    ViewBag.ProfilePicURL = _UserDetailsBusinessLogic.GetUserDetails(ticket.Name)
                                              .FirstOrDefault().ProfilePicURL;
                    ViewBag.Username = ticket.Name;
                    ViewBag.UserType = _UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim();
                    ViewBag.RedirectedUserId = "0";

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
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);

                this.HttpContext.Session["ErrorMsg"] = "PageLoadError";
                return RedirectToAction("Index", "LoginError");
            }
        }

        public JsonResult GetLatestTopErrors()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _TrendingErrorsBusinessLogic = new TrendingErrorsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _TrendingErrorsBusinessLogic.GetLatestTopErrors();

                return Json(retObj);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json(null);

            }

        }

        public JsonResult GetMostSearchedTopErrors()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _TrendingErrorsBusinessLogic = new TrendingErrorsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _TrendingErrorsBusinessLogic.GetMostSearchedTopErrors();

                return Json(retObj);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json(null);

            }

        }

    }
}
