using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class EmployeeLeaveModel
    {
        public EmployeeLeaveModel()
        {
            EmpleaveList = new List<EmployeeLeaveModel>();
        }

        public int applicationhdrid { get; set; }
        public decimal? noofdays { get; set; }
        public DateTime? prefixfromdate { get; set; }
        public DateTime? suffixtodate { get; set; }
        public string enddayhalfind { get; set; }
        public string Applicationstatus { get; set; }
        public int Empno { get; set; }
        public  int? lateleave { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public List<EmployeeLeaveModel> EmpleaveList { get; set; }

        public string Month { get; set; }
        public double lateleaveAmount { get; set; }
        public double leaveAmount { get; set; }
        public double TotalLeaveAmount { get; set; }
        public int Days { get; set; }
        public double? AbsentDays { get; set; }
    }
}
