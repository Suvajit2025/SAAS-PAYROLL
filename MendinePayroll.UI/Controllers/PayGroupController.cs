using Common.Utility;
using MendinePayroll.Models;
using MendinePayroll.UI.Models;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using UI.BLL;

namespace MendinePayroll.UI.Controllers
{

    public class PayGroupController : Controller
    {
        ApiCommon ObjAPI = new ApiCommon();
        // GET: PayGroup
        public ActionResult Index()
        {
            if (Session["UserName"] == null && Session["TenantID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Access Log Data Insert 
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "PAYGROUP-ENTRY";
            clsAccessLog.AccessLog_Save(info);


            #region bind HR
            List<EmpbasicModel> data = new List<EmpbasicModel>();
            EmpbasicModel empbasicModel = new EmpbasicModel();
            List<SelectListItem> HRList = new List<SelectListItem>();
            HRList.Add(new SelectListItem { Text = "Select HR Name", Value = "0" });
            //empbasicModel.empdesignation = Convert.ToInt32(9);
            empbasicModel.TenantID = Session["TenantID"].ToString();
            string contentss = JsonConvert.SerializeObject(empbasicModel);
            HttpResponseMessage responses = ObjAPI.CallAPI("api/PayGroup/GetAllEmployeeByDesignation", contentss);
            if (responses.IsSuccessStatusCode)
            {
                string responseString = responses.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<EmpbasicModel>>(responseString); ;
                }
            }
            foreach (var items in data)
            {
                HRList.Add(new SelectListItem
                {
                    Text = items.empfirstname + " " + items.empmiddlename + " "+items.emplastname,
                    Value = items.empid.ToString()
                });
            }
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.masterModel.selectListItems = HRList;
            #endregion

            //tenderModel = new TenderModel() { TenderID = 0 };
            //PayGroupModel payGroupModel   = new PayGroupModel();
            payGroupModel.PayGroupID = 0;
            List<PayGroupModel> PaygroupList = new List<PayGroupModel>();
            //tenderModel.TenderID = 0;
            payGroupModel.TenantID = Session["TenantID"].ToString();
            //tenderModel.ProsumersID = Convert.ToInt32(Session["ProsumersID"]);

            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/GetAllPayGroupList", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    payGroupModel.PayGroupList = JsonConvert.DeserializeObject<List<PayGroupModel>>(responseString);
                }
            }
            
            //tenderModel.TenderList = tenderList;
            return View(payGroupModel);
        }
        #region Save PayGroup
        public JsonResult SavePayGroup(string PayGroupName,string Description,string PayGroupMasterCode,string ConcernHRPersonnel)
        {
            var data="";
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID = 0;
            payGroupModel.PayGroupName = PayGroupName;
            payGroupModel.Description = Description;
            payGroupModel.PayGroupMasterCode = PayGroupMasterCode;
            payGroupModel.ConcernHRPersonnel =Convert.ToInt32(ConcernHRPersonnel);
            payGroupModel.TenantID = Session["TenantID"].ToString();
            payGroupModel.EntryUser = Session["UserEmail"].ToString();

            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SavePayGroup", contents);
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

        #region Get PayGroup
        public JsonResult GetPayGroup(string PayGroupID)
        {
            List<PayGroupModel> data = new List<PayGroupModel>();
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID =Convert.ToInt32(PayGroupID);
            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/PayGroupListById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayGroupModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Update PayGroup
        public JsonResult UpdatePayGroup(string PayGroupId, string PayGroupName, string Description, string PayGroupMasterCode, string ConcernHRPersonnel)
        {
            var data = "";
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID = Convert.ToInt32(PayGroupId);
            payGroupModel.PayGroupName = PayGroupName;
            payGroupModel.Description = Description;
            payGroupModel.PayGroupMasterCode = PayGroupMasterCode;
            payGroupModel.ConcernHRPersonnel = Convert.ToInt32(ConcernHRPersonnel);
            payGroupModel.TenantID = Session["TenantID"].ToString();
            payGroupModel.EntryUser = Session["UserEmail"].ToString();

            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SavePayGroup", contents);
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

        #region SavePayGroupBonus 
        public JsonResult SavePayGroupBonus(string ID, string PayGroupId, string Month, string Year, string OverTime, string ProductionBonus,string TargetAchieved)
        {
            var data = "";
            PayGroupBonusModel payGroupBonusModel = new PayGroupBonusModel();
            payGroupBonusModel.ID = Convert.ToInt32(ID);
            payGroupBonusModel.PayGroupId = Convert.ToInt32(PayGroupId);
            payGroupBonusModel.Month = Month;
            payGroupBonusModel.Year = Convert.ToInt32(Year);
            payGroupBonusModel.OverTimeHours = Convert.ToDouble(OverTime);
            payGroupBonusModel.ProductionBonus = Convert.ToDouble(ProductionBonus);
            payGroupBonusModel.TargetAchieved = Convert.ToDouble(TargetAchieved);

            string contents = JsonConvert.SerializeObject(payGroupBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/SavePayGroupBonus", contents);
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

        #region Delete Pay Group
        public JsonResult DeletePayGroup(string PayGroupID)
        {
            var data = "";
            PayGroupModel payGroupModel = new PayGroupModel();
            payGroupModel.PayGroupID = Convert.ToInt32(PayGroupID);
            string contents = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGoup/DetetePayGroup", contents);
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


        #region paygroup bonus
        public ActionResult PayGroupBonusDetails(string cal)
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            PayGroupBonusModel payGroupBonusModel = new PayGroupBonusModel();
            List<PayGroupBonusModel> PaygroupList = new List<PayGroupBonusModel>();
            payGroupBonusModel.PayGroupId = Convert.ToInt32(cal);
            string contents = JsonConvert.SerializeObject(payGroupBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/GetAllPayGroupBonusById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    payGroupBonusModel.PayGroupBonusList = JsonConvert.DeserializeObject<List<PayGroupBonusModel>>(responseString);
                }
            }

            return View(payGroupBonusModel);
            
        }

        #endregion

        #region Get PayGroupBonusByID
        public JsonResult GetPayGroupBonusById(string ID)
        {
            List<PayGroupBonusModel> data = new List<PayGroupBonusModel>();
            PayGroupBonusModel payGroupBonusModel = new PayGroupBonusModel();
            payGroupBonusModel.ID = Convert.ToInt32(ID);
            string contents = JsonConvert.SerializeObject(payGroupBonusModel);
            HttpResponseMessage response = ObjAPI.CallAPI("api/PayGroup/GetPayGroupBonusById", contents);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(responseString))
                {
                    data = JsonConvert.DeserializeObject<List<PayGroupBonusModel>>(responseString); ;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region PayGroup_Access
        public ActionResult PayGroupAccess(int? selectedEmpId = null)
        {
            // 1. Session Security & Access Log
            if (Session["UserName"] == null || Session["TenantID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            clsAccessLogInfo info = new clsAccessLogInfo { AccessType = "PAYGROUP-ACCESS-MAPPING" };
            clsAccessLog.AccessLog_Save(info);

            // 2. Prepare the Model
            PayGroupModel payGroupModel = new PayGroupModel();
            string tenantID = Session["TenantID"].ToString();
            string jsonParams = JsonConvert.SerializeObject(new { TenantID = tenantID });

            #region Bind HR Dropdown (Direct SP Call)
            List<SelectListItem> HRList = new List<SelectListItem>();
            HRList.Add(new SelectListItem { Text = "-- Select HR Personnel --", Value = "0" });

            try
            {
                // Calling the Database Operation directly
                // Ensure the parameter names match your Stored Procedure (@TenantID)
                DataTable dtEmp = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEE_LIST", tenantID);

                if (dtEmp != null && dtEmp.Rows.Count > 0)
                {
                    foreach (DataRow row in dtEmp.Rows)
                    {
                        int empId = Convert.ToInt32(row["empid"]);
                        string firstName = row["empfirstname"].ToString();
                        string lastName = row["emplastname"].ToString();
                        string empCode = row["empcode"].ToString();

                        HRList.Add(new SelectListItem
                        {
                            Text = $"{firstName} {lastName} ({empCode})",
                            Value = empId.ToString(),
                            Selected = (selectedEmpId.HasValue && empId == selectedEmpId.Value)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error accordingly
                ViewBag.Error = "Error loading HR Personnel: " + ex.Message;
            }

            payGroupModel.masterModel.selectListItems = HRList;
            #endregion

            #region Bind PayGroup Checkbox List
            payGroupModel.TenantID = tenantID;
            payGroupModel.PayGroupID = 0; // To get all groups

            string pgContent = JsonConvert.SerializeObject(payGroupModel);
            HttpResponseMessage pgResponse = ObjAPI.CallAPI("api/PayGroup/GetAllPayGroupList", pgContent);

            if (pgResponse.IsSuccessStatusCode)
            {
                string pgResString = pgResponse.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(pgResString))
                {
                    var fullList = JsonConvert.DeserializeObject<List<PayGroupModel>>(pgResString);

                    // --- LOGIC: Check previously mapped groups from Database ---
                    if (selectedEmpId.HasValue && selectedEmpId > 0)
                    {
                        int EmployeeID= selectedEmpId.Value;
                        DataTable dtMapping = clsDatabase.fnDataTable("SP_Get_User_Mapped_PayGroups", EmployeeID, tenantID);

                        if (dtMapping != null && dtMapping.Rows.Count > 0)
                        {
                            // Extract mapped IDs into a hashset for faster lookup
                            var mappedIds = dtMapping.AsEnumerable()
                                                     .Select(r => r.Field<int>("PayGroupID"))
                                                     .ToList();

                            // Mark items as checked in the list
                            foreach (var pg in fullList)
                            {
                                if (mappedIds.Contains(pg.PayGroupID))
                                {
                                    pg.IsChecked = true;
                                }
                            }
                        }
                    }

                    payGroupModel.PayGroupList = fullList;
                }
            }
            #endregion

            return View(payGroupModel);
        }

        [HttpPost]
        public JsonResult SaveMapping(string employeeId, string payGroupIds)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId) || employeeId == "0")
                {
                    return Json(new { success = false, message = "Invalid Employee Selection" });
                }

                // 1. Prepare Parameters
                int EmployeeID = Convert.ToInt32(employeeId);
                string TenantID = Session["TenantID"].ToString();
                string EntryUser = Session["UserName"]?.ToString() ?? "System";

                // 2. Call Stored Procedure using clsDatabase
                // Assuming your utility supports: fnExecuteNonQuery(SPName, params...) 

                clsDatabase.fnDBOperation("SP_Payroll_Update_PayGroup_Mapping", EmployeeID, payGroupIds, TenantID, EntryUser);

                return Json(new { success = true, message = "Access permissions saved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Database Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetMappedPayGroups(string EmployeeID)
        {
            try
            {
                string tenantId = Session["TenantID"].ToString();

                // Use your Stored Procedure here 
                DataTable dt = clsDatabase.fnDataTable("SP_Get_User_Mapped_PayGroups", EmployeeID, tenantId);

                // Convert the "PayGroupID" column into a List of ints
                var mappedIds = dt.AsEnumerable()
                                 .Select(r => r.Field<int>("PayGroupID"))
                                 .ToList();

                return Json(mappedIds, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new List<int>(), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}