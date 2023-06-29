using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class ManualSalaryConfigModel
    {
        public int ID { get; set; }
        public int? PayGroupID { get; set; }
        public int? ManualPayConfigId { get; set; }
        public bool? ISActive { get; set; }
        public string PayConfigName { get; set; }
    }
}
