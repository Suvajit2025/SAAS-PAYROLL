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
            string resolvedTo = !string.IsNullOrWhiteSpace(toEmail) ? toEmail : adminEmail;
            string resolvedFrom = !string.IsNullOrWhiteSpace(fromEmail) ? fromEmail : defaultFrom;

            using (var mail = new MailMessage())
            {
                // FROM
                mail.From = new MailAddress(resolvedFrom);

                // TO
                mail.To.Add(resolvedTo);

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
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                    smtp.EnableSsl = enableSsl;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 60000;

                    clsDatabase.fnErrorLog(
                        "MailSendAttempt",
                        $"Host={smtpHost}; Port={smtpPort}; EnableSsl={enableSsl}; From={resolvedFrom}; To={resolvedTo}; AttachmentBytes={(attachmentBytes == null ? 0 : attachmentBytes.Length)}");

                    smtp.Send(mail);

                    clsDatabase.fnErrorLog(
                        "MailSendSuccess",
                        $"Host={smtpHost}; Port={smtpPort}; From={resolvedFrom}; To={resolvedTo}");
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
