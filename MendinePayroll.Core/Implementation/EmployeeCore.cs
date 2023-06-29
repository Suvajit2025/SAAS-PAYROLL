using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MendinePayroll.Models;
using MendinePayroll.Data;
using MendinePayroll.Core.Interface;

namespace MendinePayroll.Core.Implementation
{
    public class EmployeeCore: IEmployeeCore
    {
        esspEntities esspEntities = new esspEntities();
        EmployeeListModel empbasicModel = new EmployeeListModel();
        public List<EmployeeListModel> Empllist()
        {
            List<EmployeeListModel> listmodel = new List<EmployeeListModel>();
            try
            {
                var empListsss = esspEntities.EmployeeList_Get().ToList();

                List<EmployeeList_Get_Result> empList = esspEntities.EmployeeList_Get().ToList();


                listmodel = empList.Select(X =>
                {
                    return new EmployeeListModel
                    {
                        empid=X.empid,
                        empno=X.empno,
                        EmployeeName=X.EmployeeName,
                        empcode=X.empcode,
                        postname=X.postname,
                        Designation = X.Designation,
                        Department=X.Department,
                        PayGroupName = X.PayGroupName,
                        PayGroupID = X.PayGroupID==null?0:Convert.ToInt32(X.PayGroupID)
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public List<EmployeeListModel> GetEmplistById(EmployeeListModel employeeListModel)
        {
            List<EmployeeListModel> Emplist = new List<EmployeeListModel>(); 
            try
            {
                List<EmployeeList_GetbyID_Result> listmodel = esspEntities.EmployeeList_GetbyID(employeeListModel.empid).ToList();
                Emplist = listmodel.Select(X =>
                {
                    return new EmployeeListModel
                    {
                        empid = X.empid,
                        empno = X.empno,
                        EmployeeName = X.EmployeeName,
                        empcode = X.empcode,
                        postname = X.postname,
                        Designation = X.Designation,
                        Department = X.Department,
                        PayGroupName = X.PayGroupName,
                        PayGroupID = Convert.ToInt32(X.PayGroupID),
                        SalaryConfigureID= Convert.ToInt32(X.SalaryConfigureID),
                        empbankid=X.empbankid,
                        empbankbranchifsccode=X.empbankbranchifsccode,
                        empbankbranchname=X.empbankbranchname,
                        bankname=X.bankname,
                        empaccountno=X.empaccountno,
                        empuanno=X.empuanno,
                        empemail=X.empemail,
                        Company=X.Company,
                        empepfno=X.empepfno,
                        empesicno=X.empesicno,
                        emppancardno=X.emppancardno,
                        Joining=X.joining
                        //Basic=X.Basic,
                        //GrossAmount=X.GrossAmount,
                        //InternetAllowance=X.InternetAllowance,
                        //MobileAllowance=X.MobileAllowance,
                        //ContinuousAllowance=X.ContinuousAllowance,
                        //OverTimeHours=X.OverTimeHours,
                        //ProductionBonus=X.ProductionBonus,
                        //TargetAchieved=X.TargetAchieved
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Emplist;

        }
        public List<EmployeeListModel> GetEmplist(EmployeeListModel employeeListModel)
        {
            List<EmployeeListModel> listmodel = new List<EmployeeListModel>();
            try
            {              
                List<EmployeeList_Get_List_Result> empList = esspEntities.EmployeeList_Get_List(employeeListModel.PageNumber,employeeListModel.Rowofpage, employeeListModel.SearchVal).ToList();


                listmodel = empList.Select(X =>
                {
                    return new EmployeeListModel
                    {
                        empid = X.empid,
                        empno = X.empno,
                        EmployeeName = X.EmployeeName,
                        empcode = X.empcode,
                        postname = X.postname,
                        Designation = X.Designation,
                        Department = X.Department,
                        PayGroupName = X.PayGroupName,
                        PayGroupID = X.PayGroupID == null ? 0 : Convert.ToInt32(X.PayGroupID)
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public int? GetEmplist_Count(EmployeeListModel employeeListModel)
        {
            int? result = 0;
            try
            {
               
                var Total = esspEntities.EmployeeList_Get_Count(employeeListModel.SearchVal).ToList();
                 result = Total[0];
              
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;

        }

        public List<EmployeeLeaveModel> GetEmpleave(EmployeeLeaveModel employeeLeaveModel)
        {
            List<EmployeeLeaveModel> Emplist = new List<EmployeeLeaveModel>();
            try
            {
                List<CalculateTotalLeave_Result> listmodel = esspEntities.CalculateTotalLeave(employeeLeaveModel.Empno,employeeLeaveModel.prefixfromdate,
                    employeeLeaveModel.suffixtodate,employeeLeaveModel.month,employeeLeaveModel.year).ToList();
                Emplist = listmodel.Select(X =>
                {
                    return new EmployeeLeaveModel
                    {
                        applicationhdrid=X.applicationhdrid,
                        noofdays=X.noofdays,
                        prefixfromdate=Convert.ToDateTime(X.prefixfromdate),
                        suffixtodate= Convert.ToDateTime(X.suffixtodate),
                        enddayhalfind=X.enddayhalfind,
                        Applicationstatus=X.Applicationstatus,
                        lateleave = X.lateleave
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Emplist;

        }


        public List<EmpbasicModel> GEetEmployeeDetails(EmpbasicModel empbasicModel)
        {
            List<EmpbasicModel> listmodel = new List<EmpbasicModel>();
            try
            {
                List<GetEmmployeeDetails_ById_Result> empList = esspEntities.GetEmmployeeDetails_ById(empbasicModel.empid).ToList();
                listmodel = empList.Select(X =>
                {
                    return new EmpbasicModel
                    {
                        empid = X.empid,
                        empinitialname=X.empinitialname,
                        empfirstname=X.empfirstname,
                        empmiddlename=X.empmiddlename,
                        emplastname=X.emplastname,
                        empdob=X.empdob,
                        empgender=X.empgender,
                        companycode=X.companycode,
                        empcategory=X.empcategory,
                        empno=X.empno,
                        empcode=X.empcode,
                        adharno=X.adharno,
                        adahrcardimageid=X.adahrcardimageid,
                        emppersonalcontactno=X.emppersonalcontactno,
                        joining= X.joining,
                        empdept=X.empdept,
                        emppost=X.emppost,
                        empdesignation=X.empdesignation,
                        empreportto=X.empreportto,
                        empreporttoperson=X.empreporttoperson,
                        empstatus=X.empstatus,
                        closeingdate= X.closeingdate,
                        emppersonalemail=X.emppersonalemail,
                        empofficialcontactno=X.empofficialcontactno,
                        empemail=X.empemail,
                        empaddon=X.empaddon,
                        empaddby=X.empaddby,
                        lastupdatedon=X.lastupdatedon,
                        lastupdatedby=X.lastupdatedby,
                        postingstateid=X.postingstateid,
                        postname=X.postname,
                        Department=X.Department,
                        Designation=X.DesignationName,
                        Empdofb=X.Empdofb,
                        Joiningdate=X.Joiningdate,
                        Closeingdateof=X.Closeingdateof
   
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public List<EmployeeSalaryConfigModel> GEetEmployeeSalaryDetails(EmployeeSalaryConfigModel employeeSalaryConfigModel)
        {
            List<EmployeeSalaryConfigModel> listmodel = new List<EmployeeSalaryConfigModel>();
            try
            {
                List<EmployeeSalaryConfig_GetByID_Result> empList = esspEntities.EmployeeSalaryConfig_GetByID(employeeSalaryConfigModel.EmpId).ToList();
                listmodel = empList.Select(X =>
                {
                    return new EmployeeSalaryConfigModel
                    {
                        Id=X.Id,
                        EmpId=X.EmpId,
                        PayGroupId=X.PayGroupId,
                        PayConfigID=X.PayConfigID,
                        PayConfigName=X.PayConfigName,
                        PayConfigType=X.PayConfigType,
                        Values=X.Values,
                        IsPTaxActive=X.IsPTaxActive,
                        ContinuousAllowance=X.ContinuousAttendanceAllowance,
                        ProductionBonus=X.ProductionBonus,
                        SalaryConfigureType=X.SalaryConfigureType,
                        SalaryConfigureID=X.SalaryConfigureID,
                        IsCalculative=X.IScalculative
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public int SaveEmpSalary(EmployeeSalaryConfigModel employeeSalaryConfigModel)
        {
            try
            {
                List<EmployeeSalaryConfig_Save_Result> Savelist= esspEntities.EmployeeSalaryConfig_Save(employeeSalaryConfigModel.Id, employeeSalaryConfigModel.EmpId, employeeSalaryConfigModel.PayGroupId,employeeSalaryConfigModel.IsPTaxActive,employeeSalaryConfigModel.ContinuousAllowance,employeeSalaryConfigModel.ProductionBonus).ToList();
                
                if (Savelist != null)
                {
                    return Convert.ToInt32(Savelist[0].ReturnStatus);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int SaveEmpLoan(EmployeeLoanModel employeeLoanModel)
        {
            try
            {
                int SaveEmpLoan = esspEntities.EmployeeLoan_save(employeeLoanModel.LoanId, employeeLoanModel.Empid, employeeLoanModel.LoanAmount, employeeLoanModel.MonthlyInstallment, employeeLoanModel.IsStart);
                if (SaveEmpLoan != 0)
                {
                    
                    return SaveEmpLoan;
                }
                else
                {
                    return 0;
                }

            }
            catch(Exception ex)
            {
                return 0;
            }
        }


        public List<EmployeeLoanModel> GEetEmployeeLoanDetails(EmployeeLoanModel employeeLoanModel)
        {
            List<EmployeeLoanModel> listmodel = new List<EmployeeLoanModel>();
            try
            {
                List<EmployeeLoan_GetByID_Result> empList = esspEntities.EmployeeLoan_GetByID(employeeLoanModel.Empid).ToList();
                listmodel = empList.Select(X =>
                {
                    return new EmployeeLoanModel
                    {
                        LoanId=X.LoanId,
                        Empid=X.Empid,
                        LoanAmount=X.LoanAmount,
                        MonthlyInstallment=X.MonthlyInstallment,
                        IsStart =X.IsStart,
                        LoanDate =X.LoanDate,
                        ClosingBalance=X.ClosingBalance,
                        OpeningBalance=X.OpeningBalance,
                        LoanTransactionId=X.LoanTransactionId
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public int SaveLoanTransaction(LoanTransactionModel loanTransactionModel)
        {
            try
            {
                int SaveEmpLoan = esspEntities.LoanTransaction_save(loanTransactionModel.LoanId, loanTransactionModel.Empid, loanTransactionModel.InstallmentAmount, loanTransactionModel.OpeningBalance, loanTransactionModel.ClosingBalance);
                if (SaveEmpLoan != 0)
                {
                    return SaveEmpLoan;
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public List<LoanTransactionModel> GetEmployeeLoanTDetails(LoanTransactionModel loanTransactionModel)
        {
            List<LoanTransactionModel> listmodel = new List<LoanTransactionModel>();
            try
            {
                List<LoanTransaction_GetByID_Result> empList = esspEntities.LoanTransaction_GetByID(loanTransactionModel.Empid).ToList();
                listmodel = empList.Select(X =>
                {
                    return new LoanTransactionModel
                    {
                        LoanId=X.LoanId,
                        Empid=X.Empid,
                        DateOfInStallment=X.DateOfInStallment,
                        InstallmentAmount=X.InstallmentAmount,
                        ClosingBalance = X.ClosingBalance,
                        OpeningBalance = X.OpeningBalance,
                        LoanTransactionId = X.LoanTransactionId
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }
        public List<EmployeeSalaryModel> EmployeeDuplicateSalarycheck(EmployeeSalaryModel employeeSalaryModel)
        {
            List<EmployeeSalaryModel> listmodel = new List<EmployeeSalaryModel>();
            try
            {
                List<EmployeeSalary_duplicateCheck_Result> empList = esspEntities.EmployeeSalary_duplicateCheck(employeeSalaryModel.Empid, employeeSalaryModel.Month, employeeSalaryModel.Year).ToList();
                listmodel = empList.Select(X =>
                {
                    return new EmployeeSalaryModel
                    {
                        Empid=X.Empid
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public int SaveallempSalary(EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {

                //int Saveempsal = 1;
                int Saveempsal = esspEntities.EmployeeSalary_save(employeeSalaryModel.Empid, employeeSalaryModel.Empno, employeeSalaryModel.PayGroupId, employeeSalaryModel.Basic, employeeSalaryModel.HRA,
                    employeeSalaryModel.Attire_Allowance, employeeSalaryModel.Internet_Allowance, employeeSalaryModel.Mobile_Allowance,
                    employeeSalaryModel.PF, employeeSalaryModel.ESI, employeeSalaryModel.P_Tax, employeeSalaryModel.Education_Allowances
                    , employeeSalaryModel.Medical_Allowances, employeeSalaryModel.LTA, employeeSalaryModel.Gross_Amount, employeeSalaryModel.Loan_Installment,
                    employeeSalaryModel.Continuous_Attendance_Allowance, employeeSalaryModel.Overtime_Allowance, employeeSalaryModel.Lodging_Allowance,
                    employeeSalaryModel.ProductionBonus, employeeSalaryModel.Leave_Amount, employeeSalaryModel.Total_Allowances, employeeSalaryModel.Total_Deduction, employeeSalaryModel.Net_Payable_Amount,
                    employeeSalaryModel.Month, employeeSalaryModel.Year, employeeSalaryModel.STIPEND, employeeSalaryModel.Arrears, employeeSalaryModel.PerformanceAllowance,
                    employeeSalaryModel.SpecialAllowance, employeeSalaryModel.Books___Periodical, employeeSalaryModel.Conveyence_Allowance,
                    employeeSalaryModel.Training__Allowance, employeeSalaryModel.Vehical_Allownce, employeeSalaryModel.Food_Allowance, employeeSalaryModel.TDS);

                if (Saveempsal != 0)
                {
                    return Saveempsal;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int EmployeeTotalinsertedinamonth(EmployeeSalaryModel employeeSalaryModel)
        {
            try
            {
                List<EmployeeSalary_TotalnoByMonth_Result> Saveempsal = esspEntities.EmployeeSalary_TotalnoByMonth(employeeSalaryModel.Month, employeeSalaryModel.Year).ToList();

                if (Saveempsal != null)
                {
                    int saveempsal = Convert.ToInt32(Saveempsal[0].ReturnStatus);
                    return saveempsal;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public List<EmployeeSalaryModel> AllEmployeeSalaryview(EmployeeSalaryModel employeeSalaryModel)
        {
            List<EmployeeSalaryModel> listmodel = new List<EmployeeSalaryModel>();
            try
            {
                List<Employeesalary_viewAllByMonth_Result> empList = esspEntities.Employeesalary_viewAllByMonth(employeeSalaryModel.Month, employeeSalaryModel.Year, employeeSalaryModel.Empid,
                    employeeSalaryModel.Status).ToList();
                
                listmodel = empList.Select(X =>
                {
                    return new EmployeeSalaryModel
                    {
                        Empid=X.empid,
                        Empno=X.empno,
                        EmployeeName=X.EmployeeName,
                        PayGroupId=X.PayGroupID,
                        PayGroupeName=X.PayGroupName,
                        Basic=X.Basic,
                        Gross_Amount=X.Gross_Amount,
                        Total_Allowances=X.Total_Allowances,
                        Total_Deduction=X.Total_Deduction,
                        Net_Payable_Amount=X.Net_Payable_Amount,
                        IsPaid=X.IsPaid
                    };
                }).ToList();
            }
           
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public List<EmployeeSalaryModel> EmployeeSalaryview_ByPageWise(EmployeeSalaryModel employeeSalaryModel)
        {
            List<EmployeeSalaryModel> listmodel = new List<EmployeeSalaryModel>();
            try
            {
                List<Employeesalary_view_PageWise_ByMonth_Result> empList = esspEntities.Employeesalary_view_PageWise_ByMonth(employeeSalaryModel.Month, employeeSalaryModel.Year, employeeSalaryModel.Empid,
                    employeeSalaryModel.Status, employeeSalaryModel.PageNumber, employeeSalaryModel.Rowofpage).ToList();

                listmodel = empList.Select(X =>
                {
                    return new EmployeeSalaryModel
                    {
                        Empid = X.empid,
                        Empno = X.empno,
                        EmployeeName = X.EmployeeName,
                        PayGroupId = X.PayGroupID,
                        PayGroupeName = X.PayGroupName,
                        Basic = X.Basic,
                        Gross_Amount = X.Gross_Amount,
                        Total_Allowances = X.Total_Allowances,
                        Total_Deduction = X.Total_Deduction,
                        Net_Payable_Amount = X.Net_Payable_Amount,
                        IsPaid = X.IsPaid
                    };
                }).ToList();
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }
        public int? AllEmployeeSalaryview_Count(EmployeeSalaryModel employeeSalaryModel)
        {
            int? result = 0;
            try
            {

                var Total = esspEntities.Employeesalary_viewAllByMonth_Count(employeeSalaryModel.Month, employeeSalaryModel.Year, employeeSalaryModel.Empid,
                    employeeSalaryModel.Status).ToList();
                result = Total[0];

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;

        }

        public List<ExportExcelModel> EmployeeSalaryforexcel(ExportExcelModel exportExcelModel)
        {
            List<ExportExcelModel> listmodel = new List<ExportExcelModel>();
            try
            {
                List<ExcelReport_GetAll_Result> empList = esspEntities.ExcelReport_GetAll(exportExcelModel.Empid, exportExcelModel.Prefixfromdate, exportExcelModel.suffixtodate,exportExcelModel.Month, exportExcelModel.TotalDays, exportExcelModel.empno,exportExcelModel.year).ToList();
                listmodel = empList.Select(X =>
                {
                    return new ExportExcelModel
                    {
                       SLNO=X.SLNO,
                       EMPLOYEECODE=X.EMPLOYEECODE,
                       NAME_OF_EMPLOYEES=X.NAME_OF_EMPLOYEES,
                       SITE=X.SITE,
                       W=X.W,
                       O=X.O,
                       Designation=X.Designation,
                       E=X.E,
                       T_D=X.T_D,
                       BASIC=X.BASIC,
                       
                       STIPEND=X.STIPEND,
                       WORKING_DAYS=X.WORKING_DAYS,
                       DAY_S_ABSENT=Convert.ToDouble(X.DAY_S_ABSENT),
                       DEDUCTION_AGAINST_ABSENSE=X.DEDUCTION_AGAINST_ABSENSE,
                       E_L_=X.E_L_,
                       PAYMENT_AGAINST_E_L_=X.PAYMENT_AGAINST_E_L_,
                       GROSS_AMOUNT=X.GROSS_AMOUNT,
                       //GROSS_AMOUNT_EARNED=X.GROSS_AMOUNT_EARNED,
                       HRA=X.HRA,
                       LODGING_ALLOWANCE=X.LODGING_ALLOWANCE,
                        //LODGING_ALLOWANCE_ACHIEVED=X.LODGING_ALLOWANCE_ACHIEVED,
                        TRAINING_ALLOWANCE_ACHIEVED = X.TRAINING_ALLOWANCE_ACHIEVED,
                       //TRAINING_ALLOWANCE_ACHIEVED=X.TRAINING_ALLOWANCE_ACHIEVED,
                       MOBILE=X.MOBILE,
                       BOOKS_AND_PERIODICALS_ALLOWANCE=X.BOOKS_AND_PERIODICALS_ALLOWANCE,
                       INTERNET_ALLOWANCE =X.INTERNET_ALLOWANCE,
                       CONINUOUS_ATTENDENCE_ALLOWANCE=X.CONINUOUS_ATTENDENCE_ALLOWANCE,
                       //CONINUOUS_ATTENDENCE_ALLOWANCE_ACHIEVED=X.CONINUOUS_ATTENDENCE_ALLOWANCE_ACHIEVED,
                       PRODUCTION_BONUS=X.PRODUCTION_BONUS,
                       PRODUCTION_BONUS_ACHIEVED=X.PRODUCTION_BONUS_ACHIEVED,
                       OVERTIME=X.OVERTIME,
                       OVERTIME_AMOUNT=X.OVERTIME_AMOUNT,
                       MEDICAL_ALLOWANCES=X.MEDICAL_ALLOWANCES,
                      // MEDICAL_ALLOWANCES_ACHIEVED=X.MEDICAL_ALLOWANCES_ACHIEVED,
                       LTA=X.LTA,
                       //LTA_ACHIEVED=X.LTA_ACHIEVED,
                       EDUCATION_ALLOWANCES=X.EDUCATION_ALLOWANCES,
                       //EDUCATION_ALLOWANCES_ACHIEVED=X.EDUCATION_ALLOWANCES_ACHIEVED,
                       TOTAL_ALLOWANCES=X.TOTAL_ALLOWANCES,
                       //GROSS_AMOUNT_WITH_ALL_ALLOWANCES=X.GROSS_AMOUNT_WITH_ALL_ALLOWANCES,
                       //GROSS_PAYABLE_WITH_ALL_INCENTIVE=X.GROSS_PAYABLE_WITH_ALL_INCENTIVE,
                       E_S_I=X.E_S_I,
                       PF=X.PF,
                       PTAX=X.PTAX,
                       TOTAL_DEDUCTION=X.TOTAL_DEDUCTION,
                       //PAYABLE_AMOUNT=X.PAYABLE_AMOUNT,
                       OPENING_BALANCE=X.OPENING_BALANCE,
                       DEDUCTION =X.DEDUCTION,
                       CLOSING_BALANCE =X.CLOSING_BALANCE,
                       NET_AMOUNT =X.NET_AMOUNT,
                       Attire_Allowance=X.Attire_Allowance,
                       PAID_DATE=X.PAID_DATE
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;

        }

        public List<EmpbasicModel> Employeelogin(EmpbasicModel empbasicModel)
        {
            List<EmpbasicModel> Emplist = new List<EmpbasicModel>();
            try
            {
                List<EmployeeLoginCheck_Result> listmodel = esspEntities.EmployeeLoginCheck(empbasicModel.empemail,empbasicModel.empcode).ToList();
                Emplist = listmodel.Select(X =>
                {
                    return new EmpbasicModel
                    {
                        EmployeeName = X.EmployeeName
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Emplist;

        }

        public int SaveEmployeeManualSalary(EmpoloyeeSalaryConfigManualValueModel empoloyeeSalaryConfigManualValueModel)
        {
            //return 0;
            try
            {
                int SaveEmpLoan = esspEntities.EmpoloyeeSalaryConfigManualValues_Update(
                    empoloyeeSalaryConfigManualValueModel.PayConfigIDS, empoloyeeSalaryConfigManualValueModel.Values, empoloyeeSalaryConfigManualValueModel.EmployeeSalaryConfigid);

                if (SaveEmpLoan != 0)
                {
                    return SaveEmpLoan;
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public List<EmployeeListModel> EmpInActivelist()
        {
            List<EmployeeListModel> listmodel = new List<EmployeeListModel>();
            try
            {
                var empListsss = esspEntities.EmployeeList_GetInActive().ToList();

                List<EmployeeList_GetInActive_Result> empList = esspEntities.EmployeeList_GetInActive().ToList();


                listmodel = empList.Select(X =>
                {
                    return new EmployeeListModel
                    {
                        empid = X.empid,
                        empno = X.empno,
                        EmployeeName = X.EmployeeName,
                        empcode = X.empcode,
                        postname = X.postname,
                        Designation = X.Designation,
                        Department = X.Department,
                        PayGroupName = X.PayGroupName,
                        PayGroupID = X.PayGroupID == null ? 0 : Convert.ToInt32(X.PayGroupID)
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listmodel;
        }

        public List<IndividualSalryModel> ViewIndividualSalary(IndividualSalryModel individualSalryModel)
        {
            List<IndividualSalryModel> Emplistmodel = new List<IndividualSalryModel>();
            try
            {
                List<EmployeeSalary_ById_Result> Empslist = esspEntities.EmployeeSalary_ById(individualSalryModel.Empid, individualSalryModel.Month, individualSalryModel.Year,
                    Convert.ToDateTime(individualSalryModel.FirstDays),
                    Convert.ToDateTime(individualSalryModel.LastDays)).ToList();
                Emplistmodel = Empslist.Select(X =>
                {
                    return new IndividualSalryModel
                    {
                        Empid = X.Empid,
                        Empno = X.Empno,
                        Department = X.Department,
                        Designation = X.Designation,
                        EmployeeName = X.EmployeeName,
                        Basic = X.Basic,
                        CompanyAddress=X.Address,
                        Internet_Allowance = X.Internet_Allowance,
                        PF = X.PF,
                        Education_Allowances = X.Education_Allowances,
                        Medical_Allowances = X.Medical_Allowances,
                        HRA = X.HRA,
                        Attire_Allowance = X.Attire_Allowance,
                        P_Tax = X.P_Tax,
                        ESI = X.ESI,
                        Leave_Amount = X.Leave_Amount,
                        joining=X.joining,
                        Total_Allowances=X.Total_Allowances,
                        Total_Deduction=X.Total_Deduction,
                        Net_Payable_Amount=X.Net_Payable_Amount,
                        Month=X.Month,
                        Year=X.Year,
                        empaccountno=X.empaccountno,
                        empcode=X.empcode,
                        empepfno=X.empepfno,
                        empesicno=X.empesicno,
                        emppancardno=X.emppancardno,
                        empuanno=X.empuanno,
                        Company=X.Company,
                        AbsentDays=X.AbsentDays.ToString(),
                        EarnedLeaveDays=X.EarnedLeave.ToString(),
                        TotalLeaveDays=X.TotalLeave.ToString(),
                        LateLeave= Convert.ToInt32(X.lateleave).ToString(),
                        Gross_Amount=X.Gross_Amount,
                        Continuous_Attendance_Allowance=X.Continuous_Attendance_Allowance,
                        ProductionBonus=X.ProductionBonus,
                        Arrears = X.Arrears,
                        Overtime_Allowance=X.Overtime_Allowance,
                        PerformanceAllowance=X.PerformanceAllowance,
                        SpecialAllowance=X.SpecialAllowances,
                        LTA=X.LTA,
                        Stipend=X.Stipend,
                        Lodging_Allowance=X.Lodging_Allowance,
                        Mobile_Allowance=X.Mobile_Allowance,
                        Conveyence_Allowance=X.Conveyence_Allowance,
                        Training__Allowance=X.Training__Allowance,
                        Vehical_Allownce=X.Vehical_Allownce,
                        Books___Periodical=X.Books___Periodical,
                        Food_Allowance=X.Food_Allowance,
                        DOB=X.empdob,
                        TDS=X.TDS,
                        Loan_Installment= X.Loan_Installment

                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Emplistmodel;
        }

        public int DeleteAllEmployeeSalaryByMonth(EmployeeSalaryModel employeeSalaryModel)
        {
            int deletedsalary = 0;
            if (employeeSalaryModel != null)
            {
                try
                {
                    deletedsalary = esspEntities.EmployeeSalary_DeleteAllByMonth(employeeSalaryModel.Empid,employeeSalaryModel.Month, employeeSalaryModel.Year);
                }
                catch(Exception ex)
                {
                    deletedsalary = -1;
                }
            }
            return deletedsalary;
        }

        public int SaveEmployeeBonus(EmployeeBonusModel employeeBonusModel)
        {
            int Savedata = 0;
            try
            {
                Savedata =Convert.ToInt32(esspEntities.EmployeeBonus_Save(employeeBonusModel.ID, employeeBonusModel.Empid, employeeBonusModel.PayGroupId,
                    employeeBonusModel.Month, employeeBonusModel.Year, employeeBonusModel.OverTimeHours, employeeBonusModel.ProductionBonus,
                    employeeBonusModel.TargetAchieved).FirstOrDefault());
            }
            catch(Exception ex)
            {
                Savedata = -1;
            }
            return Savedata;
        }


        public List<EmployeeBonusModel> GetAllEmployeeOvertime(EmployeeBonusModel employeeBonusModel)
        {
            List<EmployeeBonusModel> OvertimeList = new List<EmployeeBonusModel>();
            List<EmployeeBonus_GetAllByEmpid_Result> Overtimelist = esspEntities.EmployeeBonus_GetAllByEmpid(employeeBonusModel.ID, employeeBonusModel.Empid).ToList();
            OvertimeList = Overtimelist.Select(x =>
             {
                 return new EmployeeBonusModel
                 {
                     ID = x.ID,
                     Empid = x.Empid,
                     Month = x.Month,
                     Year = x.Year,
                     OverTimeHours = x.OverTimeHours,
                     ProductionBonus = x.ProductionBonus,
                     TargetAchieved = x.TargetAchieved
                 };
             }).ToList();

            return OvertimeList;
        }

        public List<EmployeeBonusModel> GetAllEmployeeOvertimeByMonth(EmployeeBonusModel employeeBonusModel)
        {
            List<EmployeeBonusModel> OvertimeList = new List<EmployeeBonusModel>();
            List<EmployeeBonus_GetByMonth_Result> Overtimelist = esspEntities.EmployeeBonus_GetByMonth(employeeBonusModel.Empid, employeeBonusModel.Month,employeeBonusModel.Year).ToList();
            OvertimeList = Overtimelist.Select(x =>
            {
                return new EmployeeBonusModel
                {
                    ID = x.ID,
                    Empid = x.Empid,
                    Month = x.Month,
                    Year = x.Year,
                    OverTimeHours = x.OverTimeHours,
                    ProductionBonus = x.ProductionBonus,
                    TargetAchieved = x.TargetAchieved
                };
            }).ToList();

            return OvertimeList;
        }

        public int UpdateEmployeeSalryPaid(EmployeeSalaryModel employeeSalaryModel)
        {
            int updateddata = 0;
            try
            {
                updateddata = esspEntities.EmployeeSalary_UpdatePayment(employeeSalaryModel.Empid, employeeSalaryModel.Month, employeeSalaryModel.Year);
            }
            catch(Exception ex)
            {
                updateddata = 0;
            }
            return updateddata;
        }

        #region EditLeaveDetails
        public int SaveLeavedetails(LeaveDetailsModel leaveDetailsModel)
        {
            int created = 0;
            if (leaveDetailsModel != null)
            {
                try
                {
                    //List<LeaveDetails_Save_Result> Saveresult = esspEntities.LeaveDetails_Save(leaveDetailsModel.Empno, leaveDetailsModel.LeaveDays, leaveDetailsModel.LateLeaveDays, leaveDetailsModel.CreatedBy, leaveDetailsModel.Month, leaveDetailsModel.Year).ToList();
                    //created = Convert.ToInt32(Saveresult[0].ReturnStatus);
                    created = Convert.ToInt32(esspEntities.LeaveDetails_Save(leaveDetailsModel.Empno, leaveDetailsModel.LeaveDays, leaveDetailsModel.LateLeaveDays, leaveDetailsModel.CreatedBy, leaveDetailsModel.Month, leaveDetailsModel.Year,leaveDetailsModel.Noofdaysabsent,leaveDetailsModel.Type).FirstOrDefault());
                }
                catch(Exception ex)
                {
                    created = 0;
                }
            }
            return created;
        }

        #endregion

        #region get edit leavedetails
        public List<LeaveDetailsModel> GetLeaveDetails(LeaveDetailsModel leaveDetailsModel)
        {
            var LeaveDetailslist = esspEntities.LeaveDetails_GetByEmpno(leaveDetailsModel.Empno, leaveDetailsModel.Month, leaveDetailsModel.Year).ToList();
            List<LeaveDetailsModel> LeavedetailsList = new List<LeaveDetailsModel>();
            LeavedetailsList = LeaveDetailslist.Select(x =>
             {
                 return new LeaveDetailsModel()
                 {
                     ID = x.ID,
                     CreatedBy = x.CreatedBy,
                     Empno = x.Empno,
                     LateLeaveDays = x.LateLeaveDays,
                     LeaveDays = x.LeaveDays,
                     Month = x.Month,
                     Year = x.Year,
                     Noofdaysabsent=x.Noofdaysabsent
                     
                 };
             }).ToList();
            return LeavedetailsList;
        }
        #endregion


        #region Check TDS
        public List<EmployeeSalaryConfigModel> CheckTDSEligiblePaygroup()
        {
            var TDSPaygroup = esspEntities.Employee_TDSReport();
            List<EmployeeSalaryConfigModel> data = new List<EmployeeSalaryConfigModel>();
            data = TDSPaygroup.Select(x =>
            {
                return new EmployeeSalaryConfigModel
                {
                    PayGroupId = x.PayGroupID
                };
            }).ToList();
            return data;
        }

        #endregion
    }
}
