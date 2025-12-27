using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class payrollProcess
    {
    }
    public class PayrollProcessRequest
    {
        public string PayrollType { get; set; }   // "1" Monthly, "2" Contractual

        public int? ProcessMonth { get; set; }
        public int? ProcessYear { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<EmployeeAttendanceDto> Employees { get; set; }
    }
    public class EmployeeAttendanceDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } 
        public decimal PaidDays { get; set; }
    }
    public class EmployeeSalaryResponse
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal PaidDays { get; set; }

        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal MonthlyCTC { get; set; }

        public List<SalaryComponentResult> Components { get; set; }
    }
    public class SalaryComponentResult
    {
        /* =========================
           Identification
        ========================= */
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }

        /* =========================
           Classification
        ========================= */
        public string PayType { get; set; }        // Allowances / Deduction / CC / MANUAL
        public string LogicType { get; set; }      // FIXED / FORMULA / MANUAL_INPUT

        /* =========================
           Calculation
        ========================= */
        public decimal Amount { get; set; }        // Final calculated (prorated) amount
        public decimal BaseAmount { get; set; }    // Base value used for % calculation
        public decimal Rate { get; set; }          // Percentage (ManualRate)

        /* =========================
           Flags (VERY IMPORTANT)
        ========================= */
        public bool IsPercentage { get; set; }
        public bool IsStatutory { get; set; }

        public bool IsBasicComponent { get; set; } // Used for PF base
        public bool IsGrossComponent { get; set; } // Used for ESIC / PTAX base

        public string StatutoryType { get; set; }  // PF_EE / PF_ER / ESIC_EE / ESIC_ER / PTAX / TDS
        public string PTaxSlabsJson { get; set; }  // JSON slab definition

        /* =========================
           NEW: Allowance Flags (FOR UI / REPORTING)
        ========================= */
        public bool IsAllowance { get; set; }      // PC.IsAllowance
        public string AllowanceType { get; set; }  // BASIC / HRA / MEDICAL / etc.

        /* =========================
           NEW: Other / External Flags
        ========================= */
        public bool IsOther { get; set; }           // EC.IsOther
        public string OtherType { get; set; }       // LOANRECOVERY / BONUS / INCENTIVE

        /* =========================
           Attendance Context
        ========================= */
        public decimal PaidDays { get; set; }
        public decimal TotalDays { get; set; }

        /* =========================
           Limits & Rounding
        ========================= */
        public decimal? MaxLimit { get; set; }     // PF/ESIC ceiling
        public string RoundingType { get; set; }   // ROUND / CEIL / FLOOR

        /* =========================
           Audit / Debug (CRITICAL)
        ========================= */
        public string CalculationExpression { get; set; } // Stored DB formula
    }

    public class EmployeeSalaryHeader
    {
        public int EmpSalaryConfigID { get; set; }
        public int EmployeeId { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal BasicSalary { get; set; }
        public int TotalDays { get; set; }
    }
    public class PTaxSlab
    {
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public decimal PTAXAmount { get; set; }
    }
    public class PayrollPivotResponse
    {
        public bool Success { get; set; }
        public string PayrollType { get; set; }
        public int? ProcessMonth { get; set; }
        public int? ProcessYear { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<PivotColumnDto> Columns { get; set; } = new List<PivotColumnDto>();
        public List<PivotRowDto> Rows { get; set; } = new List<PivotRowDto>();
    }

    public class PivotColumnDto
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayType { get; set; }        // Allowances / Deduction / CC / MANUAL
        public string LogicType { get; set; }      // FIXED / FORMULA / MANUAL_INPUT / EXCEL_UPLOAD

        public bool IsAllowance { get; set; }
        public string AllowanceType { get; set; }

        public bool IsStatutory { get; set; }
        public string StatutoryType { get; set; }

        public bool IsOther { get; set; }
        public string OtherType { get; set; }

        public bool IsEditable { get; set; }       // UI rule: MANUAL_INPUT / EXCEL_UPLOAD / OtherType etc
        public bool IsWaivable { get; set; }       // Loan waive
        public int DisplayOrder { get; set; }

        public bool IsBasicComponent { get; set; } // Used for PF base
        public bool IsGrossComponent { get; set; } // Used for ESIC / PTAX base

    }

    public class PivotRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal PaidDays { get; set; }

        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal MonthlyCTC { get; set; }

        public bool IsNetRow { get; set; }
        public bool IsCTCRow { get; set; }
        public List<PivotCellDto> Cells { get; set; } = new List<PivotCellDto>();
    }

    public class PivotCellDto
    {
        public int PayConfigId { get; set; }
        public decimal Amount { get; set; }

        // for UI behavior
        public decimal BaseAmount { get; set; }
        public bool IsEditable { get; set; }

        // Loan waive
        public bool IsWaivable { get; set; }
        public bool IsWaived { get; set; }         // default false, UI can toggle
        public decimal OriginalAmount { get; set; } // keep original when waived
    }
    /*====Save Employee Salary DTO============*/
    public class PayrollBatchSaveDto
    {
        

        // ===== Reference =====
        public string RefNo { get; set; }
        public DateTime RefDate { get; set; }

        // ===== Payroll Context =====
        public int PayGroupId { get; set; }
        public int PayrollType { get; set; } // 1 = Monthly, 2 = Contractual

        // Monthly payroll
        public int? ProcessMonth { get; set; }
        public int? ProcessYear { get; set; }

        // Contractual payroll
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // ===== Calculated (Server Side) =====
        public int TotalEmployees { get; set; }
        public decimal TotalCost { get; set; }      // Sum of Gross
        public decimal TotalNetPay { get; set; }

        // ===== Optional =====
        public string Comments { get; set; }

        // ===== Employees =====
        public List<PayrollBatchEmployeeDto> Employees { get; set; }
            = new List<PayrollBatchEmployeeDto>();
    }

    public class PayrollBatchEmployeeDto
    {
        // ===== Identity =====
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }   // Snapshot safety
        public int PayGroupId { get; set; }

        // ===== Attendance =====
        public decimal PaidDays { get; set; }
        public decimal? TotalHours { get; set; }

        // ===== Earnings =====
        public decimal TotalGross { get; set; }
        public decimal TotalAddition { get; set; }
        public decimal ManualAddition { get; set; }

        // ===== Deductions =====
        public decimal TotalDeduction { get; set; }
        public decimal ManualDeduction { get; set; }
        public decimal LoanDeduction { get; set; }
        public bool LoanWaivedYN { get; set; }

        // ===== Net =====
        public decimal NetPay { get; set; }

        // ===== UI-Only (NOT persisted) =====
        public decimal MonthlyCTC { get; set; }

        // ===== Salary Components =====
        public List<PayrollBatchEmployeeComponentDto> Components { get; set; }
            = new List<PayrollBatchEmployeeComponentDto>();
    }

    public class PayrollBatchEmployeeComponentDto
    {
        // ===== Pay Structure =====
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }      // EARNING / DEDUCTION / STATUTORY

        // ===== Calculation =====
        public string CalculationSource { get; set; } // FIXED / FORMULA / MANUAL / LOAN / TDS
        public decimal PayValue { get; set; }

        // ===== Loan / Adjustment (UI-Only) =====
        public decimal? OriginalAmount { get; set; }
        public bool IsWaived { get; set; }
    }



}