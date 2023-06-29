using Common.Utility;
using MendinePayroll.UI.BLL;
using MendinePayroll.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class clsEmpLoanInfo 
    {
     
        public double? MonthlyInstallment { get; set; }
    }
    public class clsEmployeeInfo
    {
        public long IDEmployee { get; set; } = 0;
        public String  EmployeeNo { get; set; } = "";
        public String  EmployeeName { get; set; } = "";
        public String PAN { get; set; } = "";
    }
    public class clsMonthInfo
    {
        public long Value { get; set; } = 0;
        public String Name { get; set; } = "";
        public Boolean Selected { get; set; } = false;
    }
    public class clsYearInfo
    {
        public long Value { get; set; } = 0;
        public String Name { get; set; } = "";
        public Boolean Selected { get; set; } = false;
    }
    public class clsCompanyInfo
    {
        public long ID { get; set; } = 0;
        public String Code { get; set; } = "";
        public String Name { get; set; } = "";
    }
    public class clsLoanApprovalInfo
    {
        public long IDApplication { get; set; } = 0;
        public Boolean Approval { get; set; } = false;
        public Decimal ApprovedAmount { get; set; } = 0;
        public Boolean Rejected { get; set; } = false;
        public String RejectReason { get; set; } = "";
        public String User { get; set; } = "";

    }
}