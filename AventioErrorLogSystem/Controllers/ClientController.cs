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
    public class ClientController : Controller
    {
        //
        // GET: /Client/
        public string currentFile = String.Empty, methodName = String.Empty;
        private ClientBusinessLogic _ClientBusinessLogic;
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;


        public ActionResult Index()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                // string LoginUserType = System.Web.HttpContext.Current.Session["LoginUserType"].ToString();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);

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
                 ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);

                this.HttpContext.Session["ErrorMsg"] = "PageLoadError";
                return RedirectToAction("Index", "LoginError");
            }
        }



        public JsonResult LoadClients(int isActive)
        {
            //isActive=>Denotes which type of data to get(i.e,active,inactive or all)
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ClientBusinessLogic = new ClientBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var ClientArr = _ClientBusinessLogic.GetAllClients(isActive);


                return Json(ClientArr);
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

        public JsonResult GetClientDetailsForCode(string ClientCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ClientBusinessLogic = new ClientBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var ClientArr = _ClientBusinessLogic.GetClientDetailsForCode(ClientCode);

                return Json(ClientArr);
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

        public JsonResult GetClientCodesForAutoComplete(string ClientCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ClientBusinessLogic = new ClientBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var ClientArr = _ClientBusinessLogic.GetClientCodesForAutoComplete(ClientCode);


                return Json(ClientArr);
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

        public JsonResult AddOrUpdateClient(Client ClientObj)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ClientBusinessLogic = new ClientBusinessLogic();
                bool isSuccess = false;
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                int currentUserId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);


                if (_ClientBusinessLogic.IsClientAvailable(ClientObj.ClientCode) == true)
                {

                    isSuccess = _ClientBusinessLogic.UpdateClient(ClientObj);


                }
                else
                {
                    isSuccess = _ClientBusinessLogic.SaveClient(ClientObj, currentUserId);
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

        public JsonResult UpdateStatusOfSelectedClients(Client[] ClientObj)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ClientBusinessLogic = new ClientBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                bool isSuccess = false;

                isSuccess = _ClientBusinessLogic.UpdateStatusOfSelectedClients(ClientObj);

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

        public JsonResult ChangeStatusForAllClients(bool isActiveForAll)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ClientBusinessLogic = new ClientBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                bool isSuccess = false;

                isSuccess = _ClientBusinessLogic.ChangeStatusForAllClients(isActiveForAll);

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
