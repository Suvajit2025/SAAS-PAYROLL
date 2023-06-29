using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UI.Models
{
    public class clsSalaryProcessValidationInfo
    {
        public long IDPayroll { get; set; } = 0;
        public long IDEmployee { get; set; } = 0;
        public String EmployeeNo { get; set; } = "";
        public String EmployeeName { get; set; } = "";
        public String Department { get; set; } = "";
        public String PayGroup { get; set; } = "";
        public Boolean Valid { get; set; } = false;
    }
    public class clsPayGroupInfo
    {
        public long IDPaygroup { get; set; } = 0;
        public String Name { get; set; } = "";
    }
    public class clsPayrollInfo
    {
        public long IDPayroll { get; set; } = 0;
        public String Month { get; set; } = "";
        public long Year { get; set; } = 0;
    }

}