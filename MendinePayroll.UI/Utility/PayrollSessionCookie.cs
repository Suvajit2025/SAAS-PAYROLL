using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace MendinePayroll.UI.Utility
{
    public class PayrollSessionCookie
    {
        private const string CookieName = "PRSL"; // Payroll Session
        private const int ExpiryDays = 1;

        public static void Write(HttpResponseBase response,
            string roleId, string userName, string userId,
            string employeeNo, string userEmail, string tenantId)
        {
            // Pipe-delimited, then encrypted with MachineKey
            string raw = $"{roleId}|{userName}|{userId}|{employeeNo}|{userEmail}|{tenantId}";
            string encrypted = FormsAuthentication.Encrypt(
                new FormsAuthenticationTicket(1, "PRSL", DateTime.Now,
                    DateTime.Now.AddDays(ExpiryDays), true, raw));

            var cookie = new HttpCookie(CookieName, encrypted)
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(ExpiryDays)
            }; 
            cookie["SameSite"] = "Lax";

            response.Cookies.Set(cookie);
        }

        public static string[] Read(HttpRequestBase request)
        {
            var cookie = request.Cookies[CookieName];
            if (cookie == null) return null;

            try
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket == null || ticket.Expired) return null;
                return ticket.UserData.Split('|');
                // [0]=RoleID [1]=UserName [2]=UserId
                // [3]=EmployeeNo [4]=UserEmail [5]=TenantID
            }
            catch { return null; }
        }

        public static void Clear(HttpResponseBase response)
        {
            var cookie = new HttpCookie(CookieName)
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true,
                Secure = true
            };
            cookie["SameSite"] = "Lax";
            response.Cookies.Set(cookie);
        }
    }
}