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
    public class DataMigrationController : Controller
    {

        // GET: DataMigration
        public string currentFile = String.Empty, methodName = String.Empty;
        private  DataMigrationBusinessLogic _DataMigrationBusinessLogic;
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
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);

                this.HttpContext.Session["ErrorMsg"] = "PageLoadError";
                return RedirectToAction("Index", "LoginError");
            }
        }

        //Check whether the destination mongo URL is working/active
        public  JsonResult IsDestinationURLValid(string targetMongoDbURL)
        {
            //isActive=>Denotes which type of data to get(i.e,active,inactive or all)
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _DataMigrationBusinessLogic = new DataMigrationBusinessLogic(targetMongoDbURL);

                var result = _DataMigrationBusinessLogic.IsDestinationURLValid();


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


        //Get all collections
        public JsonResult GetAllCollections()
        {
            //isActive=>Denotes which type of data to get(i.e,active,inactive or all)
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _DataMigrationBusinessLogic = new DataMigrationBusinessLogic();

                var result = _DataMigrationBusinessLogic.GetAllCollections();


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

        //Retrieve all available tables/collections for selection
        public JsonResult PushDataToTargetDatabase(string targetMongoDbURL,string[] collectionArr)
        {
            //isActive=>Denotes which type of data to get(i.e,active,inactive or all)
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _DataMigrationBusinessLogic = new DataMigrationBusinessLogic(targetMongoDbURL);

                var result = _DataMigrationBusinessLogic.PushDataToTargetDatabase(collectionArr);


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