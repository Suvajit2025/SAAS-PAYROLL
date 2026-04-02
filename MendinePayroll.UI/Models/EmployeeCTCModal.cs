using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class EmployeeCTCModal
    {
    }
    public class EmployeeCTCCalculateRequest
    {
        public long EmployeeID { get; set; }
        public long PayGroupID { get; set; }

        public bool IsPF { get; set; }
        public bool IsESIC { get; set; }
        public bool IsPTAX { get; set; }

        public long EmpSalaryConfigID { get; set; }

        public List<EmployeeCTCInputValue> Inputs { get; set; } = new List<EmployeeCTCInputValue>();
    }

    public class EmployeeCTCInputValue
    {
        public long PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public decimal MonthlyAmount { get; set; }
    }
    public class EmployeeCTCCalculateResponse
    {
        public EmployeeCTCHeaderResult Header { get; set; } = new EmployeeCTCHeaderResult();
        public List<EmployeeCTCDetailResult> DetailList { get; set; } = new List<EmployeeCTCDetailResult>();
    }

    public class EmployeeCTCHeaderResult
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
    }

    public class EmployeeCTCDetailResult
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

        public bool IsBasicComponent { get; set; }
        public bool IsGrossComponent { get; set; }
        public bool IsStatutory { get; set; }
        public string StatutoryType { get; set; }

        public bool IsOther { get; set; }
        public string OtherType { get; set; }

        public decimal? MaxLimit { get; set; }
        public string RoundingType { get; set; }
        public string PTaxSlabsJson { get; set; }
    }
    public class SalaryComponentConfig
    {
        public long PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayType { get; set; }

        public string LogicType { get; set; }
        public string TimeBasis { get; set; }

        public decimal ManualRate { get; set; }
        public bool ISPercentage { get; set; }

        public bool IsBasicComponent { get; set; }
        public bool IsGrossComponent { get; set; }
        public bool IsLoanComponent { get; set; }
        public bool IsCalculative { get; set; }
        public bool IsVariableAmt { get; set; }

        public bool IsStatutory { get; set; }
        public string StatutoryType { get; set; }

        public decimal? MaxLimit { get; set; }
        public string RoundingType { get; set; }

        public bool IsOther { get; set; }
        public string OtherType { get; set; }

        public string MappedColumn { get; set; }
        public string CalculationFormula { get; set; }

        public bool IsInput { get; set; }
        public bool IsManualGrid { get; set; }

        public List<PTaxSlabModelCTC> PTaxSlabs { get; set; } = new List<PTaxSlabModelCTC>();
    }

    public class PTaxSlabModelCTC
    {
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public decimal PTaxAmount { get; set; }
    }
}