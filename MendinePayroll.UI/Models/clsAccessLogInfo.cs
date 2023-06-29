using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class clsAccessLogInfo
    {
        public long IDLog { get; set; } = 0;
        public String UserName { get; set; } = "";
        public String AccessType { get; set; } = "";
        public String AccessTime { get; set; } = "";
    }
}