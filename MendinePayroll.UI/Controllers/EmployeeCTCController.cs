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
            // 1. Load Pay Group configuration.
            DataTable dt = clsDatabase.fnDataTable("PRC_GetSalaryConfigByPayGroup",model.PayGroupID,tenantID);

            var configList = MapConfig(dt);

            int calculationMode = 1;

            if (dt.Rows.Count > 0 &&dt.Columns.Contains("CalculationMode") && dt.Rows[0]["CalculationMode"] != DBNull.Value)
            {
                calculationMode = Convert.ToInt32(dt.Rows[0]["CalculationMode"]);
            }

            bool isCTCInput = calculationMode == 2;

            // 2. Initialize configured Pay Group components.
            var calcs = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in configList)
            {
                string key = item.PayConfigName.Trim();

                if (!calcs.ContainsKey(key))calcs[key] = 0m;
            }

            // 3. Load the tenant-specific Gross component.
            var tenantGross = GetTenantGrossComponent();
            long grossPayConfigId = tenantGross.PayConfigId;
            string grossKey = tenantGross.PayConfigName;

            // The dictionary key is the tenant's actual component name.
            if (!calcs.ContainsKey(grossKey)) calcs[grossKey] = 0m;

            // 4. Load input components. 
            foreach (var item in configList.Where(x => x.IsInput))
            {
                var input = model.Inputs?.FirstOrDefault(
                    x => x.PayConfigId == item.PayConfigId);

                calcs[item.PayConfigName.Trim()] =
                    input?.MonthlyAmount ?? 0m;
            }

            // 5. Load fixed, uploaded and manually entered components.
            foreach (var item in configList.Where(x =>
                !x.IsInput &&
                (x.LogicType == "FIXED" ||
                 x.LogicType == "EXCEL_UPLOAD" ||
                 x.LogicType == "MANUAL_INPUT")))
            {
                var input = model.Inputs?.FirstOrDefault(
                    x => x.PayConfigId == item.PayConfigId);

                calcs[item.PayConfigName.Trim()] =
                    input != null
                        ? input.MonthlyAmount
                        : item.ManualRate;
            }

            var basicItem = configList.FirstOrDefault(x => x.IsBasicComponent);

            var ctcItem = configList.FirstOrDefault(x =>
                string.Equals(
                    x.OtherType,
                    "MONTHLY_CTC",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    x.PayConfigName,
                    "MONTHLY_CTC",
                    StringComparison.OrdinalIgnoreCase));

            decimal grossVal = 0m;

            if (isCTCInput)
            {
                // ================================================
                // MODE 2: MONTHLY CTC INPUT
                // ================================================

                if (ctcItem == null)
                {
                    throw new InvalidOperationException(
                        "Monthly CTC component is not configured for this Pay Group.");
                }

                var ctcInput = model.Inputs?.FirstOrDefault(x => x.PayConfigId == ctcItem.PayConfigId);

                decimal monthlyCTC = ctcInput?.MonthlyAmount ?? 0m;

                if (monthlyCTC <= 0m)
                {
                    calcs[ctcItem.PayConfigName.Trim()] = 0m;
                    calcs[basicItem?.PayConfigName?.Trim() ?? "BASIC"] = 0m;
                    calcs[grossKey] = 0m;

                    return BuildCalculatedResponse(
                        configList,
                        calcs,
                        model,
                        grossKey,
                        grossPayConfigId);
                }

                calcs[ctcItem.PayConfigName.Trim()] = monthlyCTC;

                // Make the CTC available to formulas using its configured name.
                calcs["MONTHLY_CTC"] = monthlyCTC;

                // 6. Calculate Basic.
                if (basicItem != null)
                {
                    decimal basicBase = monthlyCTC;

                    if (basicItem.LogicType == "FORMULA" &&
                        !string.IsNullOrWhiteSpace(
                            basicItem.CalculationFormula))
                    {
                        basicBase = EvaluateFormula(
                            basicItem.CalculationFormula,
                            calcs);
                    }

                    decimal basicVal = basicItem.ISPercentage
                        ? basicBase * basicItem.ManualRate / 100m
                        : basicBase;

                    calcs[basicItem.PayConfigName.Trim()] = basicVal;
                }

                // 7. Calculate employer-cost components, applying statutory rules.
                decimal totalEmployerCost = 0m;

                foreach (var item in configList.Where(x =>
                    string.Equals(
                        x.PayType,
                        "CC",
                        StringComparison.OrdinalIgnoreCase)))
                {
                    string statutoryType = (item.StatutoryType ?? string.Empty)
                        .Trim()
                        .ToUpperInvariant();

                    decimal amount = 0m;
                    decimal maxLimit = item.MaxLimit ?? 0m;

                    // Statutory employer contributions.
                    if (item.IsStatutory)
                    {
                        switch (statutoryType)
                        {
                            case "PF_ER":
                                {
                                    if (!model.IsPF)
                                        break;

                                    // PF is calculated on Basic, capped at MaxLimit.
                                    decimal pfBase = basicItem != null
                                        ? calcs[basicItem.PayConfigName.Trim()]
                                        : 0m;

                                    if (maxLimit > 0m)
                                        pfBase = Math.Min(pfBase, maxLimit);

                                    amount = ApplyRounding(
                                        pfBase * item.ManualRate / 100m,
                                        item.RoundingType);

                                    break;
                                }

                            case "ESIC_ER":
                                {
                                    if (!model.IsESIC)
                                        break;

                                    decimal basic = basicItem != null
                                        ? calcs[basicItem.PayConfigName.Trim()]
                                        : 0m;

                                    // Gross is not calculated yet. Derive the ESIC base
                                    // from the currently known salary values.
                                    decimal esicBase = basic;

                                    if (string.Equals(
                                        item.CalculationRule,
                                        "ESIC_BASIC_50_GROSS",
                                        StringComparison.OrdinalIgnoreCase))
                                    {
                                        // This rule requires Gross, which is not yet known.
                                        // Use the statutory engine after Gross is derived.
                                        amount = 0m;
                                        calcs[item.PayConfigName.Trim()] = amount;
                                        continue;
                                    }

                                    decimal esicLimit = maxLimit > 0m
                                        ? maxLimit
                                        : 21000m;

                                    amount = esicBase <= esicLimit
                                        ? ApplyRounding(
                                            esicBase * item.ManualRate / 100m,
                                            item.RoundingType)
                                        : 0m;

                                    break;
                                }

                            case "GRATUITY":
                                {
                                    decimal basic = basicItem != null
                                        ? calcs[basicItem.PayConfigName.Trim()]
                                        : 0m;

                                    amount = ApplyRounding(
                                        basic * item.ManualRate / 100m,
                                        item.RoundingType);

                                    break;
                                }

                            default:
                                {
                                    // Other statutory contribution types should be handled
                                    // by their own explicit rule.
                                    amount = 0m;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        // Non-statutory employer costs, such as Ex Gratia.
                        decimal baseVal = 0m;

                        if (item.LogicType == "FORMULA" &&
                            !string.IsNullOrWhiteSpace(item.CalculationFormula))
                        {
                            baseVal = EvaluateFormula(
                                item.CalculationFormula,
                                calcs );
                        }
                        else if (item.LogicType == "FIXED")
                        {
                            baseVal = item.ManualRate;
                        }

                        amount = item.ISPercentage
                            ? baseVal * item.ManualRate / 100m
                            : baseVal;

                        if (maxLimit > 0m && amount > maxLimit)
                            amount = maxLimit;

                        amount = ApplyRounding(amount, item.RoundingType);
                    }

                    calcs[item.PayConfigName.Trim()] = amount;
                    totalEmployerCost += RoundRupee(amount);
                }

                // 8. Derive Gross from Monthly CTC.
                grossVal = monthlyCTC - totalEmployerCost;

                if (grossVal < 0m)
                {
                    throw new InvalidOperationException(
                        "Employer costs exceed the entered Monthly CTC.");
                }

                calcs[grossKey.Trim()] = grossVal;

                // 8. Gross = Monthly CTC - employer costs.
                grossVal = monthlyCTC - totalEmployerCost;

                if (grossVal < 0m)
                {
                    throw new InvalidOperationException(
                        "Employer costs exceed the entered Monthly CTC.");
                }

                calcs[grossKey] = grossVal;
            }
            else
            {
                // MODE 1: GROSS INPUT

                var grossInput = model.Inputs?.FirstOrDefault(
                    x => x.PayConfigId == grossPayConfigId);

                if (grossInput == null)
                {
                    throw new InvalidOperationException(
                        "Please provide an amount for the tenant's Gross component.");
                }

                grossVal = grossInput.MonthlyAmount;

                if (grossVal < 0m)
                {
                    throw new InvalidOperationException(
                        "Gross amount cannot be negative.");
                }

                calcs[grossKey] = grossVal;

                if (basicItem != null && basicItem.ISPercentage)
                {
                    decimal basicVal =
                        grossVal * basicItem.ManualRate / 100m;

                    calcs[basicItem.PayConfigName.Trim()] = basicVal;
                }
            }

            // 9. Calculate remaining formulas.
            // Do not overwrite Basic or the employer-cost components
            // already calculated in CTC mode.
            for (int i = 0; i < 3; i++)
            {
                foreach (var item in configList.Where(x =>
                    x.LogicType == "FORMULA" &&
                    !x.IsStatutory &&
                    !x.IsBasicComponent &&
                    (!isCTCInput ||
                     !string.Equals(
                         x.PayType,
                         "CC",
                         StringComparison.OrdinalIgnoreCase))))
                {
                    decimal baseVal = EvaluateFormula(
                        item.CalculationFormula,
                        calcs);

                    decimal amount = item.ISPercentage
                        ? baseVal * item.ManualRate / 100m
                        : baseVal;

                    calcs[item.PayConfigName.Trim()] = amount;
                }
            }

            // 10. Apply statutory calculations.
            ApplyStatutoryCalculation(configList,calcs,model,grossVal);

            // 11. Build the response.
            return BuildCalculatedResponse(configList, calcs, model, grossKey, grossPayConfigId);
        }
        //private EmployeeCTCCalculateResponse CalculateSalaryOnBackend(EmployeeCTCCalculateRequest model)
        //{
        //    DataTable dt = clsDatabase.fnDataTable("PRC_GetSalaryConfigByPayGroup",model.PayGroupID,tenantID);

        //    var configList = MapConfig(dt);

        //    var calcs = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        //    foreach (var item in configList)
        //        calcs[item.PayConfigName.Trim().ToUpper()] = 0;

        //    // 1. Input values
        //    foreach (var item in configList.Where(x => x.IsInput))
        //    {
        //        var input = model.Inputs.FirstOrDefault(x => x.PayConfigId == item.PayConfigId);
        //        decimal val = input != null ? input.MonthlyAmount : 0;
        //        calcs[item.PayConfigName.Trim().ToUpper()] = val;
        //    }

        //    // 2. Fixed, Excel Upload and Manual grid values (e.g. Incentive, Fixed allowances/deductions)
        //    foreach (var item in configList.Where(x => !x.IsInput && (x.LogicType == "FIXED" || x.LogicType == "EXCEL_UPLOAD" || x.LogicType == "MANUAL_INPUT")))
        //    {
        //        var input = model.Inputs.FirstOrDefault(x => x.PayConfigId == item.PayConfigId);
        //        decimal val = input != null ? input.MonthlyAmount : item.ManualRate;
        //        calcs[item.PayConfigName.Trim().ToUpper()] = val;
        //    }

        //    // 3. Gross and basic
        //    var grossItem = configList.FirstOrDefault(x => x.IsGrossComponent);
        //    var basicItem = configList.FirstOrDefault(x => x.IsBasicComponent);

        //    decimal grossVal = 0;
        //    if (grossItem != null)
        //        grossVal = calcs[grossItem.PayConfigName.Trim().ToUpper()];

        //    if (basicItem != null && basicItem.ISPercentage)
        //    {
        //        decimal basicVal = grossVal * basicItem.ManualRate / 100;
        //        calcs[basicItem.PayConfigName.Trim().ToUpper()] = basicVal;
        //    }

        //    // 4. Formula calculation
        //    for (int i = 0; i < 3; i++)
        //    {
        //        foreach (var item in configList.Where(x => x.LogicType == "FORMULA" && !x.IsStatutory))
        //        {
        //            decimal baseVal = EvaluateFormula(item.CalculationFormula, calcs);
        //            decimal val = item.ISPercentage ? (baseVal * item.ManualRate / 100) : baseVal;
        //            calcs[item.PayConfigName.Trim().ToUpper()] = val;
        //        }
        //    }
        //    // Minimal fix: update Gross after formulas are calculated
        //    if (grossItem != null)
        //    {
        //        string grossKey = grossItem.PayConfigName.Trim().ToUpper();

        //        grossVal = calcs.ContainsKey(grossKey)
        //            ? calcs[grossKey]
        //            : 0;
        //    }

        //    // 5. Statutory
        //    ApplyStatutoryCalculation(configList, calcs, model, grossVal);

        //    // 6. Build response
        //    return BuildCalculatedResponse(configList, calcs, model);
        //}

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
                        // Match PTAX slabs against taxable allowances, excluding summary rows.
                        decimal totalTaxable = configList
                            .Where(x =>
                            {
                                string payType = (x.PayType ?? string.Empty).Trim().ToUpperInvariant();
                                string componentName = (x.PayConfigName ?? string.Empty).Trim().ToUpperInvariant();
                                string otherType = (x.OtherType ?? string.Empty).Trim().ToUpperInvariant();

                                return x.IsTaxable &&
                                    (payType == "ALLOWANCE" || payType == "ALLOWANCES") &&
                                    !x.IsGrossComponent &&
                                    componentName != "MONTHLY_CTC" && componentName != "MONTHLY_NET" &&
                                    otherType != "MONTHLY_CTC" && otherType != "MONTHLY_NET";
                            })
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
        //private EmployeeCTCCalculateResponse BuildCalculatedResponse(List<SalaryComponentConfig> configList,Dictionary<string, decimal> calcs,EmployeeCTCCalculateRequest model,string grossKey)
        //{
        //    decimal totalAllowance = 0;
        //    decimal totalDeduction = 0;
        //    decimal totalContribution = 0;

        //    string GetKey(SalaryComponentConfig item)
        //    {
        //        return (item?.PayConfigName ?? string.Empty)
        //            .Trim()
        //            .ToUpperInvariant();
        //    }

        //    decimal GetRawValue(SalaryComponentConfig item)
        //    {
        //        if (item == null)
        //            return 0;

        //        string key = GetKey(item);

        //        return calcs.TryGetValue(key, out decimal value)
        //            ? value
        //            : 0;
        //    }

        //    decimal GetRoundedValue(SalaryComponentConfig item)
        //    {
        //        if (item == null)
        //            return 0;

        //        decimal value = GetRawValue(item);

        //        if (item.IsStatutory)
        //        {
        //            return RoundStatutory(
        //                value,
        //                item.StatutoryType,
        //                item.RoundingType);
        //        }

        //        return ApplyRounding(value, item.RoundingType);
        //    }

        //    /*
        //     * calcs is keyed by PayConfigName.
        //     * Therefore, the response calculation must also use each component
        //     * name only once. Otherwise, duplicate configuration rows will add
        //     * the same dictionary value multiple times.
        //     */
        //    var uniqueConfigList = configList
        //        .Where(x => x != null)
        //        .GroupBy(x => GetKey(x), StringComparer.OrdinalIgnoreCase)
        //        .Select(group => group.First())
        //        .ToList();

        //    var basicItem = uniqueConfigList
        //        .FirstOrDefault(x => x.IsBasicComponent);

        //    var grossItem = uniqueConfigList
        //        .FirstOrDefault(x => x.IsGrossComponent);

        //    decimal basicValue = GetRoundedValue(basicItem);


        //    foreach (var item in uniqueConfigList)
        //    {
        //        string key = GetKey(item);

        //        /*
        //         * These are summary or aggregate components and must not
        //         * separately participate in allowance/deduction totals.
        //         */
        //        if (item.IsBasicComponent ||item.IsGrossComponent ||key == "MONTHLY_CTC" ||key == "MONTHLY_NET")
        //        {
        //            continue;
        //        }

        //        decimal value = GetRoundedValue(item);

        //        string payType = (item.PayType ?? string.Empty)
        //            .Trim()
        //            .ToUpperInvariant();

        //        switch (payType)
        //        {

        //            case "ALLOWANCES":
        //                totalAllowance += value;
        //                break;

        //            case "DEDUCTION": 
        //                totalDeduction += value;
        //                break;

        //            case "CC": 
        //                totalContribution += value;
        //                break;
        //        }
        //    }

        //    /*
        //     * Use the calculated Gross component directly.
        //     * Basic + Allowances is only a fallback when no Gross component exists.
        //     */
        //    //decimal totalGross = grossItem != null
        //    //    ? GetRoundedValue(grossItem)
        //    //    : basicValue + totalAllowance;
        //    decimal totalGross = calcs.TryGetValue(
        //                grossKey.Trim(),
        //                out decimal calculatedGross)
        //                    ? calculatedGross
        //                    : 0m;

        //    //totalGross = RoundMoney(totalGross);
        //    totalGross = grossItem != null
        //        ? ApplyRounding(totalGross, grossItem.RoundingType)
        //        : RoundMoney(totalGross);

        //    totalAllowance = RoundMoney(totalAllowance);
        //    totalDeduction = RoundMoney(totalDeduction);
        //    totalContribution = RoundMoney(totalContribution);

        //    decimal netPay = totalGross - totalDeduction;

        //    /*
        //     * Monthly CTC mode:
        //     * Use the entered MONTHLY_CTC value.
        //     *
        //     * Gross/Net mode:
        //     * Derive Monthly CTC from Gross + Employer Contributions.
        //     */
        //    decimal monthlyCTC;

        //    var monthlyCTCItem = uniqueConfigList
        //        .FirstOrDefault(x => GetKey(x) == "MONTHLY_CTC");

        //    decimal inputMonthlyCTC = monthlyCTCItem != null
        //        ? GetRoundedValue(monthlyCTCItem)
        //        : 0;

        //    if (inputMonthlyCTC > 0)
        //    {
        //        monthlyCTC = inputMonthlyCTC;
        //    }
        //    else
        //    {
        //        monthlyCTC = totalGross + totalContribution;
        //    }

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
        //            TotalAllowance = RoundMoney(totalAllowance),
        //            TotalDeduction = RoundMoney(totalDeduction),
        //            TotalContribution = RoundMoney(totalContribution),

        //            NetPay = roundedNetPay,
        //            MonthlyCTC = roundedMonthlyCTC,
        //            AnnualCTC = roundedAnnualCTC
        //        }
        //    };

        //    foreach (var item in uniqueConfigList)
        //    {
        //        decimal roundedMonthly = GetRoundedValue(item);

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

        //            PTaxSlabsJson =
        //                item.PTaxSlabs != null &&
        //                item.PTaxSlabs.Count > 0
        //                    ? JsonConvert.SerializeObject(item.PTaxSlabs)
        //                    : null
        //        });
        //    }

        //    return response;
        //}
        private SalaryComponentConfig GetTenantGrossComponent()
        {
            DataTable grossDt = clsDatabase.fnDataTable("PRC_GetGrossPayConfig", tenantID);

            if (grossDt == null || grossDt.Rows.Count == 0 ||
                !grossDt.Columns.Contains("PayConfigId") ||
                !grossDt.Columns.Contains("PayConfigName") ||
                grossDt.Rows[0]["PayConfigId"] == DBNull.Value)
            {
                throw new InvalidOperationException("Gross Pay Component is not configured for this tenant.");
            }

            long grossPayConfigId = Convert.ToInt64(grossDt.Rows[0]["PayConfigId"]);
            string grossKey = Convert.ToString(grossDt.Rows[0]["PayConfigName"])?.Trim();

            if (grossPayConfigId <= 0 || string.IsNullOrWhiteSpace(grossKey))
            {
                throw new InvalidOperationException("Gross Pay Component ID or name is invalid for this tenant.");
            }

            return new SalaryComponentConfig
            {
                PayConfigId = grossPayConfigId,
                PayConfigName = grossKey,
                IsGrossComponent = true
            };
        }

        private void EnsureGrossDetail(List<EmployeeCTCDetailResult> details, long grossPayConfigId,
            string grossKey, decimal monthlyGross)
        {
            var grossDetail = details.FirstOrDefault(x => x.PayConfigId == grossPayConfigId)
                ?? details.FirstOrDefault(x => string.Equals(
                    x.PayConfigName?.Trim(), grossKey, StringComparison.OrdinalIgnoreCase));

            if (grossDetail == null)
            {
                grossDetail = new EmployeeCTCDetailResult
                {
                    PayType = "Manual",
                    TimeBasis = "MONTHLY",
                    LogicType = "FIXED"
                };
                details.Add(grossDetail);
            }

            grossDetail.PayConfigId = grossPayConfigId;
            grossDetail.PayConfigName = grossKey;
            grossDetail.IsGrossComponent = true;
            if (string.IsNullOrWhiteSpace(grossDetail.RoundingType))
            {
                grossDetail.RoundingType = "Round";
            }
            grossDetail.MonthlyAmount = monthlyGross;
            grossDetail.YearlyAmount = RoundMoney(monthlyGross * 12m);
        }

        private EmployeeCTCCalculateResponse BuildCalculatedResponse(List<SalaryComponentConfig> configList,Dictionary<string, decimal> calcs,EmployeeCTCCalculateRequest model,string grossKey,long grossPayConfigId)
        {
            decimal totalAllowance = 0m;
            decimal totalDeduction = 0m;
            decimal totalContribution = 0m;

            string GetKey(SalaryComponentConfig item)
            {
                return (item?.PayConfigName ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();
            }

            decimal GetRawValue(SalaryComponentConfig item)
            {
                if (item == null)
                    return 0m;

                string key = GetKey(item);

                return calcs.TryGetValue(key, out decimal value)
                    ? value
                    : 0m;
            }

            decimal GetRoundedValue(SalaryComponentConfig item)
            {
                if (item == null)
                    return 0m;

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

            // The calculation dictionary is keyed by component name.
            // Keep one configuration entry per component name.
            var uniqueConfigList = (configList ?? new List<SalaryComponentConfig>())
                .Where(x => x != null)
                .GroupBy(x => GetKey(x), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            var basicItem = uniqueConfigList
                .FirstOrDefault(x => x.IsBasicComponent);

            var grossItem = uniqueConfigList
                .FirstOrDefault(x => x.IsGrossComponent);

            decimal basicValue = GetRoundedValue(basicItem);

            // 1. Calculate totals using raw decimal values.
            foreach (var item in uniqueConfigList)
            {
                string key = GetKey(item);

                // Do not include summary/input components in these totals.
                if (item.IsBasicComponent ||
                    item.IsGrossComponent ||
                    key == "MONTHLY_CTC" ||
                    key == "MONTHLY_NET")
                {
                    continue;
                }

                decimal value = GetRawValue(item);

                string payType = (item.PayType ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();

                switch (payType)
                {
                    case "ALLOWANCES":
                        totalAllowance += RoundRupee(GetRoundedValue(item));
                        break;

                    case "DEDUCTION":
                        totalDeduction += RoundRupee(GetRoundedValue(item));
                        break;

                    case "CC":
                        totalContribution += RoundRupee(GetRoundedValue(item));
                        break;
                }
            }

            // 2. Round only after summing all component values.
            totalAllowance = RoundMoney(totalAllowance);
            totalDeduction = RoundMoney(totalDeduction);
            totalContribution = RoundMoney(totalContribution);

            // 3. Read the tenant-specific Gross value.
            if (string.IsNullOrWhiteSpace(grossKey))
            {
                throw new InvalidOperationException(
                    "The tenant's Gross component name is not configured.");
            }

            if (!calcs.TryGetValue(grossKey.Trim(), out decimal calculatedGross))
            {
                throw new InvalidOperationException(
                    $"Calculated Gross value was not found for component '{grossKey}'.");
            }

            decimal totalGross = grossItem != null
                ? ApplyRounding(calculatedGross, grossItem.RoundingType)
                : RoundMoney(calculatedGross);

            // 4. Calculate Net Pay.
            decimal netPay = totalGross - totalDeduction;

            // 5. Get Monthly CTC.
            var monthlyCTCItem = uniqueConfigList.FirstOrDefault(x =>
                string.Equals(
                    x.OtherType,
                    "MONTHLY_CTC",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    GetKey(x),
                    "MONTHLY_CTC",
                    StringComparison.OrdinalIgnoreCase));

            decimal inputMonthlyCTC = monthlyCTCItem != null
                ? GetRawValue(monthlyCTCItem)
                : 0m;

            decimal monthlyCTC = inputMonthlyCTC > 0m
                ? inputMonthlyCTC
                : totalGross + totalContribution;

            decimal roundedNetPay = RoundRupee(netPay);
            decimal roundedMonthlyCTC = RoundRupee(monthlyCTC);
            decimal roundedAnnualCTC = roundedMonthlyCTC * 12m;

            // 6. Build response header.
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
                    TotalAllowance = totalAllowance,
                    TotalDeduction = totalDeduction,
                    TotalContribution = totalContribution,

                    NetPay = roundedNetPay,
                    MonthlyCTC = roundedMonthlyCTC,
                    AnnualCTC = roundedAnnualCTC
                }
            };

            // 7. Build component detail rows.
            foreach (var item in uniqueConfigList)
            {
                decimal roundedMonthly = GetRoundedValue(item);

                response.DetailList.Add(new EmployeeCTCDetailResult
                {
                    PayConfigId = item.PayConfigId,
                    PayConfigName = item.PayConfigName,
                    PayType = item.PayType,

                    MonthlyAmount = roundedMonthly,
                    YearlyAmount = RoundMoney(roundedMonthly * 12m),

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

            // Gross may be absent from the Pay Group in CTC-input mode, but must be saved.
            EnsureGrossDetail(response.DetailList, grossPayConfigId, grossKey, response.Header.MonthlyGross);

            return response;
        }
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

                var tenantGross = GetTenantGrossComponent();
                var existingGross = model.DetailList.FirstOrDefault(x => x.PayConfigId == tenantGross.PayConfigId)
                    ?? model.DetailList.FirstOrDefault(x => string.Equals(
                        x.PayConfigName?.Trim(), tenantGross.PayConfigName, StringComparison.OrdinalIgnoreCase));

                EnsureGrossDetail(model.DetailList, tenantGross.PayConfigId, tenantGross.PayConfigName,
                    existingGross?.MonthlyAmount ?? model.MonthlyGross);
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
                    .FirstOrDefault(x => x.PayConfigId == tenantGross.PayConfigId);

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
