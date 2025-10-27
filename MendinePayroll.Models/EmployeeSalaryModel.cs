using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace MendinePayroll.Models
{
    public class EmployeeSalaryModel
    {
        public int Rowid { get; set; }

        public int? Empid { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int Rowofpage { get; set; }
        public string sEmpid { get; set; }
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
        public double? ProductionBonus { get; set; }
        public double? Lodging_Allowance { get; set; }
        public double? Total_Allowances { get; set; }
        public double? Total_Deduction { get; set; }
        public double? Net_Payable_Amount { get; set; }
        public string Month { get; set; }
        public int? Year { get; set; }
        public string EmployeeName { get; set; }
        public string PayGroupeName { get; set; }
        public double? Leave_Amount { get; set; }
        public string Status { get; set; }
        public double? STIPEND { get; set; }
        public double? Arrears { get; set; }
        public double? PerformanceAllowance { get; set; }
        public double? SpecialAllowance { get; set; }
        public bool? IsPaid { get; set; }
        public double? Books___Periodical { get; set; }
        public double? Conveyence_Allowance { get; set; }
        public double? Training__Allowance { get; set; }
        public double? Vehical_Allownce { get; set; }
        public double? Food_Allowance { get; set; }
        public double?  EPFWages { get; set; }
        public double? EDLIWages { get; set; }
        public double? EPSWages { get; set; }
        public double EPFRemitted { get; set; }
        public double EPSRemitted { get; set; }
        public double EPFEPSDIFF { get; set; }
        public string NCPDays { get; set; }
        public string ADVANCES { get; set; }

        public double TDS { get; set; }
        public int ISTDS { get; set; }

        public string FinancialYear { get; set; }
        public int DepartmentId { get; set; }

        public string companycode { get; set; }
        public List<SelectListItem> MonthList { get; set; }
        public List<SelectListItem> YearList { get; set; }
        public List<SelectListItem> DepartmentList { get; set; }
        public List<SelectListItem> PayGroupList { get; set; }
        public EmployeeSalaryModel()
        {
            masterModel = new EmpbasicModel();
            SalaryList = new List<EmployeeSalaryModel>();
            TDSReportList = new List<EmployeeSalaryModel>();
        }
        public EmpbasicModel masterModel {get;set;}
        public List<EmployeeSalaryModel> SalaryList { get; set; }
        public List<EmployeeSalaryModel> TDSReportList { get; set; }
       
    }
    public class Payroll_ESIC_EmployeeSlab
    {
        public int EmployeeSlabId { get; set; }

        public int EmployeeId { get; set; }

        public int SlabId { get; set; }

        public bool IsEligible { get; set; }

        public string Remark { get; set; }

        public string CreatedBy { get; set; }
    }

    public class LoanRegisterViewModel
    {
        public string Department { get; set; }
        public string EmployeeName { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal LoanOpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        // holds Apr-25, May-25, ... Mar-26 dynamically
        public Dictionary<string, string> MonthlyValues { get; set; }

        public LoanRegisterViewModel()
        {
            MonthlyValues = new Dictionary<string, string>();
        }
    }


}
