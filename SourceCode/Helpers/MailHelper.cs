using System.Net;
using System.Net.Mail;

namespace SDK.Helpers
{
    /// <summary>
    /// Generic Mail Helper for SAP Business One Add-ons
    /// </summary>
    public static class MailHelper
    {
        /// <summary>
        /// Sends an email with a single attachment
        /// </summary>
        public static bool SendMail(
            string toEmail,
            string subject,
            string bodyHtml,
            string attachmentPath)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient(
                    AppConfig.SmtpHost,
                    AppConfig.SmtpPort))
                {
                    smtp.EnableSsl = AppConfig.EnableSsl;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(
                        AppConfig.SmtpUser,
                        AppConfig.SmtpPassword);

                    using (MailMessage message = new MailMessage())
                    {
                        message.From = new MailAddress(AppConfig.SmtpUser);
                        message.Subject = subject;
                        message.Body = bodyHtml;
                        message.IsBodyHtml = true;

                        // Support multiple recipients separated by ;
                        foreach (var mail in toEmail.Split(';'))
                        {
                            if (!string.IsNullOrWhiteSpace(mail))
                                message.To.Add(mail.Trim());
                        }

                        if (!string.IsNullOrEmpty(attachmentPath))
                        {
                            message.Attachments.Add(
                                new Attachment(attachmentPath));
                        }

                        smtp.Send(message);
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Global.oApplication.SetStatusBarMessage(
                    "Mail sending failed : " + ex.Message,
                    SAPbouiCOM.BoMessageTime.bmt_Short,
                    true);

                return false;
            }
        }
    }
}

