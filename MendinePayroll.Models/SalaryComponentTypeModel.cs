using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace MendinePayroll.Models
{
   public class SalaryComponentTypeModel
    {
        public SalaryComponentTypeModel()
        {
            SalaryComponentTypeList = new List<SalaryComponentTypeModel>();
            selectListItems = new List<SelectListItem>();
        }
        public int Id { get; set; }
        public string SalaryComponentType { get; set; }
        public List<SalaryComponentTypeModel> SalaryComponentTypeList { get; set; }
        public List<SelectListItem> selectListItems { get; set; }
    }
}
