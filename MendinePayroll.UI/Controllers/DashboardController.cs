using Common.Utility;
using MendinePayroll.UI.BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{
    
    public class DashboardController : Controller
    {

        public class PayrollDashboardVM
        {
           

            public List<SelectListItem> CompanyList { get; set; }
            public List<SelectListItem> DepartmentList { get; set; }

            public int? SelectedYear { get; set; }
            public int? SelectedMonth { get; set; }

            public List<SelectListItem> YearList { get; set; }
            public List<SelectListItem> MonthList { get; set; }
        }
        // GET: Dashboard
        private string TenantId
        {
            get
            {
                // 1️⃣ Background jobs
                //if (!string.IsNullOrEmpty(ManualTenantId))
                //    return ManualTenantId;

                // 2️⃣ Web request via querystring
                if (System.Web.HttpContext.Current?.Request?.QueryString["tenantId"] != null)
                    return System.Web.HttpContext.Current.Request.QueryString["tenantId"];

                // 3️⃣ Normal logged-in web session
                if (System.Web.HttpContext.Current?.Session?["TenantID"] != null)
                    return System.Web.HttpContext.Current.Session["TenantID"].ToString();

                throw new Exception("TenantID not found in Manual property, QueryString, or Session.");
            }
        }
        public ActionResult Index(int? year)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            int selectedYear = year ?? currentYear;

            var model = new PayrollDashboardVM
            {
                CompanyList = GetCompanies(),
                DepartmentList = GetDepartments(),
                YearList = GetYearList(),
                MonthList = GetMonthList(selectedYear),
                SelectedYear = selectedYear,
                SelectedMonth = currentMonth == 1 ? 12 : currentMonth - 1   // ✅ Current Month - 1
            };

            return View(model);
        }
        private List<SelectListItem> GetCompanies()
        {
            DataTable dt = clsDatabase.fnDataTable("Proc_Saas_CompanyMst", TenantId);

            var list = dt.AsEnumerable()
                .Select(r => new SelectListItem
                {
                    Value = r["CompanyID"].ToString(),
                    Text = r["CompanyName"].ToString()
                })
                .ToList();

            // Add All Company at top
            list.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "All Company"
            });

            return list;
        }
        private List<SelectListItem> GetDepartments()
        {
            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_Department", TenantId);

            var list = dt.AsEnumerable()
                .Select(r => new SelectListItem
                {
                    Value = r["DepartmentId"].ToString(),
                    Text = r["Department"].ToString()
                })
                .ToList();

            //  Add All Department at top
            list.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "All Department"
            });

            return list;
        }
        private List<SelectListItem> GetMonthList(int year)
        {
            var list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Value = "",
                Text = "All Month"
            });


            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            int maxMonth = 12;

            // If selected year is current year → restrict to current month -1
            if (year == currentYear)
            {
                maxMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            }

            for (int month = maxMonth; month >= 1; month--)
            {
                list.Add(new SelectListItem
                {
                    Value = month.ToString(),
                    Text = new DateTime(1, month, 1).ToString("MMMM")
                });
            }

            return list;
        }
        private List<SelectListItem> GetYearList()
        {
            var list = new List<SelectListItem>();

            int startYear = 2021;
            int endYear = DateTime.Now.Year;

            for (int year = endYear; year >= startYear; year--)
            {
                list.Add(new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                });
            }

            return list;
        }

        [HttpGet]
        public JsonResult GetPayrollSummary(string companyCode, string departmentId, int? year, int? month)
        {
            try
            {
                string tenantId = TenantId;

                DataSet dts = clsDatabase.fnDataSet(
                    "SP_GetPayrollSummary",
                    string.IsNullOrEmpty(companyCode) ? (object)DBNull.Value : Convert.ToInt32(companyCode),
                    string.IsNullOrEmpty(departmentId) ? (object)DBNull.Value : departmentId,
                    year ?? (object)DBNull.Value,
                    month ?? (object)DBNull.Value,
                    tenantId
                );

                //-------------------------------------------------------
                // 1 SUMMARY
                //-------------------------------------------------------
                decimal totalCTC = 0;
                decimal netSalary = 0;
                decimal employerContribution = 0;
                decimal totalRemainingLoan = 0;

                if (dts.Tables.Count > 0 && dts.Tables[0].Rows.Count > 0)
                {
                    var r = dts.Tables[0].Rows[0];

                    totalCTC = r["TotalCTC"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalCTC"]);
                    netSalary = r["TotalNetSalary"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalNetSalary"]);
                    employerContribution = r["TotalEmployerContribution"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalEmployerContribution"]);
                    totalRemainingLoan = r["TotalRemainingLoan"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalRemainingLoan"]);
                }

                var summary = new
                {
                    TotalCTC = totalCTC,
                    NetSalary = netSalary,
                    EmployerContribution = employerContribution,
                    TotalRemainingLoan = totalRemainingLoan
                };

                //-------------------------------------------------------
                // 2 DEPARTMENT LIST
                //-------------------------------------------------------
                var deptList = new List<object>();

                if (dts.Tables.Count > 1)
                {
                    deptList = dts.Tables[1].AsEnumerable()
                        .Select(r => new
                        {
                            DepartmentName = r["DepartmentName"].ToString(),
                            EstimatedCTC = r["EstimatedCTC"] == DBNull.Value ? 0 : Convert.ToDecimal(r["EstimatedCTC"]),
                            ActualCTC = r["ActualCTC"] == DBNull.Value ? 0 : Convert.ToDecimal(r["ActualCTC"]),
                            RemainingLoanAmount = r["RemainingLoanAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["RemainingLoanAmount"])
                        })
                        .ToList<object>();
                }

                //-------------------------------------------------------
                // 3 SALARY TREND
                //-------------------------------------------------------
                var trendList = new List<object>();

                if (dts.Tables.Count > 2)
                {
                    trendList = dts.Tables[2].AsEnumerable()
                        .Select(r => new
                        {
                            MonthName = r["MonthName"].ToString(),
                            TotalGross = r["TotalGross"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalGross"]),
                            TotalNet = r["TotalNet"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalNet"])
                        })
                        .ToList<object>();
                }

                //-------------------------------------------------------
                // 4 COMPONENT COMPARISON
                //-------------------------------------------------------
                var componentList = new List<object>();

                if (dts.Tables.Count > 3)
                {
                    componentList = dts.Tables[3].AsEnumerable()
                        .Select(r => new
                        {
                            MonthName = r["MonthName"].ToString(),
                            NetPayable = r["NetPayable"] == DBNull.Value ? 0 : Convert.ToDecimal(r["NetPayable"]),
                            Statutory = r["Statutory"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Statutory"]),
                            EmployerContribution = r["EmployerContribution"] == DBNull.Value ? 0 : Convert.ToDecimal(r["EmployerContribution"])
                        })
                        .ToList<object>();
                }

                //-------------------------------------------------------
                // 5 COMPANY DEPARTMENT SPEND
                //-------------------------------------------------------
                var companyDeptList = new List<object>();

                if (dts.Tables.Count > 4)
                {
                    companyDeptList = dts.Tables[4].AsEnumerable()
                        .Select(r => new
                        {
                            CompanyName = r["Company"].ToString(),
                            TotalSpend = r["TotalSpend"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalSpend"])
                        })
                        .ToList<object>();
                }

                //-------------------------------------------------------
                // 6 TOP LOANS
                //-------------------------------------------------------
                var topLoanList = new List<object>();

                if (dts.Tables.Count > 5)
                {
                    topLoanList = dts.Tables[5].AsEnumerable()
                        .Select(r => new
                        {
                            EmployeeName = r["EmployeeName"].ToString(),
                            EMIAmount = r["EMIAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["EMIAmount"]),
                            LoanAmount = r["LoanAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["LoanAmount"])
                        })
                        .ToList<object>();
                }

                //-------------------------------------------------------
                // FINAL RESPONSE
                //-------------------------------------------------------
                return Json(new
                {
                    summary,
                    deptList,
                    trendList,
                    componentList,
                    companyDeptList,
                    topLoanList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = true,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpGet]
        //public JsonResult GetPayrollSummary(string companyCode, string departmentId, int? year, int? month)
        //{
        //    try
        //    {
        //        string tenantId = TenantId;

        //        DataSet dts = clsDatabase.fnDataSet(
        //            "SP_GetPayrollSummary_Test",
        //            string.IsNullOrEmpty(companyCode) ? (object)DBNull.Value : Convert.ToInt32(companyCode),
        //            string.IsNullOrEmpty(departmentId) ? (object)DBNull.Value : departmentId,
        //            year ?? (object)DBNull.Value,
        //            month ?? (object)DBNull.Value,
        //            tenantId
        //        );

        //        //-------------------------------------------------------
        //        // 1️⃣ SUMMARY TABLE
        //        //-------------------------------------------------------
        //        decimal totalCTC = 0;
        //        decimal netSalary = 0;
        //        decimal employerContribution = 0;
        //        decimal totalRemainingLoan = 0;

        //        if (dts.Tables.Count > 0 && dts.Tables[0].Rows.Count > 0)
        //        {
        //            var row = dts.Tables[0].Rows[0];

        //            totalCTC = row["TotalCTC"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalCTC"]);
        //            netSalary = row["TotalNetSalary"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalNetSalary"]);
        //            employerContribution = row["TotalEmployerContribution"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalEmployerContribution"]);
        //            totalRemainingLoan = row["TotalRemainingLoan"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalRemainingLoan"]);
        //        }

        //        var summary = new
        //        {
        //            TotalCTC = totalCTC,
        //            NetSalary = netSalary,
        //            EmployerContribution = employerContribution,
        //            TotalRemainingLoan = totalRemainingLoan
        //        };

        //        //-------------------------------------------------------
        //        // 2️⃣ DEPARTMENT LIST
        //        //-------------------------------------------------------
        //        var deptList = new List<object>();

        //        if (dts.Tables.Count > 1)
        //        {
        //            deptList = dts.Tables[1].AsEnumerable()
        //                .Select(r => new
        //                {
        //                    DepartmentName = r["DepartmentName"]?.ToString() ?? "",
        //                    EstimatedCTC = r["EstimatedCTC"] == DBNull.Value ? 0 : Convert.ToDecimal(r["EstimatedCTC"]),
        //                    ActualCTC = r["ActualCTC"] == DBNull.Value ? 0 : Convert.ToDecimal(r["ActualCTC"]),
        //                    RemainingLoanAmount = r["RemainingLoanAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["RemainingLoanAmount"])
        //                })
        //                .ToList<object>();
        //        }

        //        //-------------------------------------------------------
        //        // FINAL JSON RESPONSE
        //        //-------------------------------------------------------
        //        return Json(new
        //        {
        //            summary,
        //            deptList
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            error = true,
        //            message = ex.Message
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public JsonResult MenuList()
        {
            var con = System.Configuration.ConfigurationManager.ConnectionStrings["Admin"].ToString();
            var d = clsMenu.Employee_Wise_Menu_List(con, Session["UserId"].ToString());
            return Json(d, JsonRequestBehavior.AllowGet);
        }
    }
}