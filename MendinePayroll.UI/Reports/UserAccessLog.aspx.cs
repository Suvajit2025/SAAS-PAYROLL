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
    public partial class UserAccessLog : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionOut();
            if (!IsPostBack)
            {
                //DefaultLoad();

            }
        }
        public void SessionOut()
        {
            if (Session["UserName"] == null)
            {
                Response.Redirect(ConfigurationManager.AppSettings["PayrollLogout"].ToString());
            }
        }
        //private void DefaultLoad()
        //{
        //    // Employee 
        //    String con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
        //    ddlEmployee.DataSource = clsSalary.EmployeeList();
        //    ddlEmployee.DataTextField = "EmployeeName";
        //    ddlEmployee.DataValueField = "IDEmployee";
        //    ddlEmployee.DataBind();
        //    ddlEmployee.Items.Insert(0, "Select");
        //}
        protected void btnShow_Click(object sender, EventArgs e)
        {
            //string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            DataTable dt = clsSalary.Payroll_Access_Log();
            RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/UserAccessLog.rdlc");
            ReportDataSource RSource = new ReportDataSource("DSAccessLog", dt);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(RSource);
        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Dashboard");
        }
    }
}