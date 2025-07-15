using MendinePayroll.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.Models;
using UI.BLL;
using System.Data;
using Newtonsoft.Json;
using MendinePayroll.UI.BLL;
using System.Globalization;

namespace MendinePayroll.UI.Controllers
{
    public class LoanController : Controller
    {
        // GET: Loan
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
        public ActionResult LoanProcessData(long IDEmployee)
        {
            DataTable DT = clsLoan.Loan_Process_Data(IDEmployee);
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
    }
}