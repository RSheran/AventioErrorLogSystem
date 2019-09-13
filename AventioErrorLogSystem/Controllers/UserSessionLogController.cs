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
    public class UserSessionLogController : Controller
    {
        // GET: UserSessionLog
        public string currentFile = String.Empty, methodName = String.Empty;
        private UserSessionLogBusinessLogic _UserSessionLogBusinessLogic;
        private UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        private EncryptionModule _EncryptionModule;
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
                    return RedirectToAction("Index", "LoginError");
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


        //Get all user log details
        public JsonResult GetAllLoggedInSessions(string userId)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {

                _UserSessionLogBusinessLogic = new UserSessionLogBusinessLogic();
                _EncryptionModule = new EncryptionModule();

                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                int decryptedUserId = userId == "0" ? 0 : Convert.ToInt32(_EncryptionModule.Decrypt(userId));

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);



                    var userLogDetails = _UserSessionLogBusinessLogic.GetAllLoggedInSessions(decryptedUserId);

                    var userLogDetailsEncrypted = userLogDetails
                                              .Select(ul => new
                                              {
                                                  UserSessionLogId = _EncryptionModule.Encrypt(ul.UserSessionLogId.ToString()),
                                                  UserId = _EncryptionModule.Encrypt(ul.UserId.ToString()),
                                                  ProfilePicURL = ul.ProfilePicURL,
                                                  Username = ul.Username,
                                                  UserGroupName = ul.UserGroupName,
                                                  UserFullName = ul.UserFullName,
                                                  UserCallingName = ul.UserCallingName,
                                                  IPAddress = ul.IPAddress,
                                                  CountryCode =ul.CountryCode,
                                                  Country = ul.Country,
                                                  City = ul.City,
                                                  Region = ul.Region,
                                                  LoggedInTimestamp = ul.LoggedInTimestamp,
                                                  LoggedOffTimestamp = ul.LoggedOffTimestamp

                                              }).ToList();



                    return Json(userLogDetailsEncrypted);
                }
                else
                {
                    return Json(null);
                }

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

        //Delete/Clear Session details for user
        public JsonResult ClearSessionDetails(string userId)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserSessionLogBusinessLogic = new UserSessionLogBusinessLogic();
                _EncryptionModule = new EncryptionModule();

                int decryptedUserId = Convert.ToInt32(_EncryptionModule.Decrypt(userId));

                var result = _UserSessionLogBusinessLogic.ClearSessionDetails(decryptedUserId);

                return Json(result);
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