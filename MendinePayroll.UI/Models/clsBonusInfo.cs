using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MendinePayroll.UI.Models
{
    public class clsBonusInfo
    {
        public long IDBonus{ get; set; } = 0;
        public long IDEmployee { get; set; } = 0;
        public String BonusDate { get; set; } = "";
        public String BonusMonth { get; set; } = "";
        public int BonusYear { get; set; } = 0;
        public Decimal Amount { get; set; } = 0;
        public String EntryUser { get; set; } = "";

    }
}