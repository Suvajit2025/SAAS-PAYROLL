using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MendinePayroll.UI.Models;
using MendinePayroll.UI.BLL;
using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using System.IO;

namespace MendinePayroll.UI.Controllers
{
    public class SalaryReportsController : Controller
    {
        // GET: SalaryReports
        public ActionResult SalaryRegister()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpPost]
        public JsonResult ReportSalaryRegister(String EmployeeNo, String StartDate, String EndDate)
        {
            List<clsEmployeeSalaryRegisterInfo> Data = clsSalary.SalaryRegister(EmployeeNo, StartDate, EndDate);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult EmployeeList()
        {
            List<clsEmployeeInfo> Data = clsSalary.EmployeeList();
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportSalaryToExcel(String EmployeeNo, String StartDate, String EndDate)
        {
            List<clsEmployeeSalaryRegisterInfo> Data = clsSalary.SalaryRegister(EmployeeNo, StartDate, EndDate);
            ListtoDataTable lsttodt = new ListtoDataTable();
            DataTable dt = lsttodt.ToDataTable(Data);

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename= EmployeeSalaryReport.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
            return RedirectToAction("SalaryRegister", "SalaryReports");

        }

        public class ListtoDataTable
        {
            public DataTable ToDataTable<T>(List<T> items)
            {
                DataTable dataTable = new DataTable(typeof(T).Name);
                //Get all the properties by using reflection   
                PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo prop in Props)
                {
                    //Setting column names as Property names  
                    dataTable.Columns.Add(prop.Name);
                }
                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {

                        values[i] = Props[i].GetValue(item, null);
                    }
                    dataTable.Rows.Add(values);
                }

                return dataTable;
            }
        }


    }
}