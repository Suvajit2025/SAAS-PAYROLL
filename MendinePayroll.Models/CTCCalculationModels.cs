using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class CTCCalculationModels
    {
    }
    public class SalarySection
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public decimal ConfigValue { get; set; }
    }

    public class PromptRequest
    {
        public string Prompt { get; set; }
    }
    public class ManualValue
    {
        public int PayConfigID { get; set; }
        public string PayConfigName { get; set; }
        public string Values { get; set; }
    }
    public class CompanyContributions
    {
        public int PayConfigID { get; set; }
        public string PayConfigName { get; set; }
        public string Values { get; set; }
    }
    public class PayConfigAllowances
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public string CalculationFormula { get; set; }
        public decimal ManualRate { get; set; } // Represented as percentage (e.g., 25 for 25%)
        public decimal ConfigValue { get; set; } // Output value
    }
    public class PayConfigDeduction
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public string CalculationFormula { get; set; } // e.g., BASIC, Gross Amount
        public decimal ManualRate { get; set; }
        public decimal LowerLimit { get; set; }
        public decimal UpperLimit { get; set; }
        public decimal Rate { get; set; }
        public string Type { get; set; } // PERCENT, SLAB
        public string CalculatedOn { get; set; } // BASIC or Gross Amount
        public string RoundoffType { get; set; } // ROUND_NEAREST, CEILING, NONE
        public decimal ConfigValues { get; set; } // Final calculated value
    }
    public class CompanyContributionConfig
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public int BreakupID { get; set; }
        public string BreakupName { get; set; }
        public string BreakupKey { get; set; }
        public decimal BreakupRate { get; set; }
        public string Type { get; set; } // PERCENT, MANUAL, FORMULA
        public string CalculatedOn { get; set; } // BASIC / GROSS
        public string RoundoffType { get; set; } // ROUND_NEAREST, CEILING
        public decimal? Values { get; set; } // Final calculated amount
    }
    public class ContributionSummary
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public string CalculationFormula { get; set; } = "";
        public decimal ManualRate { get; set; } = 0;
        public decimal ConfigValue { get; set; }
    }
    public class outPutJson
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public decimal ConfigValue { get; set; }
    }
    public class PayrollArrearMaster
    {
        public string ArrearType { get; set; }
        public int EmpID { get; set; }
        public int OldPayGroupId { get; set; }
        public int NewPayGroupId { get; set; }
        public string ArrearFrom { get; set; }
        public string ArrearTo { get; set; }
        public string ArrearMonth { get; set; }
        public int TotalMonths { get; set; }
        public decimal GrossDifference { get; set; }
        public decimal NetDifference { get; set; }
        public decimal CTCDifference { get; set; }
        public decimal TotalGrossArrear { get; set; }
        public decimal TotalNetArrear { get; set; }
        public decimal TotalCTCArrear { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
    }
    public class PayrollArrearDetail
    {
        public int PayConfigID { get; set; }
        public string PayConfigName { get; set; }
        public string PayType { get; set; }
        public decimal OldAmount { get; set; }
        public decimal NewAmount { get; set; }
        public decimal MonthlyDifference { get; set; }
        public decimal TotalDifferenceForPeriod { get; set; }
    }
    public class PayrollArrearMonthlyBreakup
    {
        public int PayConfigID { get; set; }
        public string MonthOfArrear { get; set; }
        public decimal MonthlyDifference { get; set; }
    }
    public class SalaryCalculationResult
    {
        public List<outPutJson> ManualValues { get; set; }
        public List<outPutJson> AllowanceValues { get; set; }
        public List<outPutJson> GrossValues { get; set; }
        public List<outPutJson> DeductionValues { get; set; }
        public List<outPutJson> NetValues { get; set; }
        public List<outPutJson> CompanyContributions { get; set; }
        public List<outPutJson> CompanyContributionsBreakup { get; set; }
        public List<outPutJson> TotalContributionValues { get; set; }
        public List<outPutJson> MonthlyCTCValues { get; set; }
        public List<outPutJson> YearlyCTCValues { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal MonthlyCTC { get; set; }
        public decimal YearlyCTC { get; set; }
        public int PayGroupId{ get; set; }
    }
     
    public class TDSBreakupRow
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TDSAmount { get; set; }
        public bool IsPaid { get; set; }
    }

    public class PreviousTDSInfo
    {
        public decimal PreviousGrossIncome { get; set; }
        public decimal TotalExemptions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal AnnualTaxBeforeCess { get; set; }
        public decimal CessAmount { get; set; }
        public decimal TotalAnnualTax { get; set; }
        public string FrequencyCode { get; set; }
        public int NoOfPeriods { get; set; }
        public List<TDSBreakupRow> TDSBreakups { get; set; } = new List<TDSBreakupRow>();
    }
    public class DeductionRuleModel
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string FinancialYear { get; set; }
        public string SettingName { get; set; }
        public string LowerLimit { get; set; }
        public string UpperLimit { get; set; }
        public string Rate { get; set; }
        public string SettingType { get; set; }
        public string CalculatedOn { get; set; }
        public string EffectiveFrom { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
        public string RoundOffType { get; set; }
        public string CreatedBy { get; set; }
    }
    public class TdsTaxSlab
    {
        public int Regimetypeid { get; set; }
        public string RegimetypeName { get; set; }
        public string FinancialYear { get; set; }
        public string Fromincome { get; set; }
        public string Toincome { get; set; }
        public string Tdsrate { get; set; }
        public string AssessmentYear { get; set; }
    }
     
    public class clsTdsSetup
    {
        public int RegimeId { get; set; }
        public string RegimeName { get; set; }
        public string FinancialYear { get; set; }
        public string AssessmentYear { get; set; }
        public string SectionCode { get; set; }
        public string DeductionName { get; set; }
        public decimal? MaxLimit { get; set; }
        public string DeductionType { get; set; }
        public string IsLinked { get; set; } // "Yes" or "No"
        public string AutoCalculated { get; set; } // "Yes" or "No"
        public string PayConfigName { get; set; }
        public string ProofRequired { get; set; } // "Yes" or "No"
        public decimal? StandardDeduction { get; set; }
        public decimal? RebateAmount { get; set; }
        public string CessRate { get; set; }
    }
    public class EmployeeSalaryData
    {
        public string EmpNo { get; set; }
        public decimal IncrementPercentage { get; set; }
        public int ArrearMonths { get; set; }
        public decimal OldBasic { get; set; }      // from DB
        public decimal OldGross { get; set; }      // from DB

        public decimal NewBasic { get; set; }      // calculated
        public decimal NewGross { get; set; }

        public decimal BasicValue { get; set; }
        public decimal GrossValue { get; set; }
        public string EmployeeName { get; set; }
        public string IncrementType { get; set; } // "PERCENTAGE" or "FIXED"
    }
    public class SalaryCTCInput
    {
        public string EmpId { get; set; }
        public string PayGroupId { get; set; }
        public string FinancialYear { get; set; }
        public List<ManualValue> NewManualValues { get; set; }
        public List<CompanyContributions> CompanyContributions { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string ArrearMonth { get; set; }
        public bool DeductPTAX { get; set; }
        public bool DeductPF { get; set; }
        public bool DeductESIC { get; set; }
        public bool IsArrear { get; set; }
    }

    public class PreApprovalArrearSummaryModel
    {
        public string FinancialYear { get; set; }
        public string ArrearType { get; set; }

        public int EmpID { get; set; }
        public string EmployeeName { get; set; }

        // OLD Salary
        public decimal? OldBasic { get; set; }
        public decimal? OldPF { get; set; }
        public decimal? OldGross { get; set; }

        // NEW Salary
        public decimal? NewBasic { get; set; }
        public decimal? NewPF { get; set; }
        public decimal? NewGross { get; set; }

        
        // Arrear Info
        public string ArrearFrom { get; set; }
        public string ArrearTo { get; set; }
        public string ArrearMonth { get; set; } // e.g. "July, 2025"
        public int TotalMonths { get; set; }

        // Differences
        public decimal? BasicDifference { get; set; }
        public decimal? PFDifference { get; set; }

        public decimal? GrossDifference { get; set; }
        public decimal? NetDifference { get; set; }
        public decimal? TotalGrossArrear { get; set; }
        public decimal? TotalNetArrear { get; set; }
    }


    public class ExcelRowData
    {
        public int EmployeeNo { get; set; }
        public decimal BasicPercent { get; set; }
        public decimal GrossPercent { get; set; }
        public decimal BasicAmt { get; set; }
        public decimal GrossAmt { get; set; }
    }
    public class PayIncrementConfig
    {
        public int PayConfigId { get; set; }
        public bool IsForBasic { get; set; }
        public bool IsForGross { get; set; }
        public string IncrementType { get; set; } // 'PERCENTAGE' or 'FIX'
    }
    public class EmpSalaryConfigModel
    {
        public long EmployeeID { get; set; }
        public long PayGroupID { get; set; }
        public long EmpSalaryConfigID { get; set; }
        public bool IsPF { get; set; }
        public bool IsESIC { get; set; }
        public bool IsPTAX { get; set; }

        public decimal MonthlyGross { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal TotalContribution { get; set; }
        public decimal NetPay { get; set; }
        public decimal MonthlyCTC { get; set; }
        public decimal AnnualCTC { get; set; }

        public List<EmpSalaryDetailModel> DetailList { get; set; } = new List<EmpSalaryDetailModel>();
    }


    public class EmpSalaryDetailModel
    {
        public long PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayType { get; set; }

        public decimal MonthlyAmount { get; set; }
        public decimal YearlyAmount { get; set; }

        public string TimeBasis { get; set; }
        public string LogicType { get; set; }
        public string MappedColumn { get; set; }
        public string CalculationFormula { get; set; }

        public decimal ManualRate { get; set; }
        public bool ISPercentage { get; set; }

        // === Component Flags ===
        public bool IsBasicComponent { get; set; }
        public bool IsGrossComponent { get; set; }
        public bool IsStatutory { get; set; }
        public string StatutoryType { get; set; }
        public bool IsOther { get; set; }
        public string OtherType { get; set; }

        // === Statutory Settings ===
        public decimal? MaxLimit { get; set; }
        public string RoundingType { get; set; }

        // === PTAX Slabs ===
        public string PTaxSlabsJson { get; set; }  // store JSON string
    }

}
