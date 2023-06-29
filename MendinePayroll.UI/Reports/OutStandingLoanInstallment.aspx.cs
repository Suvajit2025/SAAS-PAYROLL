using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MendinePayroll.UI.BLL;
using MendinePayroll.UI.BLL.Report;
using MendinePayroll.Models;
using System.Globalization;
using MendinePayroll.UI.Models;
using System.Data;
using System.Configuration;
using Microsoft.Reporting.WebForms;
using Common.Utility;
using UI.BLL;
using System.Text;

namespace MendinePayroll.UI.Reports
{
    public partial class OutStandingLoanInstallment : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionOut();
            if (!IsPostBack)
            {
                DefaultLoad();
            }
        }
        private void DefaultLoad()
        {
            // Employee 
            String con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            ddlEmployee.DataSource = clsSalary.EmployeeList();
            ddlEmployee.DataTextField = "EmployeeName";
            ddlEmployee.DataValueField = "IDEmployee";
            ddlEmployee.DataBind();
            ddlEmployee.Items.Insert(0, "Select");

            // Year 
            ddlYear.DataSource = clsSalary.SalaryYears();
            ddlYear.DataTextField = "Year";
            ddlYear.DataValueField = "Year";
            ddlYear.DataBind();
            ddlYear.Items.Insert(0, "Select");

        }
        public void SessionOut()
        {
            if (Session["UserName"] == null)
            {
                Response.Redirect(ConfigurationManager.AppSettings["PayrollLogout"].ToString());
            }
        }
        protected void btnShow_Click(object sender, EventArgs e)
        {
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            long idemployee = clsHelper.fnConvert2Long(ddlEmployee.SelectedValue);
            string month = ddlMonth.SelectedItem.Text;
            String year = ddlYear.SelectedItem.Text;
            DataTable dt = clsLoan.Employee_Loan_Outstanding_Installment(month, year, idemployee);
            RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/OutstandingLoanInstallment.rdlc");
            ReportDataSource RSource = new ReportDataSource("DSOutstandingLoanInstallment", dt);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(RSource);

            // Parameter 
            ReportParameter paraASONDate = new ReportParameter("ASONDate", "AS On : " + ddlMonth.SelectedItem.Text + "-" + ddlYear.SelectedItem.Text);
            RVViewer.LocalReport.SetParameters(paraASONDate);

        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Dashboard");
        }
    }
}