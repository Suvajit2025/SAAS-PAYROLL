using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Utility;
using System.Data;
using MendinePayroll.Models;

namespace MendinePayroll.UI.BLL
{
    public class clsPTaxSlab
    {
        //public static List<clsPTaxSlabInfo> PTax_List(long IDState, long Year )
        //{
        //    List<clsPTaxSlabInfo> mlist = new List<clsPTaxSlabInfo>();
        //    DataTable DT = clsDatabase.fnDataTable("PRC_SelectIDState", IDState, Year);
        //    foreach (DataRow  DR in DT.Rows)
        //    {
        //        clsPTaxSlabInfo obj = new clsPTaxSlabInfo();
        //        //obj.IDSlab = clsHelper.fnConvert2Long(DR["IDSlab"]);
        //        obj.IDState = clsHelper.fnConvert2Long(DR["IDState"]);
        //        obj.Year = clsHelper.fnConvert2Long(DR["TaxYear"]);
        //        obj.RangeFrom = clsHelper.fnConvert2Decimal(DR["RStart"]);
        //        obj.RangeToEnd = clsHelper.fnConvert2Decimal(DR["RangeEnd"]);
        //        obj.Amount = clsHelper.fnConvert2Decimal(DR["Amt"]);
        //        mlist.Add(obj);
        //    }

        //    return mlist;
        //}
        public static String Ptax_Save(List<clsPTaxSlabInfo> mList)
        {
            DataTable DT = SaveTable();
            foreach (var item in mList )
            {
                DataRow DR = DT.NewRow();
                DR["IDState"] = clsHelper.fnConvert2Long(item.IDState);
                DR["TaxYear"] = clsHelper.fnConvert2Long(item.Year);
                DR["RangeFrom"] = clsHelper.fnConvert2Decimal(item.RangeFrom);
                DR["RangeToEnd"] = clsHelper.fnConvert2Decimal(item.RangeToEnd);
                DR["Amount"] = clsHelper.fnConvert2Decimal(item.Amount);
                DT.Rows.Add(DR);
            }
            return clsDatabase.fnDBOperation("PRC_PTaxt_Save", DT);
        }

        private static DataTable  SaveTable()
        {
            DataTable DT = new DataTable("Data");
            DT.Columns.Add("IDState", typeof(System.Int32));
            DT.Columns.Add("TaxYear", typeof(System.Int32));
            DT.Columns.Add("RangeFrom", typeof(System.Decimal));
            DT.Columns.Add("RangeToEnd", typeof(System.Decimal));
            DT.Columns.Add("Amount", typeof(System.Decimal ));
            return DT;

        }
        public static List<clsPTaxSlabInfo> PTax_ListDetails(long IDState, long Year)
        {
            List<clsPTaxSlabInfo> data = new List<clsPTaxSlabInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_SelectIDState_NEW", IDState, Year);
            foreach (DataRow DR in DT.Rows)
            {
                data.Add(new clsPTaxSlabInfo
                {
                    //IDSlab = clsHelper.fnConvert2Int(DR["IDSlab"]),
                    IDState = clsHelper.fnConvert2Int(DR["IDState"]),
                    Year = clsHelper.fnConvert2Int(DR["TaxYear"]),
                    RangeFrom = clsHelper.fnConvert2Decimal(DR["RStart"]),
                    RangeToEnd = clsHelper.fnConvert2Decimal(DR["RangeEnd"]),
                    Amount = clsHelper.fnConvert2Decimal(DR["Amt"]),

            });
            }
            return data;

        }

        public static double Employee_PTax_Value(double GrossValue)
        {
            double Result = 0;
            Result = clsHelper.fnConvert2Double(clsDatabase.fnDBOperation("Payroll_PRC_PTax_Value", GrossValue, 36));
            return Result;
        }
    }
    public class clsPTaxSlabInfo
    {
        public long IDSlab { get; set; } 
        public long IDState { get; set; } 
        public long Year { get; set; }
        public decimal RangeFrom { get; set; } 
        public decimal RangeToEnd { get; set; }
        public decimal Amount { get; set; } 

    }
}