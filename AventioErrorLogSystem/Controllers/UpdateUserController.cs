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
    public class UpdateUserController : Controller
    {
        //
        // GET: /UpdateUser/
        public string currentFile = String.Empty, methodName = String.Empty;
        private UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        private EncryptionModule _EncryptionModule;
        private static int redirectedUserId = 0; //Variable to get when user Id is redirected from another page

        public ActionResult Index(string userId=null)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                // string LoginUserType = System.Web.HttpContext.Current.Session["LoginUserType"].ToString();
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                _EncryptionModule = new EncryptionModule();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);
                                       
                    ViewBag.Username = ticket.Name;
                    ViewBag.UserType = _UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim();
                    ViewBag.RedirectedUserId = userId != null ? _EncryptionModule.Decrypt(userId.ToString()) : "0";
                    redirectedUserId = userId!=null ?Convert.ToInt32(_EncryptionModule.Decrypt(userId.ToString())) : 0;

                     //Initialize AWS Configurations => Required for image uploads
                    InitAWSS3Configurations();

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

        //Definition to Initialize AWS Configurations=>Required for image uploads
        public void InitAWSS3Configurations()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                //Bucket configurations
                string AWSProfileName = System.Configuration.ConfigurationManager.AppSettings["AWSProfileName"];
                string AWSAccessKey = System.Configuration.ConfigurationManager.AppSettings["AWSAccessKey"];
                string AWSSecretKey = System.Configuration.ConfigurationManager.AppSettings["AWSSecretKey"];
                string BucketName = System.Configuration.ConfigurationManager.AppSettings["BucketName"];
                string BucketRegion = System.Configuration.ConfigurationManager.AppSettings["BucketRegion"];
                string BucketStartURL = System.Configuration.ConfigurationManager.AppSettings["BucketStartURL"];

                //File Upload Paths
                string UserProfileImagePath = System.Configuration.ConfigurationManager.AppSettings["UserProfileImagePath"];
                string OtherUserProfileFilePath = System.Configuration.ConfigurationManager.AppSettings["OtherUserProfileFilePath"];

                ViewBag.AWSProfileName = AWSProfileName;
                ViewBag.AWSAccessKey = AWSAccessKey;
                ViewBag.AWSSecretKey = AWSSecretKey;
                ViewBag.BucketName = BucketName;
                ViewBag.BucketRegion = BucketRegion;
                ViewBag.BucketStartURL = BucketStartURL;

                ViewBag.UserProfileImagePath = UserProfileImagePath;
                ViewBag.OtherUserProfileFilePath = OtherUserProfileFilePath;
            }
            catch (Exception ex)
            {
                 currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
               
            }
        }

        public JsonResult GetUserDetails()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    //if (redirectedUserId == 0)
                    //{
                    //    var userDetails = _UserDetailsBusinessLogic.GetUserDetails(ticket.Name).ToArray();

                    //    return Json(userDetails);
                    //}
                    //else
                    //{
                        var userDetails = _UserDetailsBusinessLogic.GetUserDetails(ticket.Name).ToArray();

                        return Json(userDetails);
                   // }

                   
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

        public JsonResult GetRedirectedUserDetails()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    if (redirectedUserId == 0)
                    {
                        var userDetails = _UserDetailsBusinessLogic.GetUserDetails(ticket.Name).ToArray();

                        return Json(userDetails);
                    }
                    else
                    {
                        var userDetails = _UserDetailsBusinessLogic.GetUserDetails(redirectedUserId).ToArray();

                        return Json(userDetails);
                    }


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

        public JsonResult SaveImageWithTheUserID(string imageURL)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                int userId = 0;
                if (redirectedUserId == 0)
                {
                    userId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);
                }
                else
                {
                    userId = redirectedUserId;
                }

                var isSaved = _UserDetailsBusinessLogic.SaveImageWithTheUserID(imageURL, userId);

                return Json(isSaved);
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

        public JsonResult IsOldPwdValid(string chkPassword)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                bool isValid = false;
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
               
                int userId = redirectedUserId==0?_UserDetailsBusinessLogic.GetUserID(ticket.Name): redirectedUserId;

                isValid=_UserDetailsBusinessLogic.IsOldPwdValid(chkPassword, userId);

                return Json(isValid);


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

        public JsonResult UpdateSystemUser(SystemUser systemUser)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                bool isSuccess = false;
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                int currentUserId = redirectedUserId==0? _UserDetailsBusinessLogic.GetUserID(ticket.Name): redirectedUserId;

                isSuccess = _UserDetailsBusinessLogic.UpdateSytemUser(systemUser, currentUserId);

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

        public JsonResult UpdatePassword(string newPassword)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                bool isSuccess = false;
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                int currentUserId = redirectedUserId==0?_UserDetailsBusinessLogic.GetUserID(ticket.Name): redirectedUserId;

                //Delete temperory password=>If existing only.
                _UserDetailsBusinessLogic.DeleteTemperoryPassword(currentUserId);

                isSuccess = _UserDetailsBusinessLogic.UpdatePassword(newPassword, currentUserId);

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
