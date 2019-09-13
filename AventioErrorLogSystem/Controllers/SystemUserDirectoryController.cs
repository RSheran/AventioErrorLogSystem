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
    public class SystemUserDirectoryController : Controller
    {
        // GET: SystemUserDirectory
        public string currentFile = String.Empty, methodName = String.Empty;
        private SystemUserDirectoryBusinessLogic _SystemUserDirectoryBusinessLogic;
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

        //Get all system users
        public JsonResult GetAllUsers()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _SystemUserDirectoryBusinessLogic = new SystemUserDirectoryBusinessLogic();

                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    _EncryptionModule = new EncryptionModule();

                    var userDetails = _SystemUserDirectoryBusinessLogic.GetAllUsers();

                    var userDetailsEncrypted=  userDetails
                                              .Select(u => new 
                                              {
                                                  UserId = _EncryptionModule.Encrypt(u.UserId.ToString()),
                                                  UserGroupId = u.UserGroupId,
                                                  ProfilePicURL = u.ProfilePicURL,
                                                  Username = u.Username,
                                                  UserGroupName=u.UserGroupName,
                                                  FullName = u.FullName,
                                                  CallingName = u.CallingName,
                                                  Email = u.Email,
                                                  IsActive = u.IsActive,
                                                  IsLoggedIn = u.IsLoggedIn,
                                                  LastLogInTime=u.LastLogInTime ,
                                                  LastLogOffTime=u.LastLogOffTime,

                                              }).OrderBy(a => a.Username).ThenBy(a => a.FullName).ToList();

                    

                    return Json(userDetailsEncrypted);
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

        


    }
}