using Common.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

public class MailService
{
    public bool SendMails(
        string toEmail,
        string fromEmail,
        string bodyHtml,
        string subject,
        byte[] attachmentBytes,
        string fileNameWithoutExt,
        string tenantId = null,
        string mailPurpose = null,
        string profileName = null,
        IDictionary<string, string> templateValues = null)
    {
        try
        {
            MailConfiguration dbMailConfig = GetMailConfiguration(tenantId, mailPurpose, profileName);

            string smtpUser = ConfigurationManager.AppSettings["MailUserName"];
            string smtpPass = ConfigurationManager.AppSettings["MailPassword"];
            string smtpHost = ConfigurationManager.AppSettings["Host"];
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            bool enableSsl = ConfigurationManager.AppSettings["enablessl"] == "1";
            string defaultFrom = ConfigurationManager.AppSettings["MailFrom"];
            string adminEmail = ConfigurationManager.AppSettings["AdminEmail"];

            if (dbMailConfig != null)
            {
                smtpUser = FirstNonEmpty(dbMailConfig.UserName, smtpUser);
                smtpPass = FirstNonEmpty(dbMailConfig.AppPassword, dbMailConfig.Password, smtpPass);
                smtpHost = FirstNonEmpty(dbMailConfig.SmtpHost, smtpHost);
                smtpPort = dbMailConfig.SmtpPort > 0 ? dbMailConfig.SmtpPort : smtpPort;
                enableSsl = dbMailConfig.EnableSsl;
                defaultFrom = FirstNonEmpty(dbMailConfig.FromEmail, defaultFrom);

                subject = RenderTemplate(
                    FirstNonEmpty(dbMailConfig.MailSubject, subject),
                    templateValues);

                bodyHtml = RenderTemplate(
                    FirstNonEmpty(dbMailConfig.MailBody, bodyHtml),
                    templateValues);
            }

            string resolvedTo = !string.IsNullOrWhiteSpace(toEmail) ? toEmail : adminEmail;
            string resolvedFrom = !string.IsNullOrWhiteSpace(fromEmail) ? fromEmail : defaultFrom;

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(resolvedFrom);
                mail.To.Add(resolvedTo);
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.BodyEncoding = Encoding.UTF8;

                string plainText = Regex.Replace(bodyHtml ?? string.Empty, "<.*?>", string.Empty);

                mail.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(
                        plainText, Encoding.UTF8, "text/plain"));

                mail.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(
                        bodyHtml ?? string.Empty, Encoding.UTF8, "text/html"));

                if (attachmentBytes != null && attachmentBytes.Length > 0)
                {
                    var attachmentStream = new MemoryStream(attachmentBytes);
                    var attachment = new Attachment(
                        attachmentStream,
                        $"{fileNameWithoutExt}.pdf",
                        "application/pdf");

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
                        $"Source={(dbMailConfig == null ? "WebConfig" : "Database")}; Host={smtpHost}; Port={smtpPort}; EnableSsl={enableSsl}; From={resolvedFrom}; To={resolvedTo}; AttachmentBytes={(attachmentBytes == null ? 0 : attachmentBytes.Length)}");

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
            clsDatabase.fnErrorLog("MailSendError", ex.ToString());
            return false;
        }
    }

    private MailConfiguration GetMailConfiguration(string tenantId, string mailPurpose, string profileName)
    {
        if (string.IsNullOrWhiteSpace(tenantId)
            || string.IsNullOrWhiteSpace(mailPurpose)
            || string.IsNullOrWhiteSpace(profileName))
        {
            return null;
        }

        try
        {
            DataTable dt = clsDatabase.fnDataTable(
                "SP_Saas_MailConfiguration_GetByPurpose",
                tenantId,
                mailPurpose,
                profileName);

            if (dt == null || dt.Rows.Count == 0)
                return null;

            DataRow row = dt.Rows[0];

            return new MailConfiguration
            {
                MailSubject = HtmlDecode(GetString(row, "MailSubject")),
                MailBody = HtmlDecode(GetString(row, "MailBody")),
                FromEmail = GetString(row, "FromEmail"),
                UserName = GetString(row, "UserName"),
                AppPassword = GetString(row, "AppPassword"),
                Password = GetString(row, "Password"),
                SmtpHost = GetString(row, "Smtp_Host"),
                SmtpPort = GetInt(row, "Smtp_Port"),
                EnableSsl = GetBool(row, "EnableSsl")
            };
        }
        catch (Exception ex)
        {
            clsDatabase.fnErrorLog("SaasMailConfigLoadError", ex.ToString());
            return null;
        }
    }

    private string RenderTemplate(string template, IDictionary<string, string> values)
    {
        if (string.IsNullOrEmpty(template) || values == null)
            return template ?? string.Empty;

        foreach (var item in values)
            template = template.Replace("{" + item.Key + "}", item.Value ?? string.Empty);

        return template;
    }

    private string HtmlDecode(string value)
    {
        return string.IsNullOrEmpty(value) ? value : HttpUtility.HtmlDecode(value);
    }

    private string FirstNonEmpty(params string[] values)
    {
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private string GetString(DataRow row, string columnName)
    {
        return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
            ? row[columnName].ToString()
            : string.Empty;
    }

    private int GetInt(DataRow row, string columnName)
    {
        int value;
        return int.TryParse(GetString(row, columnName), out value) ? value : 0;
    }

    private bool GetBool(DataRow row, string columnName)
    {
        string value = GetString(row, columnName);
        return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private class MailConfiguration
    {
        public string MailSubject { get; set; }
        public string MailBody { get; set; }
        public string FromEmail { get; set; }
        public string UserName { get; set; }
        public string AppPassword { get; set; }
        public string Password { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
    }
}
