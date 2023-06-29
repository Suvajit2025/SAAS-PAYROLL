using Common.Utility;
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
    public partial class LoanLedger : System.Web.UI.Page
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
            ddlEmployee.DataSource = clsALLEmpLoan.Loan_List_Employee_Wise_All();
            ddlEmployee.DataTextField = "EmployeeName";
            ddlEmployee.DataValueField = "IDEmployee";
            ddlEmployee.DataBind();
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
            string IDEmp = ddlEmployee.SelectedValue;
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            DataTable DT = clsALLEmpLoan.Employee_Loan_Ledger(IDEmp);
            RVViewer.ProcessingMode = ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/LoanLedger.rdlc");
            ReportDataSource DTLoanDetails = new ReportDataSource("DSLoanLedger", DT);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(DTLoanDetails);

            ReportParameter[] parms = new ReportParameter[1];
            parms[0] = new ReportParameter("EmployeeName", "EMPLOYEE NAME: " + ddlEmployee.SelectedItem.Text);
            RVViewer.LocalReport.SetParameters(parms);

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