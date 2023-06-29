using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Common.Utility;

namespace MendinePayroll.UI.BLL.Reports
{
    public class ClsAllEmpSalaryList
    {
        public static DataSet AllEmpSalaryList(string connection, string spname)
        {
            return clsDatabase.fnDataSet(connection, spname);
        }
    }
}