using Common.Utility;
using DocumentFormat.OpenXml.Spreadsheet;
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
            ViewBag.CurrentTenantID = tenantID;
            return View();
        }

        [HttpGet]
        public ActionResult ExportEmployeeCTCExcel(List<string> companyIds,List<string> deptIds,List<string> categoryIds,List<string> payGroupIds,string status)
        {
            string userId = Session["UserId"]?.ToString();
            string companyFilter = NormalizeMultiSelect(companyIds);
            string deptFilter = NormalizeMultiSelect(deptIds);
            string categoryFilter = NormalizeMultiSelect(categoryIds);
            var payGroups = NormalizeMultiSelectList(payGroupIds);
            string statusFilter = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status;

            DataTable dt = clsDatabase.fnDataTable("SP_SAAS_GET_EMPLOYEE_CTC",tenantID,companyFilter,deptFilter,categoryFilter,userId);

            var rows = dt.AsEnumerable()
                .Where(r =>
                {
                    string rowStatus = Convert.ToString(r["Status"]);
                    string rowPayGroup = Convert.ToString(r["PayGroupID"]);

                    if (!statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase) &&
                        !rowStatus.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                        return false;

                    if (payGroups.Any() && !payGroups.Contains(rowPayGroup))
                        return false;

                    return true;
                })
                .ToList();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Employee CTC");

                ws.Cell(1, 1).Value = "Employee Info";
                ws.Cell(1, 2).Value = "Designation & Post";
                ws.Cell(1, 3).Value = "Department";
                ws.Cell(1, 4).Value = "Pay Group";

                var header = ws.Range(1, 1, 1, 4);
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                header.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0E7777");
                header.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                header.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

                int rowIndex = 2;
                foreach (DataRow r in rows)
                {
                    ws.Cell(rowIndex, 1).Value = $"{Convert.ToString(r["EmpName"])}{Environment.NewLine}{Convert.ToString(r["EmpCode"])}";
                    ws.Cell(rowIndex, 2).Value = $"{Convert.ToString(r["Designation"])}{Environment.NewLine}{Convert.ToString(r["Postname"])}";
                    ws.Cell(rowIndex, 3).Value = Convert.ToString(r["Department"]);
                    ws.Cell(rowIndex, 4).Value = Convert.ToString(r["PayGroupName"]);
                    rowIndex++;
                }

                if (ws.RangeUsed() != null)
                {
                    ws.RangeUsed().Style.Alignment.SetVertical(ClosedXML.Excel.XLAlignmentVerticalValues.Center);
                    ws.RangeUsed().Style.Alignment.SetWrapText(true);
                }

                ws.Columns().AdjustToContents();
                ws.Column(1).Width = Math.Max(ws.Column(1).Width, 28);
                ws.Column(2).Width = Math.Max(ws.Column(2).Width, 28);
                ws.Column(3).Width = Math.Max(ws.Column(3).Width, 22);
                ws.Column(4).Width = Math.Max(ws.Column(4).Width, 24);

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    string fileName = $"Employee_CTC_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName
                    );
                }
            }
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

            // 2. Fixed, Excel Upload and Manual grid values (e.g. Incentive, Fixed allowances/deductions)
            foreach (var item in configList.Where(x => !x.IsInput && (x.LogicType == "FIXED" || x.LogicType == "EXCEL_UPLOAD" || x.LogicType == "MANUAL_INPUT")))
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
            // Minimal fix: update Gross after formulas are calculated
            if (grossItem != null)
            {
                string grossKey = grossItem.PayConfigName.Trim().ToUpper();

                grossVal = calcs.ContainsKey(grossKey)
                    ? calcs[grossKey]
                    : 0;
            }

            // 5. Statutory
            ApplyStatutoryCalculation(configList, calcs, model, grossVal);

            // 6. Build response
            return BuildCalculatedResponse(configList, calcs, model);
        }

        private string NormalizeMultiSelect(List<string> items)
        {
            return string.Join(",", NormalizeMultiSelectList(items));
        }

        private List<string> NormalizeMultiSelectList(List<string> items)
        {
            if (items == null || items.Count == 0)
                return new List<string>();

            return items
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !x.Equals("multiselect-all", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();
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
                    IsTaxable = (dt.Columns.Contains("IsTaxable") && r["IsTaxable"] != DBNull.Value && (r["IsTaxable"].ToString() == "1" || r["IsTaxable"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase) || (decimal.TryParse(r["IsTaxable"].ToString(), out decimal t1) && t1 > 0)))
                             || (dt.Columns.Contains("isTaxable") && r["isTaxable"] != DBNull.Value && (r["isTaxable"].ToString() == "1" || r["isTaxable"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase) || (decimal.TryParse(r["isTaxable"].ToString(), out decimal t2) && t2 > 0))),
                    IsLoanComponent = r["IsLoanComponent"] != DBNull.Value && Convert.ToBoolean(r["IsLoanComponent"]),
                    IsCalculative = r["IScalculative"] != DBNull.Value && Convert.ToBoolean(r["IScalculative"]),
                    IsVariableAmt = r["IsVariableAmt"] != DBNull.Value && Convert.ToBoolean(r["IsVariableAmt"]),

                    IsStatutory = r["IsStatutory"] != DBNull.Value && Convert.ToBoolean(r["IsStatutory"]),
                    StatutoryType = Convert.ToString(r["StatutoryType"]),
                    CalculationRule = dt.Columns.Contains("CalculationRule") &&  r["CalculationRule"] != DBNull.Value? Convert.ToString(r["CalculationRule"]) : null,
                    MaxLimit = r["MaxLimit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r["MaxLimit"]),
                    RoundingType = Convert.ToString(r["RoundingType"]),

                    IsOther = r["IsOther"] != DBNull.Value && Convert.ToBoolean(r["IsOther"]),
                    OtherType = Convert.ToString(r["OtherType"]),

                    MappedColumn = Convert.ToString(r["MappedColumn"]),
                    //CalculationFormula = Convert.ToString(r["CalculationFormula"])
                    //CalculationFormula = GetFormattedFormula(r["CalculationFormula"],r["ManualRate"],r["ISPercentage"],r["LogicType"])
                    CalculationFormula = dt.Columns.Contains("CalculationRule") && 
                     r["CalculationRule"] != DBNull.Value &&
                     string.Equals(
                         Convert.ToString(r["CalculationRule"]),
                         "ESIC_BASIC_50_GROSS",
                         StringComparison.OrdinalIgnoreCase)
                    ? "MAX(BASIC, 50% GROSS)"
                    : Convert.ToString(r["CalculationFormula"]),
                };

                item.IsInput = item.PayType == "Manual" || item.IsGrossComponent;
                item.IsManualGrid = (item.LogicType == "FIXED" || item.LogicType == "EXCEL_UPLOAD" || item.LogicType == "MANUAL_INPUT") && item.PayType != "Manual";

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
                return Math.Round(value, 2, MidpointRounding.AwayFromZero);

            type = type.Trim().ToUpper();

            if (type.StartsWith("ROUND"))
            {
                var digitsText = type.Replace("ROUND", "");
                if (int.TryParse(digitsText, out int digits))
                    return Math.Round(value, digits, MidpointRounding.AwayFromZero);
            }

            switch (type)
            {
                case "CEIL":
                    return Math.Ceiling(value);
                case "FLOOR":
                    return Math.Floor(value);
                case "ROUND":
                default:
                    return Math.Round(value, 2, MidpointRounding.AwayFromZero);
            }
        }

        private decimal RoundMoney(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private decimal RoundRupee(decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        private decimal RoundStatutory(decimal value, string statType, string configuredRoundingType)
        {
            statType = (statType ?? "").Trim().ToUpper();

            // Statutory deductions/contributions are kept aligned with common Indian payroll rules:
            // PF is rounded to the nearest rupee, ESIC is rounded up to the next rupee, and PTAX is slab-based.
            if (statType == "PF_EE" || statType == "PF_ER")
                return Math.Round(value, 0, MidpointRounding.AwayFromZero);

            if (statType == "ESIC_EE" || statType == "ESIC_ER")
                return Math.Ceiling(value);

            if (statType == "PTAX")
                return Math.Round(value, 0, MidpointRounding.AwayFromZero);

            if (!string.IsNullOrWhiteSpace(configuredRoundingType))
                return ApplyRounding(value, configuredRoundingType);

            return RoundMoney(value);
        }

        private void ApplyStatutoryCalculation(List<SalaryComponentConfig> configList,Dictionary<string, decimal> calcs,EmployeeCTCCalculateRequest model,decimal grossVal)
        {
            foreach (var item in configList.Where(x => x.IsStatutory))
            {
                string payConfigKey = item.PayConfigName.Trim().ToUpper();
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

                            val = RoundStatutory((cap * item.ManualRate) / 100, statType, item.RoundingType);
                        }
                        break;

                    //case "ESIC_EE":
                    //case "ESIC_ER":
                    //    if (model.IsESIC)
                    //    {
                    //        decimal esicCap = item.MaxLimit.HasValue && item.MaxLimit.Value > 0
                    //            ? item.MaxLimit.Value
                    //            : 21000;

                    //        if (baseVal <= esicCap)
                    //            val = RoundStatutory((baseVal * item.ManualRate) / 100, statType, item.RoundingType);
                    //    }
                    //    break;
                    case "ESIC_EE":
                    case "ESIC_ER":
                        if (model.IsESIC)
                        {
                            decimal esicCap = item.MaxLimit.HasValue && item.MaxLimit.Value > 0
                                ? item.MaxLimit.Value
                                : 21000;

                            decimal esicBase = baseVal;

                            // New ESIC rule:
                            // MAX(BASIC, 50% GROSS)
                            if (string.Equals(
                                    item.CalculationRule,
                                    "ESIC_BASIC_50_GROSS",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                decimal basicAmount = configList
                                    .Where(x => x.IsBasicComponent)
                                    .Sum(x =>
                                    {
                                        string key = (x.PayConfigName ?? "").Trim().ToUpper();

                                        return calcs.TryGetValue(key, out decimal basicVal)
                                            ? basicVal
                                            : 0;
                                    });

                                decimal fiftyPercentGross = grossVal * 0.50m;

                                esicBase = Math.Max(
                                    basicAmount,
                                    fiftyPercentGross
                                );
                            }

                            // Legacy ESIC behavior
                            // CalculationRule is NULL / LEGACY
                            if (esicBase <= esicCap)
                            {
                                val = RoundStatutory(
                                    (esicBase * item.ManualRate) / 100,
                                    statType,
                                    item.RoundingType
                                );
                            }
                        }
                        break;
                    case "PTAX":
                        // =========================================================================
                        // MODIFICATION: Calculate sum of all taxable components (IsTaxable == true)
                        // PTAX slab matching is now evaluated against this taxable sum instead of grossVal.
                        // =========================================================================
                        decimal totalTaxable = configList
                            .Where(x => x.IsTaxable)
                            .Sum(x =>
                            {
                                string key = (x.PayConfigName ?? "").Trim().ToUpper();
                                return calcs.TryGetValue(key, out decimal compVal) ? compVal : 0;
                            });

                        if (model.IsPTAX && totalTaxable > 0 && item.PTaxSlabs != null && item.PTaxSlabs.Count > 0)
                        {
                            var slab = item.PTaxSlabs.FirstOrDefault(x => totalTaxable >= x.MinSalary && totalTaxable <= x.MaxSalary);
                            val = slab != null ? RoundStatutory(slab.PTaxAmount, statType, item.RoundingType) : 0;
                        }
                        break;

                    case "GRATUITY":
                        decimal gratuityRate = item.ManualRate > 0 ? item.ManualRate : 4.81m;
                        val = RoundStatutory((baseVal * gratuityRate) / 100, statType, item.RoundingType);
                        break;
                    case "TDS":
                        if (calcs.ContainsKey(payConfigKey))
                            continue;

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

            string GetKey(SalaryComponentConfig item)
            {
                return (item?.PayConfigName ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();
            }

            decimal GetRawValue(SalaryComponentConfig item)
            {
                if (item == null)
                    return 0;

                string key = GetKey(item);

                return calcs.TryGetValue(key, out decimal value)
                    ? value
                    : 0;
            }

            decimal GetRoundedValue(SalaryComponentConfig item)
            {
                if (item == null)
                    return 0;

                decimal value = GetRawValue(item);

                if (item.IsStatutory)
                {
                    return RoundStatutory(
                        value,
                        item.StatutoryType,
                        item.RoundingType);
                }

                return ApplyRounding(value, item.RoundingType);
            }

            /*
             * calcs is keyed by PayConfigName.
             * Therefore, the response calculation must also use each component
             * name only once. Otherwise, duplicate configuration rows will add
             * the same dictionary value multiple times.
             */
            var uniqueConfigList = configList
                .Where(x => x != null)
                .GroupBy(x => GetKey(x), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var basicItem = uniqueConfigList
                .FirstOrDefault(x => x.IsBasicComponent);

            var grossItem = uniqueConfigList
                .FirstOrDefault(x => x.IsGrossComponent);

            decimal basicValue = GetRoundedValue(basicItem);

            foreach (var item in uniqueConfigList)
            {
                string key = GetKey(item);

                /*
                 * These are summary or aggregate components and must not
                 * separately participate in allowance/deduction totals.
                 */
                if (item.IsBasicComponent ||item.IsGrossComponent ||key == "MONTHLY_CTC" ||key == "MONTHLY_NET")
                {
                    continue;
                }

                decimal value = GetRoundedValue(item);

                string payType = (item.PayType ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

                switch (payType)
                {
                    case "ALLOWANCE":
                    case "ALLOWANCES":
                        totalAllowance += value;
                        break;

                    case "DEDUCTION":
                    case "DEDUCTIONS":
                        totalDeduction += value;
                        break;

                    case "CC":
                    case "CONTRIBUTION":
                    case "CONTRIBUTIONS":
                    case "EMPLOYER CONTRIBUTION":
                    case "EMPLOYER CONTRIBUTIONS":
                        totalContribution += value;
                        break;
                }
            }

            /*
             * Use the calculated Gross component directly.
             * Basic + Allowances is only a fallback when no Gross component exists.
             */
            decimal totalGross = grossItem != null
                ? GetRoundedValue(grossItem)
                : basicValue + totalAllowance;

            totalGross = grossItem != null
                ? ApplyRounding(totalGross, grossItem.RoundingType)
                : RoundMoney(totalGross);

            totalAllowance = RoundMoney(totalAllowance);
            totalDeduction = RoundMoney(totalDeduction);
            totalContribution = RoundMoney(totalContribution);

            decimal netPay = totalGross - totalDeduction;

            /*
             * Monthly CTC mode:
             * Use the entered MONTHLY_CTC value.
             *
             * Gross/Net mode:
             * Derive Monthly CTC from Gross + Employer Contributions.
             */
            decimal monthlyCTC;

            var monthlyCTCItem = uniqueConfigList
                .FirstOrDefault(x => GetKey(x) == "MONTHLY_CTC");

            decimal inputMonthlyCTC = monthlyCTCItem != null
                ? GetRoundedValue(monthlyCTCItem)
                : 0;

            if (inputMonthlyCTC > 0)
            {
                monthlyCTC = inputMonthlyCTC;
            }
            else
            {
                monthlyCTC = totalGross + totalContribution;
            }

            decimal roundedNetPay = RoundRupee(netPay);
            decimal roundedMonthlyCTC = RoundRupee(monthlyCTC);
            decimal roundedAnnualCTC = roundedMonthlyCTC * 12;

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

                    MonthlyGross = RoundMoney(totalGross),
                    TotalAllowance = RoundMoney(totalAllowance),
                    TotalDeduction = RoundMoney(totalDeduction),
                    TotalContribution = RoundMoney(totalContribution),

                    NetPay = roundedNetPay,
                    MonthlyCTC = roundedMonthlyCTC,
                    AnnualCTC = roundedAnnualCTC
                }
            };

            foreach (var item in uniqueConfigList)
            {
                decimal roundedMonthly = GetRoundedValue(item);

                response.DetailList.Add(new EmployeeCTCDetailResult
                {
                    PayConfigId = item.PayConfigId,
                    PayConfigName = item.PayConfigName,
                    PayType = item.PayType,

                    MonthlyAmount = roundedMonthly,
                    YearlyAmount = RoundMoney(roundedMonthly * 12),

                    TimeBasis = item.TimeBasis,
                    LogicType = item.LogicType,
                    MappedColumn = item.MappedColumn,
                    CalculationFormula = item.CalculationFormula,
                    ManualRate = item.ManualRate,
                    ISPercentage = item.ISPercentage,

                    IsBasicComponent = item.IsBasicComponent,
                    IsGrossComponent = item.IsGrossComponent,
                    IsTaxable = item.IsTaxable,
                    IsStatutory = item.IsStatutory,
                    StatutoryType = item.StatutoryType,

                    IsOther = item.IsOther,
                    OtherType = item.OtherType,
                    MaxLimit = item.MaxLimit,
                    RoundingType = item.RoundingType,

                    PTaxSlabsJson =
                        item.PTaxSlabs != null &&
                        item.PTaxSlabs.Count > 0
                            ? JsonConvert.SerializeObject(item.PTaxSlabs)
                            : null
                });
            }

            return response;
        }
        //private EmployeeCTCCalculateResponse BuildCalculatedResponse(List<SalaryComponentConfig> configList,Dictionary<string, decimal> calcs,EmployeeCTCCalculateRequest model)
        //{
        //    decimal totalAllowance = 0;
        //    decimal totalDeduction = 0;
        //    decimal totalContribution = 0;

        //    var basicItem = configList.FirstOrDefault(x => x.IsBasicComponent);
        //    decimal basicValue = basicItem != null
        //        ? calcs[basicItem.PayConfigName.Trim().ToUpper()]
        //        : 0;

        //    foreach (var item in configList)
        //    {
        //        decimal val = calcs[item.PayConfigName.Trim().ToUpper()];

        //        if (item.IsBasicComponent)
        //            continue;

        //        switch (item.PayType)
        //        {
        //            case "Allowances":
        //                totalAllowance += val;
        //                break;
        //            case "Deduction":
        //                totalDeduction += val;
        //                break;
        //            case "CC":
        //                totalContribution += val;
        //                break;
        //        }
        //    }

        //    decimal totalGross = basicValue + totalAllowance;
        //    decimal netPay = totalGross - totalDeduction;
        //    decimal monthlyCTC = totalGross + totalContribution;
        //    decimal roundedNetPay = RoundRupee(netPay);
        //    decimal roundedMonthlyCTC = RoundRupee(monthlyCTC);
        //    decimal roundedAnnualCTC = roundedMonthlyCTC * 12;

        //    var response = new EmployeeCTCCalculateResponse
        //    {
        //        Header = new EmployeeCTCHeaderResult
        //        {
        //            EmployeeID = model.EmployeeID,
        //            PayGroupID = model.PayGroupID,
        //            EmpSalaryConfigID = model.EmpSalaryConfigID,

        //            IsPF = model.IsPF,
        //            IsESIC = model.IsESIC,
        //            IsPTAX = model.IsPTAX,

        //            MonthlyGross = RoundMoney(totalGross),
        //            TotalAllowance = RoundMoney(totalGross),
        //            TotalDeduction = RoundMoney(totalDeduction),
        //            TotalContribution = RoundMoney(totalContribution),
        //            NetPay = roundedNetPay,
        //            MonthlyCTC = roundedMonthlyCTC,
        //            AnnualCTC = roundedAnnualCTC
        //        }
        //    };

        //    foreach (var item in configList)
        //    {
        //        decimal monthly = calcs[item.PayConfigName.Trim().ToUpper()];

        //        decimal roundedMonthly = item.IsStatutory
        //            ? RoundStatutory(monthly, item.StatutoryType, item.RoundingType)
        //            : RoundMoney(monthly);

        //        response.DetailList.Add(new EmployeeCTCDetailResult
        //        {
        //            PayConfigId = item.PayConfigId,
        //            PayConfigName = item.PayConfigName,
        //            PayType = item.PayType,

        //            MonthlyAmount = roundedMonthly,
        //            YearlyAmount = RoundMoney(roundedMonthly * 12),

        //            TimeBasis = item.TimeBasis,
        //            LogicType = item.LogicType,
        //            MappedColumn = item.MappedColumn,
        //            CalculationFormula = item.CalculationFormula,
        //            ManualRate = item.ManualRate,
        //            ISPercentage = item.ISPercentage,

        //            IsBasicComponent = item.IsBasicComponent,
        //            IsGrossComponent = item.IsGrossComponent,
        //            IsTaxable = item.IsTaxable,
        //            IsStatutory = item.IsStatutory,
        //            StatutoryType = item.StatutoryType,

        //            IsOther = item.IsOther,
        //            OtherType = item.OtherType,
        //            MaxLimit = item.MaxLimit,
        //            RoundingType = item.RoundingType,
        //            PTaxSlabsJson = item.PTaxSlabs != null && item.PTaxSlabs.Count > 0
        //                ? JsonConvert.SerializeObject(item.PTaxSlabs)
        //                : null
        //        });
        //    }

        //    return response;
        //}
        [HttpPost]
        public JsonResult SaveSalaryDetailsComponents(SalaryDetailsComponentSaveRequest model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { Success = false, Message = "Invalid request data." });
                }

                if (model.EmployeeID <= 0 || model.PayGroupID <= 0)
                {
                    return Json(new { Success = false, Message = "Employee and PayGroup are required." });
                }

                if (model.Allowances == null || !model.Allowances.Any())
                {
                    return Json(new { Success = false, Message = "No allowance values found." });
                }

                var validValues = model.Allowances
                    .Where(x => x.AllowanceId > 0)
                    .Select(x => new
                    {
                        x.AllowanceId,
                        x.Value
                    })
                    .ToList();

                if (!validValues.Any())
                {
                    return Json(new { Success = false, Message = "No valid allowance values found." });
                }

                string userName = Session["UserName"]?.ToString() ?? "SYSTEM";
                DateTime effectiveFrom = model.EffectiveFrom.HasValue
                    ? model.EffectiveFrom.Value
                    : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                var calculateRequest = new EmployeeCTCCalculateRequest
                {
                    EmployeeID = model.EmployeeID,
                    PayGroupID = model.PayGroupID,
                    EmpSalaryConfigID = model.EmpSalaryConfigID,
                    IsPF = model.IsPF ?? true,
                    IsESIC = model.IsESIC ?? true,
                    IsPTAX = model.IsPTAX ?? true,
                    Inputs = validValues.Select(x => new EmployeeCTCInputValue
                    {
                        PayConfigId = x.AllowanceId,
                        MonthlyAmount = x.Value
                    }).ToList()
                };

                var calculated = CalculateSalaryOnBackend(calculateRequest);

                var payload = new
                {
                    EmployeeID = model.EmployeeID,
                    PayGroupID = model.PayGroupID,
                    EmpSalaryConfigID = model.EmpSalaryConfigID,

                    IsPF = calculateRequest.IsPF,
                    IsESIC = calculateRequest.IsESIC,
                    IsPTAX = calculateRequest.IsPTAX,

                    EffectiveFrom = effectiveFrom,

                    MonthlyGross = calculated.Header.MonthlyGross,
                    TotalAllowance = calculated.Header.TotalAllowance,
                    TotalDeduction = calculated.Header.TotalDeduction,
                    TotalContribution = calculated.Header.TotalContribution,

                    NetPay = calculated.Header.NetPay,
                    MonthlyCTC = calculated.Header.MonthlyCTC,
                    AnnualCTC = calculated.Header.AnnualCTC,

                    DetailList = calculated.DetailList
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);

                DataTable dt = clsDatabase.fnDataTable(
                    "PRC_Save_EmployeeSalaryConfig",
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
                        EmpSalaryConfigID = configId,
                        EmployeeID = model.EmployeeID,
                        PayGroupID = model.PayGroupID,
                        AllowanceCount = validValues.Count
                    });
                }

                return Json(new
                {
                    Success = false,
                    Message = "No response from server."
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

        [HttpPost]
        public JsonResult SaveEmployeeSalaryRevision(EmpSalaryConfigRevisionModel model)
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
                string Normalize(string value)
                {
                    return (value ?? string.Empty)
                        .Trim()
                        .ToUpperInvariant();
                }
                /*
                * Sum all monthly earning/allowance components.
                * Exclude GROSS because it is a summary component.
                */
                decimal totalAllowance = model.DetailList
                    .Where(x =>
                    {
                        string payType = Normalize(x.PayType);
                        string componentName = Normalize(x.PayConfigName);

                        bool isAllowance =
                            payType == "ALLOWANCE" ||
                            payType == "ALLOWANCES";

                        bool isSummaryComponent =
                            x.IsGrossComponent ||
                            componentName == "GROSS" ||
                            componentName == "MONTHLY_CTC" ||
                            componentName == "MONTHLY_NET";

                        return isAllowance && !isSummaryComponent;
                    })
                    .Sum(x => x.MonthlyAmount);

                decimal totalDeduction = model.DetailList
                    .Where(x =>
                    {
                        string payType = Normalize(x.PayType);

                        return payType == "DEDUCTION" ||
                               payType == "DEDUCTIONS";
                    })
                    .Sum(x => x.MonthlyAmount);

                decimal totalContribution = model.DetailList
                    .Where(x =>
                    {
                        string payType = Normalize(x.PayType);

                        return payType == "CC" ||
                               payType == "CONTRIBUTION" ||
                               payType == "CONTRIBUTIONS" ||
                               payType == "EMPLOYER CONTRIBUTION";
                    })
                    .Sum(x => x.MonthlyAmount);

                var grossItem = model.DetailList
                    .FirstOrDefault(x =>
                        x.IsGrossComponent ||
                        Normalize(x.PayConfigName) == "GROSS");

                decimal monthlyGross = grossItem != null
                    ? grossItem.MonthlyAmount
                    : model.MonthlyGross;

                decimal netPay =
                    monthlyGross - totalDeduction;

                decimal monthlyCTC =
                    monthlyGross + totalContribution;

                decimal annualCTC =
                    monthlyCTC * 12;
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

                    MonthlyGross = monthlyGross,
                    TotalAllowance = totalAllowance,
                    TotalDeduction = totalDeduction,
                    TotalContribution = totalContribution,

                    NetPay = netPay,
                    MonthlyCTC = monthlyCTC,
                    AnnualCTC = annualCTC,

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
