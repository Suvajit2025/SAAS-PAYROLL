using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MendinePayroll.Models
{
    public class EmployeeListModel
    {
        public EmployeeListModel()
        {
            emplist = new List<EmployeeListModel> ();
            EmployeeLoanModel = new EmployeeLoanModel();
        }
        public int empno { get; set; }
        public string EmployeeName { get; set; }
        public string empcode { get; set; }
        public string postname { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int empid { get; set; }
        public string sempid { get; set; }
        public string PayGroupName { get; set; }
        public int SalaryConfigureID { get; set; }
        public string SalaryConfigureName { get; set; }
        public int PageNumber { get; set; }
        public int Rowofpage { get; set; }
        public int TotalCount { get; set; }
        public string SearchVal { get; set; }
        public int PayGroupID { get; set; }
        //public double Basic { get; set; }
        //public double? GrossAmount { get; set; }
        //public double? InternetAllowance { get; set; }
        //public double? MobileAllowance { get; set; }
        //public double? ContinuousAllowance { get; set; }
        public int? empbankid { get; set; }
        public string bankname { get; set; }
        public string empbankbranchname { get; set; }
        public string empbankbranchifsccode { get; set; }
        public string designationname { get; set; }
        public string empaccountno { get; set; }
        public string empuanno { get; set; }
        public string empemail { get; set; }
        public string Company { get; set; }
        public string empepfno { get; set; }
        public string empesicno { get; set; }
        public string empuanno1 { get; set; }
        public string emppancardno { get; set; }
        public DateTime? Joining { get; set; }
        public List<EmployeeListModel> emplist{ get; set; }
        public EmployeeLoanModel EmployeeLoanModel { get; set; }
        public List<SelectListItem> MonthList { get; set; }
        public List<SelectListItem> YearList { get; set; }
    }
}
