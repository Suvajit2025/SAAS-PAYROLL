using Common.Utility;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{
    public class PayrollSalaryReportController : Controller
    {
        // GET: PayrollSalaryReport
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

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SalaryReport() { return View(); }

        [HttpGet]
        public JsonResult GetFilterData()
        { 

            DataSet ds = clsDatabase.fnDataSet("SP_Payroll_SalaryRegister_FilterData", TenantId);

            var companies = ds.Tables[0].AsEnumerable().Select(r => new {
                Value = r["Value"]?.ToString(),
                Text = r["Text"]?.ToString()
            }).ToList();

            var payGroups = ds.Tables[1].AsEnumerable().Select(r => new {
                Value = r["Value"]?.ToString(),
                Text = r["Text"]?.ToString()
            }).ToList();

            return Json(new { success = true, companies, payGroups }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetGrid(int month, int year, int? companyId, int? payGroupId, string search, int pageNumber, int pageSize)
        {

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 50;

            DataSet ds = clsDatabase.fnDataSet(
                "SP_Payroll_SalaryRegister_Grid",
                TenantId, month, year, companyId, payGroupId, search, null, pageNumber, pageSize
            );

            int totalRows = ds.Tables[0].Rows.Count > 0 ? Convert.ToInt32(ds.Tables[0].Rows[0]["TotalRows"]) : 0;

            var rows = ds.Tables[1].AsEnumerable().Select(r => new {
                PayrollProcessEmployeeID = r["PayrollProcessEmployeeID"],
                EmployeeID = Convert.ToInt32(r["EmployeeID"]).ToString(),
                EmployeeCode = r["EmployeeCode"]?.ToString(),
                EmployeeName = r["EmployeeName"]?.ToString() ?? "",
                CompanyName = r["CompanyName"]?.ToString() ?? "",
                PayGroupName = r["PayGroupName"]?.ToString() ?? "",
                BasicAmount = Convert.ToDecimal(r["BasicAmount"] == DBNull.Value ? 0 : r["BasicAmount"]),
                AllowancesAmount = Convert.ToDecimal(r["AllowancesAmount"] == DBNull.Value ? 0 : r["AllowancesAmount"]),
                TotalGross = Convert.ToDecimal(r["TotalGross"] == DBNull.Value ? 0 : r["TotalGross"]),
                TotalAddition = Convert.ToDecimal(r["TotalAddition"] == DBNull.Value ? 0 : r["TotalAddition"]),
                TotalDeduction = Convert.ToDecimal(r["TotalDeduction"] == DBNull.Value ? 0 : r["TotalDeduction"]),
                TotalTakeHome = Convert.ToDecimal(r["TotalTakeHome"] == DBNull.Value ? 0 : r["TotalTakeHome"]),
                Status = Convert.ToInt32(r["Status"] == DBNull.Value ? 0 : r["Status"]),
                PaidStatus = Convert.ToInt32(r["PaidStatus"] == DBNull.Value ? 0 : r["PaidStatus"])
            }).ToList();

            return Json(new { success = true, totalRows, rows }, JsonRequestBehavior.AllowGet);
        }
        // PayrollSalaryReportController.cs

        [HttpPost]
        public JsonResult MarkPaid(int month, int year, string paidDate, string payrollProcessEmployeeIds)
        {
            try
            {
                 
                var userName = Session["UserName"]?.ToString() ?? "";

                if (string.IsNullOrWhiteSpace(payrollProcessEmployeeIds))
                    return Json(new { success = false, message = "No employees selected." });

                if (!DateTime.TryParse(paidDate, out DateTime paidDt))
                    return Json(new { success = false, message = "Invalid paid date." });

                DataTable dt = clsDatabase.fnDataTable(
                    "SP_Payroll_SalaryRegister_MarkPaid",
                    TenantId,
                    month,
                    year,
                    paidDt.Date,
                    userName,
                    payrollProcessEmployeeIds // "29,30,31"
                );

                int affected = 0;
                if (dt != null && dt.Rows.Count > 0)
                    affected = Convert.ToInt32(dt.Rows[0]["Affected"]);

                return Json(new { success = true, affected });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}