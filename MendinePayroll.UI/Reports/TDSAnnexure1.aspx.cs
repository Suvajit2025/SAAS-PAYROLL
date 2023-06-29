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
    public partial class TDSAnnexure1 : System.Web.UI.Page
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
            // Company 
            ddlCompany.DataSource = clsSalary.CompanyList();
            ddlCompany.DataTextField = "Name";
            ddlCompany.DataValueField = "Code";
            ddlCompany.DataBind();
            ddlCompany.Items.Insert(0, "Select");


            //  Year 
            ddlYear.DataSource = clsSalary.SalaryYears();
            ddlYear.DataTextField = "Year";
            ddlYear.DataValueField = "Year";
            ddlYear.DataBind();
            ddlYear.Items.Insert(0, "Select");
            if(ddlYear.Items.Count > 0) {ddlYear.SelectedValue = DateTime.Now.ToString("yyyy"); }


            // Month Default Seelction 
            if (ddlMonth.Items.Count >0) { ddlMonth.SelectedValue = DateTime.Now.AddMonths(-1).ToString("MMMM"); }
           

        }
        public void SessionOut()
        {
            if (Session["UserName"] == null)
            {
                Response.Redirect(ConfigurationManager.AppSettings["PayrollLogout"].ToString());
            }
        }
        protected void btnDownload_Click(object sender, EventArgs e)
        {
            Warning[] warnings;
            string[] streamIds;
            string contentType;
            string encoding;
            string extension;
            string filename = "TDSAnnexure1_" + ddlMonth.SelectedItem.Text + "_" + ddlYear.SelectedItem.Text;
            //Export the RDLC Report to Byte Array.
            byte[] bytes = RVViewer.LocalReport.Render("EXCEL", null, out contentType, out encoding, out extension, out streamIds, out warnings);

            //Download the RDLC Report in Word, Excel, PDF and Image formats.
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = contentType;
            Response.AppendHeader("Content-Disposition", "attachment; filename="+ filename+"." + extension);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();

        }

        protected void btnShow_Click(object sender, EventArgs e)
        {
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            long IDEmployee = clsHelper.fnConvert2Long(ddlEmployee.SelectedValue);
            String CCode = ddlCompany.SelectedValue.ToString() == "Select" ? "" : ddlCompany.SelectedValue.ToString();
            String Month = ddlMonth.SelectedItem.Text;
            int Year = clsHelper.fnConvert2Int(ddlYear.SelectedItem.Text);

            DataTable dt = clsSalary.TDSAnnexure1(IDEmployee, CCode, Month, Year);

            RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
            RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/TDSAnnexure1.rdlc");
            ReportDataSource RSource = new ReportDataSource("DSAnnexure92B", dt);
            RVViewer.LocalReport.DataSources.Clear();
            RVViewer.LocalReport.DataSources.Add(RSource);
            btnDownloan.Enabled = dt.Rows.Count > 0 ? true : false;

        }
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Dashboard");
        }

        protected void ddlCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Employee 
            String code = ddlCompany.SelectedValue.ToString();
            ddlEmployee.DataSource = clsSalary.EmployeeList(code);
            ddlEmployee.DataTextField = "EmployeeName";
            ddlEmployee.DataValueField = "IDEmployee";
            ddlEmployee.DataBind();
            ddlEmployee.Items.Insert(0, "Select");

        }
    }
}