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
    public class ErrorFrequencyController : Controller
    {
        //
        // GET: /ErrorFrequency/
        public string currentFile = String.Empty, methodName = String.Empty;
        private UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        private ErrorFrequencyBusinessLogic _ErrorFrequencyBusinessLogic;

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult UpdateSearchFrequencyForError(string errorCode)
        {
            FormsAuthenticationTicket ticket = null;
            try
            {
                _ErrorFrequencyBusinessLogic = new ErrorFrequencyBusinessLogic();
                HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                ticket = FormsAuthentication.Decrypt(authCookie.Value);

                bool retVal = _ErrorFrequencyBusinessLogic.UpdateSearchFrequencyForError(errorCode);
                return Json(retVal);

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
