using Common.Utility;
using MendinePayroll.UI.BLL;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MendinePayroll.UI.Reports
{
    public partial class BonusReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionOut();
            if (!IsPostBack)
            {
                DefaultLoad();

            }
        }

        public void SessionOut()
        {
            if (Session["UserName"] == null)
            {
                Response.Redirect(ConfigurationManager.AppSettings["PayrollLogout"].ToString());
            }
        }

        private void DefaultLoad()
        {
            // Employee 
            String con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            ddlEmployee.DataSource = clsSalary.EmployeeList();
            ddlEmployee.DataTextField = "EmployeeName";
            ddlEmployee.DataValueField = "EmployeeNo";
            ddlEmployee.DataBind();
            ddlEmployee.Items.Insert(0, "Select");

            // Department 
            ddlDepartment.DataSource = clsSalary.Department_List();
            ddlDepartment.DataTextField = "Name";
            ddlDepartment.DataValueField = "IDDepartment";
            ddlDepartment.DataBind();
            ddlDepartment.Items.Insert(0, "Select");


            // Start Year 
            ddlYear.DataSource = clsSalary.FinancialYears();
            ddlYear.DataTextField = "FinancialYear";
            ddlYear.DataValueField = "Year";
            ddlYear.DataBind();
            ddlYear.Items.Insert(0, "Select");
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Dashboard");
        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            long IDEmployee = clsHelper.fnConvert2Long(ddlEmployee.SelectedValue);
            long IDDepartment = clsHelper.fnConvert2Long(ddlDepartment.SelectedValue);
            int Year = clsHelper.fnConvert2Int(ddlYear.SelectedValue);
            var Financial_Year = ddlYear.SelectedItem.Text;

            RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/BonusList.rdlc");
            ReportParameter FinancialYear = new ReportParameter("FinancialYear", Financial_Year);
            RVViewer.LocalReport.SetParameters(new ReportParameter[] { FinancialYear });

            DataTable dt = clsSalary.GetBonusList(IDEmployee, IDDepartment, Year);

            ReportDataSource RSource = new ReportDataSource("DSBonusList", dt);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(RSource);
        }
    }
}