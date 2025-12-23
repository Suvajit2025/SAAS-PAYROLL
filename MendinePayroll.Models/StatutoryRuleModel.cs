using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class StatutoryRuleModel
    {
        // ==========================
        // IDENTIFIERS
        // ==========================
        public long? StatutoryRuleId { get; set; } = 0;   // NULL = Add, >0 = Edit
        public int PayConfigId { get; set; }
        public string StatutoryType { get; set; }    // PF_EE, PF_ER, ESIC_EE, LWF, PTAX

        // ==========================
        // COMMON
        // ==========================
        public DateTime EffectiveDate { get; set; }

        // ==========================
        // PF / ESIC
        // ==========================
        public decimal? MaxLimit { get; set; }        // PF, ESIC only
        public string RoundingType { get; set; }      // Round / Floor / Ceil / No Rounding

        // ==========================
        // LWF
        // ==========================
        public string LWFMonth { get; set; }          // January / June / December
        public decimal? LWFAmount { get; set; }       // LWF amount

        // ==========================
        // PTAX
        // ==========================
        public int? StateId { get; set; }

        // Will carry JSON string to SP
        public string SlabJson { get; set; }

        // Slab list coming from UI for PTAX
        public List<PTaxSlabModel> Slabs { get; set; }
    }


    public class PTaxSlabModel
    {
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal Value { get; set; }
    }

}
