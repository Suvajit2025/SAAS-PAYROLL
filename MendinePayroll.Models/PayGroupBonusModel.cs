using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MendinePayroll.Models
{
    public class PayGroupBonusModel
    {
        public PayGroupBonusModel()
        {
            PayGroupBonusList = new List<PayGroupBonusModel>();
        }
        public int ID { get; set; }
        public int PayGroupId { get; set; }
        public string Month { get; set; }
        public int? Year { get; set; }
        public double? OverTimeHours { get; set; }
        public double? ProductionBonus { get; set; }
        public double? TargetAchieved { get; set; }
        public List<PayGroupBonusModel> PayGroupBonusList { get; set; }
    }
}
