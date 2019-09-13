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
    public class UserGroupController : Controller
    {
        public string currentFile = String.Empty, methodName = String.Empty;
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        // GET: UserGroup
        public ActionResult Index()
        {
            try
            {

                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                // string LoginUserType = System.Web.HttpContext.Current.Session["LoginUserType"].ToString();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie != null)
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

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
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);

                this.HttpContext.Session["ErrorMsg"] = "PageLoadError";
                return RedirectToAction("Index", "LoginError");
            }
        }

        public JsonResult LoadUserGroups(int isActive)
        {
            //isActive=>Denotes which type of data to get(i.e,active,inactive or all)
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                string userType = "N/A";

                if (authCookie != null)
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    userType = _UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim();

                    if (userType == "administrator")
                    {
                        var resultArr = _UserDetailsBusinessLogic.GetAllUserGroups(isActive);
                        return Json(resultArr);
                    }
                    else
                    {
                        var resultArr = _UserDetailsBusinessLogic.GetAllUserGroupsForOtherUser(isActive);
                        return Json(resultArr);
                    }
                }
                else
                {
                    var resultArr = _UserDetailsBusinessLogic.GetAllUserGroupsForOtherUser(isActive);
                    return Json(resultArr);
                }

               
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);
                return Json(null);

            }

        }

        public JsonResult GetUserGroupDetails(string userGroupName)
        {
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();


                var domainArr = _UserDetailsBusinessLogic.GetUserGroupDetails(userGroupName);

                return Json(domainArr);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);
                return Json(null);

            }
        }

        public JsonResult GetUserGroupNamesForAutoComplete(string userGroupName)
        {
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                var resultArr = _UserDetailsBusinessLogic.GetUserGroupNamesForAutoComplete(userGroupName);


                return Json(resultArr);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);
                return Json(false);

            }
        }

        public JsonResult AddOrUpdateUserGroup(SystemUserGroup systemUserGroupObj)
        {
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                bool isSuccess = false;

                if (_UserDetailsBusinessLogic.IsUserGroupAvailable(systemUserGroupObj.UserGroupName) == true)
                {

                    isSuccess = _UserDetailsBusinessLogic.UpdateUserGroup(systemUserGroupObj);


                }
                else
                {
                    isSuccess = _UserDetailsBusinessLogic.SaveUserGroup(systemUserGroupObj);
                }

                return Json(isSuccess);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);
                return Json(false);

            }

        }

        public JsonResult UpdateStatusOfSelectedGroups(SystemUserGroup[] userGroupObj)
        {
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                bool isSuccess = false;

                isSuccess = _UserDetailsBusinessLogic.UpdateStatusOfSelectedGroups(userGroupObj);

                return Json(isSuccess);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);
                return Json(false);

            }

        }

        public JsonResult ChangeStatusForAllGroups(bool isActiveForAll)
        {
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                bool isSuccess = false;

                isSuccess = _UserDetailsBusinessLogic.ChangeStatusForAllGroups(isActiveForAll);

                return Json(isSuccess);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);
                return Json(false);

            }

        }
    }
}