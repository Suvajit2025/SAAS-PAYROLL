using Common.Utility;
using MendinePayroll.Models;
using MendinePayroll.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UI.BLL;

namespace MendinePayroll.UI.Controllers
{
    public class EmployeeCTCController : Controller
    {
        private string tenantID
        {
            get
            {
                if (Session["TenantID"] == null)
                    throw new Exception("Session expired. Please log in again.");

                return Session["TenantID"].ToString();
            }
        }
        // GET: EmployeeCTC
        public ActionResult Index()
        {
            clsAccessLogInfo info = new clsAccessLogInfo();
            info.AccessType = "EMPLOYEE-CTC-ENTRY";
            clsAccessLog.AccessLog_Save(info);
            return View();
        }

        [HttpPost]
        public JsonResult CalculateEmployeeCTC(EmployeeCTCCalculateRequest model)
        {
            try
            {
                var result = CalculateSalaryOnBackend(model);

                return Json(new
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        private EmployeeCTCCalculateResponse CalculateSalaryOnBackend(EmployeeCTCCalculateRequest model)
        {
            DataTable dt = clsDatabase.fnDataTable("PRC_GetSalaryConfigByPayGroup",model.PayGroupID,tenantID);

            var configList = MapConfig(dt);

            var calcs = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in configList)
                calcs[item.PayConfigName.Trim().ToUpper()] = 0;

            // 1. Input values
            foreach (var item in configList.Where(x => x.IsInput))
            {
                var input = model.Inputs.FirstOrDefault(x => x.PayConfigId == item.PayConfigId);
                decimal val = input != null ? input.MonthlyAmount : 0;
                calcs[item.PayConfigName.Trim().ToUpper()] = val;
            }

            // 2. Fixed values
            foreach (var item in configList.Where(x => !x.IsInput && x.LogicType == "FIXED"))
            {
                var input = model.Inputs.FirstOrDefault(x => x.PayConfigId == item.PayConfigId);
                decimal val = input != null ? input.MonthlyAmount : item.ManualRate;
                calcs[item.PayConfigName.Trim().ToUpper()] = val;
            }

            // 3. Gross and basic
            var grossItem = configList.FirstOrDefault(x => x.IsGrossComponent);
            var basicItem = configList.FirstOrDefault(x => x.IsBasicComponent);

            decimal grossVal = 0;
            if (grossItem != null)
                grossVal = calcs[grossItem.PayConfigName.Trim().ToUpper()];

            if (basicItem != null && basicItem.ISPercentage)
            {
                decimal basicVal = grossVal * basicItem.ManualRate / 100;
                calcs[basicItem.PayConfigName.Trim().ToUpper()] = basicVal;
            }

            // 4. Formula calculation
            for (int i = 0; i < 3; i++)
            {
                foreach (var item in configList.Where(x => x.LogicType == "FORMULA" && !x.IsStatutory))
                {
                    decimal baseVal = EvaluateFormula(item.CalculationFormula, calcs);
                    decimal val = item.ISPercentage ? (baseVal * item.ManualRate / 100) : baseVal;
                    calcs[item.PayConfigName.Trim().ToUpper()] = val;
                }
            }

            // 5. Statutory
            ApplyStatutoryCalculation(configList, calcs, model, grossVal);

            // 6. Build response
            return BuildCalculatedResponse(configList, calcs, model);
        }

        private List<SalaryComponentConfig> MapConfig(DataTable dt)
        {
            var list = new List<SalaryComponentConfig>();

            foreach (DataRow r in dt.Rows)
            {
                var item = new SalaryComponentConfig
                {
                    PayConfigId = Convert.ToInt64(r["PayConfigId"]),
                    PayConfigName = Convert.ToString(r["PayConfigName"]),
                    PayType = Convert.ToString(r["PayType"]),

                    LogicType = Convert.ToString(r["LogicType"]),
                    TimeBasis = Convert.ToString(r["TimeBasis"]),

                    ManualRate = r["ManualRate"] == DBNull.Value ? 0 : Convert.ToDecimal(r["ManualRate"]),
                    ISPercentage = r["ISPercentage"] != DBNull.Value && Convert.ToBoolean(r["ISPercentage"]),

                    IsBasicComponent = r["IsBasicComponent"] != DBNull.Value && Convert.ToBoolean(r["IsBasicComponent"]),
                    IsGrossComponent = r["IsGrossComponent"] != DBNull.Value && Convert.ToBoolean(r["IsGrossComponent"]),
                    IsLoanComponent = r["IsLoanComponent"] != DBNull.Value && Convert.ToBoolean(r["IsLoanComponent"]),
                    IsCalculative = r["IScalculative"] != DBNull.Value && Convert.ToBoolean(r["IScalculative"]),
                    IsVariableAmt = r["IsVariableAmt"] != DBNull.Value && Convert.ToBoolean(r["IsVariableAmt"]),

                    IsStatutory = r["IsStatutory"] != DBNull.Value && Convert.ToBoolean(r["IsStatutory"]),
                    StatutoryType = Convert.ToString(r["StatutoryType"]),

                    MaxLimit = r["MaxLimit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["MaxLimit"]),
                    RoundingType = Convert.ToString(r["RoundingType"]),

                    IsOther = r["IsOther"] != DBNull.Value && Convert.ToBoolean(r["IsOther"]),
                    OtherType = Convert.ToString(r["OtherType"]),

                    MappedColumn = Convert.ToString(r["MappedColumn"]),
                    CalculationFormula = Convert.ToString(r["CalculationFormula"])
                    //CalculationFormula = GetFormattedFormula(r["CalculationFormula"],r["ManualRate"],r["ISPercentage"],r["LogicType"])
                };

                item.IsInput = item.PayType == "Manual" || item.IsGrossComponent;
                item.IsManualGrid = item.LogicType == "FIXED" && item.PayType != "Manual";

                if (dt.Columns.Contains("PTAXSlabs") && r["PTAXSlabs"] != DBNull.Value)
                {
                    var json = Convert.ToString(r["PTAXSlabs"]);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        item.PTaxSlabs = JsonConvert.DeserializeObject<List<PTaxSlabModelCTC>>(json);
                    }
                }

                list.Add(item);
            }

            return list.Where(x => !x.IsVariableAmt).ToList();
        }
        public static string GetFormattedFormula(object calculationFormula,object manualRate,object isPercentage,object logicType)
        {
            string baseFormula = Convert.ToString(calculationFormula)?.Trim();

            decimal rate = manualRate != DBNull.Value && manualRate != null
                ? Convert.ToDecimal(manualRate)
                : 0;

            bool isPercent = isPercentage != DBNull.Value && isPercentage != null
                ? Convert.ToBoolean(isPercentage)
                : false;

            string logic = Convert.ToString(logicType);

            if (logic == "FORMULA")
            {
                string per = isPercent ? "%" : "";

                if (!string.IsNullOrEmpty(baseFormula))
                {
                    if (rate > 0)
                        return $"{rate}{per} of {baseFormula}";
                    else
                        return $"Based on {baseFormula}";
                }
                else
                {
                    return "Formula";
                }
            }

            return baseFormula;
        }
        private decimal EvaluateFormula(string formula, Dictionary<string, decimal> calcs)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return 0;

            string expr = formula.ToUpper().Trim();

            foreach (var key in calcs.Keys.OrderByDescending(x => x.Length))
            {
                var value = calcs[key];
                expr = System.Text.RegularExpressions.Regex.Replace(
                    expr,
                    $@"\b{System.Text.RegularExpressions.Regex.Escape(key)}\b",
                    value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                );
            }

            expr = System.Text.RegularExpressions.Regex.Replace(expr, @"[^0-9+\-*/().]", "");

            if (string.IsNullOrWhiteSpace(expr))
                return 0;

            try
            {
                var dt = new DataTable();
                var result = dt.Compute(expr, "");
                return Convert.ToDecimal(result);
            }
            catch
            {
                return 0;
            }
        }
        private decimal ApplyRounding(decimal value, string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return Math.Round(value, 0);

            type = type.Trim().ToUpper();

            if (type.StartsWith("ROUND"))
            {
                var digitsText = type.Replace("ROUND", "");
                if (int.TryParse(digitsText, out int digits))
                    return Math.Round(value, digits);
            }

            switch (type)
            {
                case "CEIL":
                    return Math.Ceiling(value);
                case "FLOOR":
                    return Math.Floor(value);
                case "ROUND":
                default:
                    return Math.Round(value, 0);
            }
        }

        private void ApplyStatutoryCalculation(List<SalaryComponentConfig> configList,Dictionary<string, decimal> calcs,EmployeeCTCCalculateRequest model,decimal grossVal)
        {
            foreach (var item in configList.Where(x => x.IsStatutory))
            {
                decimal baseVal = EvaluateFormula(item.CalculationFormula, calcs);
                if (baseVal == 0)
                    baseVal = grossVal;

                decimal val = 0;
                string statType = (item.StatutoryType ?? "").ToUpper();

                switch (statType)
                {
                    case "PF_EE":
                    case "PF_ER":
                        if (model.IsPF)
                        {
                            decimal cap = item.MaxLimit.HasValue && item.MaxLimit.Value > 0
                                ? Math.Min(baseVal, item.MaxLimit.Value)
                                : baseVal;

                            val = ApplyRounding((cap * item.ManualRate) / 100, item.RoundingType);
                        }
                        break;

                    case "ESIC_EE":
                    case "ESIC_ER":
                        if (model.IsESIC)
                        {
                            decimal esicCap = item.MaxLimit.HasValue && item.MaxLimit.Value > 0
                                ? item.MaxLimit.Value
                                : 21000;

                            if (baseVal <= esicCap)
                                val = ApplyRounding((baseVal * item.ManualRate) / 100, item.RoundingType);
                        }
                        break;

                    case "PTAX":
                        if (model.IsPTAX && grossVal > 0 && item.PTaxSlabs != null && item.PTaxSlabs.Count > 0)
                        {
                            var slab = item.PTaxSlabs.FirstOrDefault(x => grossVal >= x.MinSalary && grossVal <= x.MaxSalary);
                            val = slab != null ? slab.PTaxAmount : 0;
                        }
                        break;

                    case "GRATUITY":
                        decimal gratuityRate = item.ManualRate > 0 ? item.ManualRate : 4.81m;
                        val = ApplyRounding((baseVal * gratuityRate) / 100, item.RoundingType);
                        break;
                }

                calcs[item.PayConfigName.Trim().ToUpper()] = val;
            }
        }

        private EmployeeCTCCalculateResponse BuildCalculatedResponse(List<SalaryComponentConfig> configList,Dictionary<string, decimal> calcs,EmployeeCTCCalculateRequest model)
        {
            decimal totalAllowance = 0;
            decimal totalDeduction = 0;
            decimal totalContribution = 0;

            var basicItem = configList.FirstOrDefault(x => x.IsBasicComponent);
            decimal basicValue = basicItem != null
                ? calcs[basicItem.PayConfigName.Trim().ToUpper()]
                : 0;

            foreach (var item in configList)
            {
                decimal val = calcs[item.PayConfigName.Trim().ToUpper()];

                if (item.IsBasicComponent)
                    continue;

                switch (item.PayType)
                {
                    case "Allowances":
                        totalAllowance += val;
                        break;
                    case "Deduction":
                        totalDeduction += val;
                        break;
                    case "CC":
                        totalContribution += val;
                        break;
                }
            }

            decimal totalGross = basicValue + totalAllowance;
            decimal netPay = totalGross - totalDeduction;
            decimal monthlyCTC = totalGross + totalContribution;
            decimal annualCTC = monthlyCTC * 12;

            var response = new EmployeeCTCCalculateResponse
            {
                Header = new EmployeeCTCHeaderResult
                {
                    EmployeeID = model.EmployeeID,
                    PayGroupID = model.PayGroupID,
                    EmpSalaryConfigID = model.EmpSalaryConfigID,

                    IsPF = model.IsPF,
                    IsESIC = model.IsESIC,
                    IsPTAX = model.IsPTAX,

                    MonthlyGross = ApplyRounding(totalGross, "ROUND"),
                    TotalAllowance = ApplyRounding(totalGross, "ROUND"),
                    TotalDeduction = ApplyRounding(totalDeduction, "ROUND"),
                    TotalContribution = ApplyRounding(totalContribution, "ROUND"),
                    NetPay = ApplyRounding(netPay, "ROUND"),
                    MonthlyCTC = ApplyRounding(monthlyCTC, "ROUND"),
                    AnnualCTC = ApplyRounding(annualCTC, "ROUND")
                }
            };

            foreach (var item in configList)
            {
                decimal monthly = calcs[item.PayConfigName.Trim().ToUpper()];

                response.DetailList.Add(new EmployeeCTCDetailResult
                {
                    PayConfigId = item.PayConfigId,
                    PayConfigName = item.PayConfigName,
                    PayType = item.PayType,

                    MonthlyAmount = ApplyRounding(monthly, "ROUND"),
                    YearlyAmount = ApplyRounding(monthly * 12, "ROUND"),

                    TimeBasis = item.TimeBasis,
                    LogicType = item.LogicType,
                    MappedColumn = item.MappedColumn,
                    CalculationFormula = item.CalculationFormula,
                    ManualRate = item.ManualRate,
                    ISPercentage = item.ISPercentage,

                    IsBasicComponent = item.IsBasicComponent,
                    IsGrossComponent = item.IsGrossComponent,
                    IsStatutory = item.IsStatutory,
                    StatutoryType = item.StatutoryType,

                    IsOther = item.IsOther,
                    OtherType = item.OtherType,
                    MaxLimit = item.MaxLimit,
                    RoundingType = item.RoundingType,
                    PTaxSlabsJson = item.PTaxSlabs != null && item.PTaxSlabs.Count > 0
                        ? JsonConvert.SerializeObject(item.PTaxSlabs)
                        : null
                });
            }

            return response;
        }
        [HttpPost]
        public JsonResult SaveEmployeeSalaryRevision(EmpSalaryConfigModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { Success = false, Message = "Invalid request data." });
                }

                if (model.EmployeeID <= 0 || model.PayGroupID <= 0)
                {
                    return Json(new { Success = false, Message = "Invalid Employee or PayGroup." });
                }

                if (model.DetailList == null || !model.DetailList.Any())
                {
                    return Json(new { Success = false, Message = "Salary details missing." });
                }

                string userName = Session["UserName"]?.ToString() ?? "SYSTEM";

                // 🔥 FINAL PAYLOAD STRUCTURE (MATCHES SP)
                var payload = new
                {
                    EmployeeID = model.EmployeeID,
                    PayGroupID = model.PayGroupID,
                    EmpSalaryConfigID = model.EmpSalaryConfigID,

                    IsPF = model.IsPF,
                    IsESIC = model.IsESIC,
                    IsPTAX = model.IsPTAX,

                    EffectiveFrom = model.EffectiveFrom,

                    MonthlyGross = model.MonthlyGross,
                    TotalAllowance = model.TotalAllowance,
                    TotalDeduction = model.TotalDeduction,
                    TotalContribution = model.TotalContribution,

                    NetPay = model.NetPay,
                    MonthlyCTC = model.MonthlyCTC,
                    AnnualCTC = model.AnnualCTC,

                    DetailList = model.DetailList
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);

                // 🔥 CALL SP
                DataTable dt = clsDatabase.fnDataTable(
                    "PRC_Save_EmployeeSalaryConfig",   // 👈 Use new SP
                    tenantID,
                    jsonPayload,
                    userName
                );

                if (dt != null && dt.Rows.Count > 0)
                {
                    int status = Convert.ToInt32(dt.Rows[0][0]);
                    string message = dt.Rows[0][1].ToString();
                    long configId = Convert.ToInt64(dt.Rows[0][2]);

                    return Json(new
                    {
                        Success = status == 1,
                        Message = message,
                        EmpSalaryConfigID = configId
                    });
                }

                return Json(new { Success = false, Message = "No response from server." });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}