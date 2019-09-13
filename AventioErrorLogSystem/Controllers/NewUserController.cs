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
    public class NewUserController : Controller
    {
        //
        // GET: /NewUser/
        public string currentFile = String.Empty, methodName = String.Empty;
        string loggedUserType = String.Empty;

        UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        public ActionResult Index()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                int userId = 0;
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];


                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    userId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);
                    loggedUserType = _UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim();
                }
                else
                {
                    loggedUserType = System.Web.HttpContext.Current.Session["LoginUserType"].ToString().ToLower().Trim();
                   
                }

                //This will be used in the cshtml to check whether the user is actively logged in
                //and is of Administrator User Type
               ViewBag.UserId = userId;
               ViewBag.UserType = loggedUserType;
               ViewBag.RedirectedUserId = "0";

                return View();
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

       
        //Check whether username is existing
        public JsonResult IsUsernameExisting(string chkUserName)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                bool isExisting = _UserDetailsBusinessLogic.IsUsernameExisting(chkUserName);

                return Json(isExisting);
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

        //Check whether username is existing
        public JsonResult IsUserEmailExisting(string chkUserEmail)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = authCookie==null?null:FormsAuthentication.Decrypt(authCookie.Value);

                bool isExisting = _UserDetailsBusinessLogic.IsUserEmailExisting(chkUserEmail);

                return Json(isExisting);
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

        //Get Current User=>To detect whether the user is added by another user
        public JsonResult GetCurrentUser()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                string currrentUserName=String.Empty;
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    currrentUserName = ticket.Name;
                }
               
                return Json(currrentUserName);
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

        /****Adding New User****/
        public JsonResult AddOrUpdateSystemUser(SystemUser systemUser)
        {
            FormsAuthenticationTicket ticket = null;
                try
                {
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                    bool isSuccess;
                     int currentUserId = 0;

                    //Getting current user..
                    HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                    if (authCookie != null)
                    {
                        ticket = FormsAuthentication.Decrypt(authCookie.Value);
                        currentUserId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);
                    }

                    if (_UserDetailsBusinessLogic.IsUsernameExisting(systemUser.Username) == true && currentUserId != 0)
                    {
                        isSuccess=_UserDetailsBusinessLogic.UpdateSytemUser(systemUser, currentUserId);
                    }
                    else
                    {
                       isSuccess=_UserDetailsBusinessLogic.AddSystemUser(systemUser, currentUserId);
                    }

                return Json(isSuccess);
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

        /******************************/

    }
}
