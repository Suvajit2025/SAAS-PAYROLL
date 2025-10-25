using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class payslip
    {
    }
    public class AttendanceLog
    {
        public string LogDate { get; set; }
        public int EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public string Duration { get; set; }
        public string WorkType { get; set; }
    }
    public class PayslipViewModel
    {
        public string Month { get; set; }
        public int Year { get; set; }
        // --- Company Info ---
        public string CompanyName { get; set; }
        public string CompanyLogoUrl { get; set; }
        public string CompanyAddressLine1 { get; set; }
        public string CompanyAddressLine2 { get; set; }

        // --- Employee Info ---
        public int EmpId { get; set; }
        public string EmpNo { get; set; }
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public DateTime DOJ { get; set; }
        public string PAN { get; set; }
        public string UAN { get; set; }
        public string PFNo { get; set; }
        public string ESICNo { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }

        // --- Attendance ---
        public int TotalDays { get; set; }
        public int WorkedDays { get; set; }
        public int LOPDays { get; set; }
        public int PayableDays { get; set; }
        public int LeaveTaken { get; set; }

        // --- Leave Balances ---
        public List<LeaveBalance> LeaveBalances { get; set; }
        // --- Salary Heads ---
        public List<SalaryHead> Earnings { get; set; }
        public List<SalaryHead> Deductions { get; set; }

        // --- Totals (must be assignable in MVC5) ---
        public decimal GrossEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal OpeningLoan { get; set; }
        public decimal ClosingLoan { get; set; }
        public decimal LoanInstallment { get; set; }
        public string NetPayWords { get; set; }
        public bool isPdf { get; set; }
        public PayslipViewModel()
        {
            LeaveBalances = new List<LeaveBalance>();
            Earnings = new List<SalaryHead>();
            Deductions = new List<SalaryHead>();
        }
    }

    public class LeaveBalance
    {
        public string CodeType { get; set; }
        public decimal Balance { get; set; }
    }

    public class SalaryHead
    {
        public string PayConfigName { get; set; }
        public decimal Amount { get; set; }
    }

    public class EmployeeSalaryReportModel
    {
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string Company { get; set; }
        public decimal Basic { get; set; }
        public decimal HRA { get; set; }
        public decimal ConveyenceAllowance { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal OtherEarning { get; set; }
        public decimal TotalEarning { get; set; }
        public decimal PF { get; set; }
        public decimal ESI { get; set; }
        public decimal PT { get; set; }
        public decimal TDS { get; set; }
        public decimal OtherDeduction { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPayable { get; set; }
        public int PresentDays { get; set; }
    }
    public class DepartmentSalaryGroup
    {
        public string Department { get; set; }
        public List<EmployeeSalaryReportModel> Employees { get; set; }
        public EmployeeSalaryReportModel SubTotal { get; set; }
    }

    public class SalaryReportViewModel
    {
        public string CompanyName { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public List<DepartmentSalaryGroup> Departments { get; set; }
        public EmployeeSalaryReportModel GrandTotal { get; set; }
    }
}