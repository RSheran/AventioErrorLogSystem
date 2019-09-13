using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AventioErrorLogSystem.Models;
using System.Security.Cryptography;
using AventioErrorLogSystem.Common;
using ErrorLogBusinessLogic;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Specialized;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using ErrorLogDataAccess.DataClasses;
using System.Web.Routing;

namespace AventioErrorLogSystem.Controllers
{
    public class LoginController : Controller
    {
        //        //
        //        // GET: /Login/
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        public string currentFile = String.Empty, methodName = String.Empty;
        public static string IPAddress, city, countryCode,country, region,userMsgRequestId;
        
        public ActionResult Index(string id=null,string userMsgRequestIdParam=null)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                int userId = 0,latestSessionId=0;
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                userMsgRequestId = userMsgRequestIdParam;

                if (id == null || id == "")
                {
                    System.Web.Security.FormsAuthentication.SignOut();
                    Session.Abandon();
                    HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                    if (authCookie.Value != "")
                    {
                        //Before destroying the session cookie
                        // 1.Update the session details to UserSessionLog(Update the LoggedOffTimestamp)
                        // 2.Update the 'IsLoggedIn' flsg of SystemUser as false
                        ticket = FormsAuthentication.Decrypt(authCookie.Value);
                        userId = _UserDetailsBusinessLogic.GetUserID(ticket.Name);
                        latestSessionId = _UserDetailsBusinessLogic.GetLatestSessionIdForUser(userId);
                        _UserDetailsBusinessLogic.UpdateUserSessionLog(latestSessionId);
                        _UserDetailsBusinessLogic.UpdateLoggedInStatus(userId, false);                                                             

                        authCookie.Expires = DateTime.Now.AddSeconds(-1);
                        Response.Cookies.Add(authCookie);
                    }
                }

                else
                {

                    System.Web.HttpContext.Current.Session["LoginUserType"] = id;
                }

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

       

        public ActionResult IndexMobile()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]

        public ActionResult Authenticate(LoginViewModel model)
        {
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                int userId = 0;
                if (LoginAuth(model.Username, model.Password))
                {

                    if (_UserDetailsBusinessLogic.IsUserActive(model.Username) == true)
                    {
                        //Before redirecting:
                        // 1.Add the session details to UserSessionLog
                        // 2.Update the 'IsLoggedIn' flsg of SystemUser as true
                         userId = _UserDetailsBusinessLogic.GetUserID(model.Username);
                        _UserDetailsBusinessLogic.SaveSessionDetails(userId,IPAddress,countryCode,country,city,region);
                        _UserDetailsBusinessLogic.UpdateLoggedInStatus(userId,true);

                       
                        if (userMsgRequestId != null)
                        {
                            return RedirectToAction("Index", "UserMessage", new RouteValueDictionary(new { requestId = userMsgRequestId }));
                        }

                        return RedirectToAction("Index", "LandingPage");
                    }
                    else
                    {
                        TempData["notice"] = "Oops..Your account has been inactivated temporarily.Please contact the System Administrator";
                        return RedirectToAction("Index", "Login");
                    }


                }
                else
                {
                   
                    this.HttpContext.Session["ErrorMsg"] = "LoginErr";
                    TempData["notice"] = "Username Pasword Combination  Is Incorrect";
                    return RedirectToAction("Index", "Login");
                }
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "N/A", ex);

                return RedirectToAction("Index", "Login");
            }
        }


        private bool LoginAuth(string UserName, string Password)
        {
            _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

            if (_UserDetailsBusinessLogic.IsExistingUser(UserName, Password))
            {

                var authTicket = new FormsAuthenticationTicket(1,
                     UserName,
                     DateTime.Now,
                     DateTime.Now.AddMinutes(20),
                     false,
                     "test"
                     );

                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);


                return true;
            }
            else
            {
                return false;
            }
        }


        #region "Set Logged In User's IP-based details"

        public void SetIPAddressDetails(UserSessionLog locationDetailsObj)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {                             

                    IPAddress = locationDetailsObj.IPAddress;
                    countryCode = locationDetailsObj.CountryCode;
                    country = locationDetailsObj.Country;
                    city = locationDetailsObj.City;
                    region = locationDetailsObj.Region;
                              

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

        #endregion

    }
}
