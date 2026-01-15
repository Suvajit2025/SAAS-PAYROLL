using Common.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace MendinePayroll.UI.Controllers
{

    public class bulkCTCCalculationController : Controller
    {
        private string tenantID
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

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AutoCalculateByEmployeeBulk(string employeeIds)
        {
            string user = "AUTO_ENGINE";

            if (string.IsNullOrWhiteSpace(employeeIds))
                return Json(new { status = 0, message = "No Employee IDs provided" });

            // Parse comma / newline separated IDs
            var empIdList = employeeIds
                .Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt64(x.Trim()))
                .Distinct()
                .ToList();

            int success = 0;
            List<string> failed = new List<string>();

            foreach (var empId in empIdList)
            {
                try
                {
                    // 🔁 reuse existing single-employee logic
                    AutoCalculateInternal(empId, user);
                    success++;
                }
                catch (Exception ex)
                {
                    failed.Add($"EmpId {empId} → {ex.Message}");
                }
            }

            return Json(new
            {
                status = 1,
                total = empIdList.Count,
                success,
                failed = failed.Count,
                errors = failed
            });
        }

        /* =====================================================
           ENTRY POINT (UI passes only EmployeeId)
        ===================================================== */
        //[HttpPost]
        //public ActionResult AutoCalculateByEmployee(long employeeId,string user)
        //{


        //    try
        //    {
        //        // 1️⃣ Get INPUT values (PayGroupId + PayConfigId + Value)
        //        var inputs = GetEmployeeSalaryInputs(employeeId);

        //        if (!inputs.Any())
        //            return Json(new { status = 0, message = "No input values found" });

        //        int payGroupId = inputs.First().PayGroupId;

        //        // 2️⃣ Load PayGroup salary template
        //        var template = GetSalaryTemplate(payGroupId);

        //        if (!template.Any())
        //            return Json(new { status = 0, message = "Salary template not found" });

        //        // 3️⃣ Apply INPUT values (AUTOMATED JS replacement)
        //        foreach (var input in inputs)
        //        {
        //            var comp = template.FirstOrDefault(x => x.PayConfigId == input.PayConfigId);
        //            if (comp != null)
        //                comp.MonthlyAmount = input.Value;
        //        }

        //        // 4️⃣ Run Salary Engine
        //        var result = CalculateSalary(template);

        //        // 5️⃣ Save Salary
        //        SaveSalary(employeeId, payGroupId, tenantID, result, user);

        //        return Json(new
        //        {
        //            status = 1,
        //            message = "Salary calculated and saved successfully"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            status = 0,
        //            message = ex.Message
        //        });
        //    }
        //}
        private void AutoCalculateInternal(long employeeId, string user)
        {
            // 1️⃣ Get INPUT values
            var inputs = GetEmployeeSalaryInputs(employeeId);
            if (!inputs.Any())
                throw new Exception("No input values found");

            int payGroupId = inputs.First().PayGroupId;

            // 2️⃣ Load template
            var template = GetSalaryTemplate(payGroupId);
            if (!template.Any())
                throw new Exception("Salary template not found");

            // 3️⃣ Apply INPUT values
            foreach (var input in inputs)
            {
                var comp = template.FirstOrDefault(x => x.PayConfigId == input.PayConfigId);
                if (comp != null)
                    comp.MonthlyAmount = input.Value;
            }

            // 4️⃣ Calculate
            var result = CalculateSalary(template);

            // 5️⃣ Save
            SaveSalary(employeeId, payGroupId, tenantID, result, user);
        }

        /* =====================================================
           1️⃣ GET EMPLOYEE INPUT VALUES (FROM SP)
        ===================================================== */
        private List<EmployeeInput> GetEmployeeSalaryInputs(long EmployeeId)
        {
            var list = new List<EmployeeInput>();

            DataTable dt = clsDatabase.fnDataTable(
                "PRC_GetEmployeeSalaryInputs",
                EmployeeId 
            );

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new EmployeeInput
                {
                    EmployeeId = Convert.ToInt64(row["EmployeeId"]),
                    PayGroupId = Convert.ToInt32(row["PayGroupId"]),
                    PayConfigId = Convert.ToInt64(row["PayConfigId"]),
                    Value = Convert.ToDecimal(row["Value"])
                });
            }

            return list;
        }

        /* =====================================================
           2️⃣ LOAD PAYGROUP SALARY TEMPLATE
        ===================================================== */
        private List<SalaryComponent> GetSalaryTemplate(int payGroupId)
        {
            var list = new List<SalaryComponent>();

            DataTable dt = clsDatabase.fnDataTable(
                "PRC_GetSalaryConfigByPayGroup",
                payGroupId,
                tenantID
            );

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SalaryComponent
                {
                    PayConfigId = Convert.ToInt64(row["PayConfigId"]),
                    PayConfigName = row["PayConfigName"].ToString(),
                    PayType = row["PayConfigType"].ToString(),

                    TimeBasis = row["TimeBasis"]?.ToString(),
                    LogicType = row["LogicType"]?.ToString(),
                    MappedColumn = row["MappedColumn"]?.ToString(),

                    CalculationFormula = row["CalculationFormula"]?.ToString(),
                    ManualRate = row["ManualRate"] == DBNull.Value ? 0 : Convert.ToDecimal(row["ManualRate"]),
                    ISPercentage = Convert.ToInt32(row["ISPercentage"]) == 1,

                    IsBasicComponent = Convert.ToInt32(row["IsBasicComponent"]) == 1,
                    IsGrossComponent = Convert.ToInt32(row["IsGrossComponent"]) == 1,
                    IsStatutory = Convert.ToInt32(row["IsStatutory"]) == 1,
                    StatutoryType = row["StatutoryType"]?.ToString(),

                    IsOther = Convert.ToInt32(row["IsOther"]) == 1,
                    OtherType = row["OtherType"]?.ToString(),

                    MaxLimit = row["MaxLimit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["MaxLimit"]),
                    RoundingType = row["RoundingType"]?.ToString(),
                    PTaxSlabsJson = row["PTAXSlabs"]?.ToString(),

                    MonthlyAmount = 0,
                    YearlyAmount = 0
                });

            }

            return list;
        }

        /* =====================================================
    3️⃣ SALARY ENGINE (JS → C# PORT) – FIXED
 ===================================================== */
        private SalaryResult CalculateSalary(List<SalaryComponent> cfg)
        {
            var calcs = cfg.ToDictionary(
                x => x.PayConfigName.ToUpper(),
                x => x.MonthlyAmount
            );

            /* -------------------------
               FIXED COMPONENTS
            ------------------------- */
            foreach (var x in cfg.Where(x => x.LogicType == "FIXED"))
            {
                if (x.MonthlyAmount == 0 && x.ManualRate > 0)
                    x.MonthlyAmount = x.ManualRate;

                calcs[x.PayConfigName.ToUpper()] = x.MonthlyAmount;
            }

            /* -------------------------
               FORMULA (3 PASS)
            ------------------------- */
            for (int pass = 0; pass < 3; pass++)
            {
                foreach (var x in cfg.Where(x => x.LogicType == "FORMULA" && !x.IsStatutory))
                {
                    decimal baseVal = EvaluateFormula(x.CalculationFormula, calcs);
                    decimal val = x.ISPercentage
                        ? baseVal * x.ManualRate / 100
                        : baseVal;

                    x.MonthlyAmount = ApplyRounding(val, x.RoundingType);
                    calcs[x.PayConfigName.ToUpper()] = x.MonthlyAmount;
                }
            }

            /* -------------------------
               BASIC & GROSS
            ------------------------- */
            decimal gross = cfg.First(x => x.IsGrossComponent).MonthlyAmount;
            decimal basic = cfg.First(x => x.IsBasicComponent).MonthlyAmount;

            /* -------------------------
               STATUTORY
            ------------------------- */
            foreach (var x in cfg.Where(x => x.IsStatutory))
            {
                switch (x.StatutoryType)
                {
                    case "PF_EE":
                    case "PF_ER":
                        x.MonthlyAmount = ApplyRounding(
                            Math.Min(basic, x.MaxLimit) * x.ManualRate / 100,
                            x.RoundingType
                        );
                        break;

                    case "ESIC_EE":
                    case "ESIC_ER":
                        x.MonthlyAmount = gross <= x.MaxLimit
                            ? ApplyRounding(gross * x.ManualRate / 100, x.RoundingType)
                            : 0;
                        break;

                    case "PTAX":
                        x.MonthlyAmount = GetPTax(gross, x.PTaxSlabsJson);
                        break;
                }

                calcs[x.PayConfigName.ToUpper()] = x.MonthlyAmount;
            }

            /* -------------------------
               TOTALS (CORRECTED)
            ------------------------- */
            decimal allowance = cfg
                .Where(x => x.PayType == "Allowances" && !x.IsBasicComponent)
                .Sum(x => x.MonthlyAmount);

            decimal deduction = cfg
                .Where(x => x.PayType == "Deduction")
                .Sum(x => x.MonthlyAmount);

            decimal contribution = cfg
                .Where(x => x.PayType == "CC")
                .Sum(x => x.MonthlyAmount);

            //decimal earnings = basic + allowance;
            decimal earnings = gross;
            /* -------------------------
               YEARLY VALUES
            ------------------------- */
            foreach (var x in cfg)
            {
                x.YearlyAmount = x.MonthlyAmount * 12;
            }

            return new SalaryResult
            {
                MonthlyGross = earnings,
                TotalAllowance = allowance,
                TotalDeduction = deduction,
                TotalContribution = contribution,
                NetPay = earnings - deduction,
                MonthlyCTC = earnings + contribution,
                AnnualCTC = (earnings + contribution) * 12,
                Details = cfg
            };
        }


        /* =====================================================
           HELPERS
        ===================================================== */
        private decimal EvaluateFormula(string formula, Dictionary<string, decimal> calcs)
        {
            if (string.IsNullOrWhiteSpace(formula)) return 0;

            string expr = formula.ToUpper();
            foreach (var k in calcs.OrderByDescending(x => x.Key.Length))
                expr = expr.Replace(k.Key, calcs[k.Key].ToString());

            expr = new string(expr.Where(c => "0123456789.+-*/()".Contains(c)).ToArray());
            return Convert.ToDecimal(new DataTable().Compute(expr, ""));
        }

        private decimal ApplyRounding(decimal val, string type)
        {
            switch ((type ?? "ROUND").ToUpper())
            {
                case "CEIL": return Math.Ceiling(val);
                case "FLOOR": return Math.Floor(val);
                default: return Math.Round(val);
            }
        }

        private decimal GetPTax(decimal gross, string json)
        {
            if (string.IsNullOrEmpty(json)) return 0;

            var slabs = JsonConvert.DeserializeObject<List<PTaxSlab>>(json);
            var slab = slabs.FirstOrDefault(s => gross >= s.MinSalary && gross <= s.MaxSalary);
            return slab?.PTAXAmount ?? 0;
        }

        /* =====================================================
           4️⃣ SAVE SALARY (EXISTING SP)
        ===================================================== */
        private void SaveSalary(long empId, int payGroupId, string tenantId, SalaryResult r, string user)
        {
            var payload = JsonConvert.SerializeObject(new
            {
                EmpSalaryConfigID = 0,
                EmployeeID = empId,
                PayGroupID = payGroupId,
                IsPF = true,
                IsESIC = true,
                IsPTAX = true,
                r.MonthlyGross,
                r.TotalAllowance,
                r.TotalDeduction,
                r.TotalContribution,
                r.NetPay,
                r.MonthlyCTC,
                r.AnnualCTC,
                DetailList = r.Details
            });

            clsDatabase.fnDBOperation(
                "PRC_Save_EmployeeSalaryConfig",
                tenantId,
                payload,
                user
            );
        }

        /* =====================================================
           MODELS (LOCAL)
        ===================================================== */
        private class EmployeeInput
        {
            public long EmployeeId { get; set; }
            public int PayGroupId { get; set; }
            public long PayConfigId { get; set; }
            public decimal Value { get; set; }
        }

        private class SalaryComponent
        {
            public long PayConfigId { get; set; }

            public string PayConfigName { get; set; }     // ✅ REQUIRED
            public string PayType { get; set; }           // ✅ REQUIRED

            public decimal MonthlyAmount { get; set; }
            public decimal YearlyAmount { get; set; }

            public string TimeBasis { get; set; }
            public string LogicType { get; set; }
            public string MappedColumn { get; set; }

            public string CalculationFormula { get; set; }
            public decimal ManualRate { get; set; }
            public bool ISPercentage { get; set; }

            public bool IsBasicComponent { get; set; }
            public bool IsGrossComponent { get; set; }
            public bool IsStatutory { get; set; }
            public string StatutoryType { get; set; }

            public bool IsOther { get; set; }
            public string OtherType { get; set; }

            public decimal MaxLimit { get; set; }
            public string RoundingType { get; set; }

            public string PTaxSlabsJson { get; set; }
        }

        private class SalaryResult
        {
            public decimal MonthlyGross { get; set; }
            public decimal TotalAllowance { get; set; }
            public decimal TotalDeduction { get; set; }
            public decimal TotalContribution { get; set; }
            public decimal NetPay { get; set; }
            public decimal MonthlyCTC { get; set; }
            public decimal AnnualCTC { get; set; }
            public List<SalaryComponent> Details { get; set; }
        }

        private class PTaxSlab
        {
            public decimal MinSalary { get; set; }
            public decimal MaxSalary { get; set; }
            public decimal PTAXAmount { get; set; }
        }
    }
}
