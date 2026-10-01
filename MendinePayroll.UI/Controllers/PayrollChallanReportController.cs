using ClosedXML.Excel;
using Common.Utility;
using DocumentFormat.OpenXml.Office2010.Excel;
using ExcelDataReader;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.html;
using MendinePayroll.Models;
using MendinePayroll.UI.BLL;
using MendinePayroll.UI.Models;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using UI.BLL;

namespace MendinePayroll.UI.Controllers
{
    public class PayrollChallanReportController : Controller
    {
        // GET: PayrollChallanReport
        //Saikat Mondal Start 02/02/2026 
        private string TenantId
        {
            get
            {
                try
                {
                    if (Session["TenantID"] == null)
                    {
                        // Redirect to login or handle gracefully

                        throw new Exception("Session expired. Please log in again.");
                    }
                    return Session["TenantID"].ToString();
                }
                catch (Exception ex)
                {
                    // Log the error (you can use your existing logging mechanism)
                    System.Diagnostics.Debug.WriteLine("TenantID retrieval error: " + ex.Message);
                    throw; // Let it bubble to global error handler if configured
                }
            }
        }

        //Saikat Mondal End 02/02/2026 

        // GET: EmployeeList
       
        static int Month = DateTime.Now.AddMonths(-1).Month;
        static int Year = DateTime.Now.AddMonths(-1).Year;
        static int days = DateTime.DaysInMonth(Year, Convert.ToInt32(Month));
        static DateTime first = new DateTime(Year, Convert.ToInt32(Month), 1);
        DateTime last = first.AddMonths(1).AddSeconds(-1);

        public ActionResult EpfEcrChallanReport()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            //#region bind Employee
            //EmployeeListModel employeeListModel = new EmployeeListModel();
            //List<SelectListItem> EmpList = new List<SelectListItem>();
            //EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            //employeeListModel.empid = 0;
            //List<EmployeeListModel> employeedata = GetAllEmployee();
            
            //foreach (var items in employeedata)
            //{
            //    EmpList.Add(new SelectListItem
            //    {
            //        Text = items.EmployeeName,
            //        Value = items.empid.ToString()
            //    });
            //}
            //#endregion
            //employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.MonthList = GetMonthList();
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);
        }

        public List<EmployeeListModel> GetAllEmployee()
        {
            //Saikat Mondal start
            List<EmployeeListModel> list = new List<EmployeeListModel>();
            DataTable dt = clsDatabase.fnDataTable("EmployeeList_Get", TenantId);

            if (dt != null && dt.Rows.Count > 0)
            {
                list = (from DataRow row in dt.Rows
                        select new EmployeeListModel
                        {
                            empid = Convert.ToInt32(row["empid"]),
                            empno = Convert.ToInt32(row["empno"]),
                            EmployeeName = row["EmployeeName"].ToString(),
                            empcode = row["empcode"].ToString(),
                            postname = row["postname"].ToString(),
                            Designation = row["Designation"].ToString(),
                            Department = row["Department"].ToString(),
                            //PayGroupName = row["PayGroupName"] == DBNull.Value ? null : row["PayGroupName"].ToString()
                        }).ToList();
            }

            return list;
            // Saikat Mondal  End
            //employeeListModel.empid = 0;
            //HttpResponseMessage response = ObjAPI.CallAPI("Salary/GetEmployeeList", contents);
            //if (response.IsSuccessStatusCode)
            //{
            //    string responseString = response.Content.ReadAsStringAsync().Result;

            //    if (!string.IsNullOrEmpty(responseString))
            //    {
            //        employeeListModel.emplist = JsonConvert.DeserializeObject<List<EmployeeListModel>>(responseString);
            //    }
            //}
            // return employeeListModel.emplist;
        }
        public List<SelectListItem> GetMonthList()
        {
            List<SelectListItem> MonthList = new List<SelectListItem>();
            var months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            DateTimeFormatInfo info = DateTimeFormatInfo.GetInstance(null);
            for (int i = 1; i < 13; i++)
            {
                MonthList.Add(new SelectListItem
                {
                    Text = info.GetMonthName(i),
                    Value = i.ToString(),
                    Selected = (i == DateTime.Now.AddMonths(-1).Month ? true : false)
                });

            }
            return MonthList;
        }
        public List<SelectListItem> GetYearList()
        {
            List<SelectListItem> YearList = new List<SelectListItem>();
            int year = DateTime.Now.Year;
            for (int j = year - 20; j < year + 20; j++)
            {
                YearList.Add(new SelectListItem
                {
                    Text = j.ToString(),
                    Value = j.ToString(),
                    Selected = (j == DateTime.Now.AddMonths(-1).Year ? true : false)
                });

            }
            return YearList;
        }

        [HttpPost]
        public JsonResult GetAllEmployeePFChallan(int Month, int Year, int? empParam, string Status, int numberofRow, int pagenumber)
        {
            int? Empid = (empParam == 0 ? (int?)null : empParam);
            int rowsToFetch = numberofRow > 0 ? numberofRow : 9999;
            int page = pagenumber > 0 ? pagenumber : 1;

            // Call SP
            var dt = clsDatabase.fnDataTable("SP_GetEmployeePF_ECR_Report", Month, Year, Empid, Status ?? "1", rowsToFetch, page, TenantId);

            // Convert DataTable → List<Dictionary<string, object>>
            var rows = new List<Dictionary<string, object>>();
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = dr[col];
                }
                rows.Add(dict);
            }

            return Json(rows, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ExportPFChallanToCSV(int Month, int Year, int? empParam)
        {
            int? Empid = (empParam == 0 ? (int?)null : empParam);

            // Fetch data from SP
            //var dt = clsDatabase.fnDataTable("SP_GetEmployeePF_ECR_Report", Month, Year, Empid, "1", 9999, 1);
            var dt = clsDatabase.fnDataTable("SP_GetEmployeePF_ECR_Report", Month, Year, Empid, "1", 9999, 1, TenantId);
            // Remove TotalCount column if exists
            if (dt.Columns.Contains("TotalCount"))
                dt.Columns.Remove("TotalCount");
            // Build CSV content
            var csv = new StringBuilder();

            // Header row
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                csv.Append(dt.Columns[i].ColumnName);
                if (i < dt.Columns.Count - 1)
                    csv.Append(",");
            }
            csv.AppendLine();

            // Data rows
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    csv.Append(row[i].ToString().Replace(",", " ")); // avoid breaking commas
                    if (i < dt.Columns.Count - 1)
                        csv.Append(",");
                }
                csv.AppendLine();
            }

            // File name format: EPF_ECR_Month_Year.csv
            string fileName = $"EPF_ECR_{Month}_{Year}.csv";
            string folderPath = Server.MapPath("~/Downloads/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

            return Json(fileName, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadCsv(string fileName)
        {
            string fullPath = Server.MapPath("~/Downloads/" + fileName);
            if (!System.IO.File.Exists(fullPath))
                return HttpNotFound();

            return File(fullPath, "text/csv", fileName);
        }


        private string NormalizeMultiSelect(List<string> items)
        {
            if (items == null || items.Count == 0)
                return "";

            // Remove "multiselect-all"
            items = items
                .Where(x => !x.Equals("multiselect-all", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If after removing "multiselect-all" nothing left → treat as All selected
            if (items.Count == 0)
                return "";

            return string.Join(",", items);
        }

        public JsonResult GetEmployeeList(List<string> companyIds, List<string> deptIds, List<string> categoryIds)
        {
            if (string.IsNullOrEmpty(TenantId))
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            DataTable dt = clsDatabase.fnDataTable("EmployeeList_Get", TenantId);
            if (dt == null || dt.Rows.Count == 0)
            {
                dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEE_LIST", TenantId);
            }

            if (dt == null || dt.Rows.Count == 0)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var list = dt.AsEnumerable().Select(r => new
            {
                EmpID = r.Table.Columns.Contains("empid") ? r["empid"]?.ToString() : r["EmpID"]?.ToString(),
                EmpName = (r.Table.Columns.Contains("EmployeeName") ? r["EmployeeName"]?.ToString() : (r["empfirstname"]?.ToString() + " " + r["emplastname"]?.ToString())).Trim(),
                EmpCode = r.Table.Columns.Contains("empcode") ? r["empcode"]?.ToString() : r["EmpCode"]?.ToString(),
                Department = r.Table.Columns.Contains("Department") ? r["Department"]?.ToString() : "",
                Empstatus = "ACTIVE"
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ExportPFChallanToExcel(int Month, int Year, string empIds = null)
        {
            var dt = clsDatabase.fnDataTable("SP_GetEmployeePF_ECR_Report", Month, Year, (int?)null, "1", 9999, 1, TenantId);

            if (dt == null || dt.Rows.Count == 0)
                return Json("NODATA", JsonRequestBehavior.AllowGet);

            DataTable exportDt = new DataTable("EPF_ECR");
            exportDt.Columns.Add("UAN NO", typeof(string));
            exportDt.Columns.Add("MEMBER NAME", typeof(string));
            exportDt.Columns.Add("GROSS WAGES", typeof(decimal));
            exportDt.Columns.Add("EPF WAGES", typeof(decimal));
            exportDt.Columns.Add("EPS WAGES", typeof(decimal));
            exportDt.Columns.Add("EDLI WAGES", typeof(decimal));
            exportDt.Columns.Add("EPF CONTRI REMITTED", typeof(decimal));
            exportDt.Columns.Add("EPS CONTRI REMITTED", typeof(decimal));
            exportDt.Columns.Add("EPF EPS DIFF REMITTED", typeof(decimal));
            exportDt.Columns.Add("NCP DAYS", typeof(int));
            exportDt.Columns.Add("ADVANCES", typeof(decimal));

            HashSet<string> empIdSet = null;
            if (!string.IsNullOrWhiteSpace(empIds))
            {
                empIdSet = new HashSet<string>(empIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
            }

            foreach (DataRow row in dt.Rows)
            {
                string rowEmpId = row["EmpId"]?.ToString();
                if (empIdSet != null && !empIdSet.Contains(rowEmpId))
                {
                    continue;
                }

                exportDt.Rows.Add(
                    row["UAN"] != DBNull.Value ? row["UAN"].ToString() : "",
                    row["EmployeeName"] != DBNull.Value ? row["EmployeeName"].ToString() : "",
                    row["GROSS_WAGES"] != DBNull.Value ? Convert.ToDecimal(row["GROSS_WAGES"]) : 0m,
                    row["EPFWages"] != DBNull.Value ? Convert.ToDecimal(row["EPFWages"]) : 0m,
                    row["EPSWages"] != DBNull.Value ? Convert.ToDecimal(row["EPSWages"]) : 0m,
                    row["EDLIWages"] != DBNull.Value ? Convert.ToDecimal(row["EDLIWages"]) : 0m,
                    row["EPFRemitted"] != DBNull.Value ? Convert.ToDecimal(row["EPFRemitted"]) : 0m,
                    row["EPSRemitted"] != DBNull.Value ? Convert.ToDecimal(row["EPSRemitted"]) : 0m,
                    row["EPFEPSDIFF"] != DBNull.Value ? Convert.ToDecimal(row["EPFEPSDIFF"]) : 0m,
                    row["NCPDays"] != DBNull.Value ? Convert.ToInt32(row["NCPDays"]) : 0,
                    row["ADVANCES"] != DBNull.Value ? Convert.ToDecimal(row["ADVANCES"]) : 0m
                );
            }

            if (exportDt.Rows.Count == 0)
                return Json("NODATA", JsonRequestBehavior.AllowGet);

            string fileName = $"EPF_ECR_{Month}_{Year}.xlsx";
            string folderPath = Server.MapPath("~/Downloads/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);

            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet = wb.Worksheets.Add(exportDt, "EPF_ECR");
                sheet.Table("Table1").ShowAutoFilter = false;
                sheet.Table("Table1").Theme = XLTableTheme.None;
                sheet.Columns().AdjustToContents();
                wb.SaveAs(filePath);
            }

            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadExcel(string fileName)
        {
            string fullPath = Path.Combine(Server.MapPath("~/Downloads/"), fileName);
            if (!System.IO.File.Exists(fullPath))
                return HttpNotFound();

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            System.IO.File.Delete(fullPath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


    }
}