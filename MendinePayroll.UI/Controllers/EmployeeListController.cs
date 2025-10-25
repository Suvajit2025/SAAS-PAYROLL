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
    public class EmployeeListController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();

        // GET: EmployeeList
        EmployeeListModel employeeListModel = new EmployeeListModel();
        EmpbasicModel EmpbasicModel = new EmpbasicModel();
        EmployeeLeaveModel EmployeeLeaveModel = new EmployeeLeaveModel();
        EmployeeSalaryConfigModel EmployeeSalaryConfigModel = new EmployeeSalaryConfigModel();
        ConfigureSalaryComponentModel configureSalaryComponentModel = new ConfigureSalaryComponentModel();
        EmployeeBonusModel EmployeeBonusModel = new EmployeeBonusModel();
        EmployeeLoanModel EmployeeLoanModel = new EmployeeLoanModel();
        IndividualSalryModel individualSalryModel = new IndividualSalryModel();
        LeaveDetailsModel LeaveDetailsModel = new LeaveDetailsModel();
        string fullmonthname = DateTime.Now.AddMonths(-1).ToString("MMMM");
        static int Month = DateTime.Now.AddMonths(-1).Month;
        static int Year = DateTime.Now.AddMonths(-1).Year;
        static int days = DateTime.DaysInMonth(Year, Convert.ToInt32(Month));
        static DateTime first = new DateTime(Year, Convert.ToInt32(Month), 1);
        DateTime last = first.AddMonths(1).AddSeconds(-1);
        
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }



            //// Access Log Data Insert 
            //clsAccessLogInfo info = new clsAccessLogInfo();
            //info.AccessType = "EMPLOYEE-LIST-ENTRY";
            //clsAccessLog.AccessLog_Save(info);



            //employeeListModel.emplist = GetAllEmployee();
            //if (employeeListModel.emplist.Count > 0)
            //{
            //    for (int i = 0; i < employeeListModel.emplist.Count; i++)
            //    {
            //        employeeListModel.emplist[i].sempid = DataEncryption.Encrypt(Convert.ToString(employeeListModel.emplist[i].empid), "passKey");
            //    }
            //}

            return View();
        }
        #region GetAll Employee
        public List<EmployeeListModel> GetAllEmployee()
        {
            employeeListModel.empid = 0;
            string contents = JsonConvert.SerializeObject(employeeListModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetAllEmployeeList", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    employeeListModel.emplist = JsonConvert.DeserializeObject<List<EmployeeListModel>>(responseString);
                }
            }
            return employeeListModel.emplist;
        }
        #endregion


        #region GetEmployeeList Employee
        
        public JsonResult GetEmployeeList(int pagenumber, int numberofrow, string SearchValue)
        {
            if (SearchValue == "null" || SearchValue == "")
            {
                SearchValue = null;
            }
            employeeListModel.PageNumber = pagenumber;
            employeeListModel.Rowofpage = numberofrow;
            employeeListModel.SearchVal = SearchValue;
            string contents = JsonConvert.SerializeObject(employeeListModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetAllEmployeeByPage", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    employeeListModel.emplist = JsonConvert.DeserializeObject<List<EmployeeListModel>>(responseString);
                }
            }
            if (employeeListModel.emplist.Count > 0)
            {
                for (int i = 0; i < employeeListModel.emplist.Count; i++)
                {
                    employeeListModel.emplist[i].sempid = DataEncryption.Encrypt(Convert.ToString(employeeListModel.emplist[i].empid), "passKey");
                }
            }
            employeeListModel.TotalCount = GetEmployeecount(SearchValue);
            return Json(employeeListModel, JsonRequestBehavior.AllowGet);
        }
        // New Mayukh
        //public List<EmployeeListModel> GetEmplist(EmployeeListModel employeeListModel)
        //{
        //    List<EmployeeListModel> listmodel = new List<EmployeeListModel>();
        //    try
        //    {
        //        List<EmployeeList_Get_List_Result> empList = esspEntities.EmployeeList_Get_List(employeeListModel.PageNumber, employeeListModel.Rowofpage, employeeListModel.SearchVal).ToList();


        //        listmodel = empList.Select(X =>
        //        {
        //            return
        //        }).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return listmodel;

        //}
        [HttpPost]
        public JsonResult GetEmployeeListNew(int pagenumber, int numberofrow, string SearchValue,String UserEmail )
        {
            //if (SearchValue == "null" || SearchValue == "")
            //{
            //    SearchValue = null;
            //}
            // Mayukh New
            if (SearchValue == "null" || SearchValue == "")
            {
                SearchValue = "";
            }
            List<EmployeeListModel> mlist = new List<EmployeeListModel>();
            DataTable DT = clsDatabase.fnDataTable("EmployeeList_Get_List_NEW", pagenumber, numberofrow, SearchValue, UserEmail);
            foreach (DataRow dr in DT.Rows)
            {
                EmployeeListModel X = new EmployeeListModel();
                X.empid = clsHelper.fnConvert2Int(dr["empid"]);
                X.empno = Convert.ToInt32(dr["empno"]);
                X.EmployeeName = dr["EmployeeName"].ToString();
                X.empcode = dr["empcode"].ToString();
                X.postname = dr["postname"].ToString();
                X.Designation = dr["Designation"].ToString();
                X.Department = dr["Department"].ToString();
                X.PayGroupName = dr["PayGroupName"].ToString();
                X.PayGroupID = clsHelper.fnConvert2Int(dr["PayGroupID"]);
                mlist.Add(X);
            }
            employeeListModel.PageNumber = pagenumber;
            employeeListModel.Rowofpage = numberofrow;
            employeeListModel.SearchVal = SearchValue;
            employeeListModel.emplist = mlist;
            if (employeeListModel.emplist.Count > 0)
            {
                for (int i = 0; i < employeeListModel.emplist.Count; i++)
                {
                    employeeListModel.emplist[i].sempid = DataEncryption.Encrypt(Convert.ToString(employeeListModel.emplist[i].empid), "passKey");
                }
            }
            employeeListModel.TotalCount = GetEmployeecount(SearchValue);
            return Json(employeeListModel, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public int GetEmployeecount(string SearchValue)
        {
            employeeListModel.SearchVal = SearchValue;
            int Totalcount = 0;
            string contents = JsonConvert.SerializeObject(employeeListModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetAllEmployeeByCount", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    Totalcount = Convert.ToInt32(responseString);
                }
            }

            return Totalcount;
        }
        #region Get Employeebyid
        public JsonResult GetEmployeebyid(string empid)
        {
            empid = DataEncryption.Decrypt(Convert.ToString(empid), "passKey");
            employeeListModel.empid = Convert.ToInt32(empid);
            List<EmployeeListModel> data = GetEmployeeListById(empid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public List<EmployeeListModel> GetEmployeeListById(string empid)
        {
            List<EmployeeListModel> data = new List<EmployeeListModel>();
            if (!string.IsNullOrEmpty(empid) || !string.IsNullOrWhiteSpace(empid))
            {
                employeeListModel.empid = Convert.ToInt32(empid);
                string contents = JsonConvert.SerializeObject(employeeListModel);
                HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetAllEmployeeListbyid", contents);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(responseString))
                    {
                        data = JsonConvert.DeserializeObject<List<EmployeeListModel>>(responseString); ;
                    }
                }
            }
            return data;
        }
        #endregion
        #region Get Leave Amount
        public JsonResult GetDeductamount(string empno, string month, string Empid, string GrossAmount)
        {
            List<EmployeeLeaveModel> EmployeeLeaveData = GetEmployeeLeave(empno);
            decimal? leavedays = 0;
            decimal onedayamt = 0;
            decimal? totalLeavedays = 0;
            int? Totallateleave = 0;
            double? AbsentDays = 0;
            if (EmployeeLeaveData.Count > 0)
            {
                leavedays = EmployeeLeaveData[0].noofdays;
                Totallateleave = EmployeeLeaveData[0].lateleave;
                AbsentDays = EmployeeLeaveData[0].AbsentDays;
                totalLeavedays = EmployeeLeaveData[0].noofdays + EmployeeLeaveData[0].lateleave;
            }
            List<EmployeeSalaryConfigModel> Salarydata = GetSalaryData(Empid);
            onedayamt = Math.Round(Convert.ToDecimal(GrossAmount) / 30, 2);
            double leaveamt = 0;
            leaveamt = Convert.ToDouble(onedayamt) * Convert.ToDouble(leavedays);
            List<EmployeeLeaveModel> dataleave = new List<EmployeeLeaveModel>();
            double lateleaveamount = 0;
            double leaveamount = 0;

            leaveamount = Convert.ToDouble(onedayamt) * Convert.ToDouble(totalLeavedays);

            lateleaveamount = Convert.ToDouble(onedayamt) * Convert.ToDouble(Totallateleave);
            dataleave.Add(new EmployeeLeaveModel
            {
                Month = fullmonthname,
                year = Year,
                lateleave = Convert.ToInt32(Totallateleave),
                noofdays = leavedays,
                TotalLeaveAmount = leaveamount,
                lateleaveAmount = lateleaveamount,
                leaveAmount = leaveamt,
                Days = days,
                AbsentDays = AbsentDays
            });

            return Json(dataleave, JsonRequestBehavior.AllowGet);
        }

        public List<EmployeeLeaveModel> GetEmployeeLeave(string empno)
        {
            List<EmployeeLeaveModel> data = new List<EmployeeLeaveModel>();
            EmployeeLeaveModel.Empno = Convert.ToInt32(empno);
            EmployeeLeaveModel.month = Month;
            EmployeeLeaveModel.year = Year;
            EmployeeLeaveModel.prefixfromdate = first;
            EmployeeLeaveModel.suffixtodate = last;
            string contents = JsonConvert.SerializeObject(EmployeeLeaveModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetEmployeeLeave", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmployeeLeaveModel>>(responseString); ;
                }
            }
            //double TotalLeave = 0;
            double AbsentDays = 0;
            double LeaveWithoutPay = 0;
            double lateleave = 0;
            List<LeaveDetailsModel> leaveDetails = GetEditedLeaveDetails(empno);
            if (leaveDetails.Count > 0)
            {
                AbsentDays = leaveDetails[0].Noofdaysabsent == null ? 0 : Convert.ToDouble(leaveDetails[0].Noofdaysabsent);
                LeaveWithoutPay = leaveDetails[0].LeaveDays == null ? 0 : Convert.ToDouble(leaveDetails[0].LeaveDays);
                lateleave = leaveDetails[0].LateLeaveDays == null ? 0 : Convert.ToDouble(leaveDetails[0].LateLeaveDays);
            }
            //if (LeaveWithoutPay <= 0)
            //{
            //    for (int k = 0; k < data.Count; k++)
            //    {
            //        LeaveWithoutPay = Convert.ToDouble(LeaveWithoutPay + Convert.ToDouble(data[k].noofdays));
            //    }
            //}
            //if (lateleave <= 0)
            //{
            //    if (data.Count > 0)
            //    {
            //        lateleave = Convert.ToDouble(lateleave + Convert.ToDouble(data[0].lateleave));
            //    }
            //}
            List<EmployeeLeaveModel> LeaveData = new List<EmployeeLeaveModel>();
            LeaveData.Add(new EmployeeLeaveModel
            {
                lateleave = Convert.ToInt32(lateleave),
                noofdays = Convert.ToDecimal(LeaveWithoutPay),
                AbsentDays = AbsentDays
            });

            return LeaveData;
        }
        #endregion

        public ActionResult EmployeeDetails()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        public ActionResult EmployeeSalaryDetails(string Empid, string PayGroupID)
        {
            Empid = DataEncryption.Decrypt(Convert.ToString(Empid), "passKey");
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            //int licenceid = Convert.ToInt32(Request.QueryString["Empid"]);
            int licenceid = Convert.ToInt32(Empid);
            return View();
        }

        public ActionResult EmployeePayableSalary(string cal)
        {
            cal = DataEncryption.Decrypt(Convert.ToString(cal), "passKey");
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            TempData["LeaveEdit"] = ConfigurationManager.AppSettings["LeaveEditor"];
            employeeListModel.emplist = GetEmployeeListById(cal);
            if (employeeListModel.emplist.Count > 0)
            {
                employeeListModel.EmployeeName = employeeListModel.emplist[0].EmployeeName;
                employeeListModel.sempid = DataEncryption.Encrypt(Convert.ToString(employeeListModel.emplist[0].empid), "passKey");
                employeeListModel.empcode = employeeListModel.emplist[0].empcode;
                employeeListModel.Designation = employeeListModel.emplist[0].Designation;
                employeeListModel.Department = employeeListModel.emplist[0].Department;
                employeeListModel.empaccountno = employeeListModel.emplist[0].empaccountno;
                employeeListModel.empuanno = employeeListModel.emplist[0].empuanno;
                employeeListModel.empid = employeeListModel.emplist[0].empid;
                employeeListModel.empno = employeeListModel.emplist[0].empno;
                employeeListModel.SalaryConfigureID = employeeListModel.emplist[0].SalaryConfigureID;
                employeeListModel.EmployeeLoanModel.MonthlyInstallment = 0;
                // Employee Loan Amount
                double EmployeeLoanAmount = Employee_Loan_Amount(employeeListModel.empid);
                employeeListModel.EmployeeLoanModel.MonthlyInstallment = EmployeeLoanAmount;

                //List<EmployeeLoanModel> Loandata = GetEmployeeLoanDetailsById(Convert.ToString(employeeListModel.empid));
                //if (Loandata.Count > 0) {
                //    if (Loandata[0].IsStart == true && Loandata[0].ClosingBalance != 0)
                //    {
                //        employeeListModel.EmployeeLoanModel.MonthlyInstallment = Loandata[0].MonthlyInstallment;
                //    }

                //}

            }
            employeeListModel.MonthList = GetMonthList();
            return View(employeeListModel);
        }

        #region Get EmployeeDetails
        public JsonResult GetEmployeeDetails(string Empid)
        {
            List<EmpbasicModel> data = GetEmployeeDetailsById(Empid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public List<EmpbasicModel> GetEmployeeDetailsById(string Empid)
        {
            Empid = DataEncryption.Decrypt(Convert.ToString(Empid), "passKey");
            List<EmpbasicModel> data = new List<EmpbasicModel>();
            EmpbasicModel.empid = Convert.ToInt32(Empid);
            string contents = JsonConvert.SerializeObject(EmpbasicModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetEmployeeDetailsbyid", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmpbasicModel>>(responseString);
                }
            }
            return data;
        }
        #endregion  

        #region Get Employee Salary Details
        public JsonResult GetEmployeeSalaryDetails(string Empid)
        {
            Empid = DataEncryption.Decrypt(Convert.ToString(Empid), "passKey");
            List<EmployeeSalaryConfigModel> data = GetSalaryData(Empid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public List<EmployeeSalaryConfigModel> GetEmployeeSalaryManualData(string Empid)
        {
            List<EmployeeSalaryConfigModel> data = GetSalaryData(Empid);
            List<EmployeeListModel> empdata = GetEmployeeListById(Empid);
            DateTime Today = DateTime.Now.AddMonths(-1);
            if (empdata.Count > 0)
            {
                DateTime JoiningDate = empdata[0].Joining.HasValue ? Convert.ToDateTime(empdata[0].Joining) : Convert.ToDateTime(null);
                if (Today.Year == JoiningDate.Year && Today.Month == JoiningDate.Month)
                {
                    int Monthdays = DateTime.DaysInMonth(Today.Year, Today.Month);
                    int differencedate = Monthdays - JoiningDate.Day + 1;
                    for (int p = 0; p < data.Count; p++)
                    {
                        data[p].Values = Convert.ToString(Math.Round((Convert.ToDouble(data[p].Values) / Monthdays) * differencedate));
                    }
                }
            }
            return data;
        }
        public JsonResult GetEmployeeManualSalaryDetails(string Empid)
        {
            List<EmployeeSalaryConfigModel> data = GetEmployeeSalaryManualData(Empid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public List<EmployeeSalaryConfigModel> GetSalaryData(string Empid)
        {
            //Empid = DataEncryption.Decrypt(Convert.ToString(Empid), "passKey");
            List<EmployeeSalaryConfigModel> SalaryConfigdata = new List<EmployeeSalaryConfigModel>();
            EmployeeSalaryConfigModel.EmpId = Convert.ToInt32(Empid);
            string Salarycontents = JsonConvert.SerializeObject(EmployeeSalaryConfigModel);
            HttpResponseMessage Salaryresponse = ObjAPI.CallAPI("api/Employee/GetEmployeeSalaryDetails", Salarycontents);
            if (Salaryresponse.IsSuccessStatusCode)
            {
                string responseString = Salaryresponse.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    SalaryConfigdata = JsonConvert.DeserializeObject<List<EmployeeSalaryConfigModel>>(responseString); ;
                }
            }
            return SalaryConfigdata;
        }
        #endregion  

        #region Save Employee Salary
        public JsonResult SaveEmployeeSalary(string Id, string Empid, string paygroupId, string IsPTaxActive, string ContinuousAllowance, string ProductionBonus)
        {
            Empid = DataEncryption.Decrypt(Convert.ToString(Empid), "passKey");
            var data = "";
            EmployeeSalaryConfigModel employeeSalaryConfigModel = new EmployeeSalaryConfigModel();
            employeeSalaryConfigModel.Id = Convert.ToInt32(Id);
            employeeSalaryConfigModel.EmpId = Convert.ToInt32(Empid);
            employeeSalaryConfigModel.PayGroupId = Convert.ToInt32(paygroupId);
            employeeSalaryConfigModel.IsPTaxActive = Convert.ToBoolean(IsPTaxActive);
            employeeSalaryConfigModel.ContinuousAllowance = ContinuousAllowance;
            employeeSalaryConfigModel.ProductionBonus = ProductionBonus;
            string contents = JsonConvert.SerializeObject(employeeSalaryConfigModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/SaveEmployeeSalary", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Employee Manual Salary
        //public JsonResult SaveEmployeeManualSalary(string modelParameter, string PayConfigID, string EmployeeSalaryConfigid)
        //{
        //    var data = "";
        //    EmpoloyeeSalaryConfigManualValueModel empoloyeeSalaryConfigManualValueModel = new EmpoloyeeSalaryConfigManualValueModel();
        //    empoloyeeSalaryConfigManualValueModel.Values = modelParameter;
        //    empoloyeeSalaryConfigManualValueModel.PayConfigIDS = PayConfigID;
        //    empoloyeeSalaryConfigManualValueModel.EmployeeSalaryConfigid = Convert.ToInt32(EmployeeSalaryConfigid);
        //    string contents = JsonConvert.SerializeObject(empoloyeeSalaryConfigManualValueModel);
        //    HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/SaveEmployeeManualSalary", contents);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string responseString = response.Content.ReadAsStringAsync().Result;

        //        if (!string.IsNullOrEmpty(responseString))
        //        {
        //            data = JsonConvert.DeserializeObject(responseString).ToString();
        //        }
        //    }
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult SaveEmployeeManualSalary()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            // Read the raw JSON body
            string jsonString;
            using (var reader = new StreamReader(Request.InputStream))
            {
                jsonString = reader.ReadToEnd();
            }

            // Deserialize into model
            var request = JsonConvert.DeserializeObject<ManualSalaryRequest>(jsonString);
            foreach (var item in request.ManualSalaryList)
            {
                if (string.IsNullOrWhiteSpace(item.Value?.ToString()))
                    item.Value = "0";   // Default blank values to 0
            }
            // Convert lists to comma-separated strings
            var PayConfigIDs = string.Join(",", request.ManualSalaryList.Select(x => x.PayConfigID));
            var payConfigValues = string.Join(",", request.ManualSalaryList.Select(x => x.Value));
            var salaryConfigId = request.ManualSalaryList.First().EmployeeSalaryConfigid;

            info.AccessType = "EMPLOYEE-MANUAL-SALARY-ENTRY-UPDATE";
            clsAccessLog.AccessLog_Save(info);

            var result = "";
            result=clsDatabase.fnDBOperation("EmpoloyeeSalaryConfigManualValues_Update_New", PayConfigIDs,payConfigValues,salaryConfigId);


            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region  GetConfigureSalaryComponent
        public List<ConfigureSalaryComponentModel> GetConfigureSalaryComponent(string SalaryConfigureID)
        {
            List<ConfigureSalaryComponentModel> data = new List<ConfigureSalaryComponentModel>();
            configureSalaryComponentModel.ConfigureSalaryID = Convert.ToInt32(SalaryConfigureID);
            string contents = JsonConvert.SerializeObject(configureSalaryComponentModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/ConfigureSalaryComponent/GetAllConfigureSalaryComponentbyid", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<ConfigureSalaryComponentModel>>(responseString); ;
                }
            }
            return data;
        }
        #endregion

        #region EmployeeBonusdata
        public List<EmployeeBonusModel> GetEmployeeBonusDataById(string Empid)
        {
            List<EmployeeBonusModel> data = new List<EmployeeBonusModel>();
            EmployeeBonusModel.Empid = Convert.ToInt32(Empid);
            EmployeeBonusModel.Year = Convert.ToInt32(Year);
            EmployeeBonusModel.Month = fullmonthname;
            string EmployeeBonuscontents = JsonConvert.SerializeObject(EmployeeBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetEmployeeBonusByMonth", EmployeeBonuscontents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmployeeBonusModel>>(responseString); ;
                }
            }
            return data;
        }

        #endregion

        #region calculate configuresalary
        public double Employee_PTax_Value(double GrossValue)
        {
            return clsPTaxSlab.Employee_PTax_Value(GrossValue);
        }
        public double Employee_Loan_Amount(long IDEmployee)
        {
            return clsLoan.Employee_Loan_Amount(IDEmployee);
        }

        // Employee Salary Calculation (Preview)
        public JsonResult GetConfigureSalaryComponentByID(string Empid)
        {
            // Employee Detail 
            List<EmployeeListModel> empdata = GetEmployeeListById(Empid);
            // Employee production Bonus
            List<EmployeeBonusModel> PayBonusdata = GetEmployeeBonusDataById(Empid);
            List<ConfigureSalaryComponentModel> caldata = new List<ConfigureSalaryComponentModel>();

            Boolean IsesicActive = true;

            // Employee Exist Then 
            if (empdata.Count > 0)
            {
                // Abscent days 
                List<LeaveDetailsModel> LeavedetailsList = GetEditedLeaveDetails(empdata[0].empno.ToString());
                List<EmployeeLeaveModel> dataleave = GetEmployeeLeave(empdata[0].empno.ToString());
                decimal leavedays = 0;

                if (LeavedetailsList.Count > 0)
                {
                    // Total Leave (Leave Days + Leave for Late )
                    leavedays = leavedays + Convert.ToDecimal(LeavedetailsList[0].LeaveDays) + Convert.ToDecimal(LeavedetailsList[0].LateLeaveDays);
                }
                else
                {
                    // Find total Leave days 
                    for (int i = 0; i < dataleave.Count; i++)
                    {
                        leavedays = Convert.ToDecimal(leavedays + dataleave[i].noofdays);
                    }
                }
                // Working day (FOr this month- Leave)
                var workingdays = days - leavedays;
                

                // Employee Wise Manual Fields and Value  + (Continous Allowance and  Ptax Reaquired or not )
                List<EmployeeSalaryConfigModel> Salarydata = GetEmployeeSalaryManualData(Empid);
                double Basic = 0;
                double GrossAmount = 0;
                //double InternetAllowance = 0;
                double ContinuousAllowance = 0;
                //double MobileAllowance = 0;
                bool IsPtaxActive = false;
                double LoanAmount = Employee_Loan_Amount(Convert.ToInt64(Empid));

                List<EmployeeSalaryConfigModel> EsicActive = GetSalaryData(Empid);

                if (EsicActive != null && EsicActive.Count > 0)
                {
                    foreach (var item in EsicActive)
                    {
                        if (item.PayConfigName != null && item.PayConfigName.ToUpper() == "GROSS AMOUNT")
                        {
                            GrossAmount = Convert.ToDouble(item.Values);

                            if (GrossAmount > 21000)
                            {
                                IsesicActive = false;
                                break; // stop loop once condition met
                            }
                        }
                    }
                }


                // Mayukh New 
                //employeeSalaryModel.Loan_Installment = Employee_Loan_Amount(Empid);

                // Continous Allowance and PTax Required or not
                if (Salarydata.Count > 0)
                {
                    ContinuousAllowance = Convert.ToDouble(Salarydata[0].ContinuousAllowance);
                    if (Salarydata[0].IsPTaxActive != null)
                    {
                        IsPtaxActive = Convert.ToBoolean(Salarydata[0].IsPTaxActive);
                    }
                }
                // Gross Amount findng from Manual input
                for (int p = 0; p < Salarydata.Count; p++)
                {

                    if (Regex.Replace(Salarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("GROSSAMOUNT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        GrossAmount = Convert.ToDouble(Salarydata[p].Values);
                    }

                }
                // PTax as per Gross Amount 
                //double PTaxValue = Employee_PTax_Value(GrossAmount);
                // OLD Mayukh
                //List<EmployeeSalaryConfigModel> Continuousdata = GetcontinuousAllowance(Empid, Convert.ToString(Basic));
                List<EmployeeSalaryConfigModel> Continuousdata = new List<EmployeeSalaryConfigModel>();
                if (Salarydata[0].PayConfigName.ToUpper() =="BASIC")
                {
                    Continuousdata = GetcontinuousAllowance(Empid, Convert.ToString(Salarydata[0].Values));
                }
                // Employee Wise Salary Configure 
                List<ConfigureSalaryComponentModel> data = GetConfigureSalaryComponent(empdata[0].SalaryConfigureID.ToString());
                var calformula = "";
                var ManualRate = "";
                double tallowance = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    double result = 0;
                    double factoryworkergrossamount = 0;
                    // int ptax = 0;
                    double ptax = 0;

                    var attendance = 0.0;
                    calformula = data[i].CalculationFormula;
                    ManualRate = Convert.ToString(data[i].ManualRate);
                    if (ManualRate == null || ManualRate == "")
                    {
                        ManualRate = "100";
                    }
                    if (calformula == null)
                    {
                        calformula = 0.ToString();
                    }
                    if (Salarydata.Count > 0)
                    {
                        if (Salarydata[0].SalaryConfigureType == 1)
                        {
                            for (int p = 0; p < Salarydata.Count; p++)
                            {
                                calformula = calformula.Replace(Salarydata[p].PayConfigName, Convert.ToString(Salarydata[p].Values));
                            }
                            calformula = calformula.Replace("–", "-");

                            calformula = calformula.Replace("%", Convert.ToString("/100"));
                            DataTable dt = new DataTable();
                            string math = calformula;
                            double res = Convert.ToDouble(new DataTable().Compute(math, null));
                            var name = Regex.Replace(data[i].PayConfigName, @"[^0-9a-zA-Z]+", "");
                            result = Convert.ToDouble(res) * Convert.ToDouble(ManualRate) / 100;
                            if (name.Equals("esic", StringComparison.InvariantCultureIgnoreCase))
                            {
                                result = Math.Ceiling(result);
                            }
                            else
                            {
                                result = Math.Round(result);
                            }

                            if (name.Equals("ptax", StringComparison.InvariantCultureIgnoreCase))
                            {

                                if (IsPtaxActive == true)
                                {
                                    //ptax = PTaxValue;
                                    if (GrossAmount <= 10000)
                                    {
                                        ptax = 0;
                                    }
                                    else if (GrossAmount > 10000 && GrossAmount <= 15000)
                                    {
                                        ptax = 110;
                                    }
                                    else if (GrossAmount > 15000 && GrossAmount <= 25000)
                                    {
                                        ptax = 130;
                                    }
                                    else if (GrossAmount > 25000 && GrossAmount <= 40000)
                                    {
                                        ptax = 150;
                                    }
                                    else
                                    {
                                        ptax = 200;
                                    }
                                }
                                else
                                {
                                    ptax = 0;
                                }
                                caldata.Add(new ConfigureSalaryComponentModel
                                {


                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,

                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = ptax.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID

                                });
                            }

                            else if (name.Equals("esic", StringComparison.InvariantCultureIgnoreCase) ||
                                name.Equals("esi", StringComparison.InvariantCultureIgnoreCase))
                            {
                                bool eligible = false;
                                int slabStart = 0, slabEnd = 0, id = 0;

                                DataTable esidt = clsDatabase.fnDataTable("SP_Check_ESIC_SLAB", Empid);
                                if (esidt != null && esidt.Rows.Count > 0)
                                {
                                    slabStart = Convert.ToInt32(esidt.Rows[0]["SlabStartMonth"]);
                                    slabEnd = Convert.ToInt32(esidt.Rows[0]["SlabEndMonth"]);
                                    eligible = Convert.ToBoolean(esidt.Rows[0]["IsEligible"]);
                                    id = Convert.ToInt32(esidt.Rows[0]["ID"]);

                                    int salaryMonth = DateTime.Now.AddMonths(-1).Month;

                                    if (salaryMonth >= slabStart && salaryMonth <= slabEnd && eligible)
                                    {
                                        result = Math.Ceiling(result);
                                    }
                                    else if (GrossAmount >= 21000 || IsesicActive == false)
                                    {
                                        result = 0;
                                    }
                                    else result = 0;

                                    if (salaryMonth == slabEnd)
                                        clsDatabase.fnDataTable("SP_UPDATE_ESIC_TAG", Empid, id);
                                }
                                //else if (GrossAmount <= 21000 && !IsesicActive)
                                //{
                                //    result = Math.Ceiling(result);
                                //}
                                else if (GrossAmount >= 21000 || IsesicActive == false)
                                {
                                    result = 0;
                                }
                                else result = result;

                                caldata.Add(new ConfigureSalaryComponentModel
                                {


                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,

                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = result.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID

                                });
                            }

                            else if (Regex.Replace(data[i].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("CONTINUOUSALLOWANCE", StringComparison.InvariantCultureIgnoreCase) ||
                                Regex.Replace(data[i].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("CONTINUOUSATTENDANCEALLOWANCE", StringComparison.InvariantCultureIgnoreCase))
                            {
                                attendance = 0;
                                caldata.Add(new ConfigureSalaryComponentModel
                                {


                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,

                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = attendance.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID

                                });
                            }

                            else
                            {
                                caldata.Add(new ConfigureSalaryComponentModel
                                {
                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,
                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = result.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID
                                });
                            }
                        }
                        else
                        {
                            for (int p = 0; p < Salarydata.Count; p++)
                            {

                                calformula = calformula.Replace(Salarydata[p].PayConfigName, Convert.ToString(Salarydata[p].Values));

                                //calformula = calformula.Replace(Salarydata[p].PayConfigName, Convert.ToString(Salarydata[p].Values));
                                string payconfigname = Salarydata[p].PayConfigName.Replace(@"[^0-9a-zA-Z]+", "");
                                if (payconfigname.Equals("arrears", StringComparison.InvariantCultureIgnoreCase) ||
                                    payconfigname.Equals("arrear", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    factoryworkergrossamount = factoryworkergrossamount + 0;
                                }
                                else
                                {
                                    factoryworkergrossamount = factoryworkergrossamount + Convert.ToDouble(Salarydata[p].Values);
                                }

                            }
                            factoryworkergrossamount = factoryworkergrossamount + Math.Round(Convert.ToDouble(Continuousdata[0].OvertimeAllowance) + Convert.ToDouble(Continuousdata[0].ProductionBonus) + Convert.ToDouble(Continuousdata[0].ContinuousAllowance));

                            if (data[i].PayConfigType == "Allowances" || data[i].PayConfigType == "Allowance")
                            {

                                calformula = calformula.Replace("Gross Amount", Convert.ToString(factoryworkergrossamount));
                                calformula = calformula.Replace("Gross Amounts", Convert.ToString(factoryworkergrossamount));
                                calformula = calformula.Replace("GROSS AMOUNTS", Convert.ToString(factoryworkergrossamount));
                                calformula = calformula.Replace("GROSS AMOUNT", Convert.ToString(factoryworkergrossamount));
                                calformula = calformula.Replace("–", "-");
                                calformula = calformula.Replace("%", Convert.ToString("/100"));
                                DataTable dt = new DataTable();
                                string math = calformula;


                                double res = Convert.ToDouble(new DataTable().Compute(math, null));
                                result = Convert.ToDouble(res) * Convert.ToDouble(ManualRate) / 100;
                                result = Math.Round(result);
                                tallowance = result + tallowance;
                                GrossAmount = tallowance + factoryworkergrossamount;
                            }
                            else
                            {
                                calformula = calformula.Replace("Gross Amount", Convert.ToString(GrossAmount));
                                calformula = calformula.Replace("Gross Amounts", Convert.ToString(GrossAmount));
                                calformula = calformula.Replace("GROSS AMOUNTS", Convert.ToString(GrossAmount));
                                calformula = calformula.Replace("GROSS AMOUNT", Convert.ToString(GrossAmount));
                                calformula = calformula.Replace("–", "-");

                                calformula = calformula.Replace("%", Convert.ToString("/100"));
                                DataTable dt = new DataTable();
                                string math = calformula;


                                double res = Convert.ToDouble(new DataTable().Compute(math, null));
                                result = Convert.ToDouble(res) * Convert.ToDouble(ManualRate) / 100;
                                result = Math.Round(result);
                            }
                            var name = Regex.Replace(data[i].PayConfigName, @"[^0-9a-zA-Z]+", "");
                            if (name.Equals("ptax", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (IsPtaxActive == true)
                                {
                                    //ptax = PTaxValue;
                                    if (GrossAmount <= 10000)
                                    {
                                        ptax = 0;
                                    }
                                    else if (GrossAmount > 10000 && GrossAmount <= 15000)
                                    {
                                        ptax = 110;
                                    }
                                    else if (GrossAmount > 15000 && GrossAmount <= 25000)
                                    {
                                        ptax = 130;
                                    }
                                    else if (GrossAmount > 25000 && GrossAmount <= 40000)
                                    {
                                        ptax = 150;
                                    }
                                    else
                                    {
                                        ptax = 200;
                                    }
                                }
                                else
                                {
                                    ptax = 0;
                                }
                                caldata.Add(new ConfigureSalaryComponentModel
                                {


                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,

                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = ptax.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID

                                });
                            }

                            else if (name.Equals("esic", StringComparison.InvariantCultureIgnoreCase) ||
                            name.Equals("esi", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (GrossAmount >= 21000)
                                {
                                    result = 0;
                                }
                                else
                                {

                                    double res = Convert.ToDouble(GrossAmount);
                                    name = Regex.Replace(data[i].PayConfigName, @"[^0-9a-zA-Z]+", "");
                                    result = Convert.ToDouble(res) * Convert.ToDouble(ManualRate) / 100;
                                    if (name.Equals("esic", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        result = Math.Ceiling(result);
                                    }
                                }

                                caldata.Add(new ConfigureSalaryComponentModel
                                {


                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,

                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = result.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID

                                });
                            }

                            else if (Regex.Replace(data[i].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("CONTINUOUSALLOWANCE", StringComparison.InvariantCultureIgnoreCase) ||
                            Regex.Replace(data[i].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("CONTINUOUSATTENDANCEALLOWANCE", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (result == 100)
                                {
                                    attendance = Convert.ToDouble(110) * Convert.ToDouble(ContinuousAllowance) / 100;
                                }
                                else if (result >= 95 && result < 100)
                                {
                                    attendance = Convert.ToDouble(100) * Convert.ToDouble(ContinuousAllowance) / 100;
                                }
                                else if (result >= 85 && result < 95)
                                {
                                    attendance = Convert.ToDouble(95) * Convert.ToDouble(ContinuousAllowance) / 100;
                                }
                                else if (result >= 75 && result < 85)
                                {
                                    attendance = Convert.ToDouble(90) * Convert.ToDouble(ContinuousAllowance) / 100;
                                }
                                else
                                {
                                    attendance = 0;
                                }
                                caldata.Add(new ConfigureSalaryComponentModel
                                {


                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,

                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = attendance.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID

                                });
                            }


                            else
                            {
                                caldata.Add(new ConfigureSalaryComponentModel
                                {


                                    PayConfigId = data[i].PayConfigId,
                                    PayConfigName = data[i].PayConfigName,
                                    PayConfigType = data[i].PayConfigType,

                                    IScalculative = data[i].IScalculative,
                                    ISPercentage = data[i].ISPercentage,
                                    ManualRate = data[i].ManualRate,
                                    CalculationFormula = result.ToString(),
                                    ConfigureSalaryID = data[i].ConfigureSalaryID

                                });
                            }
                        }

                    }
                }

            }
            return Json(caldata, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Employee Loan details index page
        public ActionResult EmployeeLoanDetails(string cal)
        {
            cal = DataEncryption.Decrypt(Convert.ToString(cal), "passKey");
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            LoanTransactionModel loanTransactionModel = new LoanTransactionModel();

            loanTransactionModel.Empid = Convert.ToInt32(cal);
            string contents = JsonConvert.SerializeObject(loanTransactionModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetEmployeeLoanTDetails", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    loanTransactionModel.LoanList = JsonConvert.DeserializeObject<List<LoanTransactionModel>>(responseString);
                }
            }
            return View(loanTransactionModel);

        }
        #endregion

        #region Save Loan
        public JsonResult SaveEmployeeLoan(string Id, string Empid, string LoanAmount, string MonthlyInstallment, string TimePeriod)
        {

            var data = "";
            EmployeeLoanModel employeeLoanModel = new EmployeeLoanModel();
            string EmpID = Empid;
            int index = EmpID.IndexOf("&");
            if (index > 0)
            {
                EmpID = EmpID.Substring(0, index);
                EmpID = DataEncryption.Decrypt(Convert.ToString(EmpID), "passKey");
            }
            employeeLoanModel.LoanId = Convert.ToInt32(Id);
            employeeLoanModel.Empid = Convert.ToInt32(EmpID);
            employeeLoanModel.LoanAmount = Convert.ToDouble(LoanAmount);
            employeeLoanModel.MonthlyInstallment = Convert.ToDouble(MonthlyInstallment);
            employeeLoanModel.IsStart = Convert.ToBoolean(TimePeriod);
            //employeeSalaryConfigModel.InternetAllowance = Convert.ToDouble(Internet);
            //employeeSalaryConfigModel.MobileAllowance = Convert.ToDouble(Mobile);


            string contents = JsonConvert.SerializeObject(employeeLoanModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/SaveEmployeeLoan", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }

            //string math = "(10000 - (5000+500))";
            //string value = new DataTable().Compute(math, null).ToString();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Loan Transaction
        public JsonResult SaveEmployeeLoanTransaction(string Id, string Empid, string LoanAmount, string MonthlyInstallment)
        {
            var data = "";
            LoanTransactionModel loanTransactionModel = new LoanTransactionModel();
            loanTransactionModel.LoanId = Convert.ToInt32(Id);
            loanTransactionModel.Empid = Convert.ToInt32(Empid);
            loanTransactionModel.OpeningBalance = Convert.ToDecimal(LoanAmount);
            loanTransactionModel.InstallmentAmount = Convert.ToDecimal(MonthlyInstallment);
            loanTransactionModel.ClosingBalance = Convert.ToDecimal(LoanAmount);

            string contents = JsonConvert.SerializeObject(loanTransactionModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/SaveLoantransaction", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveLoantransaction(string Id, string Empid, string LoanAmount, string mi)
        {
            var data = "";
            LoanTransactionModel loanTransactionModel = new LoanTransactionModel();
            loanTransactionModel.LoanId = Convert.ToInt32(Id);
            loanTransactionModel.Empid = Convert.ToInt32(Empid);
            loanTransactionModel.OpeningBalance = Convert.ToDecimal(LoanAmount);
            loanTransactionModel.InstallmentAmount = Convert.ToDecimal(mi);
            loanTransactionModel.ClosingBalance = Convert.ToDecimal(Convert.ToDouble(LoanAmount) - Convert.ToDouble(mi));



            string contents = JsonConvert.SerializeObject(loanTransactionModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/SaveLoantransaction", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Get Employee Loan Details
        public List<EmployeeLoanModel> GetEmployeeLoanDetailsById(string Empid)
        {

            List<EmployeeLoanModel> Loandata = new List<EmployeeLoanModel>();
            string EmpID = Empid;
            int index = EmpID.IndexOf("&");
            if (index > 0)
            {
                EmpID = EmpID.Substring(0, index);
                EmpID = DataEncryption.Decrypt(Convert.ToString(EmpID), "passKey");
            }

            //string empid = Request.QueryString["EmpID"];
            //EmployeeLoanModel.Empid = Convert.ToInt32(empid);
            EmployeeLoanModel.Empid = Convert.ToInt32(EmpID);
            string Loancontents = JsonConvert.SerializeObject(EmployeeLoanModel);
            HttpResponseMessage Loanresponse = ObjAPI.CallAPI("api/Employee/GetEmployeeLoanDetails", Loancontents);
            if (Loanresponse.IsSuccessStatusCode)
            {
                string responseString = Loanresponse.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    Loandata = JsonConvert.DeserializeObject<List<EmployeeLoanModel>>(responseString); ;
                }
            }
            return Loandata;
        }
        public JsonResult GetEmployeeLoanDetails(string Empid)
        {
            List<EmployeeLoanModel> Loandata = GetEmployeeLoanDetailsById(Empid);
            return Json(Loandata, JsonRequestBehavior.AllowGet);
        }
        #endregion  
        #region save all employee Salary details 
        // Salary Process Mayukh
        [HttpPost]
        public JsonResult CalculateAllEmployeeSalary(string modelParameter)
        {
            var savedata = "";
            string[] arrayRowid = modelParameter.Split(',');
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            try
            {
                for (int i = 0; i < arrayRowid.Length; i++)
                {
                    List<EmployeeListModel> data = GetEmployeeListById(arrayRowid[i]);
                    if (data.Count > 0)
                    {
                        // Duplicate Reccord checking for Each Employee,Month,Year
                        List<EmployeeSalaryModel> Salarydata = new List<EmployeeSalaryModel>();
                        employeeSalaryModel.Empid = Convert.ToInt32(data[0].empid);
                        employeeSalaryModel.Month = fullmonthname;
                        employeeSalaryModel.Year = Year;
                        string Salarycontents = JsonConvert.SerializeObject(employeeSalaryModel);
                        HttpResponseMessage Salaryresponse = ObjAPI.CallAPI("api/Employee/EmployeeDuplicateSalarycheck", Salarycontents);
                        if (Salaryresponse.IsSuccessStatusCode)
                        {
                            string responseString = Salaryresponse.Content.ReadAsStringAsync().Result;
                            if (!string.IsNullOrEmpty(responseString))
                            {
                                Salarydata = JsonConvert.DeserializeObject<List<EmployeeSalaryModel>>(responseString);
                            }
                        }
                        // When Duplicate record exist no insertion of data
                        if (Salarydata.Count > 0)
                        {
                            savedata = 0.ToString();
                        }
                        else
                        {
                            // Get Manual fields Employee wise
                            List<EmployeeSalaryConfigModel> EmployeeSalarydata = GetEmployeeSalaryManualData(data[0].empid.ToString());
                            double TDS = 0;
                            double Basic = 0;
                            double GrossAmount = 0;
                            double InternetAllowance = 0;
                            double ContinuousAllowance = 0;
                            double MobileAllowance = 0;
                            double stipend = 0;
                            double Arrears = 0;
                            bool IsPtaxActive = false;
                            double result = 0;
                            double tallowance = 0;
                            double specialallowance = 0;
                            double performanceallowance = 0;
                            double FoodAllowance = 0;
                            double vehicalAllowance = 0;

                            // Manual Data PTax Checking
                            if (EmployeeSalarydata.Count > 0)
                            {
                                if (EmployeeSalarydata[0].IsPTaxActive != null)
                                {
                                    IsPtaxActive = Convert.ToBoolean(EmployeeSalarydata[0].IsPTaxActive);
                                }
                            }
                            // Manual Data loop and put the value in variable
                            for (int p = 0; p < EmployeeSalarydata.Count; p++)
                            {
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("BASIC", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Basic = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("GROSSAMOUNT", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    GrossAmount = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("INTERNETALLOWANCE", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    InternetAllowance = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("CONTINUOUSALLOWANCE", StringComparison.InvariantCultureIgnoreCase) ||
                                    Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("CONTINUOUSATTENDANCEALLOWANCE", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    ContinuousAllowance = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("MOBILEALLOWANCE", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    MobileAllowance = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }

                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("STIPEND", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    stipend = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }

                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("ARREARS", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("ARREAR", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Arrears = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("SPECIALALLOWANCE", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("SPECIALALLOWANCES", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    specialallowance = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("PERFORMANCEALLOWANCE", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("PERFORMANCEALLOWANCES", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    performanceallowance = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("FOODALLOWANCE", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("FOODALLOWANCES", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    FoodAllowance = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("VEHICALALLOWANCE", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("VEHICALALLOWANCES", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    vehicalAllowance = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }
                                if (Regex.Replace(EmployeeSalarydata[p].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim().Equals("TDS", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    TDS = Convert.ToDouble(EmployeeSalarydata[p].Values);
                                }

                            }

                            double Totalallowances = 0;
                            double TotalDeduction = 0;
                            double Netamount = 0;
                            double Loanamount = 0;
                            employeeSalaryModel.Empid = data[0].empid;
                            employeeSalaryModel.Empno = data[0].empno;
                            employeeSalaryModel.PayGroupId = data[0].PayGroupID;

                            List<EmployeeBonusModel> PayBonusdata = GetEmployeeBonusDataById(data[0].empid.ToString());
                            List<ConfigureSalaryComponentModel> condata = GetConfigureSalaryComponent(data[0].SalaryConfigureID.ToString());
                            // Mayukh New 
                            employeeSalaryModel.Loan_Installment = Employee_Loan_Amount(data[0].empid);
                            double leavedays = 0;
                            List<EmployeeLeaveModel> dataleave = GetEmployeeLeave(data[0].empno.ToString());
                            if (dataleave.Count > 0)
                            {
                                leavedays = Convert.ToDouble(dataleave[0].noofdays) + Convert.ToDouble(dataleave[0].lateleave);
                            }
                            decimal onedayamt = 0;
                            double leaveamt = 0;
                            var ptax = 0;
                            var attendance = 0.0;
                            List<EmployeeSalaryConfigModel> Continuousdata = GetcontinuousAllowance(data[0].empid.ToString(), Convert.ToString(Basic));
                            if (Continuousdata.Count > 0)
                            {
                                employeeSalaryModel.ProductionBonus = Convert.ToDouble(Continuousdata[0].ProductionBonus);
                                employeeSalaryModel.Overtime_Allowance = Convert.ToDouble(Continuousdata[0].OvertimeAllowance);
                            }
                            if (EmployeeSalarydata[0].SalaryConfigureType == 1)
                            {
                                employeeSalaryModel.Continuous_Attendance_Allowance = 0;
                            }
                            else
                            {
                                employeeSalaryModel.Continuous_Attendance_Allowance = Convert.ToDouble(Continuousdata[0].ContinuousAllowance);
                            }
                            var calformula = "";
                            var ManualRate = "";
                            List<ConfigureSalaryComponentModel> caldata = new List<ConfigureSalaryComponentModel>();
                            for (int j = 0; j < condata.Count; j++)
                            {
                                double factoryworkergrossamount = 0;
                                calformula = condata[j].CalculationFormula;
                                ManualRate = Convert.ToString(condata[j].ManualRate);
                                if (ManualRate == null || ManualRate == "")
                                {
                                    ManualRate = "100";
                                }
                                if (calformula == null)
                                {
                                    calformula = 0.ToString();
                                }

                                DateTime Today = DateTime.Now.AddMonths(0);
                                DateTime JoiningDate = data[0].Joining.HasValue ? Convert.ToDateTime(data[0].Joining) : Convert.ToDateTime(null);

                                if (EmployeeSalarydata[0].SalaryConfigureType == 1)
                                {
                                    for (int p = 0; p < EmployeeSalarydata.Count; p++)
                                    {
                                        calformula = calformula.Replace(EmployeeSalarydata[p].PayConfigName, Convert.ToString(EmployeeSalarydata[p].Values));
                                    }

                                    calformula = calformula.Replace("–", "-");
                                    calformula = calformula.Replace("%", Convert.ToString("/100"));
                                    DataTable dt = new DataTable();
                                    string math = calformula;
                                    double res = Convert.ToDouble(new DataTable().Compute(math, null));
                                    result = Convert.ToDouble(res) * Convert.ToDouble(ManualRate) / 100;
                                    result = Math.Round(result);
                                }
                                else
                                {
                                    for (int p = 0; p < EmployeeSalarydata.Count; p++)
                                    {
                                        calformula = calformula.Replace(EmployeeSalarydata[p].PayConfigName, Convert.ToString(EmployeeSalarydata[p].Values));
                                        string payconfigname = EmployeeSalarydata[p].PayConfigName.Trim().Replace(@"[^0-9a-zA-Z]+", "");
                                        if (payconfigname.Equals("arrears", StringComparison.InvariantCultureIgnoreCase) ||
                                            payconfigname.Equals("arrear", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            factoryworkergrossamount = factoryworkergrossamount + 0;
                                        }
                                        else
                                        {
                                            factoryworkergrossamount = factoryworkergrossamount + Convert.ToDouble(EmployeeSalarydata[p].Values);
                                        }

                                    }


                                    factoryworkergrossamount = factoryworkergrossamount + Math.Round(Convert.ToDouble(Continuousdata[0].ContinuousAllowance)) + Math.Round(Convert.ToDouble(Continuousdata[0].OvertimeAllowance)) + Math.Round(Convert.ToDouble(Continuousdata[0].ProductionBonus));
                                    if (condata[j].PayConfigType == "Allowances" || condata[j].PayConfigType == "Allowance")
                                    {
                                        calformula = calformula.Replace("Gross Amount", Convert.ToString(factoryworkergrossamount));
                                        calformula = calformula.Replace("Gross Amounts", Convert.ToString(factoryworkergrossamount));
                                        calformula = calformula.Replace("GROSS AMOUNTS", Convert.ToString(factoryworkergrossamount));
                                        calformula = calformula.Replace("GROSS AMOUNT", Convert.ToString(factoryworkergrossamount));

                                        calformula = calformula.Replace("–", "-");
                                        calformula = calformula.Replace("%", Convert.ToString("/100"));
                                        DataTable dt = new DataTable();
                                        string math = calformula;
                                        double res = Convert.ToDouble(new DataTable().Compute(math, null));
                                        result = Convert.ToDouble(res) * Convert.ToDouble(ManualRate) / 100;
                                        result = Math.Round(result);
                                        tallowance = result + tallowance;
                                        GrossAmount = tallowance + factoryworkergrossamount;
                                    }

                                    else
                                    {
                                        calformula = calformula.Replace("Gross Amount", Convert.ToString(GrossAmount));
                                        calformula = calformula.Replace("Gross Amounts", Convert.ToString(GrossAmount));
                                        calformula = calformula.Replace("GROSS AMOUNTS", Convert.ToString(GrossAmount));
                                        calformula = calformula.Replace("GROSS AMOUNT", Convert.ToString(GrossAmount));
                                        calformula = calformula.Replace("–", "-");
                                        calformula = calformula.Replace("%", Convert.ToString("/100"));
                                        DataTable dt = new DataTable();
                                        string math = calformula;
                                        double res = Convert.ToDouble(new DataTable().Compute(math, null));
                                        result = Convert.ToDouble(res) * Convert.ToDouble(ManualRate) / 100;
                                        if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ESIC", StringComparison.InvariantCultureIgnoreCase)
                                            || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ESI", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            result = Math.Ceiling(result);
                                        }
                                        else
                                        {
                                            result = Math.Round(result);
                                        }


                                    }
                                }
                                var name = Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Trim();
                                if (name.Equals("ptax", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.P_Tax = Employee_PTax_Value(GrossAmount);// ptax;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("PF", StringComparison.InvariantCultureIgnoreCase) ||
                                    Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("PFCONTRIBUTION", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.PF = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ESIC", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ESI", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (GrossAmount >= 21000)
                                    {
                                        employeeSalaryModel.ESI = 0;
                                    }
                                    else
                                    {
                                        employeeSalaryModel.ESI = result;
                                    }
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("AttireAllowance", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.Attire_Allowance = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("HRA", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.HRA = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("EducationAllowance", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("EducationAllowances", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.Education_Allowances = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("MedicalAllowance", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("MedicalAllowances", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.Medical_Allowances = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("BOOKSPERIODICAL", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("BOOKSPERIODICALAllowances", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.Books___Periodical = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ConveyenceAllowance", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ConveyenceAllowances", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.Conveyence_Allowance = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("TrainingAllowance", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("TrainingAllowances", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.Training__Allowance = result;
                                }
                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("LTA", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.LTA = result;
                                }

                                if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("LodgingAllowance", StringComparison.InvariantCultureIgnoreCase)
                                    || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("LodgingAllowances", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    employeeSalaryModel.Lodging_Allowance = result;
                                }
                                if (condata[j].PayConfigType == "Allowances")
                                {
                                    if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ContinuousAttendanceAllowance", StringComparison.InvariantCultureIgnoreCase) ||
                                        Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ContinuousAllowance", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        Totalallowances = Totalallowances + attendance;
                                    }
                                    else
                                    {
                                        Totalallowances = Totalallowances + result;
                                    }
                                }
                                else
                                {
                                    if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ptax", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        TotalDeduction = TotalDeduction + ptax;
                                    }
                                    else
                                    {
                                        if (Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ESIC", StringComparison.InvariantCultureIgnoreCase)
                                            || Regex.Replace(condata[j].PayConfigName, @"[^0-9a-zA-Z]+", "").Equals("ESI", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            if (GrossAmount >= 21000)
                                            {
                                                result = 0;
                                            }
                                        }
                                        TotalDeduction = TotalDeduction + result;
                                    }
                                }

                            }


                            TotalDeduction = Math.Round(Convert.ToDouble(TotalDeduction) + Convert.ToDouble(Loanamount));
                            onedayamt = Math.Round(Convert.ToDecimal(GrossAmount) / 30, 2);
                            leaveamt = Convert.ToDouble(onedayamt) * Convert.ToDouble(leavedays);
                            employeeSalaryModel.Leave_Amount = leaveamt;
                            TotalDeduction = Math.Round(Convert.ToDouble(TotalDeduction) + Convert.ToDouble(leaveamt));

                            for (int v = 0; v < EmployeeSalarydata.Count; v++)
                            {
                                if (EmployeeSalarydata[v].PayConfigType == "Allowances")
                                    Totalallowances = Math.Round(Convert.ToDouble(Totalallowances) + Convert.ToDouble(EmployeeSalarydata[v].Values));
                            }
                            for (int v = 0; v < EmployeeSalarydata.Count; v++)
                            {
                                if (EmployeeSalarydata[v].PayConfigType == "Deduction")
                                    TotalDeduction = Math.Round(Convert.ToDouble(TotalDeduction) + Convert.ToDouble(EmployeeSalarydata[v].Values));
                            }
                            if (EmployeeSalarydata[0].SalaryConfigureType == 1)
                            {
                                Totalallowances = Math.Round(Convert.ToDouble(Totalallowances) - Convert.ToDouble(Basic) - Convert.ToDouble(GrossAmount));
                            }
                            else
                            {
                                Totalallowances = Math.Round(Convert.ToDouble(Totalallowances) - Convert.ToDouble(Basic) /*- Convert.ToDouble(GrossAmount)*/);

                            }

                            employeeSalaryModel.Food_Allowance = FoodAllowance;
                            employeeSalaryModel.Vehical_Allownce = vehicalAllowance;
                            employeeSalaryModel.STIPEND = stipend;
                            employeeSalaryModel.Basic = Basic;
                            employeeSalaryModel.PerformanceAllowance = performanceallowance;
                            employeeSalaryModel.SpecialAllowance = specialallowance;
                            employeeSalaryModel.Gross_Amount = GrossAmount;
                            employeeSalaryModel.Arrears = Arrears;
                            employeeSalaryModel.Internet_Allowance = InternetAllowance;
                            employeeSalaryModel.Mobile_Allowance = MobileAllowance;
                            employeeSalaryModel.Total_Allowances = Totalallowances;
                            employeeSalaryModel.Total_Deduction = TotalDeduction;
                            Netamount = (Convert.ToDouble(GrossAmount) + Convert.ToDouble(MobileAllowance) + Convert.ToDouble(Arrears)
                                //Convert.ToDouble(employeeSalaryModel.Continuous_Attendance_Allowance) + Convert.ToDouble(Arrears)+ Convert.ToDouble(employeeSalaryModel.Overtime_Allowance)
                                - Convert.ToDouble(TotalDeduction));
                            employeeSalaryModel.Net_Payable_Amount = Math.Round(Netamount);
                            employeeSalaryModel.TDS = TDS;

                            employeeSalaryModel.Month = fullmonthname;
                            employeeSalaryModel.Year = Year;

                            string Savecontents = JsonConvert.SerializeObject(employeeSalaryModel);
                            HttpResponseMessage saveresponse = ObjAPI.CallAPI("api/Employee/SaveallempSalary", Savecontents);
                            if (saveresponse.IsSuccessStatusCode)
                            {
                                string responseString = saveresponse.Content.ReadAsStringAsync().Result;

                                if (!string.IsNullOrEmpty(responseString))
                                {
                                    savedata = JsonConvert.DeserializeObject(responseString).ToString();
                                }
                            }

                        }
                    }
                    else
                    {

                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(savedata, JsonRequestBehavior.AllowGet);
        }
        private string createEmailBody(int Empid, string Months, int Years)
        {
            double lateleaveamount = 0;
            double leaveamount = 0;
            int K = 0;
            int p = 0;
            double onedayamt = 0;
            string body = string.Empty;
            int monthno = DateTime.ParseExact(Months, "MMMM", CultureInfo.CurrentCulture).Month;
            List<IndividualSalryModel> Emplist = GetIndividualSalaryData(Years, monthno, Empid, Months);
            if (Emplist.Count > 0)
            {

                using (StreamReader reader = new StreamReader(Server.MapPath("~/HtmlPages/PaySlipdesign.html")))
                {
                    body = reader.ReadToEnd();
                }
                onedayamt = (Emplist[0].Gross_Amount != null || Emplist[0].Gross_Amount.ToString() != "") ? Math.Round(Convert.ToDouble(Emplist[0].Gross_Amount) / 30, 2) : 0;
                leaveamount = Convert.ToDouble(onedayamt) * Convert.ToDouble((Emplist[0].AbsentDays == null || Emplist[0].AbsentDays == "") ? 0.ToString() : Emplist[0].AbsentDays);
                lateleaveamount = Convert.ToDouble(onedayamt) * Convert.ToDouble((Emplist[0].LateLeave == null || Emplist[0].LateLeave == "") ? 0.ToString() : Emplist[0].LateLeave);
                int daysinmonth = DateTime.DaysInMonth(Year, Convert.ToInt32(monthno));
                body = body.Replace("{TotalLeave}", (Emplist[0].AbsentDays == null || Emplist[0].AbsentDays == "") ? 0.ToString() : Emplist[0].AbsentDays.ToString()); //replacing the required things  
                body = body.Replace("{WorkingDays}", daysinmonth.ToString());
                body = body.Replace("{EarnedLeave}", (Emplist[0].EarnedLeaveDays == null || Emplist[0].EarnedLeaveDays == "") ? 0.ToString() : Emplist[0].EarnedLeaveDays.ToString());
                body = body.Replace("{AbsentDays}", (Emplist[0].TotalLeaveDays == null || Emplist[0].TotalLeaveDays == "") ? 0.ToString() : Emplist[0].TotalLeaveDays.ToString());
                body = body.Replace("{TotalLeaveAmount}", leaveamount.ToString());
                body = body.Replace("{Latedays}", (Emplist[0].LateLeave == null || Emplist[0].LateLeave == "") ? 0.ToString() : Emplist[0].LateLeave.ToString());
                body = body.Replace("{LateAmount}", lateleaveamount.ToString());
                body = body.Replace("{Month}", Emplist[0].Month == null ? "" : Emplist[0].Month.ToString()); //replacing the required things  
                body = body.Replace("{Year}", Emplist[0].Year == null ? "" : Emplist[0].Year.ToString());
                body = body.Replace("{EName}", Emplist[0].EmployeeName == null ? "" : Emplist[0].EmployeeName.ToString());
                body = body.Replace("{Company}", Emplist[0].Month == null ? "" : Emplist[0].Company.ToString());
                body = body.Replace("{CompanyAddress}", Emplist[0].CompanyAddress == null ? "" : Emplist[0].CompanyAddress.ToString());
                body = body.Replace("{EMPNO}", Emplist[0].Empno == null ? "" : Emplist[0].Empno.ToString()); //replacing the required things  
                body = body.Replace("{Pan}", Emplist[0].emppancardno == null ? "" : Emplist[0].emppancardno.ToString());
                body = body.Replace("{UAN}", Emplist[0].empuanno == null ? "" : Emplist[0].empuanno.ToString());
                body = body.Replace("{PFACNO}", Emplist[0].empepfno == null ? "" : Emplist[0].empepfno.ToString());
                body = body.Replace("{ESICNO}", Emplist[0].empesicno == null ? "" : Emplist[0].empesicno.ToString());
                body = body.Replace("{BANK}", Emplist[0].empaccountno == null ? "" : Emplist[0].empaccountno.ToString());
                body = body.Replace("{Department}", Emplist[0].Department == null ? "" : Emplist[0].Department.ToString());
                body = body.Replace("{Designation}", Emplist[0].Designation == null ? "" : Emplist[0].Designation.ToString());
                body = body.Replace("{Basic}", Emplist[0].Basic == null ? "0" : Emplist[0].Basic.ToString());
                body = body.Replace("{Leave}", Emplist[0].Leave_Amount == null ? "0" : Emplist[0].Leave_Amount.ToString());
                body = body.Replace("{Joining}", Emplist[0].joining == null ? "" : Convert.ToDateTime(Emplist[0].joining).ToString("dd/MM/yyyy"));
                body = body.Replace("{Location}", "");
                string dynamictable = "<table style='width:80%;' align='center'class='table-bordered table-new'>";
                dynamictable += "<tr style='height:25px;'>";
                dynamictable += "<td style='width:50%;' valign='top'>";
                dynamictable += "<table style='width:100%;float:left' class='table-bordered table-new'>";
                dynamictable += "<thead><tr style='height:25px;'>";
                dynamictable += "<th scope='col' style='background-color:gray;color:white;'>Earnings</th>";
                dynamictable += "<th scope='col' style='background-color:gray;color:white;'>Amount</th>";
                dynamictable += "</tr></thead><tbody>";
                if (Emplist[0].Basic > 0)
                {
                    K++;
                    if (K % 2 == 0)
                    {
                        dynamictable += "<tr style='height:25px;'><th scope='row'>Basic</th><td scope='col' style=''>" + Emplist[0].Basic + "</td></tr>";
                    }
                    else
                    {
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row'>Basic</th><td scope='col' style=''>" + Emplist[0].Basic + "</td></tr>";
                    }
                }
                if (Emplist[0].Stipend > 0)
                {
                    K++;
                    if (K % 2 == 0)
                    {
                        dynamictable += "<tr style='height:25px;'><th scope='row'>Stipend</th><td scope='col' style=''>" + Emplist[0].Stipend + "</td></tr>";
                    }
                    else
                    {
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row'>Stipend</th><td scope='col' style=''>" + Emplist[0].Stipend + "</td></tr>";
                    }
                }

                if (Emplist[0].Mobile_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Mobile Allowance</th><td scope='col' style=''>" + Emplist[0].Mobile_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Mobile Allowance</th><td scope='col' style=''>" + Emplist[0].Mobile_Allowance + "</td></tr>";

                }
                if (Emplist[0].Internet_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Internet Allowance</th><td scope='col' style=''>" + Emplist[0].Internet_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Internet Allowance</th><td scope='col' style=''>" + Emplist[0].Internet_Allowance + "</td></tr>";
                }
                if (Emplist[0].HRA > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>House Rent Allowance</th><td scope='col' style=''>" + Emplist[0].HRA + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>House Rent Allowance</th><td scope='col' style=''>" + Emplist[0].HRA + "</td></tr>";

                }
                if (Emplist[0].Education_Allowances > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Education Allowance</th><td scope='col' style=''>" + Emplist[0].Education_Allowances + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Education Allowance</th><td scope='col' style=''>" + Emplist[0].Education_Allowances + "</td></tr>";
                }
                if (Emplist[0].Medical_Allowances > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Medical Allowance</th><td scope='col' style=''>" + Emplist[0].Medical_Allowances + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Medical Allowance</th><td scope='col' style=''>" + Emplist[0].Medical_Allowances + "</td></tr>";

                }
                if (Emplist[0].Lodging_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Lodging Allowance</th><td scope='col' style=''>" + Emplist[0].Lodging_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Lodging Allowance</th><td scope='col' style=''>" + Emplist[0].Lodging_Allowance + "</td></tr>";

                }
                if (Emplist[0].Books___Periodical > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Books & Periodical</th><td scope='col' style=''>" + Emplist[0].Books___Periodical + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Books & Periodical</th><td scope='col' style=''>" + Emplist[0].Books___Periodical + "</td></tr>";
                }
                if (Emplist[0].Attire_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Attire Allowances</th><td scope='col' style=''>" + Emplist[0].Attire_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Attire Allowances</th><td scope='col' style=''>" + Emplist[0].Attire_Allowance + "</td></tr>";

                }
                if (Emplist[0].LTA > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>LTA</th><td scope='col' style=''>" + Emplist[0].LTA + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>LTA</th><td scope='col' style=''>" + Emplist[0].LTA + "</td></tr>";

                }
                if (Emplist[0].Training__Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Training Allowances</th><td scope='col' style=''>" + Emplist[0].Training__Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Training Allowances</th><td scope='col' style=''>" + Emplist[0].Training__Allowance + "</td></tr>";

                }
                if (Emplist[0].Food_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Food Allowances</th><td scope='col' style=''>" + Emplist[0].Food_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Food Allowances</th><td scope='col' style=''>" + Emplist[0].Food_Allowance + "</td></tr>";

                }
                if (Emplist[0].Vehical_Allownce > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Vehical Allowances</th><td scope='col' style=''>" + Emplist[0].Vehical_Allownce + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Vehical Allowances</th><td scope='col' style=''>" + Emplist[0].Vehical_Allownce + "</td></tr>";

                }
                if (Emplist[0].Conveyence_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Conveyence Allowances</th><td scope='col' style=''>" + Emplist[0].Conveyence_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Conveyence Allowances</th><td scope='col' style=''>" + Emplist[0].Conveyence_Allowance + "</td></tr>";
                }
                if (Emplist[0].PerformanceAllowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Performance Allowances</th><td scope='col' style=''>" + Emplist[0].PerformanceAllowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Performance Allowances</th><td scope='col' style=''>" + Emplist[0].PerformanceAllowance + "</td></tr>";

                }
                if (Emplist[0].SpecialAllowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Special Allowances</th><td scope='col' style=''>" + Emplist[0].SpecialAllowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Special Allowances</th><td scope='col' style=''>" + Emplist[0].SpecialAllowance + "</td></tr>";

                }
                if (Emplist[0].Continuous_Attendance_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Continuous Attendance Allowance</th><td scope='col' style=''>" + Emplist[0].Continuous_Attendance_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Continuous Attendance Allowance</th><td scope='col' style=''>" + Emplist[0].Continuous_Attendance_Allowance + "</td></tr>";

                }
                if (Emplist[0].Overtime_Allowance > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Overtime Allowance</th><td scope='col' style=''>" + Emplist[0].Overtime_Allowance + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Overtime Allowance</th><td scope='col' style=''>" + Emplist[0].Overtime_Allowance + "</td></tr>";

                }
                if (Emplist[0].ProductionBonus > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Production Bonus</th><td scope='col' style=''>" + Emplist[0].ProductionBonus + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Production Bonus</th><td scope='col' style=''>" + Emplist[0].ProductionBonus + "</td></tr>";

                }
                if (Emplist[0].Arrears > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Arrears</th><td scope='col' style=''>" + Emplist[0].Arrears + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Arrears</th><td scope='col' style=''>" + Emplist[0].Arrears + "</td></tr>";

                }
                if (Emplist[0].Gross_Amount > 0)
                {
                    K++;
                    if (K % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style=''>Gross Amount</th><td scope='col' style=''>" + Emplist[0].Gross_Amount + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style=''>Gross Amount</th><td scope='col' style=''>" + Emplist[0].Gross_Amount + "</td></tr>";

                }



                dynamictable += "</tbody></table></td>";
                dynamictable += "<td style='width:50%;' valign='top'>";
                dynamictable += "<table style='width:100%;float:right;'class='table-bordered table-new'>";
                dynamictable += "<thead> <tr style='height:25px;'>";
                dynamictable += "<th scope='col' style='background-color:gray;color:white;'>Deduction</th>";
                dynamictable += "<th scope='col' style='background-color:gray;color:white;'>Amount</th>";
                dynamictable += "</tr></thead><tbody>";
                if (Emplist[0].PF > 0)
                {
                    p++;
                    if (p % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style='width:25%;'>PF</th><td scope='col' style=''>" + Emplist[0].PF + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style='width:25%;'>PF</th><td scope='col' style=''>" + Emplist[0].PF + "</td></tr>";

                }
                if (Emplist[0].ESI > 0)
                {
                    p++;
                    if (p % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style='width:25%;'>ESIC</th><td scope='col' style=''>" + Emplist[0].ESI + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style='width:25%;'>ESIC</th><td scope='col' style=''>" + Emplist[0].ESI + "</td></tr>";

                }
                if (Emplist[0].P_Tax > 0)
                {
                    p++;
                    if (p % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style='width:25%;'>P Tax</th><td scope='col' style=''>" + Emplist[0].P_Tax + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style='width:25%;'>P Tax</th><td scope='col' style=''>" + Emplist[0].P_Tax + "</td></tr>";

                }
                if (Emplist[0].TDS > 0)
                {
                    p++;
                    if (p % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style='width:25%;'>TDS</th><td scope='col' style=''>" + Emplist[0].TDS + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style='width:25%;'>TDS</th><td scope='col' style=''>" + Emplist[0].TDS + "</td></tr>";

                }
                if (Convert.ToDouble(Emplist[0].Leave_Amount) > 0)
                {
                    p++;
                    if (p % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style='width:25%;'>Deduction Due to Late And Late</th><td scope='col' style=''>" + Emplist[0].Leave_Amount + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style='width:25%;'>Deduction Due to Late And Late</th><td scope='col' style=''>" + Emplist[0].Leave_Amount + "</td></tr>";

                }
                if (Emplist[0].Loan_Installment > 0)
                {
                    p++;
                    if (p % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style='width:25%;'>Loan Installment</th><td scope='col' style=''>" + Emplist[0].Loan_Installment + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style='width:25%;'>Loan Installment</th><td scope='col' style=''>" + Emplist[0].Loan_Installment + "</td></tr>";

                }
                if (Emplist[0].Total_Deduction > 0)
                {
                    p++;
                    if (p % 2 == 0)
                        dynamictable += "<tr style='height:25px;'><th scope='row' style='width:25%;'>Total Deduction</th><td scope='col' style=''>" + Emplist[0].Total_Deduction + "</td></tr>";
                    else
                        dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'><th scope='row' style='width:25%;'>Total Deduction</th><td scope='col' style=''>" + Emplist[0].Total_Deduction + "</td></tr>";

                }
                dynamictable += "</tbody></table></td>";
                dynamictable += "</tr>";
                dynamictable += "<tr style='height:25px;'>";
                dynamictable += "<th scope='row' style='font-size: 18px;'>Net Amount</th>";
                dynamictable += "<td scope='col' style='font-size: 16px;'><b>" + Emplist[0].Net_Payable_Amount + "</b></td>";
                dynamictable += "</tr>";
                dynamictable += "<tr style='height:25px;background-color:#f9f9f9;'>";
                dynamictable += "<th scope='row' style='font-size: 18px;'>Amount in Words</th>";
                dynamictable += "<td scope='col' style='font-size: 16px;'><b>" + ConvertNumbertoWords(Convert.ToInt64(Emplist[0].Net_Payable_Amount)) + "</b></td>";
                dynamictable += "</tr>";
                dynamictable += "</table>";
                body = body.Replace("{Content}", dynamictable);


            }

            StringBuilder sb = new StringBuilder();
            sb.Append(body);

            //sb.Append("</div>");


            return sb.ToString();

        }
        #endregion

        #region SalaryCheck

        public JsonResult EmpSalaryCheck()
        {
            int savedata;
            List<EmployeeListModel> employeedata = GetAllEmployee();
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            employeeSalaryModel.Month = fullmonthname;
            employeeSalaryModel.Year = Year;
            var data = "";
            string contents = JsonConvert.SerializeObject(employeeSalaryModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/EmployeeTotalinsertedinamonth", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }
            if (employeedata.Count == Convert.ToInt32(data) || employeedata.Count < Convert.ToInt32(data))
            {
                savedata = 1;
            }
            else
            {
                savedata = 0;
            }
            return Json(savedata, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult ViewSalary()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            #region bind Employee
            EmployeeListModel employeeListModel = new EmployeeListModel();
            List<SelectListItem> EmpList = new List<SelectListItem>();
            EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            employeeListModel.empid = 0;
            List<EmployeeListModel> employeedata = GetAllEmployee();
            foreach (var items in employeedata)
            {
                EmpList.Add(new SelectListItem
                {
                    Text = items.EmployeeName,
                    Value = items.empid.ToString()
                });
            }
            #endregion
            employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.MonthList = GetMonthList();
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);
        }


        #region Bind Month&Year
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

        #endregion

        #region ViewAllSalary
        public JsonResult GetAllEmployeeSalary(string Month, string Year, string Empid, string Status, int numberofRow, int pagenumber)
        {

            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            employeeSalaryModel.Month = Month;
            employeeSalaryModel.PageNumber = pagenumber;
            employeeSalaryModel.Rowofpage = numberofRow;
            employeeSalaryModel.Year = Convert.ToInt32(Year);
            employeeSalaryModel.Empid = Convert.ToInt32(Empid);
            employeeSalaryModel.Status = Status;
            int month = DateTime.ParseExact(Month, "MMMM", CultureInfo.CurrentCulture).Month;
            DateTime first = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(month), 1);

            DateTime last = first.AddMonths(1).AddSeconds(-1);
            string firstDayOfMonth = first.ToString();
            string lastDayOfMonth = last.ToString();
            List<EmployeeSalaryModel> data = new List<EmployeeSalaryModel>();
            string contents = JsonConvert.SerializeObject(employeeSalaryModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/EmployeeSalaryview_ByPageWise", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmployeeSalaryModel>>(responseString);
                }
            }
            List<EmployeeSalaryConfigModel> SalaryData = CheckTDS();
            var matchData = data.Where(t2 => SalaryData.Any(t1 => t1.PayGroupId == t2.PayGroupId)).ToList();
            data = data.Where(t2 => !SalaryData.Any(t1 => t1.PayGroupId == t2.PayGroupId)).ToList();
            if (matchData.Count > 0)
            {
                foreach (var X in matchData)
                {

                    data.Add(new EmployeeSalaryModel
                    {
                        Empid = X.Empid,
                        Empno = X.Empno,
                        EmployeeName = X.EmployeeName,
                        PayGroupId = X.PayGroupId,
                        PayGroupeName = X.PayGroupeName,
                        Basic = X.Basic,
                        Gross_Amount = X.Gross_Amount,
                        Total_Allowances = X.Total_Allowances,
                        Total_Deduction = X.Total_Deduction,
                        Net_Payable_Amount = X.Net_Payable_Amount,
                        IsPaid = X.IsPaid,
                        ISTDS = 1
                    });
                }
            }
            data = data.OrderBy(x => x.EmployeeName).ToList();
            if (data.Count > 0)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    data[i].sEmpid = DataEncryption.Encrypt(Convert.ToString(data[i].Empid), "passKey");
                }
                data[0].TotalCount = GetAllEmployeeSalary_Count(contents);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public int GetAllEmployeeSalary_Count(string contents)
        {

            int Totalcount = 0;
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/AllEmployeeSalaryview_Count", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    Totalcount = Convert.ToInt32(responseString);
                }
            }

            return Totalcount;
        }




        #region excel
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
        [HttpPost]
        public ActionResult ExcelAllEmployeeSalary(string MonthName, string year)
        {

            List<EmployeeListModel> employeedata = GetAllEmployee();
            int[] aray = new int[employeedata.Count];
            for (int a = 0; a < employeedata.Count; a++)
            {
                aray[a] = employeedata[a].empid;
            }
            int month = DateTime.ParseExact(MonthName, "MMMM", CultureInfo.CurrentCulture).Month;
            ExportExcelModel exportExcelModel = new ExportExcelModel();
            List<ExportExcelModel> exceldata = new List<ExportExcelModel>();
            for (int i = 0; i < aray.Length; i++)
            {
                string SID = DataEncryption.Encrypt(Convert.ToString(aray[i]), "passKey");


                List<EmpbasicModel> data = GetEmployeeDetailsById(SID);
                if (data.Count > 0)
                {
                    exportExcelModel.Empid = Convert.ToInt32(data[0].empid);
                    exportExcelModel.empno = Convert.ToInt32(data[0].empno);
                    DateTime afirst = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1);
                    DateTime alast = afirst.AddMonths(1).AddSeconds(-1);

                    exportExcelModel.Prefixfromdate = afirst;
                    exportExcelModel.suffixtodate = alast;
                    exportExcelModel.Month = MonthName;
                    exportExcelModel.year = Convert.ToInt32(year);
                    exportExcelModel.TotalDays = days;

                    //List<ExportExcelModel> individualdata = new List<ExportExcelModel>();
                    //string excelcontents = JsonConvert.SerializeObject(exportExcelModel);
                    //HttpResponseMessage excelresponse = ObjAPI.CallAPI("api/Employee/GetEmployeeSalaryDetailsforexcel", excelcontents);
                    //if (excelresponse.IsSuccessStatusCode)
                    //{
                    //    string responseString = excelresponse.Content.ReadAsStringAsync().Result;

                    //    if (!string.IsNullOrEmpty(responseString))
                    //    {
                    //        individualdata = JsonConvert.DeserializeObject<List<ExportExcelModel>>(responseString); ;
                    //    }
                    //}

                    // New Mayukh 
                    String SDate = Convert.ToDateTime(afirst).ToString("dd/MMM/yyyy");
                    String EDate = Convert.ToDateTime(alast).ToString("dd/MMM/yyyy"); ;
                    List<SalaryRegisterlModel> individualdata = new List<SalaryRegisterlModel>();
                    individualdata = clsSalary.Salary_Register(exportExcelModel.Empid, SDate, EDate, exportExcelModel.Month, exportExcelModel.TotalDays, exportExcelModel.empno, exportExcelModel.year);
                    if (individualdata.Count > 0)
                    {
                        exceldata.Add(new ExportExcelModel
                        {
                            SLNO = individualdata[0].SLNO,
                            EMPLOYEECODE = individualdata[0].EMPLOYEECODE,
                            NAME_OF_EMPLOYEES = individualdata[0].NAME_OF_EMPLOYEES,
                            SITE = individualdata[0].SITE,
                            W = individualdata[0].W,
                            O = individualdata[0].O,
                            Department = individualdata[0].Department,
                            Designation = individualdata[0].Designation,
                            E = individualdata[0].E,
                            T_D = individualdata[0].T_D,
                            BASIC = individualdata[0].BASIC,
                            STIPEND = individualdata[0].STIPEND,
                            WORKING_DAYS = individualdata[0].WORKING_DAYS,
                            DAY_S_ABSENT = individualdata[0].DAY_S_ABSENT,
                            DEDUCTION_AGAINST_ABSENSE = individualdata[0].DEDUCTION_AGAINST_ABSENSE,
                            E_L_ = individualdata[0].E_L_,
                            PAYMENT_AGAINST_E_L_ = individualdata[0].PAYMENT_AGAINST_E_L_,
                            GROSS_AMOUNT = individualdata[0].GROSS_AMOUNT,
                            HRA = individualdata[0].HRA,
                            LODGING_ALLOWANCE = individualdata[0].LODGING_ALLOWANCE,
                            TRAINING_ALLOWANCE_ACHIEVED = individualdata[0].TRAINING_ALLOWANCE_ACHIEVED,
                            MOBILE = individualdata[0].MOBILE,
                            BOOKS_AND_PERIODICALS_ALLOWANCE = individualdata[0].BOOKS_AND_PERIODICALS_ALLOWANCE,
                            INTERNET_ALLOWANCE = individualdata[0].INTERNET_ALLOWANCE,
                            CONINUOUS_ATTENDENCE_ALLOWANCE = individualdata[0].CONINUOUS_ATTENDENCE_ALLOWANCE,
                            Conveyence_Allowance = individualdata[0].Conveyence_Allowance,
                            PRODUCTION_BONUS = individualdata[0].PRODUCTION_BONUS,
                            PRODUCTION_BONUS_ACHIEVED = individualdata[0].PRODUCTION_BONUS_ACHIEVED,
                            OVERTIME = individualdata[0].OVERTIME,
                            OVERTIME_AMOUNT = individualdata[0].OVERTIME_AMOUNT,
                            MEDICAL_ALLOWANCES = individualdata[0].MEDICAL_ALLOWANCES,
                            LTA = individualdata[0].LTA,
                            EDUCATION_ALLOWANCES = individualdata[0].EDUCATION_ALLOWANCES,
                            TOTAL_ALLOWANCES = individualdata[0].TOTAL_ALLOWANCES,
                            E_S_I = individualdata[0].E_S_I,
                            PF = individualdata[0].PF,
                            PTAX = individualdata[0].PTAX,
                            TOTAL_DEDUCTION = individualdata[0].TOTAL_DEDUCTION,
                            OPENING_BALANCE = individualdata[0].OPENING_BALANCE,
                            DEDUCTION = individualdata[0].DEDUCTION,
                            CLOSING_BALANCE = individualdata[0].CLOSING_BALANCE,
                            NET_AMOUNT = individualdata[0].NET_AMOUNT,
                            Attire_Allowance = individualdata[0].Attire_Allowance,
                            PAID_DATE = individualdata[0].PAID_DATE,
                        });
                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            ListtoDataTable lsttodt = new ListtoDataTable();
            DataTable dt = lsttodt.ToDataTable(exceldata);
            dt.Columns.Remove("Empid");
            dt.Columns.Remove("Prefixfromdate");
            dt.Columns.Remove("suffixtodate");
            dt.Columns.Remove("Month");
            dt.Columns.Remove("TotalDays");
            dt.Columns.Remove("empno");
            dt.Columns.Remove("year");
            dt.Columns.Remove("SITE");

            string fileName = MonthName + "Report" + ".xlsx";
            var filepath = Path.Combine(Server.MapPath("~/PDF/"));
            if (System.IO.File.Exists(filepath + fileName))
            {
                System.IO.File.Delete(filepath + fileName);
            }
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet1 = wb.Worksheets.Add(dt, "Employees");
                sheet1.Table("Table1").ShowAutoFilter = false;
                sheet1.Table("Table1").Theme = XLTableTheme.None;
                sheet1.Columns().AdjustToContents();
                wb.SaveAs(filepath + fileName);

            }
            //System.IO.File.WriteAllBytes(filepath, filecontent);
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region view inactive employee Salary
        public ActionResult ViewInActiveSalary()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            employeeSalaryModel.MonthList = GetMonthList();

            #region bind Employee
            EmployeeListModel employeeListModel = new EmployeeListModel();
            List<SelectListItem> EmpList = new List<SelectListItem>();
            EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            employeeListModel.empid = 0;
            List<EmployeeListModel> employeedata = new List<EmployeeListModel>();
            string econtents = JsonConvert.SerializeObject(employeeListModel);
            HttpResponseMessage eresponse = ObjAPI.CallAPI("api/Employee/GetAllEmployeeInActiveList", econtents);
            if (eresponse.IsSuccessStatusCode)
            {
                string responseString = eresponse.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    employeedata = JsonConvert.DeserializeObject<List<EmployeeListModel>>(responseString);
                }
            }
            foreach (var items in employeedata)
            {
                EmpList.Add(new SelectListItem
                {
                    Text = items.EmployeeName,
                    Value = items.empid.ToString()
                });
            }
            #endregion
            employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);
        }
        #endregion

        #region Get IndividualSalaryData
        public List<IndividualSalryModel> GetIndividualSalaryData(int years, int month, int Empid, string Monthname)
        {
            List<IndividualSalryModel> data = new List<IndividualSalryModel>();
            DateTime firstday = new DateTime(Convert.ToInt32(years), Convert.ToInt32(month), 1);
            DateTime lastday = firstday.AddMonths(1).AddSeconds(-1);
            individualSalryModel.FirstDays = firstday.ToString();
            individualSalryModel.LastDays = lastday.ToString();
            individualSalryModel.Empid = Convert.ToInt32(Empid);
            individualSalryModel.Month = Monthname;
            individualSalryModel.Year = Convert.ToInt32(years);
            string contents = JsonConvert.SerializeObject(individualSalryModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetIndividualEmployeesalary", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<IndividualSalryModel>>(responseString);
                }
            }
            return data;
        }
        #endregion


        public ActionResult IndividualSalryview()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            double lateleaveamount = 0;
            double leaveamount = 0;

            double onedayamt = 0;
            string sEmpIde = Request.QueryString["Empid"];
            int Empid = Convert.ToInt32(DataEncryption.Decrypt(Convert.ToString(sEmpIde), "passKey"));
            //int Empid = Convert.ToInt32(Request.QueryString["Empid"]);
            string Monthname = Convert.ToString(Request.QueryString["Month"]);
            int years = Convert.ToInt32(Request.QueryString["Year"]);
            int month = DateTime.ParseExact(Monthname, "MMMM", CultureInfo.CurrentCulture).Month;
            int monthdays = DateTime.DaysInMonth(years, Convert.ToInt32(month));
            if (Empid != null || Empid != 0)
            {
                // Mayukh Change
                //List<IndividualSalryModel> Emplist = GetIndividualSalaryData(years, month, Empid, Monthname);
                List<IndividualSalryModel> Emplist = clsSalary.Salary_Slip(Empid, Monthname, month, years);
                if (Emplist.Count > 0)
                {
                    onedayamt = Math.Round(Convert.ToDouble(Emplist[0].Gross_Amount) / 30, 2);
                    leaveamount = Convert.ToDouble(onedayamt) * Convert.ToDouble((Emplist[0].AbsentDays == null || Emplist[0].AbsentDays == "") ? 0.ToString() : Emplist[0].AbsentDays);

                    lateleaveamount = Convert.ToDouble(onedayamt) * Convert.ToDouble((Emplist[0].LateLeave == null || Emplist[0].LateLeave == "") ? 0.ToString() : Emplist[0].LateLeave);

                    individualSalryModel.WorkingDays = monthdays.ToString();
                    individualSalryModel.EarnedLeaveDays = (Emplist[0].EarnedLeaveDays == null || Emplist[0].EarnedLeaveDays == "") ? 0.ToString() : Emplist[0].EarnedLeaveDays;

                    individualSalryModel.Empid = Emplist[0].Empid;
                    individualSalryModel.CompanyAddress = Emplist[0].CompanyAddress;
                    individualSalryModel.EmployeeName = Emplist[0].EmployeeName;
                    individualSalryModel.EmployeeNo = Emplist[0].EmployeeNo;
                    individualSalryModel.Department = Emplist[0].Department;
                    individualSalryModel.Designation = Emplist[0].Designation;
                    individualSalryModel.Basic = Emplist[0].Basic;
                    individualSalryModel.Internet_Allowance = Emplist[0].Internet_Allowance;
                    individualSalryModel.PF = Emplist[0].PF;
                    individualSalryModel.Education_Allowances = Emplist[0].Education_Allowances == null ? 0 : Emplist[0].Education_Allowances;
                    individualSalryModel.Medical_Allowances = Emplist[0].Medical_Allowances == null ? 0 : Emplist[0].Medical_Allowances; ;
                    individualSalryModel.HRA = Emplist[0].HRA;
                    individualSalryModel.Attire_Allowance = Emplist[0].Attire_Allowance;
                    individualSalryModel.P_Tax = Emplist[0].P_Tax;
                    individualSalryModel.ESI = Emplist[0].ESI;
                    individualSalryModel.Leave_Amount = Emplist[0].Leave_Amount == null ? 0 : Emplist[0].Leave_Amount;
                    individualSalryModel.joining = Emplist[0].joining;
                    individualSalryModel.Total_Allowances = Emplist[0].Total_Allowances;
                    individualSalryModel.Total_Deduction = Emplist[0].Total_Deduction;
                    individualSalryModel.Net_Payable_Amount = Emplist[0].Net_Payable_Amount;

                    individualSalryModel.Month = Emplist[0].Month;
                    individualSalryModel.Year = Emplist[0].Year;

                    individualSalryModel.empaccountno = Emplist[0].empaccountno;
                    individualSalryModel.empcode = Emplist[0].empcode;
                    individualSalryModel.empepfno = Emplist[0].empepfno;
                    individualSalryModel.empesicno = Emplist[0].empesicno;
                    individualSalryModel.emppancardno = Emplist[0].emppancardno;
                    individualSalryModel.empuanno = Emplist[0].empuanno;
                    individualSalryModel.Company = Emplist[0].Company;
                    individualSalryModel.TotalLeaveDays = (Emplist[0].AbsentDays == null || Emplist[0].AbsentDays == "") ? 0.ToString() : Emplist[0].AbsentDays;
                    individualSalryModel.AbsentDays = (Emplist[0].TotalLeaveDays == null || Emplist[0].TotalLeaveDays == "") ? 0.ToString() : Emplist[0].TotalLeaveDays;
                    individualSalryModel.LateLeave = (Emplist[0].LateLeave == null || Emplist[0].LateLeave == "") ? 0.ToString() : Emplist[0].LateLeave;
                    individualSalryModel.LateLeaveAmount = lateleaveamount.ToString();
                    individualSalryModel.LeaveAmount = leaveamount.ToString();
                    individualSalryModel.Continuous_Attendance_Allowance = Emplist[0].Continuous_Attendance_Allowance == null ? 0 : Emplist[0].Continuous_Attendance_Allowance;
                    individualSalryModel.ProductionBonus = Emplist[0].ProductionBonus == null ? 0 : Emplist[0].ProductionBonus;
                    individualSalryModel.Overtime_Allowance = Emplist[0].Overtime_Allowance == null ? 0 : Emplist[0].Overtime_Allowance;
                    individualSalryModel.Arrears = Emplist[0].Arrears == null ? 0 : Emplist[0].Arrears;
                    individualSalryModel.LTA = Emplist[0].LTA == null ? 0 : Emplist[0].LTA;
                    individualSalryModel.PerformanceAllowance = Emplist[0].PerformanceAllowance == null ? 0 : Emplist[0].PerformanceAllowance;
                    individualSalryModel.SpecialAllowance = Emplist[0].SpecialAllowance == null ? 0 : Emplist[0].SpecialAllowance;
                    individualSalryModel.Lodging_Allowance = Emplist[0].Lodging_Allowance == null ? 0 : Emplist[0].Lodging_Allowance;
                    individualSalryModel.Stipend = Emplist[0].Stipend == null ? 0 : Emplist[0].Stipend;
                    individualSalryModel.Gross_Amount = Emplist[0].Gross_Amount == null ? 0 : Emplist[0].Gross_Amount;
                    individualSalryModel.Mobile_Allowance = Emplist[0].Mobile_Allowance == null ? 0 : Emplist[0].Mobile_Allowance;

                    individualSalryModel.Conveyence_Allowance = Emplist[0].Conveyence_Allowance == null ? 0 : Emplist[0].Conveyence_Allowance;
                    individualSalryModel.Training__Allowance = Emplist[0].Training__Allowance == null ? 0 : Emplist[0].Training__Allowance;
                    individualSalryModel.Food_Allowance = Emplist[0].Food_Allowance == null ? 0 : Emplist[0].Food_Allowance;
                    individualSalryModel.Vehical_Allownce = Emplist[0].Vehical_Allownce == null ? 0 : Emplist[0].Vehical_Allownce;
                    individualSalryModel.Books___Periodical = Emplist[0].Books___Periodical == null ? 0 : Emplist[0].Books___Periodical;
                    individualSalryModel.TDS = Emplist[0].TDS == null ? 0 : Emplist[0].TDS;

                    individualSalryModel.Loan_Installment = Emplist[0].Loan_Installment == null ? 0 : Emplist[0].Loan_Installment;
                    individualSalryModel.Net_Payable_Amount = (Emplist[0].Net_Payable_Amount - Emplist[0].Loan_Installment);
                    individualSalryModel.Amountinwords = "Rs.  " + ConvertNumbertoWords(Convert.ToInt64(Emplist[0].Net_Payable_Amount) - Convert.ToInt64(Emplist[0].Loan_Installment));

                }

                return View(individualSalryModel);
            }
            return View(individualSalryModel);
        }
        public string ConvertNumbertoWords(long number)
        {
            if (number == 0) return "ZERO";
            if (number < 0) return "minus " + ConvertNumbertoWords(Math.Abs(number));
            string words = "";
            string constant = "Rs.";
            if ((number / 1000000) > 0)
            {
                words += ConvertNumbertoWords(number / 100000) + " LAKHS ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += ConvertNumbertoWords(number / 1000) + " THOUSAND ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += ConvertNumbertoWords(number / 100) + " HUNDRED ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "") words += "AND ";
                var unitsMap = new[]
                {
                    "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"
                };
                var tensMap = new[]
                {
                     "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"
                };
                if (number < 20) words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                }
            }
            return words;
        }


        #region View Salary History By EmpId
        public ActionResult SalaryHistory(string cal)
        {
            string sEmpid = Request.QueryString["cal"];
            //int earchValue = Convert.ToInt32(Request.QueryString["SearchValue"]);
            int Empid = Convert.ToInt32(DataEncryption.Decrypt(Convert.ToString(sEmpid), "passKey"));
            DateTime d = DateTime.Now;
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            List<EmployeeSalaryModel> Empdata = new List<EmployeeSalaryModel>();
            if (Empid != null)
            {
                for (int i = 1; i < 4; i++)
                {
                    d = d.AddMonths(-1);
                    string fullmonthname = d.ToString("MMMM");
                    employeeSalaryModel.Month = fullmonthname;
                    employeeSalaryModel.Year = Convert.ToInt32(d.Year);
                    employeeSalaryModel.Empid = Convert.ToInt32(Empid);
                    employeeSalaryModel.Status = "1";
                    List<EmployeeSalaryModel> data = new List<EmployeeSalaryModel>();
                    string contents = JsonConvert.SerializeObject(employeeSalaryModel);
                    HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/AllEmployeeSalaryview", contents);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseString = response.Content.ReadAsStringAsync().Result;

                        if (!string.IsNullOrEmpty(responseString))
                        {
                            data = JsonConvert.DeserializeObject<List<EmployeeSalaryModel>>(responseString);
                        }
                    }
                    //string responseString = response.Content.ReadAsStringAsync().Result;

                    //if (!string.IsNullOrEmpty(responseString))
                    //{
                    //    employeeListModel.emplist = JsonConvert.DeserializeObject<List<EmployeeListModel>>(responseString);
                    //}
                    if (data.Count > 0)
                    {
                        employeeSalaryModel.SalaryList.Add(new EmployeeSalaryModel
                        {
                            Empid = data[0].Empid,
                            sEmpid = DataEncryption.Encrypt(Convert.ToString(data[0].Empid), "passKey"),
                            Empno = data[0].Empno,
                            EmployeeName = data[0].EmployeeName,
                            PayGroupeName = data[0].PayGroupeName,
                            Basic = data[0].Basic,
                            Gross_Amount = data[0].Gross_Amount,
                            Total_Allowances = data[0].Total_Allowances,
                            Total_Deduction = data[0].Total_Deduction,
                            Net_Payable_Amount = data[0].Net_Payable_Amount,
                            Month = fullmonthname,
                            Year = d.Year
                        });
                    }

                }
                return View(employeeSalaryModel);
            }
            return View(employeeSalaryModel);
        }
        #endregion

        #region Delete All EmployeeSalary by month
        public JsonResult TrashAllEmployeeSalary(string modelParameter)
        {
            string deletedata = "";
            int count = 0;
            string[] arrayRowid = modelParameter.Split(',');
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            for (int i = 0; i < arrayRowid.Length; i++)
            {
                employeeSalaryModel.Empid = Convert.ToInt32(arrayRowid[i]);
                employeeSalaryModel.Month = DateTime.Now.AddMonths(-1).ToString("MMMM");
                employeeSalaryModel.Year = DateTime.Now.AddMonths(-1).Year;
                string contents = JsonConvert.SerializeObject(employeeSalaryModel);
                HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/DeleteAllEmployeeSalaryByMonth", contents);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(responseString))
                    {
                        deletedata = JsonConvert.DeserializeObject(responseString).ToString();
                        count++;
                    }
                }
            }


            return Json(count, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region OverTime Details
        public ActionResult OverTimeDetails(string cal)
        {
            cal = DataEncryption.Decrypt(Convert.ToString(cal), "passKey");
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            EmployeeBonusModel employeeBonusModel = new EmployeeBonusModel();
            var months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            DateTimeFormatInfo info = DateTimeFormatInfo.GetInstance(null);
            employeeBonusModel.MonthList = GetMonthList();
            employeeBonusModel.YearList = GetYearList();
            employeeBonusModel.ID = 0;
            employeeBonusModel.Empid = Convert.ToInt32(cal);
            string contents = JsonConvert.SerializeObject(employeeBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetEmployeeBonus", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    employeeBonusModel.EmployeeBonusList = JsonConvert.DeserializeObject<List<EmployeeBonusModel>>(responseString);
                }
            }


            return View(employeeBonusModel);
        }

        #endregion

        #region Save Employee Bonus
        public JsonResult SaveEmployeeBonus(string modelParameter)
        {
            EmployeeBonusModel employeeBonus = new EmployeeBonusModel();

            employeeBonus = JsonConvert.DeserializeObject<EmployeeBonusModel>(modelParameter);

            employeeBonus.Empid = Convert.ToInt32(DataEncryption.Decrypt(Convert.ToString(employeeBonus.sEmpid), "passKey"));
            string contents = JsonConvert.SerializeObject(employeeBonus);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/SaveEmployeeBonus", contents);
            var data = "";
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region GetEmployeeBonus
        public JsonResult GetEmployeeBonus(string modelParameter)
        {
            EmployeeBonusModel employeeBonusModel = new EmployeeBonusModel();
            employeeBonusModel = JsonConvert.DeserializeObject<EmployeeBonusModel>(modelParameter);
            string contents = JsonConvert.SerializeObject(employeeBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetEmployeeBonus", contents);
            List<EmployeeBonusModel> data = new List<EmployeeBonusModel>();
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmployeeBonusModel>>(responseString).ToList();
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Continuous,productionbonus Calculation
        public List<EmployeeSalaryConfigModel> GetcontinuousAllowance(string Empid, string Basic)
        {
            List<EmployeeSalaryConfigModel> ReturnData = new List<EmployeeSalaryConfigModel>();
            List<EmployeeSalaryConfigModel> EmployeeSalarydata = GetSalaryData(Empid);
            double? absentdays = 0;
            if (EmployeeSalarydata.Count > 0)
            {
                List<EmployeeListModel> data = GetEmployeeListById(Empid);
                string CalculationFormula = "((No of Days in a Month-No of days Absent)/No of Days in a Month) *100";
                List<EmployeeLeaveModel> dataleave = GetEmployeeLeave(data[0].empno.ToString());
                EmployeeLeaveModel employeeLeaveModel = new EmployeeLeaveModel();
                employeeLeaveModel.Empno = Convert.ToInt32(data[0].empno);
                if (dataleave.Count > 0)
                {
                    absentdays = dataleave[0].AbsentDays;
                }
                CalculationFormula = CalculationFormula.Replace("No of Days in a Month", Convert.ToString(days));
                CalculationFormula = CalculationFormula.Replace("No of days Absent", Convert.ToString(absentdays));

                DataTable dt = new DataTable();
                string math = CalculationFormula;
                double Continuousallowance = 0;
                double res = Convert.ToDouble(new DataTable().Compute(math, null));
                if (res == 100)
                {
                    Continuousallowance = Convert.ToDouble(EmployeeSalarydata[0].ContinuousAllowance) * 110 / 100;
                }
                else if (res >= 95 && res < 100)
                {
                    Continuousallowance = Convert.ToDouble(EmployeeSalarydata[0].ContinuousAllowance) * 100 / 100;
                }
                else if (res >= 85 && res < 95)
                {
                    Continuousallowance = Convert.ToDouble(EmployeeSalarydata[0].ContinuousAllowance) * 95 / 100;
                }
                else if (res >= 75 && res < 85)
                {
                    Continuousallowance = Convert.ToDouble(EmployeeSalarydata[0].ContinuousAllowance) * 90 / 100;
                }
                else
                {
                    Continuousallowance = 0;
                }
                List<EmployeeBonusModel> PayBonusdata = GetEmployeeBonusDataById(Empid);
                //string productionbonusFormula = "(Target Achieved % * Production Bonus)";
                string productionbonusFormula = "((Total Working Days - Days Absent)/ Total Working Days)*(Target Achieved % * Production Bonus)";
                string OvertimeAllowanceFormula = "(((Basic/(Working Days))/ Daily Working Hours) *2) *Overtime Hours";
                if (PayBonusdata.Count > 0)
                {
                    OvertimeAllowanceFormula = OvertimeAllowanceFormula.Replace("Overtime Hours", Convert.ToString(PayBonusdata[0].OverTimeHours));
                    productionbonusFormula = productionbonusFormula.Replace("Target Achieved", Convert.ToString(PayBonusdata[0].TargetAchieved));
                    productionbonusFormula = productionbonusFormula.Replace("Production Bonus", Convert.ToString(PayBonusdata[0].ProductionBonus));
                }
                else
                {
                    OvertimeAllowanceFormula = OvertimeAllowanceFormula.Replace("Overtime Hours", Convert.ToString(0));

                    productionbonusFormula = productionbonusFormula.Replace("Target Achieved", Convert.ToString(0));
                    productionbonusFormula = productionbonusFormula.Replace("Production Bonus", Convert.ToString(0));
                }
                productionbonusFormula = productionbonusFormula.Replace("%", Convert.ToString("/100"));
                productionbonusFormula = productionbonusFormula.Replace("Total Working Days", Convert.ToString(days));
                productionbonusFormula = productionbonusFormula.Replace("Days Absent", Convert.ToString(absentdays));

                string maths = productionbonusFormula;
                double resu = Convert.ToDouble(new DataTable().Compute(maths, null));
                OvertimeAllowanceFormula = OvertimeAllowanceFormula.Replace("–", "-");
                OvertimeAllowanceFormula = OvertimeAllowanceFormula.Replace("Working Days", Convert.ToString(days));
                OvertimeAllowanceFormula = OvertimeAllowanceFormula.Replace("Daily Working Hours", "8");
                OvertimeAllowanceFormula = OvertimeAllowanceFormula.Replace("Basic", Convert.ToString(Basic));
                string Overtimemaths = OvertimeAllowanceFormula;

                double overtimeresult = 0;
                overtimeresult = Convert.ToDouble(new DataTable().Compute(Overtimemaths, null));


                ReturnData.Add(new EmployeeSalaryConfigModel
                {
                    ContinuousAllowance = Continuousallowance.ToString(),
                    ProductionBonus = resu.ToString(),
                    OvertimeAllowance = Math.Round(overtimeresult)
                });
            }
            return ReturnData;
        }
        #endregion

        public JsonResult loadContinuousAllowance(string Empid, string Basic)
        {
            List<EmployeeSalaryConfigModel> Data = GetcontinuousAllowance(Empid, Basic);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIndividualSalaryDetails(string Empid, string months, string year)
        {
            int monthno = DateTime.ParseExact(months, "MMMM", CultureInfo.CurrentCulture).Month;
            List<IndividualSalryModel> Emplist = GetIndividualSalaryData(Convert.ToInt32(year), Convert.ToInt32(monthno), Convert.ToInt32(Empid), months);

            return Json("suxcess");
        }

        #region UpdatePayment
        public JsonResult UpdateEmployeePayment(string modelParameter, string monthSelected, string yearSelected)
        {
            string[] arrayRowid = modelParameter.Split(',');
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            var data = "";
            int count = 0;
            for (int i = 0; i < arrayRowid.Length; i++)
            {
                employeeSalaryModel.Empid = Convert.ToInt32(arrayRowid[i]);
                employeeSalaryModel.Month = monthSelected; // DateTime.Now.AddMonths(-1).ToString("MMMM");
                employeeSalaryModel.Year = Convert.ToInt32(yearSelected); //Year;
                string contents = JsonConvert.SerializeObject(employeeSalaryModel);
                HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/UpdateSalaryPayment", contents);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(responseString))
                    {
                        data = JsonConvert.DeserializeObject(responseString).ToString();
                        string body = createEmailBody(Convert.ToInt32(employeeSalaryModel.Empid), employeeSalaryModel.Month, Convert.ToInt32(employeeSalaryModel.Year));
                        byte[] filecontent = CreatePDF(body);

                        List<EmployeeListModel> Employeedata = GetEmployeeListById(arrayRowid[i].ToString());
                        if (Employeedata.Count > 0)
                        {
                            string strbody = "";
                            strbody += "Hi " + Employeedata[0].EmployeeName;
                            strbody += "<br/><br/>Attached Payslip For the Month of " + employeeSalaryModel.Month + "'" + employeeSalaryModel.Year;
                            string fileName = Employeedata[0].EmployeeName + "_" + employeeSalaryModel.Month + "_" + employeeSalaryModel.Year;
                            fileName = fileName.Replace(" ", "_");
                            SendMails(Employeedata[0].empemail, "", strbody, "Payslip for the Month Of " + employeeSalaryModel.Month + "'" + employeeSalaryModel.Year, filecontent, fileName);
                            //SendMails("chinmoy740jana@gmail.com", "", strbody, "Payslip for the Month Of " + employeeSalaryModel.Month + "'" + employeeSalaryModel.Year, filecontent, fileName);

                        }
                        count++;
                    }
                }
            }
            return Json(count, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public JsonResult GeneratePdf(string modelParameter)
        {
            individualSalryModel = JsonConvert.DeserializeObject<IndividualSalryModel>(modelParameter);
            if (!string.IsNullOrEmpty(individualSalryModel.SEmpid))
            {
                individualSalryModel.Empid = Convert.ToInt32(DataEncryption.Decrypt(Convert.ToString(individualSalryModel.SEmpid), "passKey"));
            }
            string body = createEmailBody(Convert.ToInt32(individualSalryModel.Empid), individualSalryModel.Month, Convert.ToInt32(individualSalryModel.Year));
            byte[] filecontent = CreatePDF(body);
            var fileName = individualSalryModel.EmployeeName.Replace(" ", "_") + "_ " + individualSalryModel.Month + "_" + individualSalryModel.Year + ".pdf";
            var filepath = Path.Combine(Server.MapPath("~/PDF/"), fileName);
            System.IO.File.WriteAllBytes(filepath, filecontent);
            return Json(new { fileName });

        }
        public byte[] CreatePDF(string body)
        {
            StringReader sr = new StringReader(body.ToString());
            byte[] bytes;
            Document pdfDoc = new Document(PageSize.A4);
            List<string> Cssfiles = new List<string>();
            Cssfiles.Add(@"/Contents/CustomCSS/Tabledesign.css");
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                pdfDoc.Open();
                HtmlPipelineContext context = new HtmlPipelineContext(null);
                context.SetTagFactory(iTextSharp.tool.xml.html.Tags.GetHtmlTagProcessorFactory());
                ICSSResolver cSSResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(false);
                Cssfiles.ForEach(p => cSSResolver.AddCssFile(System.Web.HttpContext.Current.Server.MapPath(p), true));
                IPipeline pipeline = new CssResolverPipeline(cSSResolver, new HtmlPipeline(context, new iTextSharp.tool.xml.pipeline.end.PdfWriterPipeline(pdfDoc, writer)));
                XMLWorker worker = new XMLWorker(pipeline, true);
                XMLParser parser = new XMLParser(worker);
                parser.Parse(sr);
                //htmlparser.Parse(sr);
                pdfDoc.Close();
                bytes = memoryStream.ToArray();
                //System.IO.File.WriteAllBytes(filepath, bytes);
                memoryStream.Close();
            }
            return bytes;
        }
        public ActionResult DownloadPdf(string fileName)
        {
            string contentRootPath = Path.Combine(Server.MapPath("~/PDF/"), fileName);
            byte[] fileByteArray = System.IO.File.ReadAllBytes(contentRootPath);
            System.IO.File.Delete(contentRootPath);
            return File(fileByteArray, "application/pdf", fileName);

        }

        public bool SendMails(string toEmail, string fromEmail, string strBody, string strSubject, byte[] bytes, string fileName)
        {
            bool retval = true;
            try
            {
                string sUserName = ConfigurationManager.AppSettings["MailUserName"];
                string sPassword = ConfigurationManager.AppSettings["MailPassword"];
                string sPort = ConfigurationManager.AppSettings["Port"];
                string sHost = ConfigurationManager.AppSettings["Host"];

                string senablessl = ConfigurationManager.AppSettings["enablessl"];

                string sMailFrom = ConfigurationManager.AppSettings["MailFrom"];

                string sAdminEmail = ConfigurationManager.AppSettings["AdminEmail"];

                MailMessage mail = new MailMessage();


                mail.From = fromEmail.Length > 0 ? new MailAddress(fromEmail) : new MailAddress(sMailFrom);

                mail.To.Add(toEmail.Length > 0 ? toEmail : sAdminEmail);


                mail.Subject = strSubject;


                mail.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
                mail.IsBodyHtml = true;
                //mail.Attachments.Add(new Attachment(Server.MapPath("~/PDF/" + filename)));
                mail.Attachments.Add(new Attachment(new MemoryStream(bytes), fileName + ".pdf"));
                System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(strBody, @"<(.|\n)*?>", string.Empty), null, "text/plain");
                System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(strBody, null, "text/html");
                mail.AlternateViews.Add(plainView);
                mail.AlternateViews.Add(htmlView);

                SmtpClient smtp = new SmtpClient(sHost);
                System.Net.NetworkCredential netcred = new System.Net.NetworkCredential(sUserName, sPassword);
                if (senablessl == "1")
                {
                    smtp.EnableSsl = true;
                }
                else
                {
                    smtp.EnableSsl = false;
                }
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = netcred;
                smtp.Port = Convert.ToInt32(sPort);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(mail);



            }
            catch (Exception ex)
            {
                retval = false;
            }

            return retval;
        }

        #region save Edit Leave details
        public string Saveeditedleave(LeaveDetailsModel leave)
        {
            string data = "";
            string contents = JsonConvert.SerializeObject(leave);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/SaveLeavedetails", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject(responseString).ToString();
                }
            }
            return data;
        }

        public JsonResult SaveLeaveDetails(string modelParameter)
        {
            string data = "";
            LeaveDetailsModel leaveDetailsModel = new LeaveDetailsModel();
            leaveDetailsModel = JsonConvert.DeserializeObject<LeaveDetailsModel>(modelParameter);
            leaveDetailsModel.CreatedBy = Convert.ToInt32(Session["Empno"]);
            leaveDetailsModel.Month = DateTime.Now.AddMonths(-1).ToString("MMMM");
            leaveDetailsModel.Year = DateTime.Now.AddMonths(-1).Year;
            data = Saveeditedleave(leaveDetailsModel);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region GetEdited LeaveDetails
        public List<LeaveDetailsModel> GetEditedLeaveDetails(string Empno)
        {
            LeaveDetailsModel.Empno = Convert.ToInt32(Empno);
            LeaveDetailsModel.Month = fullmonthname;
            LeaveDetailsModel.Year = Year;
            string Contents = JsonConvert.SerializeObject(LeaveDetailsModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GetEditedLeaveDetails", Contents);
            List<LeaveDetailsModel> data = new List<LeaveDetailsModel>();
            if (response.IsSuccessStatusCode)
            {
                string responsestring = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(responsestring))
                {
                    data = JsonConvert.DeserializeObject<List<LeaveDetailsModel>>(responsestring).ToList();
                }
            }
            return data;
        }
        #endregion

        #region Import Employee file
        public JsonResult ImportEmployee(HttpPostedFileBase excelFile)
        {
            string createdEmployee = "";

            DataSet ds = new DataSet();

            if (Request.Files["excelFile"].ContentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(Request.Files["excelFile"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Contents/ExcelFiles/") + Request.Files["excelFile"].FileName;

                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }

                    Request.Files["excelFile"].SaveAs(fileLocation);

                    string excelConnectionString = string.Empty;

                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    Path.GetFullPath(fileLocation) + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";

                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        Path.GetFullPath(fileLocation) + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }

                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        Path.GetFullPath(fileLocation) + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();

                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;

                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    for (int i = 0; i < excelSheets.Length; i++)
                    {
                        OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);

                        string query = string.Format("Select * from [{0}]", excelSheets[i]);

                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                        {

                            dataAdapter.Fill(ds);

                        }
                    }
                }
                LeaveDetailsModel leavemodel = new LeaveDetailsModel();
                if (fileExtension.ToString().ToLower().Equals(".xml"))
                {
                    string fileLocation = Server.MapPath("~/Contents/ExcelFiles/") + Request.Files["FileUpload"].FileName;

                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }

                    Request.Files["FileUpload"].SaveAs(fileLocation);
                    XmlTextReader xmlreader = new XmlTextReader(fileLocation);
                    ds.ReadXml(xmlreader);
                    xmlreader.Close();
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {

                    leavemodel = new LeaveDetailsModel
                    {
                        Type = "Import",
                        Noofdaysabsent = Convert.ToDouble(ds.Tables[0].Rows[i]["No Of Absent Days"] is DBNull ? 0 : ds.Tables[0].Rows[i]["No Of Absent Days"]),
                        LateLeaveDays = 0,
                        LeaveDays = 0,
                        Month = fullmonthname,
                        Year = Year,
                        Empno = Convert.ToInt32(ds.Tables[0].Rows[i]["Employee Code"]),
                        CreatedBy = Convert.ToInt32(Session["Empno"])
                    };
                    createdEmployee = Saveeditedleave(leavemodel);
                }
            }

            return Json(createdEmployee, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ExportToExcel
        public ActionResult ExportEmployeeInExcel()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SL No");
            dt.Columns.Add("Name");
            dt.Columns.Add("Employee Code");
            dt.Columns.Add("No Of Absent Days");
            List<EmployeeListModel> employees = GetAllEmployee();

            //employees.Where(s=>s.pa)
            for (int i = 0; i < employees.Count; i++)
            {
                dt.Rows.Add(i + 1, employees[i].EmployeeName, employees[i].empno, "");
            }
            string fileName = fullmonthname + "AbsentDays" + ".xlsx";
            var filepath = Path.Combine(Server.MapPath("~/PDF/"));
            if (System.IO.File.Exists(filepath + fileName))
            {
                System.IO.File.Delete(filepath + fileName);
            }
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet1 = wb.Worksheets.Add(dt, "Employees");
                sheet1.Table("Table1").ShowAutoFilter = false;
                sheet1.Table("Table1").Theme = XLTableTheme.None;
                sheet1.Columns().AdjustToContents();
                wb.SaveAs(filepath + fileName);

            }
            //System.IO.File.WriteAllBytes(filepath, filecontent);
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadExcel(string fileName)
        {
            string contentRootPath = Path.Combine(Server.MapPath("~/PDF/"), fileName);
            byte[] fileByteArray = System.IO.File.ReadAllBytes(contentRootPath);
            System.IO.File.Delete(contentRootPath);
            return File(fileByteArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

        }
        #endregion


        #region GetManualValues
        public List<ConfigureSalaryComponentModel> GetManualValues(string Empid)
        {
            List<ConfigureSalaryComponentModel> Alldata = new List<ConfigureSalaryComponentModel>();
            List<EmployeeListModel> empdata = GetEmployeeListById(Empid);
            DateTime Today = DateTime.Now.AddMonths(0);
            DateTime JoiningDate = empdata[0].Joining.HasValue ? Convert.ToDateTime(empdata[0].Joining) : Convert.ToDateTime(null);
            List<EmployeeSalaryConfigModel> Salarydata = GetSalaryData(Empid);
            if (Salarydata.Count > 0)
            {
                if (Salarydata[0].SalaryConfigureType == 1)
                {
                    if (Today.Year == JoiningDate.Year && Today.Month == JoiningDate.Month)
                    {
                        int differencedate = 30 - JoiningDate.Day + 1;
                        for (int p = 0; p < Salarydata.Count; p++)
                        {
                        }
                    }
                    else
                    {
                        int differencedate = 30 - JoiningDate.Day + 1;
                        for (int p = 0; p < Salarydata.Count; p++)
                        {
                        }
                    }
                }
            }

            return Alldata;
        }

        #endregion

        #region TDSCheck
        public List<EmployeeSalaryConfigModel> CheckTDS()
        {
            List<EmployeeSalaryConfigModel> data = new List<EmployeeSalaryConfigModel>();
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/GETTDSCHECK", "");
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmployeeSalaryConfigModel>>(responseString);
                }
            }
            return data;// Json(data,JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region TDF Report Download
        public JsonResult GenerateTDFPDF(string modelParameter)
        {
            individualSalryModel = JsonConvert.DeserializeObject<IndividualSalryModel>(modelParameter);
            int monthno = DateTime.ParseExact(individualSalryModel.Month, "MMMM", CultureInfo.CurrentCulture).Month;
            byte[] byteArray = null;
            List<IndividualSalryModel> Emplist = GetIndividualSalaryData(Convert.ToInt32(individualSalryModel.Year), monthno, Convert.ToInt32(individualSalryModel.Empid), individualSalryModel.Month);
            if (Emplist.Count > 0)
            {
                DateTime dob = Emplist.FirstOrDefault().DOB.HasValue ? Convert.ToDateTime(Emplist.FirstOrDefault().DOB) : Convert.ToDateTime((DateTime?)null);
                int age = CalculateAge(dob);
                double? EPFWages = Emplist.FirstOrDefault().Basic;
                double? EDLIWages = Emplist.FirstOrDefault().Basic;
                double? EPSWages = Emplist.FirstOrDefault().Basic;
                if (EPSWages > 15000)
                {
                    EPSWages = EDLIWages = 15000;
                }
                if (age > 58)
                {
                    EPSWages = 0;
                }
                double EPFRemitted = Math.Round(Convert.ToDouble(EPFWages * 12 / 100));
                double EPSRemitted = Math.Round(Convert.ToDouble(EPSWages * 8.33 / 100));
                double EPFEPSDIFF = EPFRemitted - EPSRemitted;
                string NCPDays = Emplist.FirstOrDefault().AbsentDays == null ? "0" : Emplist.FirstOrDefault().AbsentDays;
                using (var stream = new MemoryStream())
                {
                    Document doc = new Document(PageSize.A4);
                    var pdfWriter = PdfWriter.GetInstance(doc, stream);

                    doc.Open();
                    PdfPTable table = new PdfPTable(2);
                    table.AddCell("Column Name");
                    table.AddCell("Description");

                    table.AddCell("Member Name");
                    table.AddCell(Emplist.FirstOrDefault().EmployeeName);

                    table.AddCell("EPF WAGES");
                    table.AddCell(EPFWages.ToString());

                    table.AddCell("EPS WAGES");
                    table.AddCell(EPSWages.ToString());

                    table.AddCell("EDLI WAGES");
                    table.AddCell(EDLIWages.ToString());

                    table.AddCell("EPF CONTRI REMITTED");
                    table.AddCell(EPFRemitted.ToString());

                    table.AddCell("EPS CONTRI REMITTED");
                    table.AddCell(EPSRemitted.ToString());

                    table.AddCell("EPF EPS DIFF REMITTED");
                    table.AddCell(EPFEPSDIFF.ToString());

                    table.AddCell("NCP DAYS");
                    table.AddCell(NCPDays);

                    table.AddCell("ADVANCES");
                    table.AddCell("");

                    doc.Add(table);
                    doc.Close();
                    byteArray = stream.ToArray();

                    stream.Close();
                }

            }

            var fileName = "TDFReport.PDF";
            var filepath = Path.Combine(Server.MapPath("~/PDF/"), fileName);
            System.IO.File.WriteAllBytes(filepath, byteArray);
            return Json(new { fileName });


            //string body = createEmailBody(Convert.ToInt32(individualSalryModel.Empid), individualSalryModel.Month, Convert.ToInt32(individualSalryModel.Year));
            //byte[] filecontent = CreatePDF(body);
            //var fileName = individualSalryModel.EmployeeName.Replace(" ", "_") + "_ " + individualSalryModel.Month + "_" + individualSalryModel.Year + ".pdf";
            //var filepath = Path.Combine(Server.MapPath("~/PDF/"), fileName);
            //System.IO.File.WriteAllBytes(filepath, filecontent);
            //return Json(new { fileName });
        }


        #endregion



        /// <summary>  
        /// For calculating age  
        /// </summary>  
        /// <param name="Dob">Enter Date of Birth to Calculate the age</param>  
        /// <returns> years, months,days, hours...</returns>  
        //static string CalculateYourAge(DateTime Dob)
        //{
        //    DateTime Now = DateTime.Now;
        //    int Years = new DateTime(DateTime.Now.Subtract(Dob).Ticks).Year - 1;
        //    DateTime PastYearDate = Dob.AddYears(Years);
        //    int Months = 0;
        //    for (int i = 1; i <= 12; i++)
        //    {
        //        if (PastYearDate.AddMonths(i) == Now)
        //        {
        //            Months = i;
        //            break;
        //        }
        //        else if (PastYearDate.AddMonths(i) >= Now)
        //        {
        //            Months = i - 1;
        //            break;
        //        }
        //    }
        //    int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
        //    int Hours = Now.Subtract(PastYearDate).Hours;
        //    int Minutes = Now.Subtract(PastYearDate).Minutes;
        //    int Seconds = Now.Subtract(PastYearDate).Seconds;
        //    return String.Format("Age: {0} Year(s) {1} Month(s) {2} Day(s) {3} Hour(s) {4} Second(s)",
        //    Years, Months, Days, Hours, Seconds);
        //}
        private static int CalculateAge(DateTime dateOfBirth)
        {
            int age = 0;
            age = DateTime.Now.Year - dateOfBirth.Year;
            if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
                age = age - 1;

            return age;
        }

        public ActionResult ViewAllEmployeeStatutoryReport()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            #region bind Employee
            EmployeeListModel employeeListModel = new EmployeeListModel();
            List<SelectListItem> EmpList = new List<SelectListItem>();
            EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            employeeListModel.empid = 0;
            List<EmployeeListModel> employeedata = GetAllEmployee();
            foreach (var items in employeedata)
            {
                EmpList.Add(new SelectListItem
                {
                    Text = items.EmployeeName,
                    Value = items.empid.ToString()
                });
            }
            #endregion
            employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.MonthList = GetMonthList();
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);

        }
        #region ALLEmployeeStatutoryReport
        public JsonResult GetAllEmployeeStatutoryReport(string Month, string Year, string Empid, string Status, int numberofRow, int pagenumber)
        {
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            employeeSalaryModel.Month = Month;
            string tdsMonth = Month;
            employeeSalaryModel.Year = Convert.ToInt32(Year);
            int tdsYear = Convert.ToInt32(Year);
            employeeSalaryModel.Empid = Convert.ToInt32(Empid);
            employeeSalaryModel.Status = Status;
            employeeSalaryModel.PageNumber = pagenumber;
            employeeSalaryModel.Rowofpage = numberofRow;

            int month = DateTime.ParseExact(Month, "MMMM", CultureInfo.CurrentCulture).Month;
            DateTime first = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(month), 1);

            DateTime last = first.AddMonths(1).AddSeconds(-1);
            string firstDayOfMonth = first.ToString();
            string lastDayOfMonth = last.ToString();
            List<EmployeeSalaryModel> data = new List<EmployeeSalaryModel>();
            string contents = JsonConvert.SerializeObject(employeeSalaryModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/EmployeeSalaryview_ByPageWise", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmployeeSalaryModel>>(responseString);
                }
            }
            //List<EmployeeSalaryConfigModel> SalaryData = CheckTDS();
            //var matchData = data.Where(t2 => SalaryData.Any(t1 => t1.PayGroupId == t2.PayGroupId)).ToList();
            List<EmployeeSalaryModel> TDSdata = new List<EmployeeSalaryModel>();
            foreach (var x in data)
            {

                int EmpID = Convert.ToInt32(x.Empid);
                List<IndividualSalryModel> Emplist = GetIndividualSalaryData(tdsYear, month, EmpID, tdsMonth);
                if (Emplist.Count > 0)
                {
                    DateTime dob = Emplist.FirstOrDefault().DOB.HasValue ? Convert.ToDateTime(Emplist.FirstOrDefault().DOB) : Convert.ToDateTime((DateTime?)null);
                    int age = CalculateAge(dob);
                    string MemberName = Emplist.FirstOrDefault().EmployeeName;
                    double? GROSS_WAGES = Emplist.FirstOrDefault().Gross_Amount;
                    double? EPFWages = (Emplist.FirstOrDefault().Basic == null || Emplist.FirstOrDefault().Basic == 0) ? (Emplist.FirstOrDefault().Stipend == null || Emplist.FirstOrDefault().Stipend == 0 ? 0 : Emplist.FirstOrDefault().Stipend) : Emplist.FirstOrDefault().Basic;
                    double? EDLIWages = (Emplist.FirstOrDefault().Basic == null || Emplist.FirstOrDefault().Basic == 0) ? (Emplist.FirstOrDefault().Stipend == null || Emplist.FirstOrDefault().Stipend == 0 ? 0 : Emplist.FirstOrDefault().Stipend) : Emplist.FirstOrDefault().Basic;
                    double? EPSWages = (Emplist.FirstOrDefault().Basic == null || Emplist.FirstOrDefault().Basic == 0) ? (Emplist.FirstOrDefault().Stipend == null || Emplist.FirstOrDefault().Stipend == 0 ? 0 : Emplist.FirstOrDefault().Stipend) : Emplist.FirstOrDefault().Basic;
                    if (EPSWages > 15000)
                    {
                        EPSWages = EDLIWages = 15000;
                    }
                    if (age > 58)
                    {
                        EPSWages = 0;
                    }
                    double EPFRemitted = Math.Round(Convert.ToDouble(EPFWages * 12 / 100));
                    double EPSRemitted = Math.Round(Convert.ToDouble(EPSWages * 8.33 / 100));
                    double EPFEPSDIFF = EPFRemitted - EPSRemitted;
                    string NCPDays = Emplist.FirstOrDefault().AbsentDays == null ? "0" : Emplist.FirstOrDefault().AbsentDays;

                    TDSdata.Add(new EmployeeSalaryModel
                    {
                        EmployeeName = MemberName,
                        Gross_Amount = GROSS_WAGES,
                        EPFWages = EPFWages,
                        EPSWages = EPSWages,
                        EDLIWages = EDLIWages,
                        EPFRemitted = EPFRemitted,
                        EPSRemitted = EPSRemitted,
                        EPFEPSDIFF = EPFEPSDIFF,
                        NCPDays = NCPDays,
                        ADVANCES = ""
                    });

                }
            }
            employeeSalaryModel.TDSReportList = TDSdata.ToList();
            if (employeeSalaryModel.TDSReportList.Count > 0)
            {
                employeeSalaryModel.TDSReportList[0].TotalCount = GetAllEmployeeSalary_Count(contents);
            }
            return Json(employeeSalaryModel.TDSReportList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult ExportStatutoryReportInExcel(string Month, string Year, string Empid, string Status)
        {
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            employeeSalaryModel.Month = Month;
            string tdsMonth = Month;
            employeeSalaryModel.Year = Convert.ToInt32(Year);
            int tdsYear = Convert.ToInt32(Year);
            employeeSalaryModel.Empid = Convert.ToInt32(Empid);
            employeeSalaryModel.Status = Status;
            int month = DateTime.ParseExact(Month, "MMMM", CultureInfo.CurrentCulture).Month;
            DateTime first = new DateTime(Convert.ToInt32(Year), Convert.ToInt32(month), 1);

            DateTime last = first.AddMonths(1).AddSeconds(-1);
            string firstDayOfMonth = first.ToString();
            string lastDayOfMonth = last.ToString();
            List<EmployeeSalaryModel> data = new List<EmployeeSalaryModel>();
            string contents = JsonConvert.SerializeObject(employeeSalaryModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/Employee/AllEmployeeSalaryview", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmployeeSalaryModel>>(responseString);
                }
            }
            //List<EmployeeSalaryConfigModel> SalaryData = CheckTDS();
            //var matchData = data.Where(t2 => SalaryData.Any(t1 => t1.PayGroupId == t2.PayGroupId)).ToList();
            List<EmployeeSalaryModel> TDSdata = new List<EmployeeSalaryModel>();
            List<EmployeeSalaryModel> TDSRdata = new List<EmployeeSalaryModel>();
            foreach (var x in data)
            {

                int EmpID = Convert.ToInt32(x.Empid);
                List<IndividualSalryModel> Emplist = GetIndividualSalaryData(tdsYear, month, EmpID, tdsMonth);
                if (Emplist.Count > 0)
                {
                    DateTime dob = Emplist.FirstOrDefault().DOB.HasValue ? Convert.ToDateTime(Emplist.FirstOrDefault().DOB) : Convert.ToDateTime((DateTime?)null);
                    int age = CalculateAge(dob);
                    string MemberName = Emplist.FirstOrDefault().EmployeeName;
                    double? GROSS_WAGES = Emplist.FirstOrDefault().Gross_Amount;
                    double? EPFWages = (Emplist.FirstOrDefault().Basic == null || Emplist.FirstOrDefault().Basic == 0) ? (Emplist.FirstOrDefault().Stipend == null || Emplist.FirstOrDefault().Stipend == 0 ? 0 : Emplist.FirstOrDefault().Stipend) : Emplist.FirstOrDefault().Basic;
                    double? EDLIWages = (Emplist.FirstOrDefault().Basic == null || Emplist.FirstOrDefault().Basic == 0) ? (Emplist.FirstOrDefault().Stipend == null || Emplist.FirstOrDefault().Stipend == 0 ? 0 : Emplist.FirstOrDefault().Stipend) : Emplist.FirstOrDefault().Basic;
                    double? EPSWages = (Emplist.FirstOrDefault().Basic == null || Emplist.FirstOrDefault().Basic == 0) ? (Emplist.FirstOrDefault().Stipend == null || Emplist.FirstOrDefault().Stipend == 0 ? 0 : Emplist.FirstOrDefault().Stipend) : Emplist.FirstOrDefault().Basic;
                    if (EPSWages > 15000)
                    {
                        EPSWages = EDLIWages = 15000;
                    }
                    if (age > 58)
                    {
                        EPSWages = 0;
                    }
                    double EPFRemitted = Math.Round(Convert.ToDouble(EPFWages * 12 / 100));
                    double EPSRemitted = Math.Round(Convert.ToDouble(EPSWages * 8.33 / 100));
                    double EPFEPSDIFF = EPFRemitted - EPSRemitted;
                    string NCPDays = Emplist.FirstOrDefault().AbsentDays == null ? "0" : Emplist.FirstOrDefault().AbsentDays;

                    TDSdata.Add(new EmployeeSalaryModel
                    {
                        EmployeeName = MemberName,
                        Gross_Amount = GROSS_WAGES,
                        EPFWages = EPFWages,
                        EPSWages = EPSWages,
                        EDLIWages = EDLIWages,
                        EPFRemitted = EPFRemitted,
                        EPSRemitted = EPSRemitted,
                        EPFEPSDIFF = EPFEPSDIFF,
                        NCPDays = NCPDays,
                        ADVANCES = ""
                    });

                }
            }
            //employeeSalaryModel.TDSReportList = TDSdata.ToList();
            TDSRdata = TDSdata.OrderBy(m => m.EmployeeName).ToList();
            DataTable dt = new DataTable();
            dt.Columns.Add("MEMBER NAME");
            dt.Columns.Add("GROSS WAGES");
            dt.Columns.Add("EPF WAGES");
            dt.Columns.Add("EPS WAGES");
            dt.Columns.Add("EDLI WAGES");
            dt.Columns.Add("EPF CONTRI REMITTED");
            dt.Columns.Add("EPS CONTRI REMITTED");
            dt.Columns.Add("EPF EPS DIFF REMITTED");
            dt.Columns.Add("NCP DAYS");
            dt.Columns.Add("ADVANCES");

            for (int i = 0; i < TDSRdata.Count; i++)
            {
                dt.Rows.Add(TDSRdata[i].EmployeeName, TDSRdata[i].Gross_Amount, TDSRdata[i].EPFWages, TDSRdata[i].EPSWages,
                    TDSRdata[i].EDLIWages, TDSRdata[i].EPFRemitted, TDSRdata[i].EPSRemitted, TDSRdata[i].EPFEPSDIFF, TDSRdata[i].NCPDays, "");
            }
            string fileName = Month + "StatutoryReport" + ".xlsx";
            var filepath = Path.Combine(Server.MapPath("~/PDF/"));
            if (System.IO.File.Exists(filepath + fileName))
            {
                System.IO.File.Delete(filepath + fileName);
            }
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                var sheet1 = wb.Worksheets.Add(dt, "Employees");
                sheet1.Table("Table1").ShowAutoFilter = false;
                sheet1.Table("Table1").Theme = XLTableTheme.None;
                sheet1.Columns().AdjustToContents();
                wb.SaveAs(filepath + fileName);

            }
            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        #region Mayukh
        [HttpGet]
        public JsonResult SalarySave(clsSalaryInfo info)
        {
            info.Month = fullmonthname;
            info.Year = Year;
            var a = clsSalary.Salary_Save(info);
            return Json(new { a }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region PF And ESIC Challan
        public ActionResult PFChallan()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            #region bind Employee
            EmployeeListModel employeeListModel = new EmployeeListModel();
            List<SelectListItem> EmpList = new List<SelectListItem>();
            EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            employeeListModel.empid = 0;
            List<EmployeeListModel> employeedata = GetAllEmployee();
            foreach (var items in employeedata)
            {
                EmpList.Add(new SelectListItem
                {
                    Text = items.EmployeeName,
                    Value = items.empid.ToString()
                });
            }
            #endregion
            employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.MonthList = GetMonthList();
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);
        }
        [HttpPost] 
        public JsonResult GetAllEmployeePFChallan(int Month, int Year, int? empParam, string Status, int numberofRow, int pagenumber)
        {
            int? Empid = (empParam == 0 ? (int?)null : empParam);

            // Call SP
            var dt = clsDatabase.fnDataTable("SP_GetEmployeePF_ECR_Report", Month, Year, Empid, Status, numberofRow, pagenumber);

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
            var dt = clsDatabase.fnDataTable("SP_GetEmployeePF_ECR_Report", Month, Year, Empid, "1", 9999, 1);
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

        [HttpPost]
        public JsonResult ExportPFChallanToText(int Month, int Year, int? empParam)
        {
            int? Empid = (empParam == 0 ? (int?)null : empParam);

            // Fetch data from SP
            var dt = clsDatabase.fnDataTable("SP_GetEmployeePF_ECR_Report", Month, Year, Empid, "1", 9999, 1);

            if (dt == null || dt.Rows.Count == 0)
                return Json("NODATA", JsonRequestBehavior.AllowGet);

            // Remove TotalCount column if exists
            if (dt.Columns.Contains("TotalCount"))
                dt.Columns.Remove("TotalCount");

            // Build PF text file content (#~# separated)
            var sb = new StringBuilder();

            foreach (DataRow row in dt.Rows)
            {
                string line = string.Join("#~#", row.ItemArray.Select(v => v?.ToString()?.Trim() ?? ""));
                sb.AppendLine(line);
            }

            // File name format: EPF_ECR_Month_Year.txt
            string fileName = $"EPF_ECR_{Month}_{Year}.txt";
            string folderPath = Server.MapPath("~/Downloads/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            return Json(fileName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadTxt(string fileName)
        {
            string fullPath = Server.MapPath("~/Downloads/" + fileName);
            if (!System.IO.File.Exists(fullPath))
                return HttpNotFound();

            return File(fullPath, "text/plain", fileName);
        }

        //ESIC Challan

        public ActionResult ESIChallan()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            #region bind Employee
            EmployeeListModel employeeListModel = new EmployeeListModel();
            List<SelectListItem> EmpList = new List<SelectListItem>();
            EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            employeeListModel.empid = 0;
            List<EmployeeListModel> employeedata = GetAllEmployee();
            foreach (var items in employeedata)
            {
                EmpList.Add(new SelectListItem
                {
                    Text = items.EmployeeName,
                    Value = items.empid.ToString()
                });
            }
            #endregion
            employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.MonthList = GetMonthList();
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);
        }

        [HttpPost]
        public JsonResult GetAllEmployeeESICChallan(int Month, int Year, int? empParam, string Status, int numberofRow, int pagenumber)
        {
            int? Empid = (empParam == 0 ? (int?)null : empParam);

            // Call SP
            var dt = clsDatabase.fnDataTable("SP_GetEmployee_ESIC_Report", Month, Year, Empid, numberofRow, pagenumber);

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
        public JsonResult ExportESICChallanToCSV(int Month, int Year, int? empParam)
        {
            int? Empid = (empParam == 0 ? (int?)null : empParam);

            // Fetch data from SP
            var dt = clsDatabase.fnDataTable("SP_GetEmployee_ESIC_Report", Month, Year, Empid, 9999, 1);
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
            string fileName = $"ESIC_Challan_{Month}_{Year}.csv";
            string folderPath = Server.MapPath("~/Downloads/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

            return Json(fileName, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ESIC_SetUp
        public ActionResult ESIC_SetUp()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            // Esic Process
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "ESIC-EMPLOYEE-PROCESS";
            clsAccessLog.AccessLog_Save(info);
            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();
            #region bind Employee
            EmployeeListModel employeeListModel = new EmployeeListModel();
            List<SelectListItem> EmpList = new List<SelectListItem>();
            EmpList.Add(new SelectListItem { Text = "Select Employee", Value = "0", Selected = true });
            employeeListModel.empid = 0;
            List<EmployeeListModel> employeedata = GetAllEmployee();
            foreach (var items in employeedata)
            {
                EmpList.Add(new SelectListItem
                {
                    Text = items.EmployeeName,
                    Value = items.empid.ToString()
                });
            }
            #endregion
            employeeSalaryModel.masterModel.selectListItems = EmpList;
            employeeSalaryModel.MonthList = GetMonthList();
            employeeSalaryModel.YearList = GetYearList();
            return View(employeeSalaryModel);
        }
        [HttpPost]
        public JsonResult SaveEmployeeSlab(int EmployeeId, int SlabId, int IsEligible, string remark)
        {
            try
            {
                // Insert into DB (Payroll_ESIC_EmployeeSlab)
                Payroll_ESIC_EmployeeSlab model = new Payroll_ESIC_EmployeeSlab
                {
                    EmployeeId = EmployeeId,
                    SlabId = SlabId,
                    IsEligible = Convert.ToBoolean(IsEligible),
                    Remark = remark,
                    CreatedBy = Session["UserName"].ToString()
                };

                clsDatabase.fnDataTable("SP_Save_ESIC_EmployeeSlab", model.EmployeeId, model.SlabId, model.IsEligible, model.Remark, model.CreatedBy);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetEmployeeSlabs()
        {
            // Fetch from DB (Payroll_ESIC_EmployeeSlab with join on Employee and SlabMaster)
            DataTable DT = clsDatabase.fnDataTable("SP_Get_ESIC_EmployeeSlabs");
            var data = DT.AsEnumerable().Select(row => new
            {
                EmployeeSlabId = row.Field<int>("EmployeeSlabId"),
                EmployeeName = row.Field<string>("EmployeeName"),
                SlabName = row.Field<string>("SlabName"),
                IsEligible = row.Field<bool>("IsEligible")
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteEmployeeSlab(int EmployeeSlabId)
        {
            try
            {
                DataTable dataTable = clsDatabase.fnDataTable("SP_Delete_ESIC_EmployeeSlab", EmployeeSlabId);
                return Json(new { success = true, message = "Delete Successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion
        #region Loan Register
        public ActionResult LoanRegister()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Index", "Login");

            EmployeeSalaryModel employeeSalaryModel = new EmployeeSalaryModel();

            // Employee dropdown
            var empList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Select Employee", Value = "0", Selected = true }
            };
            foreach (var emp in GetAllEmployee())
            {
                empList.Add(new SelectListItem
                {
                    Text = emp.EmployeeName,
                    Value = emp.empid.ToString()
                });
            }
            employeeSalaryModel.masterModel.selectListItems = empList;

            // Month dropdown
            employeeSalaryModel.MonthList = GetMonthList();

            DataTable fyTable = clsSalary.FinancialYears(); // Must return columns: FinancialYear, Year

            List<SelectListItem> fyList = new List<SelectListItem>();

            foreach (DataRow row in fyTable.Rows)
            {
                SelectListItem item = new SelectListItem();
                item.Text = row["FinancialYear"].ToString(); // e.g., "2024-2025"
                item.Value = row["Year"].ToString();         // e.g., "2025"
                fyList.Add(item);
            }

            employeeSalaryModel.YearList = fyList;



            // Optional: select current FY
            var currentFY = employeeSalaryModel.YearList
            .FirstOrDefault(y => y.Value == (DateTime.Now.Year).ToString());
            if (currentFY != null)
                currentFY.Selected = true;


            return View(employeeSalaryModel);
        }
        [HttpPost]
        public JsonResult GetAllEmployeeLoanRegister(string Year, int? empParam, int numberofRow, int pagenumber)
        {
            try
            {
                int? IDEmployee = (empParam == 0 ? (int?)null : empParam);

                // 1. Fetch full SP result
                DataTable dt = clsDatabase.fnDataTable("PRC_Loan_Ledger_ByFY", Year, IDEmployee, numberofRow, pagenumber);

                if (dt == null || dt.Rows.Count == 0)
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);

                // 2. Convert DataTable → list of dictionary (row-wise)
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow dr in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                        dict[col.ColumnName] = dr[col] == DBNull.Value ? null : dr[col];
                    rows.Add(dict);
                }

                // 3. Department-wise subtotals
                var grouped = rows.GroupBy(r => r["Department"]?.ToString() ?? "Unknown").ToList();
                var final = new List<Dictionary<string, object>>();

                foreach (var grp in grouped)
                {
                    final.AddRange(grp); // add all department rows first

                    // subtotal row
                    var subtotal = new Dictionary<string, object>();
                    subtotal["Department"] = grp.Key;
                    subtotal["Employee Name"] = "Subtotal";

                    foreach (DataColumn col in dt.Columns)
                    {
                        string name = col.ColumnName;
                        if (IsNumericType(col.DataType))
                        {
                            decimal sum = grp.Sum(r =>
                            {
                                var val = r[name];
                                if (val == null) return 0;
                                decimal d;
                                return decimal.TryParse(val.ToString(), out d) ? d : 0;
                            });
                            subtotal[name] = sum;
                        }
                        else if (!subtotal.ContainsKey(name))
                        {
                            subtotal[name] = null;
                        }
                    }

                    final.Add(subtotal);
                }

                // 4. Grand total row
                var grand = new Dictionary<string, object>();
                grand["Department"] = "Grand Total";
                grand["Employee Name"] = "";

                foreach (DataColumn col in dt.Columns)
                {
                    string name = col.ColumnName;
                    if (IsNumericType(col.DataType))
                    {
                        decimal sum = final
                            .Where(r => r.ContainsKey("Employee Name") && (r["Employee Name"]?.ToString() == "Subtotal"))
                            .Sum(r =>
                            {
                                var val = r[name];
                                if (val == null) return 0;
                                decimal d;
                                return decimal.TryParse(val.ToString(), out d) ? d : 0;
                            });
                        grand[name] = sum;
                    }
                    else if (!grand.ContainsKey(name))
                    {
                        grand[name] = null;
                    }
                }

                final.Add(grand);

                // 5. Return full JSON list
                return Json(final, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(@"C:\Temp\LoanRegisterError.log",
                    DateTime.Now + " :: " + ex.ToString() + Environment.NewLine);
                return Json(new { error = true, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool IsNumericType(Type t)
        {
            return t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                   t == typeof(int) || t == typeof(long) || t == typeof(short);
        }
        [HttpPost]
        public JsonResult ExportLoanRegisterToCSV(string Year, int? empParam)
        {
            try
            {
                int? IDEmployee = (empParam == 0 ? (int?)null : empParam);
                int numberofRow = 9999; // fetch all rows
                int pagenumber = 1;

                // 1. Get data from SP
                DataTable dt = clsDatabase.fnDataTable("PRC_Loan_Ledger_ByFY", Year, IDEmployee, numberofRow, pagenumber);
                if (dt == null || dt.Rows.Count == 0)
                    return Json("NODATA", JsonRequestBehavior.AllowGet);
                // Remove TotalCount column if exists
                if (dt.Columns.Contains("TotalCount"))
                    dt.Columns.Remove("TotalCount");
                // Remove TotalCount column if exists
                if (dt.Columns.Contains("RN"))
                    dt.Columns.Remove("RN");
                // 2. Clone table for modifications (subtotal + grand total)
                DataTable grouped = dt.Clone();
                var deptGroups = dt.AsEnumerable().GroupBy(r => r["Department"].ToString());

                foreach (var group in deptGroups)
                {
                    // Add all rows for this department
                    foreach (var row in group)
                        grouped.ImportRow(row);

                    // Create subtotal row
                    DataRow subtotal = grouped.NewRow();

                    // Fill all string columns to avoid null issues
                    foreach (DataColumn col in grouped.Columns)
                        if (col.DataType == typeof(string))
                            subtotal[col.ColumnName] = "";

                    subtotal["Department"] = group.Key;
                    subtotal["Employee Name"] = "Subtotal";
                    subtotal["Financial Year"] = Year;

                    // Sum numeric columns
                    foreach (DataColumn col in grouped.Columns)
                    {
                        if (!new[] { "Department", "Employee Name", "Financial Year", "Loan No" }.Contains(col.ColumnName)
                            && (col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                                col.DataType == typeof(float) || col.DataType == typeof(int)))
                        {
                            decimal sum = group.Sum(r =>
                            {
                                decimal d;
                                return decimal.TryParse(r[col.ColumnName]?.ToString(), out d) ? d : 0;
                            });
                            subtotal[col.ColumnName] = sum;
                        }
                    }
                    grouped.Rows.Add(subtotal);
                }

                // 3. Add grand total row
                DataRow grand = grouped.NewRow();
                foreach (DataColumn col in grouped.Columns)
                    if (col.DataType == typeof(string))
                        grand[col.ColumnName] = "";

                grand["Department"] = "Grand Total";
                grand["Financial Year"] = Year;

                foreach (DataColumn col in grouped.Columns)
                {
                    if (!new[] { "Department", "Employee Name", "RN", "TotalCount", "Financial Year", "Loan No" }.Contains(col.ColumnName)
                        && (col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                            col.DataType == typeof(float) || col.DataType == typeof(int)))
                    {
                        decimal sum = grouped.AsEnumerable()
                            .Where(r => r["Employee Name"].ToString() == "Subtotal")
                            .Sum(r =>
                            {
                                decimal d;
                                return decimal.TryParse(r[col.ColumnName]?.ToString(), out d) ? d : 0;
                            });
                        grand[col.ColumnName] = sum;
                    }
                }
                grouped.Rows.Add(grand);

                // 4. Build CSV
                var csv = new StringBuilder();

                // Header
                for (int i = 0; i < grouped.Columns.Count; i++)
                {
                    csv.Append(grouped.Columns[i].ColumnName);
                    if (i < grouped.Columns.Count - 1)
                        csv.Append(",");
                }
                csv.AppendLine();

                // Rows
                foreach (DataRow row in grouped.Rows)
                {
                    for (int i = 0; i < grouped.Columns.Count; i++)
                    {
                        string val = row[i]?.ToString().Replace(",", " ") ?? "0";
                        if (string.IsNullOrWhiteSpace(val)) val = "0";
                        csv.Append(val);
                        if (i < grouped.Columns.Count - 1)
                            csv.Append(",");
                    }
                    csv.AppendLine();
                }

                // 5. Save file (same as ESIC)
                string fileName = $"LoanRegister_{Year}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                string folderPath = Server.MapPath("~/Downloads/");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);
                System.IO.File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);

                // 6. Return filename to AJAX
                return Json(fileName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            { 
                return Json("ERROR", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

    }
}
