using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MendinePayroll.Models
{
    public class EmployeeBonusModel
    {
        public int ID { get; set; }
        public int? Empid { get; set; }
        public string sEmpid { get; set; }
        public int? PayGroupId { get; set; }
        public string Month { get; set; }
        public int? Year { get; set; }
        public double? OverTimeHours { get; set; }
        public double? ProductionBonus { get; set; }
        public double? TargetAchieved { get; set; }
        public List<SelectListItem> MonthList { get; set; }
        public List<SelectListItem> YearList { get; set; }
        public List<EmployeeBonusModel> EmployeeBonusList { get; set; }
        public EmployeeBonusModel()
        {
            EmployeeBonusList = new List<EmployeeBonusModel>();
        }
    }
}
