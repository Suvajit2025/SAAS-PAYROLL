using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Models
{
    public class EmployeeSalaryConfigModel
    {
        public int Id { get; set; }
        public int? EmpId { get; set; }
        public int? PayGroupId { get; set; }
        public int? PayConfigID { get; set; }
        public string Values { get; set; }
        public string PayConfigName { get; set; }
        public string PayConfigType { get; set; }
        public bool? IsPTaxActive { get; set; }
        public double? OvertimeAllowance { get; set; }
        public string ContinuousAllowance { get; set; }
        public string ProductionBonus { get; set; }
        public int? SalaryConfigureType { get; set; }
        public int? SalaryConfigureID { get; set; }
        public bool? IsCalculative { get; set; }
    }
}
