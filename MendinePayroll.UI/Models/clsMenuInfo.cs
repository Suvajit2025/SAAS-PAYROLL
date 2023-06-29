using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class clsMainMenuInfo
    {
        public long MenuSRL { get; set; } = 0;
        public String MainMenu { get; set; } = "";
    }
    public class clsSubMenuInfo
    {
        public long MenuSRL { get; set; } = 0;
        public String MainMenu { get; set; } = "";
        public String SubMenu { get; set; } = "";
        public String MenuURL { get; set; } = "";
        public String MenuIcon { get; set; } = "";
    }
    public class clsMenuInfo
    {
        public List<clsMainMenuInfo> MainMenu { get; set; } = new List<clsMainMenuInfo>();
        public List<clsSubMenuInfo> SubMenu { get; set; } = new List<clsSubMenuInfo>();
    }
}