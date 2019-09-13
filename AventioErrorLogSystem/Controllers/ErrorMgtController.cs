using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ErrorLogDataAccess.DataClasses;
using ErrorLogBusinessLogic;
using System.Diagnostics;
using AventioErrorLogSystem.Common;
using System.Web.Security;

namespace AventioErrorLogSystem.Controllers
{
    public class ErrorMgtController : Controller
    {
        //
        // GET: /ErrorMgt/
        public string currentFile = String.Empty, methodName = String.Empty;
        private ErrorMgtBusinesssLogic _ErrorMgtBusinesssLogic;
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;

        public ActionResult Index(string redirectedErrorCode=null)
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
                        ViewBag.RedirectedErrorCode = redirectedErrorCode;

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
                string ErrorImagePath = System.Configuration.ConfigurationManager.AppSettings["ErrorImagePath"];
                string OtherErrorFilePath = System.Configuration.ConfigurationManager.AppSettings["OtherErrorFilePath"];
                string SolutionImagePath = System.Configuration.ConfigurationManager.AppSettings["SolutionImagePath"];
                string OtherSolutionFilePath = System.Configuration.ConfigurationManager.AppSettings["OtherSolutionFilePath"];


                ViewBag.AWSProfileName = AWSProfileName;
                ViewBag.AWSAccessKey = AWSAccessKey;
                ViewBag.AWSSecretKey = AWSSecretKey;
                ViewBag.BucketName = BucketName;
                ViewBag.BucketRegion = BucketRegion;
                ViewBag.BucketStartURL = BucketStartURL;

                ViewBag.ErrorImagePath = ErrorImagePath;
                ViewBag.OtherErrorFilePath = OtherErrorFilePath;
                ViewBag.SolutionImagePath = SolutionImagePath;
                ViewBag.OtherSolutionFilePath = OtherSolutionFilePath;
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

        public JsonResult GetNewErrorCode()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ErrorMgtBusinesssLogic = new ErrorMgtBusinesssLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var retVal = string.Empty;

                retVal = _ErrorMgtBusinesssLogic.GetNewErrorCode();

                return Json(retVal);
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

        public JsonResult GetNewSolutionCode()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ErrorMgtBusinesssLogic = new ErrorMgtBusinesssLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                var retVal = string.Empty;

                retVal = _ErrorMgtBusinesssLogic.GetNewSolutionCode();

                return Json(retVal);
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

        public JsonResult GetExistingErrorList()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ErrorMgtBusinesssLogic = new ErrorMgtBusinesssLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _ErrorMgtBusinesssLogic.GetExistingErrorList();

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

        public JsonResult GetDetailsForErrorCode(string errorCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ErrorMgtBusinesssLogic = new ErrorMgtBusinesssLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var retObj = _ErrorMgtBusinesssLogic.GetDetailsForErrorCode(errorCode);

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



        public JsonResult SaveMainErrorDetails(ErrorMaster errorObjParam, SolutionMaster solObjParam, ErrorAttachment[] errorImageList,
                                           ErrorAttachment[] solutionImageList, bool isSolutionAddForError, bool isErrorWithSolution)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
                int currentUserId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);

                _ErrorMgtBusinesssLogic = new ErrorMgtBusinesssLogic();
                 var retVal = string.Empty;

                retVal = _ErrorMgtBusinesssLogic.SaveMainErrorDetails(errorObjParam,solObjParam, errorImageList,
                                                                     solutionImageList,isSolutionAddForError, isErrorWithSolution, currentUserId);

                return Json(retVal);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json("0000");

            }

        }


    }
}
