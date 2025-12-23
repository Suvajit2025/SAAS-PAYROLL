using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class SalaryConfigureModel
    {
        public SalaryConfigureModel()
        {
            SalaryConfigureList = new List<SalaryConfigureModel>();
        }
        public int SalaryConfigureID { get; set; }
        public string SalaryConfigureName { get; set; }
        public int? PayGroupID { get; set; }
        public string PayGroupName { get; set; }
        public int? SalaryConfigureType { get; set; }
        public List<SalaryConfigureModel> SalaryConfigureList { get; set; }
        public string TenantID { get; set; }
        public string EntryUser { get; set; } = string.Empty;
        public int ActiveFlag { get; set; }
    }
}
