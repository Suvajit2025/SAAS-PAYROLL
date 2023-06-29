using MendinePayroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MendinePayroll.Core.Interface
{
    public interface IEmployeeCore
    {
        List<EmployeeListModel> Empllist();
        List<EmployeeListModel> GetEmplistById(EmployeeListModel employeeListModel);
        List<EmployeeLeaveModel> GetEmpleave(EmployeeLeaveModel employeeLeaveModel);
        List<EmpbasicModel> GEetEmployeeDetails(EmpbasicModel empbasicModel);
        List<EmployeeSalaryConfigModel> GEetEmployeeSalaryDetails(EmployeeSalaryConfigModel employeeSalaryConfigModel);
        int SaveEmpSalary(EmployeeSalaryConfigModel employeeSalaryConfigModel);
        int SaveEmpLoan(EmployeeLoanModel employeeLoanModel);
        List<EmployeeLoanModel> GEetEmployeeLoanDetails(EmployeeLoanModel employeeLoanModel);
        int SaveLoanTransaction(LoanTransactionModel loanTransactionModel);
        List<LoanTransactionModel> GetEmployeeLoanTDetails(LoanTransactionModel loanTransactionModel);
        List<EmployeeSalaryModel> EmployeeDuplicateSalarycheck(EmployeeSalaryModel employeeSalaryModel);
        int SaveallempSalary(EmployeeSalaryModel employeeSalaryModel);
        int EmployeeTotalinsertedinamonth(EmployeeSalaryModel employeeSalaryModel);
        List<EmployeeSalaryModel> AllEmployeeSalaryview(EmployeeSalaryModel employeeSalaryModel);
        List<ExportExcelModel> EmployeeSalaryforexcel(ExportExcelModel exportExcelModel);
        List<EmpbasicModel> Employeelogin(EmpbasicModel empbasicModel);
        int SaveEmployeeManualSalary(EmpoloyeeSalaryConfigManualValueModel empoloyeeSalaryConfigManualValueModel);
        List<EmployeeListModel> EmpInActivelist();
        List<IndividualSalryModel> ViewIndividualSalary(IndividualSalryModel individualSalryModel);
        int DeleteAllEmployeeSalaryByMonth(EmployeeSalaryModel employeeSalaryModel);
        int UpdateEmployeeSalryPaid(EmployeeSalaryModel employeeSalaryModel);
    }
}
