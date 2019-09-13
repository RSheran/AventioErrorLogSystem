using AventioErrorLogSystem.Common;
using ErrorLogBusinessLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace AventioErrorLogSystem.Controllers
{
    public class UserMessageController : Controller
    {
        // GET: UserMessage
        public string currentFile = String.Empty, methodName = String.Empty;
        UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        UserMessageBusinessLogic _UserMessageBusinessLogic;
        public ActionResult Index(string requestId=null)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ViewBag.RequestId = requestId;
                bool isSessionTicketDecryptable = true;
                             

                if (authCookie != null)
                {
                    try
                    {
                         ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    }
                    catch (Exception ex)
                    {
                        isSessionTicketDecryptable = false;
                    }
                   
                     if (isSessionTicketDecryptable == true)
                     {
                          ViewBag.ProfilePicURL = _UserDetailsBusinessLogic.GetUserDetails(ticket.Name)
                                              .FirstOrDefault().ProfilePicURL;
                          ViewBag.Username = ticket.Name;
                          ViewBag.UserType = _UserDetailsBusinessLogic.GetUserType(ticket.Name).ToLower().Trim();
                          return View();
                    }
                    else
                    {
                       return RedirectToAction("Index", "Login", new RouteValueDictionary(new { id="RedirectedLogin", userMsgRequestIdParam = requestId }));
                    }                                                                 
                                    
                   
                }
                else
                {
                    if (requestId != null)
                    {
                        return RedirectToAction("Index", "Login", new RouteValueDictionary(new { id = "RedirectedLogin", userMsgRequestIdParam = requestId }));
                    }
                    else
                    {
                        this.HttpContext.Session["ErrorMsg"] = "LoginErr";
                        return RedirectToAction("Index", "LoginError");
                    }
       
                }
              
            }
            catch (Exception ex)
            {
                currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(1);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);

                this.HttpContext.Session["ErrorMsg"] = "PageLoadError";
                return RedirectToAction("Index", "LoginError");
            }
        }

        
        public JsonResult GetMessageCount()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    _UserMessageBusinessLogic = new UserMessageBusinessLogic();

                    int result = _UserMessageBusinessLogic.GetMessageCount(ticket.Name);

                    return Json(result);
                }
                else
                {
                    this.HttpContext.Session["ErrorMsg"] = "LoginErr";
                    return null;
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

        public JsonResult SMGGetUserList()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie != null)
                {
                    ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    _UserMessageBusinessLogic = new UserMessageBusinessLogic();

                    var result = _UserMessageBusinessLogic.SMGGetUserList(ticket.Name);

                    return Json(result);
                }
                else
                {
                    this.HttpContext.Session["ErrorMsg"] = "LoginErr";
                    return null;
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

        public int SMGSendMessage(List<string> UserList, string Subject, string Message)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                    HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                    ticket = FormsAuthentication.Decrypt(authCookie.Value);
                    List<int> UList = new List<int>();
                    foreach (string i in UserList)
                    {
                        UList.Add(int.Parse(i));
                    }

                    _UserMessageBusinessLogic = new UserMessageBusinessLogic();

                    int result = _UserMessageBusinessLogic.SMGSendMessage(UList, Subject, Message, ticket.Name);

                    return result;
                
               
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return 1;
            }

        }


        public JsonResult VSMGLoadSentMessages()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _UserMessageBusinessLogic = new UserMessageBusinessLogic();
                List<object> returnMessageList = _UserMessageBusinessLogic.VSMGLoadSentMessages(ticket.Name);

                return Json(returnMessageList);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return null;
            }


        }

        public JsonResult VSMGDeleteMessages(List<int> DeleteList)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _UserMessageBusinessLogic = new UserMessageBusinessLogic();
                List<object> returnMessageList = _UserMessageBusinessLogic.VSMGDeleteMessages(ticket.Name, DeleteList);

                return Json(returnMessageList);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return null;
            }


        }

        public JsonResult VSMGDeleteAllMessages()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _UserMessageBusinessLogic = new UserMessageBusinessLogic();
                int returnMessageList = _UserMessageBusinessLogic.VSMGDeleteAllMessages(ticket.Name);

                return Json(returnMessageList);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json(1);
            }


        }

        public JsonResult RMSGLoadMessageList()
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _UserMessageBusinessLogic = new UserMessageBusinessLogic();
                List<object> returnMessageList = _UserMessageBusinessLogic.RMSGLoadMessageList(ticket.Name);

                return Json(returnMessageList);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json(1);
            }

        }

        public JsonResult RMSGLoadPreviousMessages(int MessageID)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _UserMessageBusinessLogic = new UserMessageBusinessLogic();
                List<object> returnMessageList = _UserMessageBusinessLogic.RMSGLoadPreviousMessages(ticket.Name,MessageID);

                return Json(returnMessageList);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json(1);
            }

        }

        public JsonResult RMSGSendMessage(int MessageID, String Subject, String Message)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                _UserMessageBusinessLogic = new UserMessageBusinessLogic();
                List<object> returnMessageList = _UserMessageBusinessLogic.RMSGSendMessage(ticket.Name, MessageID, Subject, Message);

                return Json(returnMessageList);
            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, ticket == null ? "N/A" : ticket.Name, ex);
                return Json(1);
            }
        }
    }
}