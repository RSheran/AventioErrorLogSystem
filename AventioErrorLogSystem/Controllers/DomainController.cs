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
    public class DomainController : Controller
    {
        //
        // GET: /Domain/
        public string currentFile = String.Empty, methodName=String.Empty;
        private DomainBusinessLogic _DomainBusinessLogic;
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;


        public ActionResult Index()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                

                if (authCookie != null)
                {
                   
                    if (_UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim() == "administrator")
                    {                 
                        
                        ViewBag.ProfilePicURL = _UserDetailsBusinessLogic.GetUserDetails(ticket.Name)
                                                .FirstOrDefault().ProfilePicURL;
                        ViewBag.Username = ticket.Name;
                        ViewBag.UserType = _UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim();
                        ViewBag.RedirectedUserId = "0";


                        return View();
                    }
                    else
                    {
                        this.HttpContext.Session["ErrorMsg"] = "AccessDeniedError";
                        return RedirectToAction("Index", "LoginError");
                    }



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
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null?"N/A": ticket.Name, ex);

                this.HttpContext.Session["ErrorMsg"] = "PageLoadError";
                return RedirectToAction("Index", "LoginError");
            }
        }

   

        public JsonResult LoadDomains(int isActive)
        {
            //isActive=>Denotes which type of data to get(i.e,active,inactive or all)
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _DomainBusinessLogic = new DomainBusinessLogic();

                var domainArr = _DomainBusinessLogic.GetAllDomains(isActive);


                return Json(domainArr);
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

        public JsonResult GetDomainDetailsForCode(string domainCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                _DomainBusinessLogic = new DomainBusinessLogic();

                var domainArr = _DomainBusinessLogic.GetDomainDetailsForCode(domainCode);
                
                return Json(domainArr);
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

        public JsonResult GetDomainCodesForAutoComplete(string domainCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                _DomainBusinessLogic = new DomainBusinessLogic();

                var domainArr = _DomainBusinessLogic.GetDomainCodesForAutoComplete(domainCode);


                return Json(domainArr);
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

        public JsonResult AddOrUpdateDomain(Domain domainObj)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _DomainBusinessLogic = new DomainBusinessLogic();
                bool isSuccess = false;
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                int currentUserId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);


                if (_DomainBusinessLogic.IsDomainAvailable(domainObj.DomainCode) == true)
                {

                    isSuccess= _DomainBusinessLogic.UpdateDomain(domainObj);
                   

                }
                else
                {
                    isSuccess = _DomainBusinessLogic.SaveDomain(domainObj, currentUserId);
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

        public JsonResult UpdateStatusOfSelectedDomains(Domain[] domainObj)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                _DomainBusinessLogic = new DomainBusinessLogic();
                bool isSuccess = false;

                isSuccess = _DomainBusinessLogic.UpdateStatusOfSelectedDomains(domainObj);

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

        public JsonResult ChangeStatusForAllDomains(bool isActiveForAll)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                _DomainBusinessLogic = new DomainBusinessLogic();
                bool isSuccess = false;

                isSuccess = _DomainBusinessLogic.ChangeStatusForAllDomains(isActiveForAll);

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

    }
}
