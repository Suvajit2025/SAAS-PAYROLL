using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.Utility;
using System.Data;
using MendinePayroll.Models;
using MendinePayroll.UI.Models;

namespace MendinePayroll.UI.BLL
{
    public class clsSalary
    {
        public static  List<SalaryRegisterlModel> Salary_Register(long IDEmp,String FromDate, String ToDate,String Month, long TotalDays,long EmpNO,long Year)
        {
            List<SalaryRegisterlModel> exceldata = new List<SalaryRegisterlModel>();

            DataTable DT = clsDatabase.fnDataTable("ExcelReport_GetAll", IDEmp, FromDate, ToDate,Month, TotalDays, EmpNO,Year);
            foreach (DataRow DR  in DT.Rows)
            {
                    exceldata.Add(new SalaryRegisterlModel
                    {
                        SLNO = clsHelper.fnConvert2Int(DT.Rows[0]["SLNO"]),
                        EMPLOYEECODE = DT.Rows[0]["EMPLOYEECODE"].ToString(),
                        NAME_OF_EMPLOYEES = DT.Rows[0]["NAME OF EMPLOYEES"].ToString(),
                        SITE = DT.Rows[0]["SITE"].ToString(),
                        W = DT.Rows[0]["W"].ToString(),
                        O = DT.Rows[0]["O"].ToString(),
                        Department= DT.Rows[0]["Department"].ToString(),
                        Designation = DT.Rows[0]["Designation"].ToString(),
                        E = DT.Rows[0]["E"].ToString(),
                        T_D = DT.Rows[0]["T/D"].ToString(),
                        BASIC =clsHelper.fnConvert2Double( DT.Rows[0]["BASIC"]),
                        STIPEND = clsHelper.fnConvert2Double(DT.Rows[0]["STIPEND"]),
                        WORKING_DAYS = clsHelper.fnConvert2Int(DT.Rows[0]["WORKING DAYS"]),
                        DAY_S_ABSENT = clsHelper.fnConvert2Double(DT.Rows[0]["DAY'S ABSENT"]),
                        DEDUCTION_AGAINST_ABSENSE = clsHelper.fnConvert2Double(DT.Rows[0]["DEDUCTION AGAINST ABSENSE"]),
                        E_L_ = clsHelper.fnConvert2Double(DT.Rows[0]["E.L."]),
                        PAYMENT_AGAINST_E_L_ = clsHelper.fnConvert2Double(DT.Rows[0]["PAYMENT AGAINST E.L."]),
                        GROSS_AMOUNT = clsHelper.fnConvert2Double(DT.Rows[0]["GROSS AMOUNT"]),
                        HRA = clsHelper.fnConvert2Double(DT.Rows[0]["HRA"]),
                        LODGING_ALLOWANCE = clsHelper.fnConvert2Double(DT.Rows[0]["LODGING ALLOWANCE"]),
                        TRAINING_ALLOWANCE_ACHIEVED = clsHelper.fnConvert2Double(DT.Rows[0]["TRAINING ALLOWANCE ACHIEVED"]),
                        MOBILE = clsHelper.fnConvert2Double(DT.Rows[0]["MOBILE"]),
                        BOOKS_AND_PERIODICALS_ALLOWANCE = clsHelper.fnConvert2Double(DT.Rows[0]["BOOKS AND PERIODICALS ALLOWANCE"]),
                        INTERNET_ALLOWANCE = clsHelper.fnConvert2Double(DT.Rows[0]["INTERNET ALLOWANCE"]),
                        CONINUOUS_ATTENDENCE_ALLOWANCE = clsHelper.fnConvert2Double(DT.Rows[0]["CONINUOUS ATTENDENCE ALLOWANCE"]),
                        Conveyence_Allowance = clsHelper.fnConvert2Double(DT.Rows[0]["Conveyence_Allowance"]),
                        PRODUCTION_BONUS = clsHelper.fnConvert2Double(DT.Rows[0]["PRODUCTION BONUS"]),
                        PRODUCTION_BONUS_ACHIEVED = clsHelper.fnConvert2Double(DT.Rows[0]["PRODUCTION BONUS ACHIEVED"]),
                        OVERTIME = clsHelper.fnConvert2Double(DT.Rows[0]["OVERTIME"]),
                        OVERTIME_AMOUNT = clsHelper.fnConvert2Double(DT.Rows[0]["OVERTIME AMOUNT"]),
                        MEDICAL_ALLOWANCES = clsHelper.fnConvert2Double(DT.Rows[0]["MEDICAL ALLOWANCES"]),
                        LTA = clsHelper.fnConvert2Double(DT.Rows[0]["LTA"]),
                        EDUCATION_ALLOWANCES = clsHelper.fnConvert2Double(DT.Rows[0]["EDUCATION ALLOWANCES"]),
                        TOTAL_ALLOWANCES = clsHelper.fnConvert2Double(DT.Rows[0]["TOTAL ALLOWANCES"]),
                        E_S_I = clsHelper.fnConvert2Double(DT.Rows[0]["E.S.I"]),
                        PF = clsHelper.fnConvert2Double(DT.Rows[0]["PF"]),
                        PTAX = clsHelper.fnConvert2Double(DT.Rows[0]["PTAX"]),
                        TOTAL_DEDUCTION = clsHelper.fnConvert2Double(DT.Rows[0]["TOTAL DEDUCTION"]),
                        OPENING_BALANCE = clsHelper.fnConvert2Decimal(DT.Rows[0]["OPENING BALANCE"]),
                        DEDUCTION = clsHelper.fnConvert2Double(DT.Rows[0]["DEDUCTION"]),
                        CLOSING_BALANCE = clsHelper.fnConvert2Decimal(DT.Rows[0]["CLOSING BALANCE"]),
                        NET_AMOUNT = clsHelper.fnConvert2Double(DT.Rows[0]["NET AMOUNT"]),
                        Attire_Allowance = clsHelper.fnConvert2Double(DT.Rows[0]["Attire Allowance"]),
                        PAID_DATE = DT.Rows[0]["PAID_DATE"].ToString()
                    }); 
                }
            return exceldata;
        }
        public static String SaveSalaryPLI(long EmployeeNo, Decimal PLIAmount, Decimal FinalAmount, String Month, int Year)
        {
            return clsDatabase.fnDBOperation("PRC_Salary_PLI_Save", EmployeeNo, PLIAmount, FinalAmount, Month, Year);
        }
        public static List<clsSalaryPLIInfo> LoadSalaryPLI(long EmployeeNo, String Month, int Year)
        {
            List<clsSalaryPLIInfo> mlist = new List<clsSalaryPLIInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_Salary_PLI_Load", EmployeeNo, Month, Year);
            foreach (DataRow DR in DT.Rows)
            {
                clsSalaryPLIInfo obj = new clsSalaryPLIInfo();
                obj.PLIAmount = clsHelper.fnConvert2Long(DR["PLIAmount"]);
                obj.FinalAmount = clsHelper.fnConvert2Long(DR["FinalAmount"]);
                mlist.Add(obj);
            }
            return mlist;
        }
        public static String Salary_Save(clsSalaryInfo obj)
        {
            return clsDatabase.fnDBOperation("PRC_Employee_Salary_Save",
                obj.EmpNo, obj.Basic, obj.HRA,
                obj.Attire, obj.Internet, obj.Mobile,
                obj.PF, obj.ESI, obj.PTax,
                obj.Education, obj.Medical, obj.LTA,
                obj.Gross, obj.Loan, obj.Continuous,
                obj.Overtime, obj.Lodging, obj.Production,
                obj.LeaveAmount, obj.TotalAllowance, obj.TotalDeduction,
                obj.NetPayable, 
                obj.Month, obj.Year, obj.Stipned,
                obj.Performance, obj.Special, obj.Arrear,
                obj.Books, obj.Tranning, obj.Convence,
                obj.Vehical, obj.Food, obj.TDS, obj.LWP, obj.Others, obj.User 
                );
        }
        public static List<IndividualSalryModel> Salary_Slip(int Empid, string Monthname,int month, int years)
        {
            String firstday = new DateTime(Convert.ToInt32(years), Convert.ToInt32(month), 1).ToString("dd/MMM/yyyy");
            String lastday = Convert.ToDateTime(firstday).AddMonths(1).ToString("dd/MMM/yyyy");
            List<IndividualSalryModel> data = new List<IndividualSalryModel>();
            DataTable DT = clsDatabase.fnDataTable("EmployeeSalary_ById", Empid, Monthname, years, firstday, lastday);
            foreach (DataRow DR in DT.Rows)
            {
                data.Add(new IndividualSalryModel
                {
                    Empid = clsHelper.fnConvert2Int(DR["Empid"]),
                    Empno = clsHelper.fnConvert2Int(DR["Empno"]),
                    EmployeeNo = DR["EmployeeNo"].ToString(),
                    Department = DR["Department"].ToString(),
                    Designation = DR["Designation"].ToString(),
                    EmployeeName = DR["EmployeeName"].ToString(),
                    Basic = clsHelper.fnConvert2Double(DR["Basic"]),
                    CompanyAddress = DR["Address"].ToString(),
                    Internet_Allowance = clsHelper.fnConvert2Double(DR["Internet Allowance"]),
                    PF = clsHelper.fnConvert2Double(DR["PF"]),
                    Education_Allowances = clsHelper.fnConvert2Double(DR["Education Allowances"]),
                    Medical_Allowances = clsHelper.fnConvert2Double(DR["Medical Allowances"]),
                    HRA = clsHelper.fnConvert2Double(DR["HRA"]),
                    Attire_Allowance = clsHelper.fnConvert2Double(DR["Attire Allowance"]),
                    P_Tax = clsHelper.fnConvert2Double(DR["P Tax"]),
                    ESI = clsHelper.fnConvert2Double(DR["ESI"]),
                    Leave_Amount = clsHelper.fnConvert2Double(DR["Leave Amount"]),
                    joining = Convert.ToDateTime(DR["joining"]),
                    Total_Allowances = clsHelper.fnConvert2Double(DR["Total Allowances"]),
                    Total_Deduction = clsHelper.fnConvert2Double(DR["Total Deduction"]),
                    Net_Payable_Amount = clsHelper.fnConvert2Double(DR["Net Payable Amount"]),
                    Month = DR["Month"].ToString(),
                    Year = clsHelper.fnConvert2Int(DR["Year"]),
                    empaccountno = DR["empaccountno"].ToString(),
                    empcode = DR["empcode"].ToString(),
                    empepfno = DR["empepfno"].ToString(),
                    empesicno = DR["empesicno"].ToString(),
                    emppancardno = DR["emppancardno"].ToString(),
                    empuanno = DR["empuanno"].ToString(),
                    Company = DR["Company"].ToString(),
                    AbsentDays = DR["AbsentDays"].ToString(),
                    EarnedLeaveDays = DR["EarnedLeave"].ToString(),
                    TotalLeaveDays = DR["TotalLeave"].ToString(),
                    LateLeave = DR["lateleave"].ToString(),
                    Gross_Amount = clsHelper.fnConvert2Double(DR["Gross Amount"]),
                    Continuous_Attendance_Allowance = clsHelper.fnConvert2Double(DR["Continuous Attendance Allowance"]),
                    ProductionBonus = clsHelper.fnConvert2Double(DR["ProductionBonus"]),
                    Arrears = clsHelper.fnConvert2Double(DR["Arrears"]),
                    Overtime_Allowance = clsHelper.fnConvert2Double(DR["Overtime Allowance"]),
                    PerformanceAllowance = clsHelper.fnConvert2Double(DR["PerformanceAllowance"]),
                    SpecialAllowance = clsHelper.fnConvert2Double(DR["SpecialAllowances"]),
                    LTA = clsHelper.fnConvert2Double(DR["LTA"]),
                    Stipend = clsHelper.fnConvert2Double(DR["Stipend"]),
                    Lodging_Allowance = clsHelper.fnConvert2Double(DR["Lodging Allowance"]),
                    Mobile_Allowance = clsHelper.fnConvert2Double(DR["Mobile Allowance"]),
                    Conveyence_Allowance = clsHelper.fnConvert2Double(DR["Conveyence Allowance"]),
                    Training__Allowance = clsHelper.fnConvert2Double(DR["Training  Allowance"]),
                    Vehical_Allownce = clsHelper.fnConvert2Double(DR["Vehical Allownce"]),
                    Books___Periodical = clsHelper.fnConvert2Double(DR["Books & Periodical"]),
                    Food_Allowance = clsHelper.fnConvert2Double(DR["Food Allowance"]),
                    DOB = Convert.ToDateTime(DR["empdob"]),
                    TDS = clsHelper.fnConvert2Double(DR["TDS"]),
                    Loan_Installment= clsHelper.fnConvert2Double(DR["Loan_Installment"]),
                    OtherDeduction=clsHelper.fnConvert2Double(DR["Other_Deduction"])
                });
            }
            return data;
        }

        public static List<clsEmployeeInfo> EmployeeList(String CompanyCode="")
        {
            List<clsEmployeeInfo> mlist = new List<clsEmployeeInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_Employee_List", CompanyCode);
            foreach (DataRow DR in DT.Rows)
            {
                mlist.Add(new clsEmployeeInfo()
                {
                    IDEmployee = clsHelper.fnConvert2Int(DR["Empid"]),
                    EmployeeNo = DR["EmpNo"].ToString(),
                    EmployeeName = DR["Employee"].ToString()
                });
            }
            return mlist;

        }

        public static List<clsDepartmentInfo> Department_List()
        {
            List<clsDepartmentInfo> mlist = new List<clsDepartmentInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_HRMS_Department_List");
            foreach (DataRow DR in DT.Rows)
            {
                mlist.Add(new clsDepartmentInfo
                {
                    IDDepartment = clsHelper.fnConvert2Long(DR["DepartmentId"]),
                    Name = DR["Department"].ToString()
                });
            }
            return mlist;
        }

        public static List<clsEmployeeInfo> EmployeeListCompanyWise(String CompanyCode )
        {
            List<clsEmployeeInfo> mlist = new List<clsEmployeeInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_Employee_List_Company_Wise", CompanyCode);
            foreach (DataRow DR in DT.Rows)
            {
                mlist.Add(new clsEmployeeInfo()
                {
                    IDEmployee = clsHelper.fnConvert2Int(DR["Empid"]),
                    EmployeeNo = DR["EmpNo"].ToString(),
                    EmployeeName = DR["Employee"].ToString()
                });
            }
            return mlist;

        }
        public static List<clsEmployeeInfo> LoanEmployeeList()
        {
            List<clsEmployeeInfo> mlist = new List<clsEmployeeInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_Loan_Employee_List");
            foreach (DataRow DR in DT.Rows)
            {
                mlist.Add(new clsEmployeeInfo()
                {
                    IDEmployee = clsHelper.fnConvert2Int(DR["Empid"]),
                    EmployeeNo = DR["EmpNo"].ToString(),
                    EmployeeName = DR["Employee"].ToString()
                });
            }
            return mlist;

        }
        public static List<clsEmployeeSalaryRegisterInfo> SalaryRegister(String EmployeeNo, String SDate, String EDate)
        {
            List<clsEmployeeSalaryRegisterInfo> mlist = new List<clsEmployeeSalaryRegisterInfo>();
            DataTable DT = clsDatabase.fnDataTable("PRC_Salary_Register", EmployeeNo, SDate, EDate);
            foreach (DataRow DR in DT.Rows)
            {
                mlist.Add(new clsEmployeeSalaryRegisterInfo()
                {
                    EmployeeName = DR["Employee"].ToString(),
                    SalaryDate = DR["Paid_Date"].ToString(),
                    SalaryMonth = DR["Month"].ToString(),
                    SalaryYear = clsHelper.fnConvert2Int(DR["Year"]),
                    Basic = clsHelper.fnConvert2Decimal(DR["Basic"]),
                    HRA = clsHelper.fnConvert2Decimal(DR["HRA"]),
                    Attir = clsHelper.fnConvert2Decimal(DR["Attire"]),
                    Internet = clsHelper.fnConvert2Decimal(DR["Internet"]),
                    Mobile = clsHelper.fnConvert2Decimal(DR["Mobile"]),
                    Medical = clsHelper.fnConvert2Decimal(DR["Medical"]),
                    Education = clsHelper.fnConvert2Decimal(DR["Education"]),
                    LTA = clsHelper.fnConvert2Decimal(DR["LTA"]),
                    ContinousAttendance = clsHelper.fnConvert2Decimal(DR["Continous"]),
                    Overtime = clsHelper.fnConvert2Decimal(DR["Overtime"]),
                    Lodging = clsHelper.fnConvert2Decimal(DR["Lodging"]),
                    Fooding = clsHelper.fnConvert2Decimal(DR["Fooding"]),
                    Trainning = clsHelper.fnConvert2Decimal(DR["Trainning"]),
                    BooksPeriodicals = clsHelper.fnConvert2Decimal(DR["Books"]),
                    Convenyence = clsHelper.fnConvert2Decimal(DR["Convenyence"]),
                    Vehical = clsHelper.fnConvert2Decimal(DR["Vehical"]),
                    TDS = clsHelper.fnConvert2Decimal(DR["TDS"]),
                    Stipned = clsHelper.fnConvert2Decimal(DR["Stipend"]),
                    Arrear = clsHelper.fnConvert2Decimal(DR["Arrear"]),
                    Production = clsHelper.fnConvert2Decimal(DR["Production"]),
                    PF = clsHelper.fnConvert2Decimal(DR["PF"]),
                    ESI = clsHelper.fnConvert2Decimal(DR["ESI"]),
                    PTax = clsHelper.fnConvert2Decimal(DR["PTax"]),
                    Loan = clsHelper.fnConvert2Decimal(DR["Loan"])
                });
            }
            return mlist;

        }

        public static DataTable  SalaryRegisterNew(long IDEmployee, String StartMonth, int StartYear, String EndMonth, int EndYear)
        {
            return  clsDatabase.fnDataTable("PRC_Salary_Register", IDEmployee, StartMonth, StartYear, EndMonth, EndYear);
        }

        public static DataTable GetBonusList(long IDEmployee,long IDDepartment,int Year)
        {
            return clsDatabase.fnDataTable("PRC_Get_Bonus_List", IDEmployee, IDDepartment, Year);
        }
        public static DataTable SalaryYears()
        {
            return clsDatabase.fnDataTable("PRC_Salary_Year_List");
        }


        public static DataTable FinancialYears()
        {
            return clsDatabase.fnDataTable("PRC_Financial_Year_List");
        }
        public static List<clsYearInfo> SalaryYear()
        {
            List<clsYearInfo> mlist = new List<clsYearInfo>();
            DataTable dt= clsDatabase.fnDataTable("PRC_Salary_Year_List");
            foreach (DataRow DR in dt.Rows )
            {
                clsYearInfo obj = new clsYearInfo();
                obj.Value = clsHelper.fnConvert2Long(DR["Year"]);
                obj.Name = DR["Year"].ToString();
                obj.Selected = DateTime.Today.Year == clsHelper.fnConvert2Int( DR["Year"]) ? true : false;
                mlist.Add(obj);
            }
            return mlist;
        }
        public static List<clsYearInfo> SalaryMonth()
        {
            List<clsYearInfo> mlist = new List<clsYearInfo>();
            DataTable dt = clsDatabase.fnDataTable("PRC_Salary_Year_List");
            foreach (DataRow DR in dt.Rows)
            {
                clsYearInfo obj = new clsYearInfo();
                obj.Value = clsHelper.fnConvert2Long(DR["Year"]);
                obj.Name = DR["Year"].ToString();
                obj.Selected = DateTime.Today.Year == clsHelper.fnConvert2Int(DR["Year"]) ? true : false;
                mlist.Add(obj);
            }
            return mlist;
        }

        public static DataTable Payroll_Access_Log()
        {
            //return clsDatabase.fnDataTable("PRC_User_Login_Log_Report", IDUser, Type);
            return clsDatabase.fnDataTable("PRC_Report_Payroll_Access_Log");
        }
        public static DataTable  TDSAnnexure1(long IDEmployee,String CompanyCode, String Month, int Year)
        {
            return clsDatabase.fnDataTable("Payroll_Annexure1_Register", Month, Year, IDEmployee, CompanyCode);
        }
        public static List<clsCompanyInfo> CompanyList()
        {
            List<clsCompanyInfo> mlist = new List<clsCompanyInfo>();
            DataTable dt = clsDatabase.fnDataTable("PRC_Company_List");
            foreach(DataRow dr in dt.Rows)
            {
                clsCompanyInfo obj = new clsCompanyInfo();
                obj.Code = dr["Code"].ToString();
                obj.Name = dr["Name"].ToString();
                mlist.Add(obj);
            }
            return mlist;
        }

        public static List<clsForm16Info> Form16List(int Year, String Companycode,int EmployeeNo)
        {
            List<clsForm16Info> mlist = new List<clsForm16Info>();
            DataTable dt = clsDatabase.fnDataTable("PRC_Form16_List", Year, Companycode, EmployeeNo);
            foreach (DataRow dr in dt.Rows)
            {
                clsForm16Info obj = new clsForm16Info();
                obj.IDForm = clsHelper.fnConvert2Long(dr["IDForm"]);
                obj.Employee.IDEmployee = clsHelper.fnConvert2Long(dr["IDEmp"]);
                obj.Employee.EmployeeNo = dr["Empno"].ToString();
                obj.Employee.EmployeeName = dr["EmpName"].ToString();
                obj.Employee.PAN = dr["PAN"].ToString();
                obj.StatusA = dr["StatusA"].ToString();
                obj.FileNameA= dr["FileNameA"].ToString();
                obj.FilePathA = dr["FilePathA"].ToString();
                obj.StatusB = dr["StatusB"].ToString();
                obj.FileNameB = dr["FileNameB"].ToString();
                obj.FilePathB = dr["FilePathB"].ToString();
                mlist.Add(obj);
            }
            return mlist;
        }
        public static String Form16Upload(long IDForm, int Year, String Companycode, String Employeeno, String FilePathA, String FileNameA, String FilePathB, String FileNameB)
        {
            string UserName = HttpContext.Current.Session["username"].ToString();
            return clsDatabase.fnDBOperation("PRC_Form16_Upload", IDForm, Employeeno, Companycode, Year, FilePathA, FileNameA, FilePathB, FileNameB, UserName);
        }
    }
    public class SalaryRegisterlModel
    {

        public int Empid { get; set; }
        public DateTime? Prefixfromdate { get; set; }
        public DateTime? suffixtodate { get; set; }
        public string Month { get; set; }
        public int TotalDays { get; set; }
        public int empno { get; set; }
        public int year { get; set; }
        public int SLNO { get; set; }
        public string EMPLOYEECODE { get; set; }
        public string NAME_OF_EMPLOYEES { get; set; }
        public string SITE { get; set; }
        public string W { get; set; }
        public string O { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string E { get; set; }
        public string T_D { get; set; }
        public double? BASIC { get; set; }
        public double? STIPEND { get; set; }
        public int? WORKING_DAYS { get; set; }
        public double? DAY_S_ABSENT { get; set; }
        public double? DEDUCTION_AGAINST_ABSENSE { get; set; }
        public double? E_L_ { get; set; }
        public double? PAYMENT_AGAINST_E_L_ { get; set; }
        public double? GROSS_AMOUNT { get; set; }
        public double? HRA { get; set; }
        public double? LODGING_ALLOWANCE { get; set; }
        public double? Attire_Allowance { get; set; }
        public double? TRAINING_ALLOWANCE_ACHIEVED { get; set; }
        public double? MOBILE { get; set; }
        public double? BOOKS_AND_PERIODICALS_ALLOWANCE { get; set; }
        public double? INTERNET_ALLOWANCE { get; set; }
        public double? CONINUOUS_ATTENDENCE_ALLOWANCE { get; set; }
        public double? Conveyence_Allowance { get; set; }
        
        public double? PRODUCTION_BONUS { get; set; }
        public double? PRODUCTION_BONUS_ACHIEVED { get; set; }
        public double? OVERTIME { get; set; }
        public double? OVERTIME_AMOUNT { get; set; }
        public double? MEDICAL_ALLOWANCES { get; set; }
        public double? LTA { get; set; }
        public double? EDUCATION_ALLOWANCES { get; set; }
        public double? TOTAL_ALLOWANCES { get; set; }
        public double? PF { get; set; }
        public double? PTAX { get; set; }
        public double? E_S_I { get; set; }
        public double? TOTAL_DEDUCTION { get; set; }
        public decimal? OPENING_BALANCE { get; set; }
        public double? DEDUCTION { get; set; }
        public decimal? CLOSING_BALANCE { get; set; }
        public double? NET_AMOUNT { get; set; }
        public String PAID_DATE { get; set; }
    }
    public class clsSalaryPLIInfo
    {
        public long PLIAmount { get; set; } = 0;
        public long FinalAmount { get; set; } = 0;
    }
    public class clsSalaryInfo
    {
        public long EmpNo { get; set; } = 0;
        public Decimal Basic { get; set; } = 0;
        public Decimal HRA { get; set; } = 0;
        public Decimal Attire { get; set; } = 0;
        public Decimal Internet { get; set; } = 0;
        public Decimal Mobile { get; set; } = 0;
        public Decimal PF { get; set; } = 0;
        public Decimal ESI { get; set; } = 0;
        public Decimal PTax { get; set; } = 0;
        public Decimal Education { get; set; } = 0;
        public Decimal Medical { get; set; } = 0;
        public Decimal LTA { get; set; } = 0;
        public Decimal Gross { get; set; } = 0;
        public Decimal Loan { get; set; } = 0;
        public Decimal Continuous { get; set; } = 0;
        public Decimal Overtime { get; set; } = 0;
        public Decimal Lodging { get; set; } = 0;
        public Decimal Production { get; set; } = 0;
        public Decimal LeaveAmount { get; set; } = 0;
        public Decimal TotalAllowance { get; set; } = 0;
        public Decimal TotalDeduction { get; set; } = 0;
        public Decimal NetPayable { get; set; } = 0;
        public Decimal PLIAmount { get; set; } = 0;
        public Decimal FinalAmount { get; set; } = 0;
        public String Month { get; set; } = "";
        public long Year { get; set; } = 0;
        public Decimal Stipned { get; set; } = 0;
        public Decimal Performance { get; set; } = 0;
        public Decimal Special { get; set; } = 0;
        public Decimal Arrear { get; set; } = 0;
        public Decimal Books { get; set; } = 0;
        public Decimal Tranning { get; set; } = 0;
        public Decimal Convence { get; set; } = 0;
        public Decimal Vehical { get; set; } = 0;
        public Decimal Food { get; set; } = 0;
        public Decimal TDS { get; set; } = 0;
        public Decimal LWP { get; set; } = 0;
        public Decimal Others { get; set; } = 0;
        public String User { get; set; } = "";
    }

    public class EmployeeSalarySlip
    {
        public string EmployeeName { get; set; }
        public Nullable<System.DateTime> joining { get; set; }
        public Nullable<System.DateTime> empdob { get; set; }
        public string empcode { get; set; }
        public string postname { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int Rowid { get; set; }
        public Nullable<int> Empid { get; set; }
        public Nullable<int> Empno { get; set; }
        public Nullable<int> PayGroupId { get; set; }
        public Nullable<double> Basic { get; set; }
        public Nullable<double> HRA { get; set; }
        public Nullable<double> Attire_Allowance { get; set; }
        public Nullable<double> Internet_Allowance { get; set; }
        public Nullable<double> Mobile_Allowance { get; set; }
        public Nullable<double> PF { get; set; }
        public Nullable<double> ESI { get; set; }
        public Nullable<double> P_Tax { get; set; }
        public Nullable<double> Education_Allowances { get; set; }
        public Nullable<double> Medical_Allowances { get; set; }
        public Nullable<double> LTA { get; set; }
        public Nullable<double> Gross_Amount { get; set; }
        public Nullable<double> Loan_Installment { get; set; }
        public Nullable<double> Continuous_Attendance_Allowance { get; set; }
        public Nullable<double> Overtime_Allowance { get; set; }
        public Nullable<double> Lodging_Allowance { get; set; }
        public Nullable<double> ProductionBonus { get; set; }
        public Nullable<double> Leave_Amount { get; set; }
        public Nullable<double> Total_Allowances { get; set; }
        public Nullable<double> Total_Deduction { get; set; }
        public Nullable<double> Net_Payable_Amount { get; set; }
        public string Month { get; set; }
        public Nullable<int> Year { get; set; }
        public Nullable<double> Stipend { get; set; }
        public Nullable<double> PerformanceAllowance { get; set; }
        public Nullable<double> SpecialAllowances { get; set; }
        public Nullable<double> Arrears { get; set; }
        public Nullable<bool> IsPaid { get; set; }
        public Nullable<double> Books___Periodical { get; set; }
        public Nullable<double> Training__Allowance { get; set; }
        public Nullable<double> Conveyence_Allowance { get; set; }
        public Nullable<double> Vehical_Allownce { get; set; }
        public Nullable<double> Food_Allowance { get; set; }
        public Nullable<double> TDS { get; set; }
        public Nullable<System.DateTime> Paid_Date { get; set; }
        public string empaccountno { get; set; }
        public string empepfno { get; set; }
        public string empesicno { get; set; }
        public string empuanno { get; set; }
        public string emppancardno { get; set; }
        public string Company { get; set; }
        public string Address { get; set; }
        public Nullable<double> AbsentDays { get; set; }
        public Nullable<double> EarnedLeave { get; set; }
        public Nullable<double> TotalLeave { get; set; }
        public Nullable<double> lateleave { get; set; }
    }

  }