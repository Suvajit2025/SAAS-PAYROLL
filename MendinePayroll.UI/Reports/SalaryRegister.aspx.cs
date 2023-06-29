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
namespace MendinePayroll.UI.Reports
{
    public partial class SalaryRegister : System.Web.UI.Page
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

            // Start Year 
            ddlStartYear.DataSource = clsSalary.SalaryYears();
            ddlStartYear.DataTextField = "Year";
            ddlStartYear.DataValueField = "Year";
            ddlStartYear.DataBind();
            ddlStartYear.Items.Insert(0, "Select");

            // End Year 
            ddlEndYear.DataSource = clsSalary.SalaryYears();
            ddlEndYear.DataTextField = "Year";
            ddlEndYear.DataValueField = "Year";
            ddlEndYear.DataBind();
            ddlEndYear.Items.Insert(0, "Select");
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
            long IDEmployee = clsHelper.fnConvert2Long(ddlEmployee.SelectedValue);
            String StartMonth = ddlStartMonth.SelectedItem.Text;
            int StartYear = clsHelper.fnConvert2Int(ddlStartYear.SelectedItem.Text);
            String EndMonth = ddlEndMonth.SelectedItem.Text;
            int EndYear = clsHelper.fnConvert2Int(ddlEndYear.SelectedItem.Text);

            DataTable dt = clsSalary.SalaryRegisterNew(IDEmployee, StartMonth, StartYear, EndMonth, EndYear);


            RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/SalaryRegister.rdlc");
            ReportDataSource RSource = new ReportDataSource("DSSalaryRegister", dt);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(RSource);

        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Dashboard");
        }
    }
}