using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using ErrorLogDataAccess.DataClasses;

namespace ErrorLogBusinessLogic
{
    public class CommonCommunicationBusinessLogic
    {
        private static string emailMask = System.Configuration.ConfigurationSettings.AppSettings["Email_Mask"].ToString();
        private static string smtpServer = System.Configuration.ConfigurationSettings.AppSettings["SMTP_server"].ToString();
        private static string smtpPort = System.Configuration.ConfigurationSettings.AppSettings["SMTP_Port"].ToString();
        private static string networkCredentialUserName = System.Configuration.ConfigurationSettings.AppSettings["NetworkCredential_userName"].ToString();
        private static string networkCredentialPassword = System.Configuration.ConfigurationSettings.AppSettings["NetworkCredential_Password"].ToString();
        private static string enableSSL = System.Configuration.ConfigurationSettings.AppSettings["EnableSsl"].ToString();
        private static string useDefaultCredentials = System.Configuration.ConfigurationSettings.AppSettings["UseDefaultCredentials"].ToString();
        private static string tempEmailAttachmentPath = System.Configuration.ConfigurationSettings.AppSettings["TempEmailAttachmentPath"].ToString();
        private static string fullFilePath = String.Empty;
        private static List<string> attachmentFileList;

        #region "Region To handle Sending of Emails"


        #region "Method 'ConstructEmail' is used by implementing 'Method Overiding'

        //'ConstructEmail' method for html body for datatable parameters with email body message and current date
        public static MailMessage ConstructEmail(DataTable dtSellerBalanceDailyRec, string emailBodyMsg, string currentDate)
        {
            try
            {
                //DateTime currentDate = DateTime.Now;
                //string formattedCurrentDate = currentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                StringBuilder sb = new StringBuilder();

                //Before appending the html table,append the success message
                sb.AppendLine("<p>" + emailBodyMsg + "</p><br/>");

                //Construct the table header row
                sb.AppendLine("<table width='800px' cellspacing='0' cellpadding='0' border='1'><tbody><tr bgcolor='#00790A' style='color:white'><th align='center' style='width:300px'>&nbsp;Factory Code</th><th  align='center' style='width:300px'>&nbsp;Factory Name</th><th  align='center' style='width:300px'>&nbsp;TCode</th><th  align='center' style='width:300px'>&nbsp;Loan Balance(Rs.)</th><th  align='center' style='width:300px'>&nbsp;Advance Balance(Rs.)</th><th  align='center' style='width:350px'>&nbsp;Entry Time</th></tr>");

                //Loop through the datatable to bind the rows the html table
                foreach (DataRow row in dtSellerBalanceDailyRec.Rows)
                {

                    sb.AppendLine("<tr style='background-color:#e3f9eb'><td align='left'>" +
                                  row["FactoryCode"].ToString() +
                                "&nbsp;</td><td  align='left'>&nbsp;" + row["FactoryName"].ToString() +
                                "</td><td align='center'>" + row["TCode"].ToString() +
                                "&nbsp;</td><td align='right'>&nbsp;" + row["SellerFixedLoanBalance"].ToString() +
                                "</td><td align='right'>" + row["SellerAdvLoanBalance"].ToString() +
                                "&nbsp;</td><td align='left'>" + row["EntryTime"].ToString() + "&nbsp;</td>");


                }
                sb.AppendLine("</tr></tbody></table>");


                string htmlBody = @"<!doctype html>
                                        <html>
                                        <head>
                                        <meta charset='utf-8'>
                                        <title></title>
                                        </head>
                                        <body style='font-family: 'Segoe UI''>" + sb + @"
                                        </body>
                                        </html>";

                var mail = new MailMessage();
                var ToEmailAddrList = GetToEMailAdresses();

                foreach (var emailObj in ToEmailAddrList)
                {
                    mail.To.Add(new MailAddress(emailObj.ToString()));
                }

                mail.Subject = "Navition to tbBOSS transfer process-" + currentDate;
                mail.IsBodyHtml = true;
                mail.Body = htmlBody;

                return mail;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //'ConstructEmail' method for html body for datatable parameters with email body message,file attachment name and current date 
        //public static MailMessage ConstructEmail(DataTable dtSellerBalanceDailyRec,string emailBodyMsg,string fileName,string currentDate)
        public MailMessage ConstructEmail(DataTable dtSellerBalanceDailyRec, string emailBodyMsg, string fileName, string currentDate)
        {
            try
            {

                System.Net.Mail.Attachment attachment;

                //Initialize the attachment File List
                attachmentFileList = new List<string>();

                //Convert the datatable to excel and save as excel file
                fullFilePath = tempEmailAttachmentPath + fileName;
                ExportDataSetToExcel(dtSellerBalanceDailyRec, fullFilePath);

                var mail = new MailMessage();
                var ToEmailAddrList = GetToEMailAdresses();

                foreach (var emailObj in ToEmailAddrList)
                {
                    mail.To.Add(new MailAddress(emailObj.ToString()));
                }

                mail.Subject = "Navition to tbBOSS transfer process-" + currentDate;
                mail.IsBodyHtml = true;
                mail.Body = "The above process was successfully done.";

                //Add email attachments
                foreach (var attFile in attachmentFileList)
                {
                    attachment = new System.Net.Mail.Attachment(attFile.ToString());
                    mail.Attachments.Add(attachment);
                }


                return mail;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //'ConstructEmail' method for pure string body and customized subject string for string parameters
        public static MailMessage ConstructEmail(string subject, string emailBodyString)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                string formattedCurrentDate = currentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);


                var mail = new MailMessage();
                var ToEmailAddrList = GetToEMailAdresses();

                foreach (var emailObj in ToEmailAddrList)
                {
                    mail.To.Add(new MailAddress(emailObj.ToString()));
                }

                mail.Subject = subject;
                mail.IsBodyHtml = false;
                mail.Body = emailBodyString;

                return mail;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static MailMessage ConstructEmail(string subject, string emailBodyString, List<int> UserList)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                string formattedCurrentDate = currentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);


                var mail = new MailMessage();
                var ToEmailAddrList = GetToEMailAdresses(UserList);

                foreach (var emailObj in ToEmailAddrList)
                {
                    mail.To.Add(new MailAddress(emailObj.Email.ToString()));
                }

                mail.Subject = subject;
                mail.IsBodyHtml = false;
                mail.Body = emailBodyString;

                return mail;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static List<SystemUser> GetToEMailAdresses(List<int> userList)
        {
            UserDetailsBusinessLogic _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();
            return _UserDetailsBusinessLogic.GetUserDetails(userList);
        }

        //'ConstructEmail' method for resetting pasword(Parameters are username,To Email,TempPassword)
        public static MailMessage ConstructEmail(string username, string emailTo, string tempPassword)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<p>Hi " + username + "! Your temporary password is : <b>" + tempPassword + "</b><br/>This password will expire as soon as you update it with a new password.");

                string fromEmail = emailMask;
                string mailBody = @"<!doctype html>
                                        <html>
                                        <head>
                                        <meta charset='utf-8'>
                                        <title></title>
                                        </head>

                                        <body style='font-family: 'Segoe UI''>"
                                     + sb + @"
                                        </body>
                                        </html>";


                mailMsg.Body = mailBody;

                mailMsg.IsBodyHtml = true;

                mailMsg.From = new MailAddress(fromEmail);
                mailMsg.To.Add(emailTo);
                mailMsg.Subject = "Aventio Error Log System-Temperory Password";
                mailMsg.Body = mailBody;

                return mailMsg;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion


        #region "Send Email"
        public static bool SendEmail(MailMessage mail)
        {
            try
            {


                //mail.From = new MailAddress("noreply@webitpro.lk", "tbBOSS-Information");
                mail.From = new MailAddress(emailMask, "Aventio Error Log KB System-Info");
                SmtpClient SmtpServer = new SmtpClient(smtpServer);

                //email port
                SmtpServer.Port = Convert.ToInt32(smtpPort);
                SmtpServer.UseDefaultCredentials = Convert.ToBoolean(true);
                SmtpServer.EnableSsl = Convert.ToBoolean(enableSSL);//ssl availablility
                mail.Priority = MailPriority.High;
                //mail server credentials
                var userName = networkCredentialUserName;
                var password = networkCredentialPassword;
                if (userName.Length == 0 && password.Length == 0)
                {
                    SmtpServer.Credentials = new System.Net.NetworkCredential();
                }
                else
                {
                    SmtpServer.Credentials = new System.Net.NetworkCredential(userName, password);
                }


                SmtpServer.Send(mail);
                SmtpServer.Dispose();


                return true;

            }
            catch (Exception ex)
            {
                return false;
            }

        }

        //Note: Get the 'To' email addresses from 'System Param' table,
        //having the 'Name' column value starting with 'Advance_Calculation_Email_'
        public static List<string> GetToEMailAdresses()
        {
            try
            {
                return null;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion


        #region "Section To Handle forming of attachments"


        //Convert datatable to excel file
        private void ExportDataSetToExcel(DataTable dtRowData, string fullFilePath)
        {
            DateTime currentDate = DateTime.Now;
            string formattedCurrentDate = currentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);



            //Add this file to list
            attachmentFileList.Add(fullFilePath);

            //Before sending email,delete existing files in the 'Temp_Email_Attachment_Files' folder
            System.IO.DirectoryInfo di = new DirectoryInfo(tempEmailAttachmentPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }


            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dtRowData);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.SaveAs(fullFilePath);
                wb.Dispose();
            }
        }

        #endregion

        #endregion

    }
}
