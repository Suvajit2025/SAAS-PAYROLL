using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class TaxRuleModel
    {
        public int TaxRuleId { get; set; }
        public Guid TenantId { get; set; }

        /* ===============================
           Context
           =============================== */

        public string FinancialYear { get; set; }   // e.g. 2025-26
        public int RegimeId { get; set; }            // OLD / NEW
        public int SectionId { get; set; }           // FK → Payroll_SaaS_TaxSection

        /* ===============================
           Rule Definition
           =============================== */
        public string RuleName { get; set; }

        public string RuleCode { get; set; }        // Optional internal code

        /* ===============================
           Mapping Logic
           =============================== */
        public string MappingType { get; set; }      // MANUAL / PAYROLL / STATUTORY
        public int? LinkedPayComponentId { get; set; }
        public string StatutoryCode { get; set; }   // PF_EE / PTAX / ESIC_EE

        /* ===============================
           Calculation Rules
           =============================== */
        public decimal? MaxLimitAmount { get; set; }
        public decimal? PercentageOfAmount { get; set; }

        /* ===============================
           Behaviour Flags
           =============================== */
        public bool IsAutoCalculated { get; set; } = true;
        public bool IsProofRequired { get; set; } = false;
        public bool AllowEmployeeOverride { get; set; } = false;
        public bool IsActive { get; set; } = true;

        /* ===============================
           Audit
           =============================== */

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public int? CreatedBy { get; set; }
    }
    public class TaxRuleConfigModel
    {
        public int TaxRuleId { get; set; }   

        public string FinancialYear { get; set; }
        public int RegimeId { get; set; }
        public int SectionId { get; set; }

        public string RuleName { get; set; }
        public string RuleCode { get; set; }

        public string MappingType { get; set; }     // MANUAL / PAYROLL / STATUTORY
        public int? LinkedPayComponentId { get; set; }
        public string StatutoryCode { get; set; }

        public decimal? MaxLimitAmount { get; set; }
        public decimal? PercentageOfAmount { get; set; }

        public bool IsAutoCalculated { get; set; }
        public bool IsProofRequired { get; set; }
        public bool IsPercentageBased { get; set; }
        public bool AllowEmployeeOverride { get; set; } 
        public string CalculationFormula { get; set; }
        }

    public class TaxMasterSaveModel
    {
        public string FinancialYear { get; set; }
        public int RegimeId { get; set; }

        // Constants
        public decimal StandardDeduction { get; set; }
        public decimal RebateLimit87A { get; set; }
        public decimal RebateIncomeThreshold { get; set; }
        public decimal HealthEduCessRate { get; set; }

        // Slabs
        public List<TaxSlabItem> Slabs { get; set; }
    }

    public class TaxSlabItem
    {
        public decimal FromAmount { get; set; }
        public decimal? ToAmount { get; set; } // Nullable for the 'Above' bracket
        public decimal TaxRate { get; set; }
        public string Description { get; set; }
    }
}
