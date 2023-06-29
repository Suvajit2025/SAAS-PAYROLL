using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class IndividualSalryModel
    {
        //public int empid { get; set; }
        //public int empno { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyAddress { get; set; }
        public string empcode { get; set; }
        public string postname { get; set; }
        public string Designation { get; set; }
        public System.DateTime? joining { get; set; }
        public System.DateTime? DOB { get; set; }
        public string Department { get; set; }
        public int Rowid { get; set; }
        public int? Empid { get; set; }
        public string EmployeeNo { get; set; }

        public string SEmpid { get; set; }
        public int? Empno { get; set; }
        public int? PayGroupId { get; set; }
        public double? Basic { get; set; }
        public double? HRA { get; set; }
        public double? Attire_Allowance { get; set; }
        public double? Internet_Allowance { get; set; }
        public double? Mobile_Allowance { get; set; }
        public double? PF { get; set; }
        public double? ESI { get; set; }
        public double? P_Tax { get; set; }
        public double? Education_Allowances { get; set; }
        public double? Medical_Allowances { get; set; }
        public double? LTA { get; set; }
        public double? Gross_Amount { get; set; }
        public double? Loan_Installment { get; set; }
        public double? Continuous_Attendance_Allowance { get; set; }
        public double? Overtime_Allowance { get; set; }
        public double? Lodging_Allowance { get; set; }
        public double? ProductionBonus { get; set; }
        public double? Leave_Amount { get; set; }
        public double? Total_Allowances { get; set; }
        public double? Total_Deduction { get; set; }
        public double? Net_Payable_Amount { get; set; }
        public string Month { get; set; }
        public int? Year { get; set; }
        public string Amountinwords { get; set; }
        public string empaccountno { get; set; }
        public string empepfno { get; set; }
        public string empesicno { get; set; }
        public string empuanno { get; set; }
        public string emppancardno { get; set; }
        public string Company { get; set; }

        public string FirstDays { get; set; }
        public string LastDays { get; set; }
        public string AbsentDays { get; set; }
        public string WorkingDays { get; set; }
        public string EarnedLeaveDays { get; set; }
        public string TotalLeaveDays { get; set; }
        public string LateLeave { get; set; }

        public string LateLeaveAmount { get; set; }
        public string LeaveAmount { get; set; }
        public double? Arrears { get; set; }
        public double? Stipend { get; set; }
        public double? PerformanceAllowance { get; set; }
        public double? SpecialAllowance { get; set; }
        public double? Books___Periodical { get; set; }
        public double? Conveyence_Allowance { get; set; }
        public double? Training__Allowance { get; set; }
        public double? Vehical_Allownce { get; set; }
        public double? Food_Allowance { get; set; }

        public double? TDS { get; set; }
        public double OtherDeduction { get; set; }
    }
}
