using Common.Utility;
using MendinePayroll.UI.BLL;
using MendinePayroll.UI.BLL.Report;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MendinePayroll.UI.Report.Loan
{
    public partial class LoanRegister : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionOut();
            if (!IsPostBack)
            {
                Default();
            }
        }
        private void Default()
        {
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            ddlEmployee.DataSource = clsSalary.LoanEmployeeList(); 
            ddlEmployee.DataTextField = "EmployeeName";
            ddlEmployee.DataValueField = "IDEmployee";
            ddlEmployee.DataBind();
            ddlEmployee.Items.Insert(0, "Select");
        }
        public void SessionOut()
        {
            if (Session["UserName"] == null)
            {
                Response.Redirect(ConfigurationManager.AppSettings["PayrollLogout"].ToString());
            }
        }
        private void ShowDetails()
        {
            long IDEmp = clsHelper.fnConvert2Long(ddlEmployee.SelectedValue);
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            DataTable DT = clsALLEmpLoan.Employee_Wise_LoanDetails(IDEmp);
            RVViewer.ProcessingMode = ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/LoanRegister.rdlc");
            ReportDataSource DSLoanRegister = new ReportDataSource("DSLoanRegister", DT);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(DSLoanRegister);
        }
        protected void btnShow_Click(object sender, EventArgs e)
        {
            ShowDetails();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Dashboard");
           
        }
    }
}