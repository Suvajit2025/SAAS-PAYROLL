using Common.Utility;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

public class MailService
{
    public bool SendMails(string toEmail,string fromEmail,string bodyHtml,string subject,byte[] attachmentBytes,string fileNameWithoutExt)
    {
        try
        {
            string smtpUser = ConfigurationManager.AppSettings["MailUserName"];
            string smtpPass = ConfigurationManager.AppSettings["MailPassword"];
            string smtpHost = ConfigurationManager.AppSettings["Host"];
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            bool enableSsl = ConfigurationManager.AppSettings["enablessl"] == "1";
            string defaultFrom = ConfigurationManager.AppSettings["MailFrom"];
            string adminEmail = ConfigurationManager.AppSettings["AdminEmail"];

            using (var mail = new MailMessage())
            {
                // FROM
                mail.From = new MailAddress(
                    !string.IsNullOrWhiteSpace(fromEmail) ? fromEmail : defaultFrom
                );

                // TO
                mail.To.Add(
                    !string.IsNullOrWhiteSpace(toEmail) ? toEmail : adminEmail
                );

                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.BodyEncoding = Encoding.UTF8;

                // Plain text fallback
                string plainText =
                    Regex.Replace(bodyHtml ?? string.Empty, "<.*?>", string.Empty);

                mail.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(
                        plainText, Encoding.UTF8, "text/plain")
                );

                mail.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(
                        bodyHtml, Encoding.UTF8, "text/html")
                );

                // Attachment
                if (attachmentBytes != null && attachmentBytes.Length > 0)
                {
                    var attachmentStream = new MemoryStream(attachmentBytes);
                    var attachment = new Attachment(
                        attachmentStream,
                        $"{fileNameWithoutExt}.pdf",
                        "application/pdf"
                    );

                    mail.Attachments.Add(attachment);
                }

                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.EnableSsl = enableSsl;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    smtp.Send(mail);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // 🔴 IMPORTANT: log this
            clsDatabase.fnErrorLog("MailSendError", ex.ToString());
            return false;
        }
    }
}
