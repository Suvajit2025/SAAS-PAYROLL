using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
   //public class ConfigureSalaryComponentModel
   // {
   //     public ConfigureSalaryComponentModel()
   //     {
   //         ConfigureSalaryComponentList = new List<ConfigureSalaryComponentModel>();
            
   //     }

   //     public int ConfigureSalaryID { get; set; }
   //     public int PayConfigId { get; set; }
   //     public string CalculationFormula { get; set; }
   //     public double? ManualRate { get; set; }

   //     public string PayConfigType { get; set; }
   //     public int? MasterPayConfigID { get; set; }
   //     public bool? ISPercentage { get; set; }
   //     public string PayConfigName { get; set; }
   //     public bool? IScalculative { get; set; }

   //     public List<ConfigureSalaryComponentModel> ConfigureSalaryComponentList { get; set; }
   // }

    public class ConfigureSalaryComponentModel
    {
        public ConfigureSalaryComponentModel()
        {
            ConfigureSalaryComponentList = new List<ConfigureSalaryComponentModel>();
        }

        public int ConfigureSalaryID { get; set; }
        public int SalaryConfigureID { get; set; }
        public int PayConfigId { get; set; }

        public string CalculationFormula { get; set; }
        public double? ManualRate { get; set; }

        public int? MasterPayConfigID { get; set; }
        public bool? ISPercentage { get; set; }

        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public bool? IScalculative { get; set; }

        public string TenantID { get; set; }

        public string EntryUser { get; set; } = string.Empty;

        public int ActiveFlag { get; set; }
        public string PayType { get; set; }

        public List<ConfigureSalaryComponentModel> ConfigureSalaryComponentList { get; set; }
    }
    public class SalaryConfigureVM
    {
        public SalaryConfigureModel Head { get; set; }
        public List<ConfigureSalaryComponentModel> Detail { get; set; }
    }

}
