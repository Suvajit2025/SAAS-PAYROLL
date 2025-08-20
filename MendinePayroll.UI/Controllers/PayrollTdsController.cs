using ClosedXML.Excel;
using Common.Utility;
using MendinePayroll.Models;
using MendinePayroll.UI.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LicenseContext = OfficeOpenXml.LicenseContext;
using Path = System.IO.Path;

namespace MendinePayroll.UI.Controllers
{
    public class PayrollTdsController : Controller
    {
        // GET: PayrollTds
        public ActionResult Index()
        {
            return View();
        }
        #region TDS&ARREAR&CTC SETUP
        public ActionResult EmployeeDeductionSetUp()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        public ActionResult GetDeductionRules()
        {
            var rules = new List<DeductionRuleModel>();
            DataTable dt = clsDatabase.fnDataTable("SP_GET_Payroll_EmployeeDeductionSetting");
            foreach (DataRow row in dt.Rows)
            {
                rules.Add(new DeductionRuleModel
                {
                    PayConfigId = Convert.ToInt32(row["PayConfigId"]),
                    PayConfigName = row["PayConfigName"].ToString(),
                    FinancialYear = row["FinancialYear"].ToString(),
                    SettingName = row["SettingName"].ToString(),
                    SettingType = row["Type"].ToString(),
                    LowerLimit = Convert.ToDecimal(row["LowerLimit"]).ToString(),
                    UpperLimit = Convert.ToDecimal(row["UpperLimit"]).ToString(),
                    Rate = Convert.ToDecimal(row["Rate"]).ToString(),
                    CalculatedOn = row["CalculatedOn"].ToString(),
                    EffectiveFrom = row["EffectiveFrom"].ToString(),
                    StateId = Convert.ToInt32(row["StateId"]),
                    State = row["Statename"].ToString(),
                    RoundOffType = row["RoundoffType"].ToString()
                });
            }

            // 4. Return JSON for your AJAX loader
            return Json(rules, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult SaveDeductionRules(List<DeductionRuleModel> rules)
        {
            if (rules == null || rules.Count == 0)
                return Json(new { success = false, message = "No rules received." });
            string createdBy = Session["UserName"]?.ToString() ?? "Unknown";

            // Assign CreatedBy to each rule
            foreach (var rule in rules)
            {
                rule.CreatedBy = createdBy;
            }
            // Serialize rules to JSON string
            string json = JsonConvert.SerializeObject(rules);

            string result = clsDatabase.fnDBOperation("SP_Save_Payroll_EmployeeDeductionSetting", json);


            return Json(new { success = true, message = "Saved successfully." });
        }

        public ActionResult TaxSlab()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
        [HttpGet]
        public ActionResult GetTdsTaxSlab()
        {
            var slabs = new List<TdsTaxSlab>();
            DataTable dt = clsDatabase.fnDataTable("SP_GET_Payroll_TaxSlab");
            foreach (DataRow row in dt.Rows)
            {
                slabs.Add(new TdsTaxSlab
                {
                    Regimetypeid = Convert.ToInt32(row["RegimeId"]),
                    RegimetypeName = row["RegimeName"].ToString(),
                    Fromincome = Convert.ToDecimal(row["FromIncome"]).ToString("0.######"),
                    Toincome = Convert.ToDecimal(row["ToIncome"]).ToString("0.######"), // up to 6 decimal places
                    FinancialYear = row["FinancialYear"].ToString(),
                    Tdsrate = (Convert.ToDecimal(row["TaxRate"]) * 100).ToString("0.##"),
                    AssessmentYear = row["AssessmentYear"].ToString()
                });
            }
            // 4. Return JSON for your AJAX loader
            return Json(slabs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveTdsTaxSlab(List<TdsTaxSlab> slab)
        {
            if (slab == null || slab.Count == 0)
                return Json(new { success = false, message = "No slab received." });
            // Convert Tdsrate from percentage to decimal
            foreach (var s in slab)
            {
                if (decimal.TryParse(s.Tdsrate, out decimal rate))
                {
                    s.Tdsrate = (rate / 100).ToString("0.######"); // up to 6 decimal places
                }
                else
                {
                    return Json(new { success = false, message = $"Invalid TDS rate: {s.Tdsrate}" });
                }
            }
            // Serialize rules to JSON string
            string json = JsonConvert.SerializeObject(slab);

            string result = clsDatabase.fnDBOperation("SP_Save_Payroll_TaxSlab", json);


            return Json(new { success = true, message = "Saved successfully." });
        }

        public ActionResult TDSSetup()
        {

            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [HttpGet]
        public ActionResult GetTdsSetUpData()
        {
            var slabs = new List<clsTdsSetup>();
            DataTable dt = clsDatabase.fnDataTable("SP_GET_Payroll_TaxDeductionSetting");

            foreach (DataRow row in dt.Rows)
            {
                var item = new clsTdsSetup
                {
                    RegimeId = Convert.ToInt32(row["RegimeId"]),
                    RegimeName = row["RegimeName"].ToString(),
                    FinancialYear = row["FinancialYear"].ToString(),
                    AssessmentYear = row["AssessmentYear"].ToString(),
                    SectionCode = row["SectionCode"].ToString(),
                    DeductionName = row["DeductionName"].ToString(),
                    MaxLimit = row["MaxLimit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["MaxLimit"]),
                    DeductionType = row["DeductionType"].ToString(),
                    IsLinked = row["IsLinked"].ToString(),  // Already 'Yes'/'No' from SQL
                    AutoCalculated = row["AutoCalculated"].ToString(), // Already 'Yes'/'No' from SQL
                    PayConfigName = row["PayConfigName"].ToString(),
                    ProofRequired = row["ProofRequired"].ToString(), // Already 'Yes'/'No' from SQL
                    StandardDeduction = row["StandardDeduction"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["StandardDeduction"]),
                    RebateAmount = row["RebateAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["RebateAmount"]),
                    CessRate = row["CessRate"] == DBNull.Value ? null : (Convert.ToDecimal(row["CessRate"]) * 100).ToString("0.##")

                };

                slabs.Add(item);
            }

            return Json(slabs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveTdsSetup(List<clsTdsSetup> tdsSetupList)
        {
            string createdBy = Session["UserName"]?.ToString() ?? "Unknown";
            try
            {
                foreach (var s in tdsSetupList)
                {
                    if (s.DeductionType == "STANDERED DEDUCTION" && !string.IsNullOrWhiteSpace(s.CessRate))
                    {
                        if (decimal.TryParse(s.CessRate, out decimal rate))
                        {
                            // Convert from percentage string (e.g., "4" meaning 4%) to decimal (0.04)
                            s.CessRate = (rate / 100).ToString("0.######"); // Up to 6 decimal places
                        }
                        else
                        {
                            return Json(new { success = false, message = $"Invalid Cess Rate: {s.CessRate}" });
                        }
                    }
                }

                // Serialize rules to JSON string
                string json = JsonConvert.SerializeObject(tdsSetupList);
                string result = clsDatabase.fnDBOperation("SP_Save_Payroll_TaxDeductionSetting", json, createdBy);

                return Json(new { success = true, message = "Saved successfully." });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }


        #endregion

        #region CTC And ARREAR CALCULATION
        public ActionResult EmployeeSalaryDetailsCTC(string Empid, string PayGroupID)
        {
            Empid = DataEncryption.Decrypt(Convert.ToString(Empid), "passKey");
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            //int licenceid = Convert.ToInt32(Request.QueryString["Empid"]);
            int licenceid = Convert.ToInt32(Empid);
            return View(licenceid);
        }

        [HttpPost]
        public JsonResult GetManualConfigureSalaryNew(string PayGroupID)
        {
            // Check if PayGroupID is null or empty
            if (string.IsNullOrEmpty(PayGroupID))
            {
                return Json(new { success = false, message = "PayGroupID is required" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Retrieve data from database
                DataTable dt = clsDatabase.fnDataTable("ManualSalaryConfig_GetByPayGroupID_NewCTC", PayGroupID);
                // Serialize the DataTable to JSON using Newtonsoft.Json to handle circular references
                var jsonData = JsonConvert.SerializeObject(dt, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                if (dt == null || dt.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "No data found for the provided PayGroupID" }, JsonRequestBehavior.AllowGet);
                }

                // Return the data as JSON
                return Json(new { success = true, data = jsonData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging library like NLog, Serilog, etc.)
                // Log.Error(ex, "Error occurred while fetching salary data for PayGroupID: " + PayGroupID);

                return Json(new { success = false, message = "An error occurred while processing your request", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<JsonResult> CalculateCTCJson()
        {
            try
            {
                Request.InputStream.Position = 0;

                using (var reader = new StreamReader(Request.InputStream))
                {
                    string body = reader.ReadToEnd();

                    // Deserialize JSON manually from raw body
                    JObject inputJson = JsonConvert.DeserializeObject<JObject>(body);

                    // 🔐 Decrypt EmpId
                    string encryptedEmpId = inputJson["EmpId"]?.ToString();

                    int empId = Convert.ToInt32(DataEncryption.Decrypt(encryptedEmpId, "passKey"));

                    inputJson["EmpId"] = empId.ToString();

                    // ✅ Safely extract IsArrear 
                    bool isArrear = inputJson["IsArrear"]?.ToObject<bool>() ?? false;

                    var NewjsonData = JsonConvert.SerializeObject(inputJson);

                    int payGroupId = (int)inputJson["PayGroupId"];

                    int EmpId = (int)inputJson["EmpId"];

                    JObject dbJson = GetDbJsonFromDb(empId, inputJson);
                    var oldManualValues = dbJson["OldManualValues"]?.ToObject<List<ManualValue>>() ?? new List<ManualValue>();

                    // Safely parse PayGroupId as integer
                    int parsedPayGroupId = 0;
                    int.TryParse(dbJson["PayGroupId"]?.ToString(), out parsedPayGroupId);

                    bool isFirstTimeSetup =
                        parsedPayGroupId == 0 &&
                        (oldManualValues == null || oldManualValues.Count == 0);

                    SalaryCalculationResult oldResult = new SalaryCalculationResult();
                    string OldProposedSalary = "";
                    List<object> salaryComparison = new List<object>();

                    // Step 1: Calculate old salary only if NOT first-time setup
                    if (!isFirstTimeSetup)
                    {
                        oldResult = CalculateSalaryComponents(oldManualValues, dbJson, empId);

                        OldProposedSalary = JsonConvert.SerializeObject(new
                        {
                            OldSalaryDetails = oldResult.ManualValues
                                .Concat(oldResult.AllowanceValues)
                                .Concat(oldResult.GrossValues)
                                .Concat(oldResult.DeductionValues)
                                .Concat(oldResult.NetValues)
                                .Concat(oldResult.CompanyContributions)
                                .Concat(oldResult.CompanyContributionsBreakup)
                                .Concat(oldResult.TotalContributionValues)
                                .Concat(oldResult.MonthlyCTCValues)
                                .Concat(oldResult.YearlyCTCValues)
                        });
                    }

                    // Step 2: Calculate NEW Manual Values (same as oldManualValues in this context)
                    var newManualValues = inputJson["NewManualValues"]?.ToObject<List<ManualValue>>() ?? new List<ManualValue>();
                    var newResult = CalculateSalaryComponents(newManualValues, inputJson, empId);
                    //var newResult = CalculateSalaryComponents(oldManualValues, inputJson, empId);

                    // Step 3: Serialize New Proposed Salary
                    string NewProposedSalary = JsonConvert.SerializeObject(new
                    {
                        NewSalaryDetails = newResult.ManualValues
                            .Concat(newResult.AllowanceValues)
                            .Concat(newResult.GrossValues)
                            .Concat(newResult.DeductionValues)
                            .Concat(newResult.NetValues)
                            .Concat(newResult.CompanyContributions)
                            .Concat(newResult.CompanyContributionsBreakup)
                            .Concat(newResult.TotalContributionValues)
                            .Concat(newResult.MonthlyCTCValues)
                            .Concat(newResult.YearlyCTCValues)
                    });



                    JObject jsonObject = JObject.Parse(NewjsonData);

                    if (!isFirstTimeSetup && AreSalaryResultsEqual(oldResult, newResult))
                    {
                        isFirstTimeSetup = true;
                    }

                    // Step 4: Build salary comparison table
                    salaryComparison = BuildSalaryComparisonTable(
                        isFirstTimeSetup
                            ? new List<outPutJson>() // no old values
                            : oldResult.ManualValues
                                .Concat(oldResult.AllowanceValues)
                                .Concat(oldResult.GrossValues)
                                .Concat(oldResult.DeductionValues)
                                .Concat(oldResult.NetValues)
                                .Concat(oldResult.CompanyContributions)
                                .Concat(oldResult.TotalContributionValues)
                                .Concat(oldResult.MonthlyCTCValues)
                                .Concat(oldResult.YearlyCTCValues)
                                .ToList(),
                        newResult.ManualValues
                            .Concat(newResult.AllowanceValues)
                            .Concat(newResult.GrossValues)
                            .Concat(newResult.DeductionValues)
                            .Concat(newResult.NetValues)
                            .Concat(newResult.CompanyContributions)
                            .Concat(newResult.TotalContributionValues)
                            .Concat(newResult.MonthlyCTCValues)
                            .Concat(newResult.YearlyCTCValues)
                            .ToList()
                    );
                    JObject arrearJson = null;

                    if (isArrear)
                    {
                        arrearJson = CalculateArrer(jsonObject, OldProposedSalary, NewProposedSalary, oldResult.PayGroupId, newResult.PayGroupId);

                        // Deserialize from JSON arrays
                        var arrearMasterList = arrearJson["Payroll_ArrearMaster"]?.ToObject<List<PayrollArrearMaster>>();
                        var arrearDetailsList = arrearJson["Payroll_ArrearDetails"]?.ToObject<List<PayrollArrearDetail>>();
                        var arrearMonthlyBreakupList = arrearJson["Payroll_ArrearMonthlyBreakup"]?.ToObject<List<PayrollArrearMonthlyBreakup>>();

                        // Convert lists back to JSON
                        jsonObject["Payroll_ArrearMaster"] = JArray.FromObject(arrearMasterList);
                        jsonObject["Payroll_ArrearDetails"] = JArray.FromObject(arrearDetailsList);
                        jsonObject["Payroll_ArrearMonthlyBreakup"] = JArray.FromObject(arrearMonthlyBreakupList);
                    }

                    return Json(new
                    {
                        success = true,
                        EmpId = empId,
                        // ✅ Keep master as-is (no PayConfigName inside)
                        Payroll_ArrearMaster = ToArrearMasterList(jsonObject["Payroll_ArrearMaster"]),

                        // ✅ Apply ToUpperCase to Details and MonthlyBreakup
                        Payroll_ArrearDetails = ToUpperCasePayComponentName(jsonObject["Payroll_ArrearDetails"]),
                        Payroll_ArrearMonthlyBreakup = ToArrearMonthlyBreakupList(jsonObject["Payroll_ArrearMonthlyBreakup"]),
                        SalaryBreakupTable = salaryComparison,
                        oldProposedSalary = oldResult,
                        newProposedSalary = newResult
                    }, JsonRequestBehavior.AllowGet);


                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        private bool AreSalaryResultsEqual(SalaryCalculationResult a, SalaryCalculationResult b)
        {
            if (a == null || b == null) return false;

            var tokenA = JObject.FromObject(new
            {
                a.ManualValues,
                a.AllowanceValues,
                a.GrossValues,
                a.DeductionValues,
                a.NetValues,
                a.CompanyContributions,
                a.CompanyContributionsBreakup,
                a.TotalContributionValues,
                a.MonthlyCTCValues,
                a.YearlyCTCValues
            });

            var tokenB = JObject.FromObject(new
            {
                b.ManualValues,
                b.AllowanceValues,
                b.GrossValues,
                b.DeductionValues,
                b.NetValues,
                b.CompanyContributions,
                b.CompanyContributionsBreakup,
                b.TotalContributionValues,
                b.MonthlyCTCValues,
                b.YearlyCTCValues
            });

            return JToken.DeepEquals(tokenA, tokenB);
        }
        private JObject GetDbJsonFromDb(int empId, JObject inputJson)
        {
            DataTable dt = clsDatabase.fnDataTable("GetEmployeeSalaryDetailsNew", empId);

            // Update deduction flags from DB
            bool deductPF = dt.AsEnumerable().Any(row => row["IsPFActive"].ToString() == "1");
            bool deductESIC = dt.AsEnumerable().Any(row => row["IsEsicActive"].ToString() == "1");
            bool deductPTAX = dt.AsEnumerable().Any(row => row.Field<bool>("IsPTaxActive"));
            int payGroupId = dt.Rows.Count > 0 && dt.Rows[0]["PayGroupId"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["PayGroupId"]) : 0;

            // Old Manual Values
            var oldManualValues = dt.AsEnumerable()
                .Where(row => row["Type"].ToString() == "Manual")
                .Select(row => new ManualValue
                {
                    PayConfigID = row["PayConfigId"] != DBNull.Value ? Convert.ToInt32(row["PayConfigId"]) : 0,
                    PayConfigName = row["PayConfigName"]?.ToString() ?? "",
                    Values = row["Values"]?.ToString() ?? "0"
                }).ToList();

            // Company Contributions from DB if PayGroupId is not null
            var dbCompanyContributions = dt.AsEnumerable()
                .Where(row => row["Type"].ToString() == "CC" && row["PayGroupId"] != DBNull.Value)
                .Select(row => new ManualValue
                {
                    PayConfigID = row["PayConfigId"] != DBNull.Value ? Convert.ToInt32(row["PayConfigId"]) : 0,
                    PayConfigName = row["PayConfigName"]?.ToString() ?? "",
                    Values = row["Values"]?.ToString() ?? "0"
                }).ToList();

            // Company Contributions from input
            var inputCompanyContributions = inputJson["CompanyContributions"]?.ToObject<List<ManualValue>>() ?? new List<ManualValue>();

            // Determine source for final contributions
            var useInputCompanyContributions = dt.AsEnumerable().Any(row => row["Type"].ToString() == "CC" && row["PayGroupId"] == DBNull.Value);

            var finalCompanyContributions = useInputCompanyContributions ? inputCompanyContributions : dbCompanyContributions;



            JObject dbJson = new JObject
            {
                ["EmpId"] = empId.ToString(),
                ["PayGroupId"] = payGroupId,
                ["PayGroupId"] = dt.AsEnumerable().FirstOrDefault(row => row["PayGroupId"] != DBNull.Value)?["PayGroupId"].ToString() ?? "",
                ["FinancialYear"] = dt.AsEnumerable().FirstOrDefault()?["FinancialYear"].ToString() ?? "",
                ["OldManualValues"] = JToken.FromObject(oldManualValues),
                ["CompanyContributions"] = JToken.FromObject(finalCompanyContributions),
                ["DateFrom"] = "",
                ["DateTo"] = "",
                ["ArrearMonth"] = "",
                ["DeductPF"] = deductPF,
                ["DeductESIC"] = deductESIC,
                ["DeductPTAX"] = deductPTAX,
                ["IsArrear"] = false
            };

            return dbJson;
        }
        private SalaryCalculationResult CalculateSalaryComponents(List<ManualValue> manualValues, JObject inputJson, int empId)
        {
            int payGroupId = inputJson["PayGroupId"]?.Value<int>() ?? 0;

            var manualValuesDict = manualValues.ToDictionary(
                x => x.PayConfigName,
                x => decimal.TryParse(x.Values, out var val) ? val : 0
            );

            var filteredManuals = manualValues
                .Where(x =>
                    !x.PayConfigName.ToLowerInvariant().Contains("gross") &&
                    decimal.TryParse(x.Values, out var val) && val != 0
                )
                .Select(x => new outPutJson
                {
                    PayConfigId = x.PayConfigID,
                    PayConfigName = x.PayConfigName,
                    PayConfigType = "Manual",
                    ConfigValue = decimal.TryParse(x.Values, out var val) ? val : 0
                })
                .ToList();

            decimal totalManualValues = filteredManuals.Sum(x => x.ConfigValue);

            var allowances = CalculateAllowances(payGroupId, manualValuesDict);
            var filteredAllowances = allowances
                .Where(x => x.ConfigValue != 0)
                .Select(x => new outPutJson
                {
                    PayConfigId = x.PayConfigId,
                    PayConfigName = x.PayConfigName,
                    PayConfigType = x.PayConfigType,
                    ConfigValue = x.ConfigValue
                }).ToList();

            decimal totalAllowances = allowances
                .Where(x => x.PayConfigName.ToLowerInvariant().Contains("total"))
                .Sum(x => x.ConfigValue);

            decimal basicAmount = manualValuesDict.ContainsKey("BASIC") ? manualValuesDict["BASIC"] : 0;
            decimal grossAmount = totalManualValues + totalAllowances;

            var grossKey = manualValuesDict.Keys.FirstOrDefault(k => k.ToUpperInvariant().Contains("GROSS"));

            if (!string.IsNullOrEmpty(grossKey))
            {
                if (manualValuesDict[grossKey] == 0)
                {
                    manualValuesDict[grossKey] = grossAmount;
                }
            }
            else
            {
                manualValuesDict.Add("Gross Amount", grossAmount);
            }

            // Optional: Also update manualValues list if needed
            var grossItem = manualValues.FirstOrDefault(x => x.PayConfigName.ToUpperInvariant().Contains("GROSS"));
            if (grossItem != null)
            {
                grossItem.Values = grossAmount.ToString("0.00");
            }
            else
            {
                manualValues.Add(new ManualValue
                {
                    PayConfigID = 1002, // temporary or dynamic ID
                    PayConfigName = "Gross Amount",
                    Values = grossAmount.ToString("0.00")
                });
            }

            var grossOutput = new List<outPutJson> {
                new outPutJson {
                    PayConfigId = 1002,
                    PayConfigName = "Gross Amount",
                    PayConfigType = "Summary",
                    ConfigValue = Math.Round(grossAmount, 2)
                }
            };




            var deductionFlags = new Dictionary<string, bool>
            {
                { "PF", inputJson["DeductPF"]?.ToObject<bool>() ?? false },
                { "ESIC", inputJson["DeductESIC"]?.ToObject<bool>() ?? false },
                { "PTAX", inputJson["DeductPTAX"]?.ToObject<bool>() ?? false }
            };

            var deductions = CalculateDeductions(payGroupId, deductionFlags, manualValuesDict, filteredAllowances, basicAmount, grossAmount);
            var filteredDeductions = deductions
                .Where(x => x.ConfigValues != 0)
                .Select(x => new outPutJson
                {
                    PayConfigId = x.PayConfigId,
                    PayConfigName = x.PayConfigName,
                    PayConfigType = x.PayConfigType,
                    ConfigValue = x.ConfigValues
                })
                .ToList();

            decimal totalDeductions = deductions
                .Where(x => x.PayConfigName.ToLowerInvariant().Contains("total"))
                .Sum(x => x.ConfigValues);

            decimal netAmount = grossAmount - totalDeductions;

            var netOutput = new List<outPutJson> {
                new outPutJson {
                    PayConfigId = 1004,
                    PayConfigName = "Net Amount",
                    PayConfigType = "Summary",
                    ConfigValue = Math.Round(netAmount, 2)
                }
            };

            var contributionInput = inputJson["CompanyContributions"]?.ToObject<List<CompanyContributionConfig>>() ?? new List<CompanyContributionConfig>();
            var companyContributionDict = contributionInput.ToDictionary(
                x => x.PayConfigName,
                x => (decimal)(x.Values ?? 0)  // Explicit conversion with null check
            );

            var companyContributions = GetCompanyContributionConfigs(payGroupId, basicAmount, grossAmount, companyContributionDict, filteredAllowances, manualValuesDict);
            decimal totalContribution = companyContributions.Sum(c => (decimal)c.Values);

            var contributionOutput = companyContributions
                .GroupBy(x => x.PayConfigId)
                .Where(g => g.Sum(x => x.Values) != 0)
                .Select(g => new outPutJson
                {
                    PayConfigId = g.Key,
                    PayConfigName = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.PayConfigName))?.PayConfigName ?? "",
                    PayConfigType = g.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.PayConfigType))?.PayConfigType ?? "Company Contribution",
                    ConfigValue = g.Sum(x => (decimal)(x.Values ?? 0))  // Ensure this too is explicitly cast
                })
                .ToList();

            var contributionBreakup = companyContributions
                .Where(c => c.BreakupID != 0)
                .Select(c => new outPutJson
                {
                    PayConfigId = c.BreakupID,
                    PayConfigName = c.BreakupName,
                    PayConfigType = "Pf-BreakUp",
                    ConfigValue = (decimal)c.Values
                }).ToList();

            var totalContributionOutput = new List<outPutJson> {
                new outPutJson {
                    PayConfigId = 1005,
                    PayConfigName = "Total Contribution Amount",
                    PayConfigType = "Summary",
                    ConfigValue = Math.Round(totalContribution, 2)
                }
            };

            decimal monthlyCTC = grossAmount + totalContribution;
            var monthlyCTCOutput = new List<outPutJson> {
                new outPutJson {
                    PayConfigId = 1006,
                    PayConfigName = "Monthly CTC Amount",
                    PayConfigType = "Summary",
                    ConfigValue = Math.Round(monthlyCTC, 2)
                }
            };

            decimal yearlyCTC = monthlyCTC * 12;
            var yearlyCTCOutput = new List<outPutJson> {
                new outPutJson {
                    PayConfigId = 1007,
                    PayConfigName = "Yearly CTC Amount",
                    PayConfigType = "Summary",
                    ConfigValue = Math.Round(yearlyCTC, 2)
                }
            };

            return new SalaryCalculationResult
            {
                PayGroupId = payGroupId,
                ManualValues = filteredManuals,
                AllowanceValues = filteredAllowances,
                GrossValues = grossOutput,
                DeductionValues = filteredDeductions,
                NetValues = netOutput,
                CompanyContributions = contributionOutput,
                CompanyContributionsBreakup = contributionBreakup,
                TotalContributionValues = totalContributionOutput,
                MonthlyCTCValues = monthlyCTCOutput,
                YearlyCTCValues = yearlyCTCOutput,
                GrossAmount = grossAmount,
                NetAmount = netAmount,
                MonthlyCTC = monthlyCTC,
                YearlyCTC = yearlyCTC
            };
        }
        private List<object> BuildSalaryComparisonTable(List<outPutJson> oldList, List<outPutJson> newList)
        {
            var table = new List<object>();

            var oldDict = oldList.ToDictionary(
                x => x.PayConfigName?.ToUpper() ?? "",
                x => new { x.ConfigValue, x.PayConfigId, PayConfigType = x.PayConfigType ?? "" });

            var newDict = newList.ToDictionary(
                x => x.PayConfigName?.ToUpper() ?? "",
                x => new { x.ConfigValue, x.PayConfigId, PayConfigType = x.PayConfigType ?? "" });

            var allKeys = oldDict.Keys.Union(newDict.Keys).Distinct();

            //foreach (var key in allKeys)
            //{
            //    var oldEntry = oldDict.ContainsKey(key) ? oldDict[key] : null;
            //    var newEntry = newDict.ContainsKey(key) ? newDict[key] : null;

            //    table.Add(new
            //    {
            //        PayConfigName = key,
            //        PayConfigId = oldEntry?.PayConfigId ?? newEntry?.PayConfigId ?? 0,
            //        PayConfigType = oldEntry?.PayConfigType ?? newEntry?.PayConfigType ?? "",
            //        OldAmount = oldEntry?.ConfigValue ?? 0,
            //        NewAmount = newEntry?.ConfigValue ?? 0
            //    });
            //}
            // 3) Build a strongly-typed sequence of anonymous rows
            var rows = allKeys
                .Select(key => {
                    oldDict.TryGetValue(key, out var o);
                    newDict.TryGetValue(key, out var n);

                    return new
                    {
                        PayConfigName = key,
                        PayConfigId = o?.PayConfigId ?? n?.PayConfigId ?? 0,
                        PayConfigType = o?.PayConfigType ?? n?.PayConfigType ?? "",
                        OldAmount = o?.ConfigValue ?? 0m,
                        NewAmount = n?.ConfigValue ?? 0m
                    };
                })
                .ToList();
            //return table;
            // 4) Ranking function
            int Rank(dynamic r)
            {
                var name = r.PayConfigName;
                var type = r.PayConfigType;
                if (type.Equals("Manual", StringComparison.OrdinalIgnoreCase))
                    return 1;
                if (type.Equals("Allowances", StringComparison.OrdinalIgnoreCase))
                    return 2;
                if (name.Equals("TOTAL ALLOWANCES", StringComparison.OrdinalIgnoreCase))
                    return 3;
                if (name.Equals("GROSS AMOUNT", StringComparison.OrdinalIgnoreCase))
                    return 4;
                if (type.Equals("Deduction", StringComparison.OrdinalIgnoreCase))
                    return 5;
                if (name.Equals("TOTAL DEDUCTIONS", StringComparison.OrdinalIgnoreCase))
                    return 6;
                if (name.Equals("NET AMOUNT", StringComparison.OrdinalIgnoreCase))
                    return 7;
                if (type.Equals("Company Contribution", StringComparison.OrdinalIgnoreCase))
                    return 8;
                if (name.Equals("TOTAL CONTRIBUTION AMOUNT", StringComparison.OrdinalIgnoreCase))
                    return 9;
                if (name.Equals("MONTHLY CTC AMOUNT", StringComparison.OrdinalIgnoreCase))
                    return 10;
                if (name.Equals("YEARLY CTC AMOUNT", StringComparison.OrdinalIgnoreCase))
                    return 11;
                return 12;
            }

            // 5) Sort by rank, then by name
            var sorted = rows
                .OrderBy(r => Rank(r))
                .ThenBy(r => r.PayConfigName)
                .ToList();

            // 6) Cast to object for your return type
            return sorted.Cast<object>().ToList();
        }
        private List<PayrollArrearDetail> ToUpperCasePayComponentName(JToken token)
        {
            var list = token?.ToObject<List<PayrollArrearDetail>>();
            if (list != null)
            {
                foreach (var item in list)
                {
                    item.PayConfigName = item.PayConfigName?.ToUpper();
                }
            }
            return list;
        }
        private List<PayrollArrearMaster> ToArrearMasterList(JToken token)
        {

            return token?.ToObject<List<PayrollArrearMaster>>();
        }
        private List<PayrollArrearMonthlyBreakup> ToArrearMonthlyBreakupList(JToken token)
        {

            return token?.ToObject<List<PayrollArrearMonthlyBreakup>>();
        }
        private string Normalize(string input)
        {
            if (input == null) return null;

            input = System.Text.RegularExpressions.Regex.Replace(input, @"[.\-\s]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return input.ToUpperInvariant();
        }
        public static decimal RoundEmployerESICContribution(decimal contribution)
        {
            //decimal integerPart = Math.Floor(contribution);
            //decimal decimalPart = contribution - integerPart;

            //if (decimalPart >= 0.5m)
            //{
            //    return Math.Ceiling(contribution);
            //}
            //else
            //{
            //    return integerPart; // Effectively rounding down (ignoring decimal)
            //}
            return Math.Ceiling(contribution);
        }

        public List<PayConfigAllowances> CalculateAllowances(int payGroupId, Dictionary<string, decimal> manualValuesDict)
        {
            DataTable dt = clsDatabase.fnDataTable("ConfigationWise_Allowances", payGroupId);

            List<PayConfigAllowances> calculatedConfigs = new List<PayConfigAllowances>();
            decimal totalAllowances = 0;

            foreach (DataRow row in dt.Rows)
            {
                string formula = row["CalculationFormula"].ToString();
                string configName = row["PayConfigName"].ToString();
                int configId = Convert.ToInt32(row["PayConfigId"]);
                decimal rate = Convert.ToDecimal(row["ManualRate (%)"]);

                //if (!IsFormulaSafe(formula))
                //    throw new Exception("Unsafe formula detected.");

                string processedFormula = formula;
                foreach (var kvp in manualValuesDict)
                {
                    processedFormula = processedFormula.Replace(kvp.Key, kvp.Value.ToString());
                }

                decimal evaluatedValue = 0;
                try
                {
                    var computeResult = new DataTable().Compute(processedFormula, "");
                    evaluatedValue = Convert.ToDecimal(computeResult);
                }
                catch
                {
                    evaluatedValue = 0;
                }

                decimal finalValue = Math.Round((evaluatedValue * rate / 100), 2);

                calculatedConfigs.Add(new PayConfigAllowances
                {
                    PayConfigId = configId,
                    PayConfigName = configName,
                    PayConfigType = "Allowances",
                    CalculationFormula = formula,
                    ManualRate = rate,
                    ConfigValue = finalValue
                });

                totalAllowances += finalValue;
            }

            // Add Total Allowances row
            calculatedConfigs.Add(new PayConfigAllowances
            {
                PayConfigId = 1001,
                PayConfigName = "Total Allowances",
                PayConfigType = "Summary",
                CalculationFormula = "",
                ManualRate = 0,
                ConfigValue = Math.Round(totalAllowances, 2)
            });

            // Optionally filter zero values (if needed)
            var filtered = calculatedConfigs
                .Where(c => c.ConfigValue != 0)
                .ToList();

            return filtered;
        }

        public List<PayConfigDeduction> CalculateDeductions(int payGroupId, Dictionary<string, bool> deductionFlags, Dictionary<string, decimal> manualValuesDict, List<outPutJson> allowanceValues, decimal basicAmount, decimal grossAmount)
        {

            DataTable dataTable = clsDatabase.fnDataTable("ConfigationWise_Deduction", payGroupId);
            List<PayConfigDeduction> deductionConfigs = new List<PayConfigDeduction>();
            decimal totalDeductions = 0;
            // Merge manual values + allowance values into one dictionary
            var formulaValues = new Dictionary<string, decimal>(manualValuesDict);

            foreach (var allowance in allowanceValues)
            {
                if (!formulaValues.ContainsKey(allowance.PayConfigName))
                    formulaValues.Add(allowance.PayConfigName, allowance.ConfigValue);
                else
                    formulaValues[allowance.PayConfigName] = allowance.ConfigValue; // overwrite
            }
            // Step 3: Loop through config rows
            foreach (DataRow row in dataTable.Rows)
            {
                string name = row["PayConfigName"].ToString();

                // Normalize for safe match
                string normalizedName = Normalize(name);

                bool shouldDeduct = deductionFlags.Any(flag =>
                        normalizedName.Contains(Normalize(flag.Key)) && flag.Value);


                if (!shouldDeduct)
                    continue;

                PayConfigDeduction config = new PayConfigDeduction
                {
                    PayConfigId = Convert.ToInt32(row["PayConfigId"]),
                    PayConfigName = name,
                    PayConfigType = row["PayConfigType"].ToString(),
                    CalculationFormula = row["CalculationFormula"]?.ToString(),
                    ManualRate = row["ManualRate (%)"] != DBNull.Value ? Convert.ToDecimal(row["ManualRate (%)"]) :
                                  (row["Rate"] != DBNull.Value ? Convert.ToDecimal(row["Rate"]) : 0),
                    LowerLimit = Convert.ToDecimal(row["LowerLimit"]),
                    UpperLimit = Convert.ToDecimal(row["UpperLimit"]),
                    Rate = Convert.ToDecimal(row["Rate"]),
                    Type = row["Type"].ToString().Trim(),
                    CalculatedOn = row["CalculatedOn"].ToString(),
                    RoundoffType = row["RoundoffType"].ToString().Trim()
                };

                //if (!IsFormulaSafe(config.CalculationFormula)) throw new Exception("Unsafe formula detected.");

                decimal baseValue = 0;

                if (!string.IsNullOrWhiteSpace(config.CalculationFormula))
                {
                    //baseValue = EvaluateFormula(config.CalculationFormula, basicAmount, grossAmount, manualValuesDict);
                    baseValue = EvaluateFormula(config.CalculationFormula, basicAmount, grossAmount, formulaValues);

                }
                else
                {
                    baseValue = config.CalculatedOn == "BASIC" ? basicAmount : grossAmount;
                }
                decimal value = 0;

                if (config.Type == "PERCENT")
                {
                    if (normalizedName == "ESIC" && baseValue > config.UpperLimit)
                    {
                        value = 0;
                    }
                    else
                    {
                        decimal applicableAmount = baseValue;
                        if (config.UpperLimit > 0 && applicableAmount > config.UpperLimit)
                            applicableAmount = config.UpperLimit;

                        value = (applicableAmount * config.Rate) / 100;
                    }
                }
                else if (config.Type == "SLAB")
                {
                    if (baseValue >= config.LowerLimit && baseValue <= config.UpperLimit)
                    {
                        value = config.Rate;
                    }
                }

                // Apply rounding
                switch (config.RoundoffType)
                {
                    case "ROUND_NEAREST":
                        config.ConfigValues = Math.Round(value, 0);
                        break;
                    case "CEILING":
                        config.ConfigValues = Math.Ceiling(value);
                        break;
                    default:
                        config.ConfigValues = value;
                        break;
                }

                deductionConfigs.Add(config);
                totalDeductions += config.ConfigValues;
            }

            // Step 4: Add total deduction row
            deductionConfigs.Add(new PayConfigDeduction         //1003
            {
                PayConfigId = 1003,
                PayConfigName = "Total Deductions",
                PayConfigType = "Summary",
                CalculationFormula = "",
                ManualRate = 0,
                ConfigValues = Math.Round(totalDeductions, 2)
            });

            // Step 5: Prepare simplified return list
            var deductionBreakup = deductionConfigs
                .Where(d => d.ConfigValues != 0)
                .Select(d => new
                {
                    d.PayConfigId,
                    d.PayConfigName,
                    Values = Math.Round(d.ConfigValues, 2)
                });

            return deductionConfigs;
        }

        public List<CompanyContributionConfig> GetCompanyContributionConfigs(int payGroupId, decimal basic, decimal gross, Dictionary<string, decimal> ContributionDict, List<outPutJson> allowanceValues, Dictionary<string, decimal> manualValuesDict)
        {
            DataTable dtable = clsDatabase.fnDataTable("ConfigationWise_CompanyContribution", payGroupId);


            // Allow editing of ConfigValues column if needed
            if (dtable.Columns.Contains("ConfigValues") && dtable.Columns["ConfigValues"].ReadOnly)
            {
                dtable.Columns["ConfigValues"].ReadOnly = false;
            }
            // Merge manual values + allowance values into one dictionary
            var formulaValues = new Dictionary<string, decimal>(manualValuesDict);

            foreach (var allowance in allowanceValues)
            {
                if (!formulaValues.ContainsKey(allowance.PayConfigName))
                    formulaValues.Add(allowance.PayConfigName, allowance.ConfigValue);
                else
                    formulaValues[allowance.PayConfigName] = allowance.ConfigValue; // overwrite
            }
            // Update config values from ContributionDict
            foreach (DataRow row in dtable.Rows)
            {
                string payConfigName = row["PayConfigName"]?.ToString()?.Trim() ?? "";

                if (ContributionDict.TryGetValue(payConfigName, out decimal manualVal))
                {
                    row["ConfigValues"] = manualVal;
                }
            }

            List<CompanyContributionConfig> list = new List<CompanyContributionConfig>();


            foreach (DataRow row in dtable.Rows)
            {
                string payConfigName = row["PayConfigName"]?.ToString()?.Trim() ?? "";
                int breakupID = row["BreakupID"] != DBNull.Value ? Convert.ToInt32(row["BreakupID"]) : 0;
                string breakupKey = row["BreakupKey"]?.ToString()?.Trim() ?? "";
                string breakupName = row["BreakupName"]?.ToString()?.Trim() ?? "";
                string type = row["Type"]?.ToString()?.Trim() ?? "";
                string calculatedOn = row["CalculatedOn"]?.ToString()?.Trim() ?? "";
                string formula = row["CalculationFormula"]?.ToString()?.Trim();
                decimal rate = row["BreakupRate"] != DBNull.Value ? Convert.ToDecimal(row["BreakupRate"]) : (row["Rate"] != DBNull.Value ? Convert.ToDecimal(row["Rate"]) : 0);
                decimal LowerLimit = Convert.ToDecimal(row["LowerLimit"]);
                decimal UpperLimit = Convert.ToDecimal(row["UpperLimit"]);
                decimal Values = row["ConfigValues"] != DBNull.Value ? Convert.ToDecimal(row["ConfigValues"]) : 0;
                string RoundoffType = row["RoundoffType"]?.ToString()?.Trim();

                decimal baseAmount = 0;


                if (!string.IsNullOrWhiteSpace(calculatedOn))
                {
                    //baseAmount = EvaluateFormula(calculatedOn, basic, gross, manualValuesDict);
                    baseAmount = EvaluateFormula(calculatedOn, basic, gross, formulaValues);
                }
                else
                {
                    baseAmount = basic;
                }

                decimal configValue = 0;

                string normalizedName = Normalize(payConfigName);


                if (type.Equals("PERCENT", StringComparison.OrdinalIgnoreCase))
                {
                    if (normalizedName.IndexOf("ESIC", StringComparison.OrdinalIgnoreCase) >= 0 && baseAmount > UpperLimit)
                    {
                        configValue = 0;
                    }
                    else
                    {
                        decimal applicableAmount = baseAmount;
                        if (UpperLimit > 0 && applicableAmount > UpperLimit)
                            applicableAmount = UpperLimit;

                        configValue = (applicableAmount * rate) / 100;
                    }

                }
                else if (type.Equals("FORMULA", StringComparison.OrdinalIgnoreCase))
                {

                    try
                    {

                        formula = formula.Replace("BASIC", baseAmount.ToString())
                                .Replace("GROSS", gross.ToString())
                                .Replace("GROSS AMOUNT", gross.ToString())
                                .Replace("Rate", rate.ToString());

                        foreach (var kvp in manualValuesDict)
                        {
                            formula = formula.Replace(kvp.Key, kvp.Value.ToString());
                        }

                        var result = new System.Data.DataTable().Compute(formula, "");
                        configValue = Convert.ToDecimal(result);
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors in formula evaluation
                        Console.WriteLine($"Error evaluating formula: {ex.Message}");
                        configValue = 0; // Default fallback in case of error
                    }

                }
                else if (type.Equals("MANUAL", StringComparison.OrdinalIgnoreCase))
                {
                    configValue = Values;
                }
                else
                {
                    configValue = 0;
                }
                // Apply rounding
                switch (RoundoffType)
                {
                    case "ROUND_NEAREST":
                        configValue = Math.Round(configValue, 0);
                        break;
                    case "CEILING":
                        configValue = RoundEmployerESICContribution(configValue);
                        break;
                    default:
                        configValue = configValue;
                        break;
                }
                list.Add(new CompanyContributionConfig
                {
                    PayConfigId = Convert.ToInt32(row["PayConfigId"]),
                    PayConfigName = payConfigName,
                    BreakupID = breakupID,
                    BreakupKey = breakupKey,
                    BreakupName = breakupName,
                    BreakupRate = rate,
                    Type = type,
                    CalculatedOn = calculatedOn,
                    Values = Math.Round(configValue, 2)
                });
            }

            return list;
        }
        //private decimal EvaluateFormula(string formula, decimal basic, decimal gross, Dictionary<string, decimal> extraValues)
        //{
        //    try
        //    {
        //        // 1. Replace BASIC and GROSS using actual variables
        //        formula = formula.Replace("BASIC", basic.ToString());

        //        // 2. Replace entries from extraValues, giving priority
        //        foreach (var kvp in extraValues)
        //        {
        //            formula = formula.Replace(kvp.Key, kvp.Value.ToString());
        //        }

        //        // 3. After extraValues, fallback replacement if still present
        //        formula = formula.Replace("Gross Amount", gross.ToString())
        //                         .Replace("GROSS", gross.ToString())
        //                         .Replace("Stipend", gross.ToString()); // fallback if "Stipend" not provided in extraValues

        //        // 4. Evaluate math
        //        var result = new System.Data.DataTable().Compute(formula, "");
        //        return Convert.ToDecimal(result);
        //    }
        //    catch
        //    {
        //        throw new Exception("Invalid formula: " + formula);
        //    }
        //}
        private decimal EvaluateFormula(string formula, decimal basic, decimal gross, Dictionary<string, decimal> extraValues)
        {
            try
            {
                string parsed = formula;

                // 1. Replace dictionary values (dynamic keys like BAAAASIC, Conveyence Allowance)
                foreach (var kvp in extraValues)
                {
                    // Escape spaces and special chars → replace with safe token
                    string safeKey = kvp.Key.Replace(" ", "_");

                    // Replace key in formula (using Regex for whole word match, case-insensitive)
                    parsed = System.Text.RegularExpressions.Regex.Replace(
                        parsed,
                        $@"\b{System.Text.RegularExpressions.Regex.Escape(kvp.Key)}\b",
                        kvp.Value.ToString(),
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );

                    // Also support if already normalized in formula (spaces removed)
                    parsed = parsed.Replace(safeKey, kvp.Value.ToString());
                }

                // 2. Replace fallback keywords
                parsed = parsed.Replace("Gross Amount", gross.ToString())
                               .Replace("GROSS", gross.ToString())
                               .Replace("Stipend", gross.ToString());

                // 3. Replace BASIC only if explicitly needed
                parsed = parsed.Replace("BASIC", basic.ToString());

                // 4. Handle MIN / MAX manually (since DataTable.Compute does not support them)
                if (parsed.Contains("MIN("))
                {
                    var inner = parsed.Substring(parsed.IndexOf("MIN(") + 4);
                    inner = inner.Substring(0, inner.IndexOf(")"));
                    var parts = inner.Split(',');
                    var v1 = Convert.ToDecimal(new System.Data.DataTable().Compute(parts[0], ""));
                    var v2 = Convert.ToDecimal(new System.Data.DataTable().Compute(parts[1], ""));
                    return Math.Min(v1, v2);
                }
                if (parsed.Contains("MAX("))
                {
                    var inner = parsed.Substring(parsed.IndexOf("MAX(") + 4);
                    inner = inner.Substring(0, inner.IndexOf(")"));
                    var parts = inner.Split(',');
                    var v1 = Convert.ToDecimal(new System.Data.DataTable().Compute(parts[0], ""));
                    var v2 = Convert.ToDecimal(new System.Data.DataTable().Compute(parts[1], ""));
                    return Math.Max(v1, v2);
                }

                // 5. Final evaluate with DataTable.Compute
                var result = new System.Data.DataTable().Compute(parsed, "");
                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid formula: " + formula + " | " + ex.Message);
            }
        }

        private bool GetBoolValue(object value)
        {
            if (value == DBNull.Value || value == null) return false;
            if (value is bool) return (bool)value;
            if (value is int) return (int)value == 1;
            if (value is string) return value.ToString() == "1" || value.ToString().ToLower() == "true";
            return false;
        }

        private int GetIntBoolValue(object value)
        {
            return GetBoolValue(value) ? 1 : 0;
        }

        public JObject CalculateArrer(JObject requestJson, string oldSalaryJson, string newSalaryJson, int oldPayGroupId, int newPayGroupId)
        {
            JObject root = JsonConvert.DeserializeObject<JObject>(oldSalaryJson);
            JObject Newroot = JsonConvert.DeserializeObject<JObject>(newSalaryJson);

            var oldSalary = root["OldSalaryDetails"]?.ToObject<List<JObject>>();
            var newSalary = Newroot["NewSalaryDetails"]?.ToObject<List<JObject>>();
            UpdatePayConfigNamesToUpper(ref oldSalary);
            UpdatePayConfigNamesToUpper(ref newSalary);
            int empId = Convert.ToInt32(requestJson["EmpId"]);
            DateTime dateFrom = Convert.ToDateTime(requestJson["DateFrom"]);
            DateTime dateTo = Convert.ToDateTime(requestJson["DateTo"]);
            DateTime arrearMonth = Convert.ToDateTime(requestJson["ArrearMonth"]);
            int totalMonths = ((dateTo.Year - dateFrom.Year) * 12) + dateTo.Month - dateFrom.Month + 1;

            decimal GetValue(List<JObject> list, string configName) =>
                list.FirstOrDefault(x => x["PayConfigName"]?.ToString().ToUpperInvariant() == configName.ToUpperInvariant())?["ConfigValue"]?.Value<decimal>() ?? 0;

            // Old Values
            decimal oldTotalAllowance = GetValue(oldSalary, "TOTAL ALLOWANCES");
            decimal oldGross = GetValue(oldSalary, "GROSS AMOUNT");
            decimal oldTotalDeduction = GetValue(oldSalary, "TOTAL DEDUCTIONS");
            decimal oldNet = GetValue(oldSalary, "NET AMOUNT");
            decimal oldContribution = GetValue(oldSalary, "TOTAL CONTRIBUTION AMOUNT");
            decimal oldMonthlyCTC = GetValue(oldSalary, "MONTHLY CTC AMOUNT");
            decimal oldYearlyCTC = GetValue(oldSalary, "YEARLY CTC AMOUNT");
            decimal oldPF = GetPFValue(oldSalary);
            // New Values
            decimal newTotalAllowance = GetValue(newSalary, "TOTAL ALLOWANCES");
            decimal newGross = GetValue(newSalary, "GROSS AMOUNT");
            decimal newTotalDeduction = GetValue(newSalary, "TOTAL DEDUCTIONS");
            decimal newNet = GetValue(newSalary, "NET AMOUNT");
            decimal newContribution = GetValue(newSalary, "Total Contribution Amount");
            decimal newMonthlyCTC = GetValue(newSalary, "TOTAL CONTRIBUTION AMOUNT");
            decimal newYearlyCTC = GetValue(newSalary, "YEARLY CTC AMOUNT");
            decimal newPF = GetPFValue(newSalary);

            decimal grossDiff = Math.Round(newGross - oldGross);
            decimal netDiff = Math.Round((newGross - oldGross) - (newPF - oldPF)); //newNet - oldNet;
            decimal ctcDiff = Math.Round((newGross + newContribution) - (oldGross + oldContribution));

            var changedComponents = new List<JObject>();
            var monthlyBreakups = new List<JObject>();
            var processedKeys = new HashSet<string>();

            foreach (var newItem in newSalary)
            {
                string type = newItem["PayConfigType"]?.ToString();
                if (type != null && type.Equals("Summary", StringComparison.OrdinalIgnoreCase))
                    continue;

                string name = newItem["PayConfigName"]?.ToString();
                string nameKey = name?.ToUpperInvariant();
                string typeKey = type?.ToUpperInvariant();

                var oldItem = oldSalary.FirstOrDefault(x =>
                    x["PayConfigName"]?.ToString()?.ToUpperInvariant() == nameKey &&
                    x["PayConfigType"]?.ToString()?.ToUpperInvariant() == typeKey);

                decimal newVal = Convert.ToDecimal(newItem["ConfigValue"]);
                decimal oldVal = oldItem != null ? Convert.ToDecimal(oldItem["ConfigValue"]) : 0;
                decimal monthlyDiff = newVal - oldVal;

                if (monthlyDiff != 0)
                {
                    changedComponents.Add(new JObject
                    {
                        ["PayConfigID"] = newItem["PayConfigId"],
                        ["PayConfigName"] = name,
                        ["PayType"] = type,
                        ["OldAmount"] = oldVal,
                        ["NewAmount"] = newVal,
                        ["MonthlyDifference"] = monthlyDiff,
                        ["TotalDifferenceForPeriod"] = monthlyDiff * totalMonths
                    });

                    for (int i = 0; i < totalMonths; i++)
                    {
                        string month = dateFrom.AddMonths(i).ToString("yyyy-MM-dd");
                        monthlyBreakups.Add(new JObject
                        {
                            ["PayConfigID"] = newItem["PayConfigId"],
                            ["MonthOfArrear"] = month,
                            ["MonthlyDifference"] = monthlyDiff
                        });
                    }
                }

                processedKeys.Add($"{nameKey}|{typeKey}");
            }

            foreach (var oldItem in oldSalary)
            {
                string type = oldItem["PayConfigType"]?.ToString();
                if (type != null && type.Equals("Summary", StringComparison.OrdinalIgnoreCase))
                    continue;

                string name = oldItem["PayConfigName"]?.ToString();
                string nameKey = name?.ToUpperInvariant();
                string typeKey = type?.ToUpperInvariant();

                string lookupKey = $"{nameKey}|{typeKey}";
                if (processedKeys.Contains(lookupKey))
                    continue;

                decimal oldVal = Convert.ToDecimal(oldItem["ConfigValue"]);
                decimal monthlyDiff = -oldVal;

                changedComponents.Add(new JObject
                {
                    ["PayConfigID"] = oldItem["PayConfigId"],
                    ["PayConfigName"] = name,
                    ["PayType"] = type,
                    ["OldAmount"] = oldVal,
                    ["NewAmount"] = 0,
                    ["MonthlyDifference"] = monthlyDiff,
                    ["TotalDifferenceForPeriod"] = monthlyDiff * totalMonths
                });

                for (int i = 0; i < totalMonths; i++)
                {
                    string month = dateFrom.AddMonths(i).ToString("yyyy-MM-dd");
                    monthlyBreakups.Add(new JObject
                    {
                        ["PayConfigID"] = oldItem["PayConfigId"],
                        ["MonthOfArrear"] = month,
                        ["MonthlyDifference"] = monthlyDiff
                    });
                }
            }

            var masterList = new List<PayrollArrearMaster>
            {
                new PayrollArrearMaster
                {
                    ArrearType = "INCREMENT",
                    EmpID = empId,
                    OldPayGroupId = oldPayGroupId,
                    NewPayGroupId = newPayGroupId,
                    ArrearFrom = dateFrom.ToString("yyyy-MM-dd"),
                    ArrearTo = dateTo.ToString("yyyy-MM-dd"),
                    ArrearMonth = arrearMonth.ToString("yyyy-MM-dd"),
                    TotalMonths = totalMonths,
                    GrossDifference = grossDiff,
                    NetDifference = netDiff,
                    CTCDifference = ctcDiff,
                    TotalGrossArrear = grossDiff * totalMonths,
                    TotalNetArrear = netDiff * totalMonths,
                    TotalCTCArrear = ctcDiff * totalMonths,
                    Remarks = $"Revised Salary from Arrear Period {dateFrom.ToString("MMMM, yyyy")} To {dateTo.ToString("MMMM, yyyy")}",
                    CreatedBy = System.Web.HttpContext.Current?.Session?["UserName"]?.ToString() ?? "System"
                }
            };

            return new JObject
            {
                ["Payroll_ArrearMaster"] = JArray.FromObject(masterList),
                ["Payroll_ArrearDetails"] = JArray.FromObject(changedComponents),
                ["Payroll_ArrearMonthlyBreakup"] = JArray.FromObject(monthlyBreakups)
            };
        }

        public void UpdatePayConfigNamesToUpper(ref List<JObject> salaryList)
        {
            if (salaryList == null) return;

            foreach (var item in salaryList)
            {
                var payConfigName = item["PayConfigName"]?.ToString();
                if (!string.IsNullOrEmpty(payConfigName))
                {
                    item["PayConfigName"] = payConfigName.ToUpper();
                }
            }
        }
        public decimal GetPFValue(List<JObject> list)
        {
            return list
                .FirstOrDefault(x =>
                    x["PayConfigName"]?.ToString().ToUpperInvariant().Contains("PF") == true &&
                    x["PayConfigType"]?.ToString().ToUpperInvariant() == "DEDUCTION")
                ?["ConfigValue"]?.Value<decimal>() ?? 0;
        }

        [HttpPost]
        public async Task<JsonResult> SaveEmployeeProposedSalary()
        {
            try
            {
                Request.InputStream.Position = 0;

                using (var reader = new StreamReader(Request.InputStream))
                {
                    string body = reader.ReadToEnd();

                    // Deserialize JSON manually from raw body
                    JObject json = JsonConvert.DeserializeObject<JObject>(body);

                    // 🔐 Decrypt EmpId
                    string encryptedEmpId = json["EmpId"]?.ToString();
                    if (string.IsNullOrWhiteSpace(encryptedEmpId))
                        return Json(new { success = false, message = "EmpId is required." });

                    int EmpId = Convert.ToInt32(DataEncryption.Decrypt(encryptedEmpId, "passKey"));

                    // Validate PayGroupId
                    if (!int.TryParse(json["PayGroupId"]?.ToString(), out int PayGroupId) || PayGroupId <= 0)
                        return Json(new { success = false, message = "Invalid or missing Pay Group." });

                    // Validate FinancialYear
                    string FinancialYear = json["FinancialYear"]?.ToString();
                    if (string.IsNullOrWhiteSpace(FinancialYear))
                        return Json(new { success = false, message = "Financial Year is required." });

                    bool IsPTaxActive = (bool)(json["DeductPTAX"] ?? false);
                    bool IsPFActive = (bool)(json["DeductPF"] ?? false);
                    bool IsEsicActive = (bool)(json["DeductESIC"] ?? false);

                    decimal ContinuousAttendanceAllowance = decimal.TryParse((string)json["ContinuousAllowance"], out var ca) ? ca : 0;
                    decimal ProductionBonus = decimal.TryParse((string)json["ProductionBonus"], out var pb) ? pb : 0;

                    string EntryUser = Session["UserName"]?.ToString() ?? "System";

                    // Perform save operation
                    string result = clsDatabase.fnDBOperation("Save_EmployeeSalaryConfig", EmpId, PayGroupId, IsPTaxActive, ContinuousAttendanceAllowance, ProductionBonus, EntryUser, FinancialYear, IsPFActive, IsEsicActive);

                    return Json(new { success = true, message = "Saved successfully.", result = result });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred while saving.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveEmployeeProposedSalaryValues()
        {
            try
            {
                Request.InputStream.Position = 0;

                using (var reader = new StreamReader(Request.InputStream))
                {
                    string body = reader.ReadToEnd();

                    //JArray jsonArray = JArray.Parse(body);

                    // ✅ Parse as JObject since the root is an object with properties
                    JObject jsonObject = JObject.Parse(body);

                    // 🔧 Optional: validate structure
                    var proposedSalaryData = jsonObject["ProposedSalaryData"]?.ToString();
                    var arrearData = jsonObject["ArrearData"]?.ToString();

                    string result = clsDatabase.fnDBOperation("Save_EmployeeProposedSalary", proposedSalaryData);

                    return Json(new { success = true, message = "Saved successfully.", result = result });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> SaveEmployeeManualSalaryNew()
        {
            try
            {
                Request.InputStream.Position = 0;

                using (var reader = new StreamReader(Request.InputStream))
                {
                    string body = reader.ReadToEnd();

                    //JArray jsonArray = JArray.Parse(body); 

                    string result = clsDatabase.fnDBOperation("SP_SaveEmployeeProposedSalarySetUp", body);

                    return Json(new { success = true, message = "Saved successfully.", result = result });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetEmployeeSalaryDetailsNew(string Empid)
        {
            try
            {

                int EmpId = Convert.ToInt32(DataEncryption.Decrypt(Empid, "passKey"));

                DataTable Dt = clsDatabase.fnDataTable("GetEmployeeSalaryDetailsNew", EmpId);

                var jsonList = Dt.AsEnumerable().Select(row => new
                {
                    PayConfigId = row["PayConfigId"] != DBNull.Value ? Convert.ToInt32(row["PayConfigId"]) : 0,
                    PayConfigName = row["PayConfigName"] != DBNull.Value ? row["PayConfigName"].ToString() : "",
                    Values = row["Values"] != DBNull.Value ? Convert.ToDecimal(row["Values"]) : 0,

                    IsPTaxActive = GetBoolValue(row["IsPTaxActive"]),
                    IsPFActive = GetIntBoolValue(row["IsPFActive"]),
                    IsEsicActive = GetIntBoolValue(row["IsEsicActive"]),
                    FinancialYear = row["FinancialYear"].ToString(),
                    ContinuousAllowance = row["ContinuousAllowance"] != DBNull.Value ? Convert.ToDecimal(row["ContinuousAllowance"]) : 0,
                    ProductionBonus = row["ProductionBonus"] != DBNull.Value ? Convert.ToDecimal(row["ProductionBonus"]) : 0
                }).ToList();


                return Json(new { success = true, message = "Fetched successfully", result = jsonList }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred.", error = ex.Message });
            }
        }



        #endregion

        #region TDS Calculation
        public ActionResult TDSDetails(string Empid)
        {
            Empid = DataEncryption.Decrypt(Convert.ToString(Empid), "passKey");
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            //int licenceid = Convert.ToInt32(Request.QueryString["Empid"]);
            int licenceid = Convert.ToInt32(Empid);
            return View(licenceid);
        }
        [HttpGet]
        public async Task<JsonResult> GetTDSMasterData(int empId)
        {
            DataSet ds = clsDatabase.fnDataSet("SP_TDS_CalculationMaster");

            if (ds != null && ds.Tables.Count >= 3)
            {
                var financialYears = ds.Tables[0].AsEnumerable().Select(row => new
                {
                    IDFinancialYear = row["IDFinancialYear"],
                    FinancialYearCode = row["FinancialYearCode"],
                    AssessmentYearCode = row["AssessmentYearCode"],
                    StartDate = Convert.ToDateTime(row["StartDate"]).ToString("yyyy-MM-dd"),
                    EndDate = Convert.ToDateTime(row["EndDate"]).ToString("yyyy-MM-dd"),
                    IsCurrent = row["IsCurrent"]
                });

                var taxRegimes = ds.Tables[1].AsEnumerable().Select(row => new
                {
                    RegimeId = row["RegimeId"],
                    RegimeName = row["RegimeName"]
                });

                var frequencies = ds.Tables[2].AsEnumerable().Select(row => new
                {
                    IDFrequency = row["IDFrequency"],
                    FrequencyCode = row["FrequencyCode"],
                    FrequencyName = row["FrequencyName"],
                    NoOfPeriods = row["NoOfPeriods"]
                });
                //for Previous Year Data
                var prev = clsDatabase.fnDataSet("Sp_GetPreviousTDSCalculation", empId, null);
                object previousFY = null;
                object previousRegimeId = null;
                object previousFrequencyId = null;
                if (prev.Tables[2].Rows.Count > 0)
                {
                    previousFY = prev.Tables[2].Rows[0]["PreviousFY"];
                    previousRegimeId = prev.Tables[2].Rows[0]["PreviousRegimeId"];
                    previousFrequencyId = prev.Tables[2].Rows[0]["PreviousFrequencyId"];
                }
                return Json(new
                {
                    success = true,
                    financialYears,
                    taxRegimes,
                    frequencies,
                    previousFY,
                    previousRegimeId,
                    previousFrequencyId
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Failed to fetch master data." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetTDSInputForm(string financialYear, int regimeId, int empId)
        {
            try
            {
                DataSet EmployeeDetails = clsDatabase.fnDataSet("Sp_GetEmployeeSalaryDetails", empId, financialYear, regimeId);

                if (EmployeeDetails == null || EmployeeDetails.Tables[0].Rows.Count == 0)
                {
                    return Json(new { success = false, message = "No data found for the employee." }, JsonRequestBehavior.AllowGet);
                }

                // Table[0] — Employee Basic Info
                var empInfo = EmployeeDetails.Tables[0].Rows[0];
                var employeeInfo = new
                {
                    EmpId = empInfo["empid"],
                    EmpCode = empInfo["empcode"],
                    EmpDOB = Convert.ToDateTime(empInfo["empdob"]).ToString("yyyy-MM-dd"),
                    AgeOnFYEnd = empInfo["AgeOnFYEnd"],
                    JoiningDate = Convert.ToDateTime(empInfo["joining"]).ToString("yyyy-MM-dd"),
                    YearsOfService = empInfo["YearsOfService"],
                    MonthsOfServiceInFY = empInfo["MonthsOfServiceInFY"],
                    FinancialStart = Convert.ToDateTime(empInfo["FinancialStart"]).ToString("yyyy-MM-dd"),
                    FinancialEnd = Convert.ToDateTime(empInfo["FinancialEnd"]).ToString("yyyy-MM-dd")
                };
                int monthsOfService = Convert.ToInt32(empInfo["MonthsOfServiceInFY"]);

                DateTime fyStart = Convert.ToDateTime(empInfo["FinancialStart"]);
                DateTime fyEnd = Convert.ToDateTime(empInfo["FinancialEnd"]);

                //// Table[1] — Income
                //var incomeList = EmployeeDetails.Tables[1].AsEnumerable().Select(row => new
                //{
                //    Section = "Salary [Sec 17(1)]",
                //    Details = row["PayConfigName"],
                //    Amount = row["PayValues"],
                //    AnnualSalary = Math.Round(Convert.ToDecimal(row["PayValues"]) * monthsOfService, 2)
                //}).ToList();

                // Step 1: Read Income Table (Table[1])
                var incomeRows = EmployeeDetails.Tables[1].AsEnumerable();

                // Extract all rows normally
                var incomeList = incomeRows.Select(row => new
                {
                    Section = "Salary [Sec 17(1)]",
                    Details = row["PayConfigName"],
                    Amount = row["PayValues"],
                    AnnualSalary = Math.Round(Convert.ToDecimal(row["PayValues"]) * monthsOfService, 2),
                    ActiveFlag = Convert.ToInt32(row["ActiveFlag"]),
                    PayConfigID = Convert.ToInt32(row["PayConfigID"])
                }).ToList();

                // Step 1A: Identify Gross Amount rows (old and new)
                var grossRows = incomeList
                    .Where(x => x.Details.ToString().ToLower() == "gross amount")
                    .ToList();

                decimal oldMonthlyGross = 0;
                decimal newMonthlyGross = 0;

                foreach (var row in grossRows)
                {
                    var salary = Convert.ToDecimal(row.Amount);
                    var active = Convert.ToInt32(row.ActiveFlag);

                    if (active == 1)
                        newMonthlyGross = salary;
                    else
                        oldMonthlyGross = salary;
                }
                // ✅ Table[6] — Arrear Information
                var arrearInfo = EmployeeDetails.Tables.Count > 6 && EmployeeDetails.Tables[6].Rows.Count > 0

                    ? EmployeeDetails.Tables[6].AsEnumerable().Select(row => new
                    {
                        ArrearFrom = Convert.ToDateTime(row["ArrearFrom"]).ToString("yyyy-MM-dd"),
                        ArrearTo = Convert.ToDateTime(row["ArrearTo"]).ToString("yyyy-MM-dd"),
                        ArrearMonth = Convert.ToDateTime(row["ArrearMonth"]).ToString("yyyy-MM-dd"),
                        TotalMonths = row["TotalMonths"],
                        GrossDifference = row["GrossDifference"],
                        NetDifference = row["NetDifference"],
                        CTCDifference = row["CTCDifference"],
                        TotalGrossArrear = row["TotalGrossArrear"],
                        TotalNetArrear = row["TotalNetArrear"],
                        TotalCTCArrear = row["TotalCTCArrear"]
                    }).FirstOrDefault()
                    : null;
                decimal arrearAmount = arrearInfo != null ? Convert.ToDecimal(arrearInfo.TotalGrossArrear) : 0;
                int arrearMonths = arrearInfo != null ? Convert.ToInt32(arrearInfo.TotalMonths) : 0;

                // Step 3: Compute how many months use old vs. new salary   

                // total months in the FY



                int oldSalaryMonths = 0;
                int newSalaryMonths = monthsOfService;
                decimal revisedAnnualGrossIncome = newMonthlyGross * monthsOfService;
                if (arrearInfo != null)
                {
                    // total months in the FY
                    int totalMonths = ((fyEnd.Year - fyStart.Year) * 12 + fyEnd.Month - fyStart.Month) + 1;

                    // when the new salary kicked in
                    DateTime salaryRevEffective = DateTime.Parse(arrearInfo.ArrearFrom);

                    // how many months from that revision through FY end
                    newSalaryMonths = ((fyEnd.Year - salaryRevEffective.Year) * 12
                                       + fyEnd.Month - salaryRevEffective.Month) + 1;
                    oldSalaryMonths = totalMonths - newSalaryMonths;

                    // recalc the revised annual gross, including arrear
                    revisedAnnualGrossIncome = (oldMonthlyGross * oldSalaryMonths)
                                            + (newMonthlyGross * newSalaryMonths);
                }
                // --- 1. Override incomeList’s AnnualSalary for the “Gross Amount” row ---
                incomeList = incomeList.Select(item => new
                {
                    item.Section,
                    item.Details,
                    item.Amount,
                    // if this is the Gross Amount line, show the revised annual, 
                    // otherwise keep the original per-row AnnualSalary
                    AnnualSalary = item.Details.ToString().Equals("Gross Amount", StringComparison.OrdinalIgnoreCase)
                        ? revisedAnnualGrossIncome
                        : item.AnnualSalary,
                    item.ActiveFlag,
                    item.PayConfigID
                })
                .Where(item => item.ActiveFlag == 1)
                .ToList();



                //// Table[2] — Deduction
                //var deductionList = EmployeeDetails.Tables[2].AsEnumerable().Select(row => new
                //{
                //    Section = row["SectionCode"],
                //    Details = row["DeductionName"],
                //    maxlimit = row["MaxLimit"],
                //    Amount = row["PayValues"],
                //    AnnualAmount = Math.Round(Convert.ToDecimal(row["PayValues"]) * monthsOfService, 2)
                //}).ToList();

                // 1) Read all deductions (Table[2]) with old vs new monthly values
                var deductionRows = EmployeeDetails.Tables[2].AsEnumerable()
                    .Select(r => new {
                        SectionCode = r["SectionCode"].ToString(),
                        Details = r["DeductionName"].ToString(),
                        MaxLimit = Convert.ToDecimal(r["MaxLimit"]),
                        MonthlyValue = Convert.ToDecimal(r["PayValues"]),
                        ActiveFlag = Convert.ToInt32(r["ActiveFlag"])
                    })
                    .ToList();

                // 2) Helper to prorate annual amount across old/new salary months
                decimal GetAnnualAmount(string section)
                {
                    var rows = deductionRows.Where(d => d.SectionCode == section).ToList();
                    if (rows.Count == 2)
                    {
                        var oldVal = rows.First(d => d.ActiveFlag == 0).MonthlyValue;
                        var newVal = rows.First(d => d.ActiveFlag == 1).MonthlyValue;
                        return (oldVal * oldSalaryMonths) + (newVal * newSalaryMonths);
                    }
                    // single‐rate deduction
                    return rows.First().MonthlyValue * monthsOfService;
                }

                // 3) Build your final deductionList
                var deductionList = deductionRows
                    .GroupBy(d => d.SectionCode)
                    .Select(g =>
                    {
                        var meta = g.First();
                        decimal annual = GetAnnualAmount(meta.SectionCode);
                        return new
                        {
                            Section = meta.SectionCode,
                            Details = meta.Details,
                            maxlimit = meta.MaxLimit,
                            Amount = meta.MonthlyValue,
                            AnnualAmount = Math.Round(annual, 2)
                        };
                    })
                    .ToList();
                // Table[3] — Exemption
                var exemptionList = EmployeeDetails.Tables[3].AsEnumerable()
                    .Select(row =>
                    {
                        var section = row["SectionCode"].ToString();
                        var details = row["DeductionName"].ToString();
                        var maxLimit = Convert.ToDecimal(row["MaxLimit"]);
                        var payValue = Convert.ToDecimal(row["PayValues"]);
                        var BasicPlusDA = Convert.ToDecimal(row["BasicPlusDA"]);

                        var annualAmount = Math.Round(payValue * monthsOfService, 2);
                        var annualBasicPlusDAAmount = Math.Round(BasicPlusDA * monthsOfService, 2);

                        // Apply rule: Children Education Allowance (Section 10(14)) max 2400
                        if (section == "10(14)" && annualAmount > maxLimit)
                        {
                            annualAmount = maxLimit;
                            payValue = maxLimit / monthsOfService;
                        }

                        return new
                        {
                            Section = section,
                            Details = details,
                            MaxLimit = maxLimit,
                            Amount = payValue,
                            BasicPlusDA = BasicPlusDA,
                            AnnualAmount = annualAmount,
                            annualBasicPlusDAAmount = annualBasicPlusDAAmount
                        };
                    }).ToList();


                var deductionSetUpList = EmployeeDetails.Tables[4].AsEnumerable().Select(row => new
                {
                    Section = row["SectionCode"],
                    Details = row["DeductionName"],
                    maxlimit = row["MaxLimit"],
                    Amount = row["PayValues"],
                    IsProofRequired = row["IsProofRequired"]
                }).ToList();
                // --- AFTER you calculate oldSalaryMonths & newSalaryMonths ---



                var standardDeduction = EmployeeDetails.Tables[5].AsEnumerable()
                .Select(row => new
                {
                    Section = row["SectionCode"],
                    Details = row["DeductionName"],
                    Amount = row["StandardDeduction"],
                    rebatelimit = row["RebateLimit"],
                    rebateamount = row["RebateAmount"]
                }).FirstOrDefault();



                return Json(new
                {
                    success = true,
                    message = "Fetched successfully",
                    employeeInfo,
                    incomeList,
                    deductionList,
                    deductionSetUpList,
                    exemptionList,
                    standardDeduction,
                    arrearInfo
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log error here as needed
                return Json(new { success = false, message = "Error fetching TDS setup.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        // Helper method to round income/tax according to Section 288A and 288B
        private decimal RoundToNearestTen(decimal amount)
        {
            // Ignore paise by truncating decimal part
            int rupees = (int)Math.Floor(amount);

            // Extract last digit
            int lastDigit = rupees % 10;

            if (lastDigit >= 5)
            {
                // Round up to next multiple of 10
                rupees = rupees + (10 - lastDigit);
            }
            else
            {
                // Round down to previous multiple of 10
                rupees = rupees - lastDigit;
            }

            return rupees;
        }


        [HttpGet]
        public async Task<JsonResult> CalculateTDS(string financialYear, int regimeId, decimal taxableIncome)
        {
            try
            {
                DataTable dt = clsDatabase.fnDataTable("Sp_TaxSlabDetails", regimeId, financialYear);

                if (dt == null || dt.Rows.Count == 0)
                    return Json(new { success = false, message = "No tax slabs found." }, JsonRequestBehavior.AllowGet);

                decimal totalTax = 0m;
                decimal rebateAmount = dt.Rows[0].Field<decimal>("RebateAmount");
                decimal rebateLimit = dt.Rows[0].Field<decimal>("RebateLimit");
                decimal cessRate = dt.Rows[0].Field<decimal>("CessRate");

                taxableIncome = RoundToNearestTen(taxableIncome);
                decimal remainingIncome = taxableIncome;

                List<object> tableRows = new List<object>
                {
                    new { Details = "Your Taxable Income", Amount = $"₹{taxableIncome:N2}" }
                };

                foreach (DataRow row in dt.Rows)
                {
                    decimal fromIncome = row.Field<decimal>("FromIncome");
                    decimal toIncome = row.Field<decimal>("ToIncome");
                    decimal taxRate = row.Field<decimal>("TaxRate");

                    if (taxableIncome > fromIncome)
                    {
                        decimal slabTaxable = Math.Min(remainingIncome, toIncome - fromIncome);
                        slabTaxable = Math.Max(0, Math.Round(slabTaxable, 2));
                        remainingIncome -= slabTaxable;


                        decimal slabTax = Math.Round(slabTaxable * taxRate, 0, MidpointRounding.AwayFromZero);
                        totalTax += slabTax;

                        tableRows.Add(new
                        {
                            Details = $"Tax on income from ₹{fromIncome:N0} – ₹{toIncome:N0}",
                            Amount = $"₹{slabTax:N2}"
                        });

                        if (remainingIncome <= 0)
                            break;
                    }
                }
                //totalTax= RoundToNearestTen(totalTax);
                // MARGINAL RELIEF CHECK
                decimal rebateDeduction = 0m;
                // Apply marginal relief only under New Tax Regime (regimeId == 2)
                if (regimeId == 2)
                {
                    if (taxableIncome <= rebateLimit)
                    {
                        rebateDeduction = rebateAmount;
                    }
                    else
                    {
                        decimal excessIncome = taxableIncome - rebateLimit;
                        if (totalTax > excessIncome)
                        {
                            rebateDeduction = totalTax - excessIncome;
                        }
                    }
                }
                else
                {
                    // Old Regime: flat rebate only if income ≤ limit (no marginal relief)
                    if (taxableIncome <= rebateLimit)
                    {
                        rebateDeduction = rebateAmount;
                    }
                    // else rebate remains 0
                }

                decimal taxAfterRebate = Math.Max(0, totalTax - rebateDeduction);
                decimal cessAmount = Math.Round(taxAfterRebate * cessRate, 0, MidpointRounding.AwayFromZero);
                decimal totalTaxPayable = taxAfterRebate + cessAmount;

                //totalTax= RoundToNearestTen(totalTax);
                //decimal rebateDeduction = taxableIncome <= rebateLimit ? rebateAmount : 0;
                //decimal taxAfterRebate = Math.Max(0, totalTax - rebateDeduction);
                //decimal cessAmount = Math.Floor(taxAfterRebate * cessRate);
                ////decimal totalTaxPayable = RoundToNearestTen(taxAfterRebate + cessAmount);
                //decimal totalTaxPayable = (taxAfterRebate + cessAmount);

                //// Add summary rows
                tableRows.Add(new { Details = "Total Income Tax (before Cess)", Amount = $"₹{totalTax:N2}" });
                tableRows.Add(new { Details = "Less: Rebate under Section 87A", Amount = rebateDeduction > 0 ? $"₹{rebateDeduction:N2}" : "Not applicable" });
                tableRows.Add(new { Details = $"Add: Health & Education Cess ({cessRate * 100:0.00}%)", Amount = $"₹{cessAmount:N2}" });
                tableRows.Add(new { Details = $"Total Tax Payable", Amount = $"₹{totalTaxPayable:N2}" });

                return Json(new
                {
                    success = true,
                    rows = tableRows,
                    totalTaxBeforeRebate = totalTax,
                    cessAmount = cessAmount,
                    totalTaxPayable = totalTaxPayable
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error calculating TDS.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //[HttpGet]
        //public async Task<JsonResult> GetTaxBreakUp(string financialYear,int regimeId,decimal totalTaxPayable,int taxfrequency)
        //{
        //    try
        //    {
        //        DataTable dt = clsDatabase.fnDataTable("Sp_GetTaxCalculationFrequency",taxfrequency);
        //        if (dt == null || dt.Rows.Count == 0)
        //            return Json(new { success = false, message = "No TDS Frequency found." }, JsonRequestBehavior.AllowGet);

        //        var row = dt.Rows[0];
        //        int noOfPeriods = Convert.ToInt32(row["NoOfPeriods"]);
        //        string frequency = row["Frequencycode"].ToString();
        //        return Json(new
        //        {
        //            success = true,
        //            noOfPeriods = noOfPeriods,
        //            frequency= frequency
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error fetching TDS Frequency.", error = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        [HttpGet]
        public JsonResult GetTaxBreakUp(string financialYear, int regimeId, decimal totalTaxPayable, int taxfrequency, int empId)
        {
            try
            {
                // 1. Load frequency config
                DataTable freqDt = clsDatabase.fnDataTable("Sp_GetTaxCalculationFrequency", taxfrequency);
                if (freqDt == null || freqDt.Rows.Count == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No TDS Frequency found."
                    }, JsonRequestBehavior.AllowGet);
                }

                int noOfPeriods = Convert.ToInt32(freqDt.Rows[0]["NoOfPeriods"]);
                string frequency = freqDt.Rows[0]["Frequencycode"].ToString();

                // 2. Fetch previous breakups
                var prevInfo = GetPreviousTDSCalculation(empId, financialYear);
                var allBreakups = prevInfo?.TDSBreakups ?? new List<TDSBreakupRow>();

                // If first‐time (no rows), generate a blank schedule of periods:
                if (allBreakups.Count == 0)
                {
                    // assume fiscal year always April 1–March 31
                    var fyStart = DateTime.ParseExact(
                        financialYear.Split('-')[0] + "-04-01",
                        "yyyy-MM-dd",
                        null
                    );
                    int incrementMonths = frequency == "QUARTERLY" ? 3
                                        : frequency == "HALFYEARLY" ? 6
                                        : frequency == "YEARLY" ? 12
                                        : 1;

                    DateTime periodStart = fyStart;
                    for (int i = 0; i < noOfPeriods; i++)
                    {
                        // safely add months, then back up one day for the period end:
                        DateTime periodEnd = periodStart.AddMonths(incrementMonths).AddDays(-1);

                        allBreakups.Add(new TDSBreakupRow
                        {
                            PeriodStart = periodStart,
                            PeriodEnd = periodEnd,
                            TDSAmount = 0m,
                            IsPaid = false
                        });

                        // next period starts the day after this one ends
                        periodStart = periodEnd.AddDays(1);
                    }
                }


                // 3. Split paid vs unpaid
                var paidBreakups = allBreakups.Where(b => b.IsPaid).ToList();
                var unpaidBreakups = allBreakups.Where(b => !b.IsPaid).ToList();

                int paidCount = paidBreakups.Count;
                int remainingCount = noOfPeriods - paidCount;
                decimal paidSum = paidBreakups.Sum(b => b.TDSAmount);
                decimal remainingTax = totalTaxPayable - paidSum;

                // 4. If nothing left, bail out
                if (remainingCount <= 0 || remainingTax <= 0)
                {
                    return Json(new
                    {
                        success = true,
                        noOfPeriods = 0,
                        frequency = frequency,
                        message = "TDS fully paid. No remaining periods."
                    }, JsonRequestBehavior.AllowGet);
                }

                // 5. Re-compute unpaid amounts
                decimal basePerPeriod = Math.Floor((remainingTax / remainingCount) / 10) * 10;
                decimal baseSum = basePerPeriod * (remainingCount - 1);
                decimal lastPeriodTax = Math.Round((remainingTax - baseSum) / 10) * 10;

                for (int i = 0; i < unpaidBreakups.Count; i++)
                {
                    unpaidBreakups[i].TDSAmount =
                        (i == unpaidBreakups.Count - 1)
                        ? lastPeriodTax
                        : basePerPeriod;
                }

                // 6. Project the full list for JSON
                var breakupList = allBreakups.Select(b => new
                {
                    PeriodStart = b.PeriodStart.ToString("yyyy-MM-dd"),
                    PeriodEnd = b.PeriodEnd.ToString("yyyy-MM-dd"),
                    TDSAmount = b.TDSAmount,
                    IsPaid = b.IsPaid
                }).ToList();

                // 7. Return everything your JS needs
                return Json(new
                {
                    success = true,
                    noOfPeriods = remainingCount,
                    frequency = frequency,
                    basePerPeriod = basePerPeriod,
                    lastPeriodTax = lastPeriodTax,
                    allBreakups = breakupList
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error fetching TDS Frequency or calculating breakup.",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<JsonResult> SaveTDSCalculation()
        {
            try
            {
                Request.InputStream.Position = 0;

                using (var reader = new StreamReader(Request.InputStream))
                {
                    string body = reader.ReadToEnd();

                    //JArray jsonArray = JArray.Parse(body); 

                    string result = clsDatabase.fnDBOperation("SP_SaveEmployeeTDS", body);

                    return Json(new { success = true, message = "Saved successfully.", result = result });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred.", error = ex.Message });
            }
        }

        private PreviousTDSInfo GetPreviousTDSCalculation(int empId, string financialYear)
        {
            // call your SP, which returns two result‐sets
            var ds = clsDatabase.fnDataSet("Sp_GetPreviousTDSCalculation", empId, financialYear);

            var info = new PreviousTDSInfo();

            // — Master row (Tables[0])
            if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var r = ds.Tables[0].Rows[0];
                info.PreviousGrossIncome = Convert.ToDecimal(r["AnnualGrossIncome"]);
                info.TotalExemptions = Convert.ToDecimal(r["TotalExemptions"]);
                info.TotalDeductions = Convert.ToDecimal(r["TotalDeductions"]);
                info.TaxableIncome = Convert.ToDecimal(r["TaxableIncome"]);
                info.AnnualTaxBeforeCess = Convert.ToDecimal(r["AnnualTaxBeforeCess"]);
                info.CessAmount = Convert.ToDecimal(r["CessAmount"]);
                info.TotalAnnualTax = Convert.ToDecimal(r["TotalAnnualTax"]);
                info.FrequencyCode = r["FrequencyCode"].ToString();
                info.NoOfPeriods = Convert.ToInt32(r["NoOfPeriods"]);
            }

            // — Breakup rows (Tables[1])
            if (ds.Tables.Count > 1)
            {
                foreach (DataRow r in ds.Tables[1].Rows)
                {
                    info.TDSBreakups.Add(new TDSBreakupRow
                    {
                        PeriodStart = Convert.ToDateTime(r["PeriodStart"]),
                        PeriodEnd = Convert.ToDateTime(r["PeriodEnd"]),
                        TDSAmount = Convert.ToDecimal(r["TDSAmount"]),
                        IsPaid = Convert.ToInt32(r["Paid"]) == 1
                    });
                }
            }

            return info;
        }

        #endregion

        #region Arrer Calculation
        [HttpGet]
        public ActionResult ArrearCalculation()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }


        [HttpPost]
        public JsonResult ParseExcel(HttpPostedFileBase file, string effectiveFrom, string finYear)
        {
            try
            {
                if (file == null || file.ContentLength == 0 || finYear == null)
                    return Json(new { success = false, message = "No file uploaded." });
                // Parse effectiveFrom
                if (!DateTime.TryParse(effectiveFrom, out DateTime dateFrom))
                    return Json(new { success = false, message = "Invalid Arrear Date From" });

                DateTime now = DateTime.Now;
                DateTime effectiveTo = new DateTime(now.Year, now.Month, 1).AddDays(-1); // Last day of previous month


                // Calculate month difference
                int monthDifference = ((effectiveTo.Year - dateFrom.Year) * 12) + (effectiveTo.Month - dateFrom.Month) + 1;

                // Define upload directory
                string uploadFolder = Server.MapPath("~/App_Data/Uploads");

                // ✅ Ensure the directory exists
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Save the file
                string filePath = Path.Combine(uploadFolder, Path.GetFileName(file.FileName));
                file.SaveAs(filePath);

                var employees = ParseExcel(filePath, monthDifference, dateFrom, effectiveTo, finYear); // 👈 your method

                var tempTable = new List<PreApprovalArrearSummaryModel>();

                DataTable dt = clsDatabase.fnDataTable("Sp_GetPreApprovalArrearSummary", finYear);
                foreach (DataRow row in dt.Rows)
                {
                    tempTable.Add(new PreApprovalArrearSummaryModel
                    {
                        FinancialYear = row["FinancialYear"]?.ToString(),
                        ArrearType = row["ArrearType"]?.ToString(),
                        EmpID = row["EmpID"] != DBNull.Value ? Convert.ToInt32(row["EmpID"]) : 0,
                        EmployeeName = row["Employee Name"]?.ToString(),

                        // OLD Salary
                        OldBasic = row["OldBasic"] != DBNull.Value ? Convert.ToDecimal(row["OldBasic"]) : (decimal?)null,
                        OldPF = row.Table.Columns.Contains("OldPF") && row["OldPF"] != DBNull.Value ? Convert.ToDecimal(row["OldPF"]) : (decimal?)null,
                        OldGross = row["OldGross"] != DBNull.Value ? Convert.ToDecimal(row["OldGross"]) : (decimal?)null,

                        // NEW Salary
                        NewBasic = row["NewBasic"] != DBNull.Value ? Convert.ToDecimal(row["NewBasic"]) : (decimal?)null,
                        NewPF = row.Table.Columns.Contains("NewPF") && row["NewPF"] != DBNull.Value ? Convert.ToDecimal(row["NewPF"]) : (decimal?)null,
                        NewGross = row["NewGross"] != DBNull.Value ? Convert.ToDecimal(row["NewGross"]) : (decimal?)null,

                        // Differences
                        BasicDifference = row.Table.Columns.Contains("BasicDifference") && row["BasicDifference"] != DBNull.Value ? Convert.ToDecimal(row["BasicDifference"]) : (decimal?)null,
                        PFDifference = row.Table.Columns.Contains("PFDifference") && row["PFDifference"] != DBNull.Value ? Convert.ToDecimal(row["PFDifference"]) : (decimal?)null,

                        // Arrear info
                        // Arrear info (safe date parsing)
                        ArrearFrom = row["ArrearFrom"] != DBNull.Value && DateTime.TryParse(row["ArrearFrom"].ToString(), out var af) ? af.ToString("dd-MM-yyyy") : string.Empty,
                        ArrearTo = row["ArrearTo"] != DBNull.Value && DateTime.TryParse(row["ArrearTo"].ToString(), out var at) ? at.ToString("dd-MM-yyyy") : string.Empty,
                        ArrearMonth = row["ArrearMonth"]?.ToString(), // Already formatted as "MMMM, yyyy" in SQL
                        TotalMonths = row["TotalMonths"] != DBNull.Value ? Convert.ToInt32(row["TotalMonths"]) : 0,

                        GrossDifference = row["GrossDifference"] != DBNull.Value ? Convert.ToDecimal(row["GrossDifference"]) : (decimal?)null,
                        NetDifference = row["NetDifference"] != DBNull.Value ? Convert.ToDecimal(row["NetDifference"]) : (decimal?)null,
                        TotalGrossArrear = row["TotalGrossArrear"] != DBNull.Value ? Convert.ToDecimal(row["TotalGrossArrear"]) : (decimal?)null,
                        TotalNetArrear = row["TotalNetArrear"] != DBNull.Value ? Convert.ToDecimal(row["TotalNetArrear"]) : (decimal?)null
                    });
                }

                return Json(new { success = true, data = tempTable });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<List<EmployeeSalaryData>> ParseExcel(string filePath, int monthDifference, DateTime dateFrom, DateTime dateTo, string finYear)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var employees = new List<EmployeeSalaryData>();
            List<JObject> finalJsonList = new List<JObject>();
            var excelData = new List<ExcelRowData>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                // Assuming the first row is headers and the data starts from the second row
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    string empNo = worksheet.Cells[row, 1].Text?.Trim(); // EmployeeNo
                    if (string.IsNullOrWhiteSpace(empNo)) continue;

                    string employeeName = worksheet.Cells[row, 2].Text?.Trim();
                    string incrementType = "";

                    // Try to parse both types of values
                    decimal? basicPercent = decimal.TryParse(worksheet.Cells[row, 3].Text, out var bp) ? bp : (decimal?)null;
                    decimal? grossPercent = decimal.TryParse(worksheet.Cells[row, 4].Text, out var gp) ? gp : (decimal?)null;
                    decimal? basicAmt = decimal.TryParse(worksheet.Cells[row, 5].Text, out var ba) ? ba : (decimal?)null;
                    decimal? grossAmt = decimal.TryParse(worksheet.Cells[row, 6].Text, out var ga) ? ga : (decimal?)null;

                    if ((basicPercent.HasValue || grossPercent.HasValue) && !basicAmt.HasValue && !grossAmt.HasValue)
                    {
                        incrementType = "PERCENTAGE";
                    }
                    else if ((basicAmt.HasValue || grossAmt.HasValue) && !basicPercent.HasValue && !grossPercent.HasValue)
                    {
                        incrementType = "FIXED";
                    }
                    else
                    {
                        // Mixed or invalid — skip or log as needed
                        continue;
                    }
                    // Add to shared list for reference later
                    excelData.Add(new ExcelRowData
                    {
                        EmployeeNo = Convert.ToInt32(empNo),
                        BasicPercent = basicPercent ?? 0,
                        GrossPercent = grossPercent ?? 0,
                        BasicAmt = basicAmt ?? 0,
                        GrossAmt = grossAmt ?? 0
                    });
                    // Lookup just for current employee
                    var empExcelRow = excelData.Last(); // This is current row

                    decimal basicValue = incrementType == "PERCENTAGE" ? basicPercent ?? 0 : basicAmt ?? 0;
                    decimal grossValue = incrementType == "PERCENTAGE" ? grossPercent ?? 0 : grossAmt ?? 0;

                    // Example dates and month calculation

                    // Call your function to generate final JSON
                    JObject finalJson = GetJsonFromDb(Convert.ToInt32(empNo), incrementType, basicValue, grossValue, dateFrom, dateTo, empExcelRow);
                    string encryptedEmpId = DataEncryption.Encrypt(empNo.ToString(), "passKey");
                    finalJson["EmpId"] = encryptedEmpId;
                    // ✅ 3. Generate Json For Arrear Calculation
                    // Call your arrear logic
                    JObject calculatedJson = await CalculateArrearFromJson(finalJson, finYear);
                    string jsonString = calculatedJson.ToString(Formatting.None);
                    string result = clsDatabase.fnDBOperation("Sp_Save_TempSalaryArrearFromJson", jsonString);
                }
            }

            return employees;
        }
        private JObject GetJsonFromDb(int empId, string incrementType, decimal BasicValue, decimal GrossValue, DateTime dateFrom, DateTime dateTo, ExcelRowData empExcelRow)
        {
            // Retrieve the salary details for the given employee ID from the database
            DataTable dt = clsDatabase.fnDataTable("GetEmployeeSalaryDetailsNew", empId);

            bool deductPF = dt.AsEnumerable().Any(row => row["IsPFActive"].ToString() == "1");
            bool deductESIC = dt.AsEnumerable().Any(row => row["IsEsicActive"].ToString() == "1");
            bool deductPTAX = dt.AsEnumerable().Any(row => row.Field<bool>("IsPTaxActive"));

            string payGroupId = dt.Rows.Count > 0 && dt.Rows[0]["PayGroupId"] != DBNull.Value
                ? dt.Rows[0]["PayGroupId"].ToString()
                : "0";

            string financialYear = dt.Rows.Count > 0
                ? dt.Rows[0]["FinancialYear"]?.ToString() ?? ""
                : "";

            // Assuming the PayIncrementConfig table is loaded into a list called incrementConfigs
            var incrementConfigs = LoadPayIncrementConfigs(); // Implement this function to load PayIncrementConfig data from DB

            var updatedManualValues = dt.AsEnumerable()
                .Where(row => row["Type"].ToString() == "Manual")
                .Select(row =>
                {
                    int payConfigId = Convert.ToInt32(row["PayConfigId"]);
                    string payConfigName = row["PayConfigName"]?.ToString() ?? "";
                    decimal oldVal = decimal.TryParse(row["Values"]?.ToString(), out var parsedVal) ? parsedVal : 0;

                    decimal newVal = oldVal;

                    var config = incrementConfigs.FirstOrDefault(x => x.PayConfigId == payConfigId && x.IncrementType == incrementType);


                    if (config != null && empExcelRow != null)
                    {
                        if (config.IsForBasic == true)
                        {
                            if (config.IncrementType == "PERCENTAGE")
                                newVal = Math.Round(oldVal + (oldVal * empExcelRow.BasicPercent / 100));
                            else if (config.IncrementType == "FIXED")
                                newVal = Math.Round(oldVal + empExcelRow.BasicAmt);
                        }
                        else if (config.IsForGross == true)
                        {
                            if (config.IncrementType == "PERCENTAGE")
                                newVal = Math.Round(oldVal + (oldVal * empExcelRow.GrossPercent / 100));
                            else if (config.IncrementType == "FIXED")
                                newVal = Math.Round(oldVal + empExcelRow.GrossAmt);
                        }
                    }

                    return new ManualValue
                    {
                        PayConfigID = payConfigId,
                        PayConfigName = payConfigName,
                        Values = newVal.ToString("0.00")
                    };
                }).ToList();

            var dbCompanyContributions = dt.AsEnumerable()
                .Where(row => row["Type"].ToString() == "CC" && row["PayGroupId"] != DBNull.Value)
                .Select(row => new ManualValue
                {
                    PayConfigID = row["PayConfigId"] != DBNull.Value ? Convert.ToInt32(row["PayConfigId"]) : 0,
                    PayConfigName = row["PayConfigName"]?.ToString() ?? "",
                    Values = row["Values"]?.ToString() ?? "0"
                }).ToList();

            JObject dbJson = new JObject
            {
                ["EmpId"] = empId.ToString(),
                ["PayGroupId"] = payGroupId,
                ["FinancialYear"] = financialYear,
                ["NewManualValues"] = JToken.FromObject(updatedManualValues),
                ["CompanyContributions"] = JToken.FromObject(dbCompanyContributions),
                ["DateFrom"] = dateFrom.ToString("yyyy-MM-dd"),
                ["DateTo"] = dateTo.ToString("yyyy-MM-dd"),
                ["ArrearMonth"] = DateTime.Now.ToString("yyyy-MM-dd"),
                ["DeductPF"] = deductPF,
                ["DeductESIC"] = deductESIC,
                ["DeductPTAX"] = deductPTAX,
                ["IsArrear"] = true
            };

            return dbJson;
        }

        private List<PayIncrementConfig> LoadPayIncrementConfigs()
        {
            // Query the database to fetch increment configurations
            DataTable dt = clsDatabase.fnDataTable("SP_Get_PayIncrementConfig");

            // Convert DataTable to List<PayIncrementConfig>
            var incrementConfigs = dt.AsEnumerable().Select(row => new PayIncrementConfig
            {
                PayConfigId = Convert.ToInt32(row["PayConfigId"]),
                IsForBasic = row["IsForBasic"] != DBNull.Value && Convert.ToBoolean(row["IsForBasic"]),
                IsForGross = row["IsForGross"] != DBNull.Value && Convert.ToBoolean(row["IsForGross"]),
                IncrementType = row["IncrementType"]?.ToString() ?? string.Empty
            }).ToList();

            return incrementConfigs;
        }


        private IEnumerable<outPutJson> CombineSalarySections(SalaryCalculationResult result)
        {
            return result.ManualValues
                .Concat(result.AllowanceValues)
                .Concat(result.GrossValues)
                .Concat(result.DeductionValues)
                .Concat(result.NetValues)
                .Concat(result.CompanyContributions)
                .Concat(result.CompanyContributionsBreakup)
                .Concat(result.TotalContributionValues)
                .Concat(result.MonthlyCTCValues)
                .Concat(result.YearlyCTCValues);
        }

        public async Task<JObject> CalculateArrearFromJson(JObject inputJson, string finYear)
        {
            try
            {
                string encryptedEmpId = inputJson["EmpId"]?.ToString();
                int empId = Convert.ToInt32(DataEncryption.Decrypt(encryptedEmpId, "passKey"));
                inputJson["EmpId"] = empId.ToString();

                bool isArrear = inputJson["IsArrear"]?.ToObject<bool>() ?? false;
                JObject dbJson = GetDbJsonFromDb(empId, inputJson);
                var oldManualValues = dbJson["OldManualValues"]?.ToObject<List<ManualValue>>() ?? new List<ManualValue>();
                int parsedPayGroupId = int.TryParse(dbJson["PayGroupId"]?.ToString(), out var pgid) ? pgid : 0;
                bool isFirstTimeSetup = parsedPayGroupId == 0 && !oldManualValues.Any();

                var oldResult = !isFirstTimeSetup
                    ? CalculateSalaryComponents(oldManualValues, dbJson, empId)
                    : new SalaryCalculationResult();

                var oldCombinedRaw = !isFirstTimeSetup ? CombineSalarySections(oldResult) : Enumerable.Empty<outPutJson>();
                var oldCombined = oldCombinedRaw.ToList();
                string oldProposedSalary = JsonConvert.SerializeObject(new { OldSalaryDetails = oldCombined });

                var newManualValues = inputJson["NewManualValues"]?.ToObject<List<ManualValue>>() ?? new List<ManualValue>();
                var newResult = CalculateSalaryComponents(newManualValues, inputJson, empId);
                var newCombined = CombineSalarySections(newResult).ToList();
                string newProposedSalary = JsonConvert.SerializeObject(new { NewSalaryDetails = newCombined });

                // Prepare unified JSON to pass to SP
                JObject finalJson = new JObject
                {
                    ["EmpID"] = empId,
                    ["FinancialYear"] = finYear,
                    ["UploadBatchId"] = Guid.NewGuid().ToString(),
                    ["CreatedBy"] = Session["UserName"]?.ToString() ?? "Unknown",
                    ["PayGroupId"] = inputJson["PayGroupId"]?.ToString(),
                    ["OldProposedSalary"] = new JArray(),
                    ["NewProposedSalary"] = new JArray(),
                    ["PFBreakup"] = new JArray(),
                    ["Payroll_ArrearMaster"] = new JArray(),
                    ["ManualValues"] = new JArray()
                };

                foreach (var item in oldCombined)
                {
                    JObject obj = JObject.FromObject(new
                    {
                        PayConfigID = item.PayConfigId,
                        PayConfigName = item.PayConfigName,
                        PayConfigType = item.PayConfigType,
                        PayValues = item.ConfigValue,
                        Type = "Old"
                    });

                    if (item.PayConfigType?.ToUpper() == "PF-BREAKUP")
                        ((JArray)finalJson["PFBreakup"]).Add(obj);
                    else
                        ((JArray)finalJson["OldProposedSalary"]).Add(obj);
                }

                foreach (var item in newCombined)
                {
                    JObject obj = JObject.FromObject(new
                    {
                        PayConfigID = item.PayConfigId,
                        PayConfigName = item.PayConfigName,
                        PayConfigType = item.PayConfigType,
                        PayValues = item.ConfigValue,
                        Type = "New"
                    });

                    if (item.PayConfigType?.ToUpper() == "PF-BREAKUP")
                        ((JArray)finalJson["PFBreakup"]).Add(obj);
                    else
                        ((JArray)finalJson["NewProposedSalary"]).Add(obj);
                }

                foreach (var manual in newManualValues)
                {
                    JObject m = new JObject
                    {
                        ["EmpID"] = empId,
                        ["FinancialYear"] = finYear,
                        ["PayConfigID"] = manual.PayConfigID,
                        ["Values"] = manual.Values
                    };
                    ((JArray)finalJson["ManualValues"]).Add(m);
                }

                if (isArrear)
                {
                    var arrearJson = CalculateArrer(inputJson, oldProposedSalary, newProposedSalary, oldResult.PayGroupId, newResult.PayGroupId);
                    finalJson["Payroll_ArrearMaster"] = JArray.FromObject(arrearJson["Payroll_ArrearMaster"] ?? new JArray());
                }


                dynamic data = JsonConvert.SerializeObject(finalJson);
                return finalJson;
            }
            catch (Exception ex)
            {
                return JObject.FromObject(new { success = false, error = ex.Message });
            }
        }

        //public async Task<JObject> CalculateArrearFromJson(JObject inputJson,string finYear)
        //{
        //    try
        //    {
        //        string encryptedEmpId = inputJson["EmpId"]?.ToString();
        //        int empId = Convert.ToInt32(DataEncryption.Decrypt(encryptedEmpId, "passKey"));
        //        inputJson["EmpId"] = empId.ToString();

        //        bool isArrear = inputJson["IsArrear"]?.ToObject<bool>() ?? false;
        //        JObject dbJson = GetDbJsonFromDb(empId, inputJson);
        //        var oldManualValues = dbJson["OldManualValues"]?.ToObject<List<ManualValue>>() ?? new List<ManualValue>();
        //        int parsedPayGroupId = int.TryParse(dbJson["PayGroupId"]?.ToString(), out var pgid) ? pgid : 0;
        //        bool isFirstTimeSetup = parsedPayGroupId == 0 && !oldManualValues.Any();

        //        var oldResult = !isFirstTimeSetup
        //            ? CalculateSalaryComponents(oldManualValues, dbJson, empId)
        //            : new SalaryCalculationResult();

        //        string oldProposedSalary = !isFirstTimeSetup
        //            ? JsonConvert.SerializeObject(new { OldSalaryDetails = CombineSalarySections(oldResult) })
        //            : "";

        //        var newManualValues = inputJson["NewManualValues"]?.ToObject<List<ManualValue>>() ?? new List<ManualValue>();
        //        var newResult = CalculateSalaryComponents(newManualValues, inputJson, empId);

        //        string newProposedSalary = JsonConvert.SerializeObject(new
        //        {
        //            NewSalaryDetails = CombineSalarySections(newResult)
        //        });

        //        if (isArrear)
        //        {
        //            var arrearJson = CalculateArrer(inputJson, oldProposedSalary, newProposedSalary, oldResult.PayGroupId, newResult.PayGroupId);

        //            inputJson["Payroll_ArrearMaster"] = JArray.FromObject(arrearJson["Payroll_ArrearMaster"] ?? new JArray());
        //            inputJson["Payroll_ArrearDetails"] = JArray.FromObject(arrearJson["Payroll_ArrearDetails"] ?? new JArray());
        //            inputJson["Payroll_ArrearMonthlyBreakup"] = JArray.FromObject(arrearJson["Payroll_ArrearMonthlyBreakup"] ?? new JArray());
        //        }
        //        dynamic data = JsonConvert.SerializeObject(inputJson);//<dynamic>(inputJson.ToString());
        //        return JObject.FromObject(new
        //        {
        //            success = true,
        //            EmpId = empId,
        //            FinancitalYear=finYear,
        //            OldProposedSalary = oldProposedSalary,
        //            NewProposedSalary = newProposedSalary,
        //            FullJson = inputJson
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return JObject.FromObject(new { success = false, error = ex.Message });
        //    }
        //}

        [HttpPost]
        public JsonResult SaveArrearData(List<PreApprovalArrearSummaryModel> arrearData)
        {
            try
            {
                // Get CreatedBy from session
                string CreatedBy = Session["UserName"]?.ToString() ?? "system";

                // Project only EmpID and FinancialYear
                var minimalData = arrearData
                    .Select(x => new
                    {
                        x.EmpID,
                        x.FinancialYear
                    })
                    .Distinct() // Optional: to avoid duplicates
                    .ToList();

                // Serialize to JSON
                var JsonData = JsonConvert.SerializeObject(minimalData);

                var result = clsDatabase.fnDBOperation("SP_Save_TempToFinalArrearSummary", JsonData, CreatedBy);

                return Json(new { success = true, message = "Saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ExportArrearSummaryToExcel(string finYear)
        {
            var dt = clsDatabase.fnDataTable("Sp_GetPreApprovalArrearSummary", finYear);
            List<PreApprovalArrearSummaryModel> tempTable = new List<PreApprovalArrearSummaryModel>();

            foreach (DataRow row in dt.Rows)
            {
                tempTable.Add(new PreApprovalArrearSummaryModel
                {
                    FinancialYear = row["FinancialYear"]?.ToString(),
                    ArrearType = row["ArrearType"]?.ToString(),
                    EmpID = row["EmpID"] != DBNull.Value ? Convert.ToInt32(row["EmpID"]) : 0,
                    EmployeeName = row["Employee Name"]?.ToString(),

                    OldBasic = row["OldBasic"] != DBNull.Value ? Convert.ToDecimal(row["OldBasic"]) : (decimal?)null,
                    OldPF = row.Table.Columns.Contains("OldPF") && row["OldPF"] != DBNull.Value ? Convert.ToDecimal(row["OldPF"]) : (decimal?)null,
                    OldGross = row["OldGross"] != DBNull.Value ? Convert.ToDecimal(row["OldGross"]) : (decimal?)null,

                    NewBasic = row["NewBasic"] != DBNull.Value ? Convert.ToDecimal(row["NewBasic"]) : (decimal?)null,
                    NewPF = row.Table.Columns.Contains("NewPF") && row["NewPF"] != DBNull.Value ? Convert.ToDecimal(row["NewPF"]) : (decimal?)null,
                    NewGross = row["NewGross"] != DBNull.Value ? Convert.ToDecimal(row["NewGross"]) : (decimal?)null,


                    ArrearFrom = row["ArrearFrom"] != DBNull.Value && DateTime.TryParse(row["ArrearFrom"].ToString(), out var af) ? af.ToString("dd-MM-yyyy") : string.Empty,
                    ArrearTo = row["ArrearTo"] != DBNull.Value && DateTime.TryParse(row["ArrearTo"].ToString(), out var at) ? at.ToString("dd-MM-yyyy") : string.Empty,
                    ArrearMonth = row["ArrearMonth"]?.ToString(),
                    TotalMonths = row["TotalMonths"] != DBNull.Value ? Convert.ToInt32(row["TotalMonths"]) : 0,

                    BasicDifference = row["BasicDifference"] != DBNull.Value ? Convert.ToDecimal(row["BasicDifference"]) : (decimal?)null,
                    PFDifference = row["PFDifference"] != DBNull.Value ? Convert.ToDecimal(row["PFDifference"]) : (decimal?)null,

                    GrossDifference = row["GrossDifference"] != DBNull.Value ? Convert.ToDecimal(row["GrossDifference"]) : (decimal?)null,
                    NetDifference = row["NetDifference"] != DBNull.Value ? Convert.ToDecimal(row["NetDifference"]) : (decimal?)null,
                    TotalGrossArrear = row["TotalGrossArrear"] != DBNull.Value ? Convert.ToDecimal(row["TotalGrossArrear"]) : (decimal?)null,
                    TotalNetArrear = row["TotalNetArrear"] != DBNull.Value ? Convert.ToDecimal(row["TotalNetArrear"]) : (decimal?)null
                });
            }

            // Create Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Arrear Summary");

                // Get properties (columns)
                var props = typeof(PreApprovalArrearSummaryModel).GetProperties();
                int col = 1, row = 1;

                // Write headers manually
                foreach (var prop in props)
                {
                    worksheet.Cell(row, col).Value = prop.Name;
                    col++;
                }

                // Write data rows
                row = 2;
                foreach (var item in tempTable)
                {
                    col = 1;
                    foreach (var prop in props)
                    {
                        var value = prop.GetValue(item);

                        if (value == null)
                        {
                            worksheet.Cell(row, col).Value = "";
                        }
                        else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                        {
                            worksheet.Cell(row, col).SetValue(Convert.ToDecimal(value));
                        }
                        else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                        {
                            worksheet.Cell(row, col).SetValue(Convert.ToInt32(value));
                        }
                        else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            worksheet.Cell(row, col).SetValue(Convert.ToDateTime(value).ToString("dd-MM-yyyy"));
                        }
                        else
                        {
                            worksheet.Cell(row, col).SetValue(value.ToString());
                        }

                        col++;
                    }
                    row++;
                }

                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "PreApproval_Arrear_Summary.xlsx");
                }
            }


        }

        #endregion
    }

}