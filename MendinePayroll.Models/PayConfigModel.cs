using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class PayConfigModel
    {
        public PayConfigModel()
        {
            PayConfigList = new List<PayConfigModel>();
        }
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public bool? IScalculative { get; set; }
        public bool? IsStatutory { get; set; }
        public string EntryType { get; set; }
        public List<PayConfigModel> PayConfigList { get; set; }
        public string TenantID { get; set; }
        public string EntryUser { get; set; }
    }

    public class PayComponentModel
    {
        public int PayConfigId { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public int IsCalculative { get; set; }
        public int SortOrder { get; set; }

        public bool IsBasicComponent { get; set; }
        public bool IsGrossComponent { get; set; }
        public bool IsVariableAmt { get; set; }
        public bool IsLoanComponent { get; set; }

        public bool IsStatutory { get; set; }
        public string StatutoryType { get; set; }

        public bool IsOther { get; set; }
        public string OtherType { get; set; }
        public bool IsAllowance { get; set; }
        public string AllowanceType { get; set; }
    }


    public class SalaryConfigModel
    {
        // -------- HEADER (SalaryConfigure) --------
        public int SalaryConfigureID { get; set; } = 0;
        public string SalaryConfigureName { get; set; }
        public int SalaryConfigureType { get; set; }
        public int PayGroupID { get; set; }

        // -------- DETAILS (Each component) --------
        public string ComponentListJson { get; set; } 
    }

     

}
