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
            ddlEmployee.DataSource = clsSalary.Employee_List();
            ddlEmployee.DataTextField = "EmployeeName";
            ddlEmployee.DataValueField = "EmployeeNo";
            ddlEmployee.DataBind();
            ddlEmployee.Items.Insert(0, "Select");

            // Department 
            LstDepartment.DataSource = clsSalary.Department_List();
            LstDepartment.DataTextField = "Name";
            LstDepartment.DataValueField = "IDDepartment";
            LstDepartment.DataBind();
            //ddlDepartment.Items.Insert(0, "Select");

            // Designation 
            ddlDesignation.DataSource = clsSalary.Designation_List();
            ddlDesignation.DataTextField = "Name";
            ddlDesignation.DataValueField = "IDDesignation";
            ddlDesignation.DataBind();
            ddlDesignation.Items.Insert(0, "Select");

            // Location 
            ddlLocation.DataSource = clsSalary.Location_List();
            ddlLocation.DataTextField = "Name";
            ddlLocation.DataValueField = "IDLocation";
            ddlLocation.DataBind();
            ddlLocation.Items.Insert(0, "Select");

            // Company
            ddlCompany.DataSource = clsSalary.CompanyList();
            ddlCompany.DataTextField = "Name";
            ddlCompany.DataValueField = "Code";
            ddlCompany.DataBind();
            ddlCompany.Items.Insert(0, new ListItem("Select", "0"));


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
            List<string> checkedList = new List<string>();
            foreach (ListItem item in LstDepartment.Items)
            {
                if (item.Selected)
                {
                    checkedList.Add(item.Value);
                }
            }
            String Departments = String.Join(",", checkedList); 
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            long IDEmployee = clsHelper.fnConvert2Long(ddlEmployee.SelectedValue);
            String IDDepartment = clsHelper.fnConvert2String(Departments) == "" ? "0" : clsHelper.fnConvert2String(Departments);
            String CompanyCode = ddlCompany.SelectedValue;
            long IDDesignation = clsHelper.fnConvert2Long(ddlDesignation.SelectedValue);
            long IDLocation = clsHelper.fnConvert2Long(ddlLocation.SelectedValue);
            int Year = clsHelper.fnConvert2Int(ddlYear.SelectedValue);
            var Financial_Year = ddlYear.SelectedItem.Text;

            RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/BonusList.rdlc");
            ReportParameter FinancialYear = new ReportParameter("FinancialYear", Financial_Year);
            RVViewer.LocalReport.SetParameters(new ReportParameter[] { FinancialYear });

            DataTable dt = clsSalary.GetBonusList(IDEmployee, IDDepartment, CompanyCode, IDDesignation,IDLocation, Year);

            ReportDataSource RSource = new ReportDataSource("DSBonusList", dt);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(RSource);
        }
    }
}