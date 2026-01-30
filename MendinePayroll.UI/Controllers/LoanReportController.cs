using Common.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{
    public class LoanReportController : Controller
    {
        // GET: LoanReport
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

        public ActionResult LoanLedgerReport()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetLoanLedgerReport()
        {
            DataTable dt = clsDatabase.fnDataTable("PRC_SaaS_Employee_LoanLedgerReport_REPORT", TenantId,DBNull.Value);

            var columns = dt.Columns.Cast<DataColumn>()
                            .Select(c => c.ColumnName)
                            .ToList();

            var rows = new List<Dictionary<string, object>>();

            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    row[col.ColumnName] = dr[col];
                rows.Add(row);
            }

            return Json(new
            {
                columns,
                rows
            }, JsonRequestBehavior.AllowGet);
        }
    }
}