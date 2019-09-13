using AventioErrorLogSystem.Common;
using ErrorLogBusinessLogic;
using ErrorLogDataAccess.DataClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AventioErrorLogSystem.Controllers
{
    public class SearchSolutionController : Controller
    {
        //
        // GET: /SearchError/
        public string currentFile = String.Empty, methodName = String.Empty;
        SearchSolutionBusinessLogic _SearchSolutionBusinessLogic;
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;

        public ActionResult Index(string errorCode=null)
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

                    //Bind Error Code to ViewBag
                    ViewBag.ErrorCode = errorCode;
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

        
        public JsonResult GetExistingErrorListForParam(int domainId, int fieldId, int clientId)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
               
                _SearchSolutionBusinessLogic = new SearchSolutionBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _SearchSolutionBusinessLogic.GetExistingErrorListForParam( domainId,fieldId,clientId);

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

        public JsonResult GetExistingErrorListForErrorCode(string errorCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _SearchSolutionBusinessLogic = new SearchSolutionBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _SearchSolutionBusinessLogic.GetExistingErrorListForErrorCode(errorCode);

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

        public JsonResult GetAllErrorsForSearchQuery(string searchQuery)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {

                _SearchSolutionBusinessLogic = new SearchSolutionBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _SearchSolutionBusinessLogic.GetAllErrorsForSearchQuery(searchQuery);

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

        public JsonResult GetAllErrors()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _SearchSolutionBusinessLogic = new SearchSolutionBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _SearchSolutionBusinessLogic.GetAllErrors();

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

        public JsonResult SaveSolutionFeedback(SolutionFeedback solFeedbackObj,string solutionCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                int currentUserId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);

                _SearchSolutionBusinessLogic = new SearchSolutionBusinessLogic();

                var retObj = _SearchSolutionBusinessLogic.SaveSolutionFeedback(solFeedbackObj, currentUserId,solutionCode);

                return Json(true);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json(false);

            }

        }

    }
}
