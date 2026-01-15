using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MendinePayroll.UI.Models;
using System.Configuration;

namespace UI.BLL
{
    public class clsAccessLog
    {
        private static string username
        {
            get
            {
                var context = HttpContext.Current;

                if (context == null || context.Session == null || context.Session["username"] == null)
                {
                    context?.Response?.Redirect(
                        "https://iehrms.iecsl.in/Account/ChoosePortal.aspx",
                        true   // endResponse = true
                    );

                    // Safety fallback (won't execute after redirect)
                    return string.Empty;
                }

                return context.Session["username"].ToString();
            }
        }


        public static string AccessLog_Save(clsAccessLogInfo info)
        {
            return clsDatabase.fnDBOperation("PRC_Payroll_Access_Log",username,info.AccessType);
        }
    }
}
