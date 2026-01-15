using Common.Utility;
using MendinePayroll.UI.BLL;
using MendinePayroll.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.BLL;
using UI.Models;

namespace MendinePayroll.UI.Controllers
{

    public class LoanController : Controller
    {
        // GET: Loan
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

            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "LOAN-ENTRY";
            clsAccessLog.AccessLog_Save(info);

            return View();
        }
        // Auto No 
        [HttpGet]
        public JsonResult AutoLoanNo(string Type)
        {
            String Result = clsLoan.Payroll_Loan_Autono(Type.ToUpper());
            return Json(Result, JsonRequestBehavior.AllowGet);
        }

        //Post method to add details    
        [HttpPost]
        public ActionResult SaveLoan(clsLoanInfo obj)
        {
            String Result = clsLoan.Payroll_Loan_Add_Edit(obj);
            if (Result == "")
                return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { error = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Employee Auto complete
        [HttpGet]
        public JsonResult AutoCompleteEmpDeatils()
        {
            DataTable DT = clsLoan.Payroll_Employee_Auto_Complete();
            int rows = DT.Rows.Count;
            String[] Empnos = new string[rows];
            for (int index = 0; index <= rows - 1; index++)
            {
                Empnos[index] = DT.Rows[index]["EmpDetails"].ToString();
            }
            return Json(Empnos, JsonRequestBehavior.AllowGet);
        }
        // To Open List page of Loan
        public ActionResult List()
        {
            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "LOAN-LIST-ENTRY";
            clsAccessLog.AccessLog_Save(info);

            return View();
        }

        // Loan Process Entry (Need to Be included)
        public ActionResult LoanProcess()
        {
            // Loan Process
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "LOAN-PROCESS-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }




        [HttpGet]
        public JsonResult LoanList(long EmployeeNo)
        {
            DataTable DT = clsLoan.Loan_List(EmployeeNo);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult OutstandingLoan(String month, string year, long idemployee)
        {
            DataTable DT = clsLoan.Employee_Loan_Outstanding_Installment(month, year, idemployee);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult OutstandingProcessData(String month, string year, long idemployee)
        {
            DataTable DT = clsLoan.Employee_Loan_Outstanding_Installment(month, year, idemployee);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult SalaryEmployeeList()
        {
            var employees = clsSalary.EmployeeList();
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoanEmployeeList()
        {
            var employees = clsSalary.LoanEmployeeList();
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult SalaryYearsList()
        {
            List<clsYearInfo> years = clsSalary.SalaryYear();
            return Json(years, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult SalaryMonthList()
        {
            List<clsMonthInfo> months = new List<clsMonthInfo>();
            for (int index = 1; index <= 12; index++)
            {
                clsMonthInfo obj = new clsMonthInfo();
                obj.Name = DateTimeFormatInfo.CurrentInfo.GetMonthName(index);
                obj.Selected = DateTime.Today.Month == index ? true : false;
                months.Add(obj);
            }
            return Json(months, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult OutstandingLoanProcess(String month, string year, long idemployee)
        {
            DataTable DT = clsLoan.Employee_Loan_Outstanding_Installment(month, year, idemployee);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoanDetail(String LoanNo)
        {
            DataTable DT = clsLoan.Loan_Detail(LoanNo.Trim());
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoanRevisedDetail(String LoanNo)
        {
            DataTable DT = clsLoan.Loan_Revised_Detail(LoanNo.Trim());
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult LoanRevisedMonthList(String LoanNo)
        {
            DataTable DT = clsLoan.Loan_Revised_Month_List(LoanNo.Trim());
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        //Post method to add details    
        [HttpPost]
        public ActionResult LoanProcessed(List<clsLoanProcessedInfo> obj)
        {
            var Result = clsLoan.Payroll_Loan_Processed(obj);
            return Json(new { Result = "" }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult LoanProcessedAll(List<clsLoanProcessedInfo> obj)
        {
            String Result = clsLoan.Payroll_Loan_Processed_All(obj);
            return Json(new { Result = "" }, JsonRequestBehavior.AllowGet);
        }

        public static string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }
        [HttpPost]
        public JsonResult RevisedLoan(clsChangeLoanInfo info)
        {
            DataTable DT = clsLoan.Change_Loan(info.LoanNo.Trim(), info.InsAmount, info.ChangeMonth, info.ChangeYear);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Sanctioned_Realised_Due_Loan(long IDEmployee)
        {
            DataTable DT = clsLoan.Sanctioned_Realised_Due_Loan(IDEmployee);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }


        // MOnth List
        [HttpGet]
        public JsonResult SalaryMonthsList(String LoanNo)
        {
            var Result = clsSalaryModification.Salary_Months_List(LoanNo);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Year List
        [HttpGet]
        public JsonResult SalaryYearList(String LoanNo)
        {
            var Result = clsSalaryModification.Salary_Years_List(LoanNo);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        // Revised Value Checking
        [HttpGet]
        public JsonResult RevisedValueChecking(String LoanNo, String Month, long Year)
        {
            var Result = clsSalaryModification.Revised_Value_Checking(LoanNo, Month, Year);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LoanProcessData(long IDEmployee, int Year, string Month)
        {
            DataTable DT = clsLoan.Loan_Process_Data(IDEmployee, Year, Month);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }

        #region Application
        public ActionResult LoanApplication()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "LOAN-APPLICATION-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpGet]
        public ActionResult LoanApplicationList(long idemployee)
        {
            DataTable DT = clsLoan.Loan_Application_List(idemployee);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LoanApplicationDetail(long idapplication)
        {
            DataTable DT = clsLoan.Loan_Application_Detail(idapplication);
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult LoanApplicationSave(clsLoanApplicationInfo info)
        {
            String Result = clsLoan.Loan_Application_Save(info);
            return Json(new { Result = "" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LoanApplicationApproval(long idapplication, long lovelno)
        {
            String Result = clsLoan.Loan_Application_Self_Approval(idapplication, lovelno);
            return Json(new { Result = "" }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LoanApplicationNo()
        {
            DataTable DT = clsLoan.Loan_Application_No();
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetEmployeeHavingLoanList()
        {
            DataTable DT = clsLoan.Employee_Having_Loan_List();
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Approval
        public ActionResult Approval()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "LOAN-APPROVAL-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpGet]
        public ActionResult ApprovalList()
        {
            DataTable DT = clsLoan.Loan_Approval_List();
            return Json(DataTableToJSONWithJSONNet(DT), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult LoanApproval(clsLoanApprovalInfo info)
        {
            info.RejectReason = info.RejectReason == null ? "" : info.RejectReason;
            String Result = clsLoan.Loan_Approval(info);
            return Json(new { Result = "" }, JsonRequestBehavior.AllowGet);
        }
        #endregion  

        //New Loan Module
        public ActionResult NewLoanEntry()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "NEW-LOAN-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();

        }

        [HttpPost]
        public JsonResult SaveLoan(LoanModel model)
        {
            try
            { 
                return Json(new { status = true, message = "Saved" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetAutoLoanNo(string Type)
        {
            // Generate Loan No based on Type (OPENING / TRANSACTION)
            string Result = clsDatabase.fnDataTable("PRCAutoLoanNo", Type.ToUpper(),TenantId).Rows[0][0].ToString();

            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        //[HttpGet]
        //public JsonResult GetEmpDetails()
        //{
        //    DataTable DT = clsDatabase.fnDataTable("PRC_EmpDetails",TenantId);
        //    int rows = DT.Rows.Count;
        //    String[] Empnos = new string[rows];
        //    for (int index = 0; index <= rows - 1; index++)
        //    {
        //        Empnos[index] = DT.Rows[index]["EmpDetails"].ToString();
        //    }
        //    return Json(Empnos, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetEmpDetails()
        {
            DataTable dt = clsDatabase.fnDataTable("PRC_EmpDetails", TenantId);

            var list = dt.AsEnumerable().Select(r => new
            {
                label = r["EmpDetails"].ToString(),    // shows in autocomplete
                value = r["EmpDetails"].ToString(),    // sets textbox value
                id = r["IDEmployee"].ToString()        // actual ID
            }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllLoans(int employeeId = 0)
        {
            int IDEmployee = employeeId;

            // 🎯 If permissions needed, check here
            // Example:
            // if(UserRole == "HR_View_Self_Only") employeeId = CurrentUserEmployeeId; 

            DataTable dt = clsDatabase.fnDataTable("SP_Get_EmployeeLoan", TenantId, IDEmployee);

            List<object> loans = new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                loans.Add(new
                {
                    IDLoan = Convert.ToInt32(row["IDLoan"]),
                    LoanNo = row["LoanNo"].ToString(),
                    EmpName = row["EmpName"]?.ToString(),
                    LoanType = row["LoanType"].ToString(),
                    LoanDate = Convert.ToDateTime(row["LoanDate"]).ToString("yyyy-MM-dd"),
                    LoanAmount = Convert.ToDecimal(row["LoanAmount"]),
                    BalanceAmount = Convert.ToDecimal(row["BalanceAmount"]),
                    MonthlyInstallment = Convert.ToDecimal(row["MonthlyInstallment"]),
                    LoanTenure = Convert.ToInt32(row["LoanTenure"]),
                    LoanStatus = row["LoanStatus"].ToString(),
                    PaidInstallments= Convert.ToInt32(row["PaidInstallments"]),
                    RemainingTenure = Convert.ToInt32(row["RemainingTenure"])
                });
            }

            return Json(loans, JsonRequestBehavior.AllowGet);
        }
        //Post method to add details    
        [HttpPost]
        public ActionResult SaveandUpdateLoan(LoanModel obj)
        {
            try 
            {
                string EntryUser = Session["UserName"].ToString();

                DataTable dt = clsDatabase.fnDataTable("SP_SaveEmployeeLoan",
                                obj.LoanType,
                                obj.LoanNo,
                                obj.IDEmployee,
                                obj.LoanDate,
                                obj.EndDate,
                                obj.RefNo,
                                obj.RefDate,
                                obj.LoanAmount,
                                obj.Tenure,
                                obj.InterestRate,
                                obj.GracePeriod,
                                obj.MonthlyPrincipal,
                                obj.MonthlyInterest,
                                obj.TotalEMI,
                                obj.TotalInterest,
                                obj.TotalLoan,
                                obj.RepaymentStartMonth,
                                obj.RepaymentStartYear,
                                obj.Remarks,
                                TenantId,
                                EntryUser
                                );
                // ===== READ RETURNED VALUES =====
                if (dt.Rows.Count > 0)
                {
                    return Json(new
                    {
                        Code = Convert.ToInt32(dt.Rows[0]["Code"]),
                        Message = dt.Rows[0]["Message"].ToString(),
                        IDLoan = Convert.ToInt64(dt.Rows[0]["IDLoan"])
                    });
                }

                return Json(new { Code = 0, Message = "No response from SP" });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        #region BulkEmployeeLoan Process
        // GET: Bulk Employee Loan
        public ActionResult BulkEmployeeLoan()
        {
            // Loan Process
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "LOAN-PROCESS-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpGet]
        public JsonResult BulkLoanProcessData(string EmpIds, string Month, string Year)
        {
            if (string.IsNullOrWhiteSpace(EmpIds))
                return Json("[]", JsonRequestBehavior.AllowGet);
            // Clean multiselect prefix if exists (Select2 adds "multiselect-all" sometimes)
            EmpIds = EmpIds.Replace("multiselect-all,", "").Trim(',');
            DataSet ds = new DataSet();
            ds = clsDatabase.fnDataSet("PRC_Bulk_EmployeeLoan_PendingList", EmpIds, Month, Year,TenantId);

            string json = JsonConvert.SerializeObject(ds.Tables[0]);
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //public JsonResult ProcessBulkLoans(ProcessBulkLoanModel model)
        //{
        //    if (model == null || string.IsNullOrWhiteSpace(model.JsonData))
        //        return Json(new { Result = "Invalid input data." }, JsonRequestBehavior.AllowGet);

        //    try
        //    {
        //        model.UserName = HttpContext.Session["username"]?.ToString() ?? "SystemUser";

        //        // Call stored procedure
        //        var result = clsDatabase.fnDBOperation(
        //            "PRC_Bulk_EmployeeLoan_Process",
        //            model.JsonData,
        //            model.Month,
        //            model.Year,
        //            model.UserName,
        //            TenantId
        //        );

        //        // Interpret result
        //        if (string.Equals(result, "Success", StringComparison.OrdinalIgnoreCase))
        //            return Json(new { Result = "Success" }, JsonRequestBehavior.AllowGet);
        //        else
        //            return Json(new { Result = result }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Result = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public JsonResult ProcessBulkLoans(ProcessBulkLoanModel model)
        {
            // 1. Basic Validation
            if (model == null || string.IsNullOrWhiteSpace(model.JsonData))
            {
                return Json(new { Code = 0, Message = "Invalid input data." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                model.UserName = HttpContext.Session["username"]?.ToString() ?? "SystemUser";

                // 2. Execute DB Operation
                // Since the SP contains a "SELECT" statement at the end, fnDBOperation usually returns a DataTable.
                // We cast the result to DataTable to access 'Code' and 'Result'.
                DataTable dt = clsDatabase.fnDataTable(
                    "PRC_Bulk_EmployeeLoan_Process",
                    model.JsonData,
                    model.Month,
                    model.Year,
                    model.UserName,
                    TenantId
                );

                // 3. Parse DB Response
                if (dt != null && dt.Rows.Count > 0)
                {
                    int dbCode = Convert.ToInt32(dt.Rows[0]["Code"]);
                    string dbMsg = dt.Rows[0]["Result"].ToString();

                    if (dbCode == 1)
                    {
                        // Success Case
                        return Json(new { Code = 1, Message = "Loan Processed Successfully!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // SP Logic Error (e.g. Validation inside SP)
                        return Json(new { Code = -1, Message = dbMsg }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Code = 0, Message = "Database returned no response." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // 4. Critical Exception
                return Json(new { Code = 0, Message = "Server Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult LoanRegister()
        {
            return View();
        }
        // ---------- AJAX CALL ----------
        [HttpGet]
        public JsonResult GetLoanRegister(string month = null, int? year = null)
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable("PRC_Loan_Register_Report", month, year);

                if (dt == null || dt.Rows.Count == 0)
                    return Json(new { Result = "No records found." }, JsonRequestBehavior.AllowGet);

                // ✅ Convert DataTable → List<Dictionary<string,string>>
                var data = dt.AsEnumerable()
                    .Select(r => dt.Columns.Cast<DataColumn>()
                        .ToDictionary(c => c.ColumnName, c => r[c]?.ToString() ?? string.Empty))
                    .ToList();

                return Json(new { Result = "Success", Data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [HttpGet]
        public JsonResult ViewLoanSchedule(int loanId)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_GetLoanSchedule", loanId);

            var list = dt.AsEnumerable().Select(r => new LoanScheduleViewModel()
            {
                EMI_No = Convert.ToInt32(r["EMI_No"]),
                MonthName = r["MonthName"].ToString(),
                MonthNo = Convert.ToInt32(r["MonthNo"]),
                YearNo = Convert.ToInt32(r["YearNo"]),

                BalanceBefore = Convert.ToDecimal(r["BalanceBefore"]),
                PrincipalDue = Convert.ToDecimal(r["PrincipalDue"]),
                InterestDue = Convert.ToDecimal(r["InterestDue"]),
                TotalDue = Convert.ToDecimal(r["TotalDue"]),
                BalanceAfter = Convert.ToDecimal(r["BalanceAfter"]),

                AmountPaid = SafeDecimal(r["AmountPaid"]),
                InterestPaid = SafeDecimal(r["InterestPaid"]),
                //SalaryProcessed = SafeInt(r["SalaryProcessed"]),  // <--- FIX
                ProcessSource = r["ProcessSource"].ToString(),
                Status = r["Status"].ToString()
            }).ToList();


            return Json(list, JsonRequestBehavior.AllowGet);
        }
        private decimal? SafeDecimal(object value)
        {
            if (value == null) return null;
            var s = value.ToString().Trim();
            return decimal.TryParse(s, out var d) ? d : (decimal?)null;
        }

        private int? SafeInt(object value)
        {
            if (value == null) return null;
            var s = value.ToString().Trim();
            return int.TryParse(s, out var d) ? d : (int?)null;
        }

        [HttpPost]
        public JsonResult AdjustLoan(LoanAdjustmentRequest req)
        {
            try
            {
                string UserName = Session["UserName"].ToString();

                DataTable dt = clsDatabase.fnDataTable("SP_AdjustLoan", req.LoanId,req.AmountPaid,req.PaymentDate,req.PaymentMode,req.RefNo,
                    req.Remarks,req.NewEMI,req.NewTenure,req.NewBalance,req.Strategy, UserName);

                return Json(new
                {
                    success = dt.Rows[0]["Success"].ToString() == "1",
                    message = dt.Rows[0]["Message"].ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        public JsonResult GetLoanById(int id)
        {
            DataTable dt = clsDatabase.fnDataTable("SP_GetLoanById", id);

            if (dt.Rows.Count == 0)
                return Json(new { success = false, message = "Loan not found" }, JsonRequestBehavior.AllowGet);

            var r = dt.Rows[0];

            var vm = new LoanEditViewModel
            {
                IDLoan = Convert.ToInt32(r["IDLoan"]),
                LoanType = r["LoanType"].ToString(),
                LoanNo = r["LoanNo"].ToString(),
                IDEmployee = Convert.ToInt32(r["IDEmployee"]),
                EmployeeName = r["EmployeeName"].ToString(),
                EmpCode = r["EmpCode"].ToString(),

                
                RefNo = r["RefNo"].ToString(),
               

                LoanDate = Convert.ToDateTime(r["LoanDate"]).ToString("yyyy-MM-dd"),
                EndDate = r["EndDate"] == DBNull.Value ? null : Convert.ToDateTime(r["EndDate"]).ToString("yyyy-MM-dd"),
                RefDate = r["RefDate"] == DBNull.Value ? null : Convert.ToDateTime(r["RefDate"]).ToString("yyyy-MM-dd"),


                LoanAmount = Convert.ToDecimal(r["LoanAmount"]),
                LoanTenure = Convert.ToInt32(r["LoanTenure"]),
                LoanInterestPcn = Convert.ToDecimal(r["LoanInterestPcn"]),
                MonthlyInstallment = Convert.ToDecimal(r["MonthlyInstallment"]),

                GracePeriodMonths = r["GracePeriodMonths"] == DBNull.Value ? 0 : Convert.ToInt32(r["GracePeriodMonths"]),
                RepaymentStartMonth = Convert.ToInt32(r["RepaymentStartMonth"]),
                RepaymentStartYear = Convert.ToInt32(r["RepaymentStartYear"]),

                BalanceAmount = Convert.ToDecimal(r["BalanceAmount"]),

                RemainingTenure = r["RemainingTenure"] == DBNull.Value ? null : (int?)r["RemainingTenure"],
                RevisedEMI = r["RevisedEMI"] == DBNull.Value ? null : (decimal?)r["RevisedEMI"],
                RevisedDate = r["RevisedDate"] == DBNull.Value ? null : (DateTime?)r["RevisedDate"],

                Remarks = r["Remarks"].ToString()
            };

            return Json(new { success = true, data = vm }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteLoan(int id)
        {
            try
            {
                // 1. Call the SP created above
                DataTable dt = clsDatabase.fnDataTable("sp_DeleteLoan", id);

                // 2. Check the result returned by SQL
                if (dt != null && dt.Rows.Count > 0)
                {
                    int code = Convert.ToInt32(dt.Rows[0]["Code"]);
                    string msg = dt.Rows[0]["Message"].ToString();

                    if (code == 1)
                    {
                        return Json(new { success = true, Code = 1, message = msg });
                    }
                    else
                    {
                        return Json(new { success = false, Code = 0, message = "SQL Error: " + msg });
                    }
                }

                return Json(new { success = false, Code = 0, message = "Unknown database error." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, Code = 0, message = ex.Message });
            }
        }
    }
}