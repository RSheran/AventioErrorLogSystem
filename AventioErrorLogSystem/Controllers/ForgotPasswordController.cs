using AventioErrorLogSystem.Common;
using AventioErrorLogSystem.Models;
using ErrorLogBusinessLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;

namespace AventioErrorLogSystem.Controllers
{
    public class ForgotPasswordController : Controller
    {
        public string currentFile = String.Empty, methodName = String.Empty;
        private static string networkCredentialUserName = System.Configuration.ConfigurationSettings.AppSettings["NetworkCredential_userName"].ToString();
        private static string smptpServerVal = System.Configuration.ConfigurationManager.AppSettings["SMTP_server"].ToString();
        private static int sendPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["SMTP_Port"]);
        private MailMessage _MailMessage;

        UserDetailsBusinessLogic _UserDetailsBusinessLogic;
       

        // GET: ForgotPassword
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AuthenticatePasswordReset(ForgotPasswordViewModel model)
        {
            try
            {
                _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                if ((model.Username != null && model.EMail != null))
                {
                    if (_UserDetailsBusinessLogic.IsExistingUserForPasswordRecovery(model.Username, model.EMail))
                    {
                       

                        bool isEmailServerWorking = TestConnection(smptpServerVal, sendPort);
                        bool isEmailSent = false;

                        if (isEmailServerWorking == true)
                        {
                            
                            //delete all existing temperory passwords for this user
                            _UserDetailsBusinessLogic.DeleteTemperoryPassword(model.Username);

                            string tempPassword = _UserDetailsBusinessLogic.SaveTempPassword(model.Username);

                            if (tempPassword != String.Empty)
                            {
                                _MailMessage = CommonCommunicationBusinessLogic.ConstructEmail(model.Username, model.EMail, tempPassword);
                                isEmailSent = CommonCommunicationBusinessLogic.SendEmail(_MailMessage);
                            }
                            else
                            {
                                TempData["notice"] = "Oops!..The temperory password could not be generated.Please try again.";
                            }
                        }
                        else
                        {
                            TempData["notice"] = "Oops!..Our Email servers seem to be not working at the moment." + Environment.NewLine + "Please send an email to dailyexpense365@gmail.com to get a temperory password.";
                        }

                        if (isEmailSent == true)
                        {
                            TempData["notice"] = null;

                            TempData["success"] = "Password reset successful." + Environment.NewLine + "Please see your email inbox or spam folder to get the temperory password. ";
                        }
                        else
                        {

                            TempData["notice"] = "Oops!..Could not compelete password recovery process." + Environment.NewLine + "Please send an email stating your username to dailyexpense365@gmail.com to get a temperory password. ";
                        }

                        return RedirectToAction("Index", "ForgotPassword");

                    }
                    else
                    {


                        TempData["notice"] = "Oops!..This Username and EMail combination is not found in our system. ";
                        return RedirectToAction("Index", "ForgotPassword");

                    }
                }
                else
                {
                    TempData["notice"] = "Username and EMail are required. ";
                    return RedirectToAction("Index", "ForgotPassword");
                }

            }
            catch (Exception ex)
            {
                currentFile = this.ControllerContext.RouteData.Values["controller"].ToString(); // System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(0);
                methodName = sf.GetMethod().Name;
                ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);

                TempData["notice"] = "Oops!..An Error Occured. " + Environment.NewLine + "Please send an email stating your username and full name to " + networkCredentialUserName + " to get a temperory password.";
                return RedirectToAction("Index", "ForgotPassword");

            }
        }

        /// <summary>
        /// test the smtp connection by sending a HELO command
        /// </summary>
        /// <param name="smtpServerAddress"></param>
        /// <param name="port"></param>
        public static bool TestConnection(string smtpServerAddress, int port)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    //tcpClient.Connect(smtpServerAddress, port);
                    TcpClient client = new TcpClient(smtpServerAddress, port);
                    return true;


                }
                catch (Exception)
                {
                    return false;
                }
            }

        }



    }
}