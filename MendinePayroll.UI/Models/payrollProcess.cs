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
        public int PaidDays { get; set; }
    }
    public class EmployeeSalaryResponse
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int PaidDays { get; set; }

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
        public int PaidDays { get; set; }
        public int TotalDays { get; set; }

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

    }

    public class PivotRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int PaidDays { get; set; }

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

}