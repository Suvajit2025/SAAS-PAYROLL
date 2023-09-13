using Common.Utility;
using MendinePayroll.Models;
using MendinePayroll.UI.BLL;
using MendinePayroll.UI.BLL.Reports;
using MendinePayroll.UI.Utility;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MendinePayroll.UI.Reports
{
    public partial class PaySlip : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionOut();
            if (!IsPostBack)
            {
                string sEmpIde = Request.QueryString["Empid"];
                int empid = clsHelper.fnConvert2Int(sEmpIde);//   Convert.ToInt32(DataEncryption.Decrypt(Convert.ToString(sEmpIde), "passKey"));
                string monthname = Request.QueryString["Month"];
                int year = int.Parse(Request.QueryString["Year"]);
                String type = clsHelper.fnConvert2String(Request.QueryString["Type"]);

                int month = DateTime.ParseExact(monthname, "MMMM", CultureInfo.InvariantCulture).Month;
                List<IndividualSalryModel> Emplist = clsSalary.Salary_Slip(empid, monthname, month, year);
                string employeename = Emplist[0].EmployeeName;
                employeename = employeename.Replace(" ", "_");

                String firstday = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1).ToString("dd/MMM/yyyy");
                String lastday = Convert.ToDateTime(firstday).AddMonths(1).ToString("dd/MMM/yyyy");

                string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
                DataSet DS = clsDatabase.fnDataSet("EmployeeSalary_Report_ById", empid, monthname, year, firstday, lastday);
                DataSet DSAllowance_Deduction = clsDatabase.fnDataSet("EmployeeSalary_Allowances_ById", empid, monthname, year);

                RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/ReportEmpWisePaySlip_NEW.rdlc");        
                ReportDataSource SourceEmpWisePaySlipHeader = new ReportDataSource("DSEmpWisePaySlipHeader", DS.Tables[0]);
                ReportDataSource SourcePaySlipAllowance = new ReportDataSource("DSPaySlipAllowance", DSAllowance_Deduction.Tables[0]);
                ReportDataSource SourcePaySlipDeduction = new ReportDataSource("DSPaySlipDeduction", DSAllowance_Deduction.Tables[1]);
                RVViewer.LocalReport.DataSources.Clear();
                RVViewer.LocalReport.DataSources.Add(SourceEmpWisePaySlipHeader);
                RVViewer.LocalReport.DataSources.Add(SourcePaySlipAllowance);
                RVViewer.LocalReport.DataSources.Add(SourcePaySlipDeduction);

                if (type=="PDF")
                {
                    string contentType = "";
                    string filename = employeename + "_" + monthname + "_" + year;
                    string selectedValue = "PDF";
                    string encoding;
                    string extension;
                     Warning[] warnings;
                    string[] streamIds;
                   

                    //Export the RDLC Report to Byte Array.
                    byte[] bytes = RVViewer.LocalReport.Render(selectedValue, null, out contentType, out encoding, out extension, out streamIds, out warnings);

                    //Download the RDLC Report in PDF format.
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.ContentType = contentType;
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename + "." + extension);
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();

                }
            }
        }
        // Download PDF Excel And Word
        public void SessionOut()
        {
            if (Session["UserName"] == null)
            {
                Response.Redirect(ConfigurationManager.AppSettings["PayrollLogout"].ToString());
            }
        }
        protected void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedValue = ddlDownloadItem.Items[ddlDownloadItem.SelectedIndex].Text;
                if (selectedValue != "SELECT") {
                    Warning[] warnings;
                    string[] streamIds;
                    string contentType;
                    string encoding;
                    string extension;

                    //Export the RDLC Report to Byte Array.
                    byte[] bytes = RVViewer.LocalReport.Render(selectedValue, null, out contentType, out encoding, out extension, out streamIds, out warnings);

                    string sEmpIde = Request.QueryString["Empid"];
                    //int empid = Convert.ToInt32(DataEncryption.Decrypt(Convert.ToString(sEmpIde), "passKey"));
                    int empid = Convert.ToInt32(Convert.ToString(sEmpIde));
                    string monthname = Request.QueryString["Month"];
                    int year = int.Parse(Request.QueryString["Year"]);
                    int month = DateTime.ParseExact(monthname, "MMMM", CultureInfo.InvariantCulture).Month;
                    List<IndividualSalryModel> Emplist = clsSalary.Salary_Slip(empid, monthname, month, year);
                    string employeename = Emplist[0].EmployeeName;
                    employeename = employeename.Replace(" ", "_");
                    string filename = employeename + "_" + monthname + "_" + year;

                    //Download the RDLC Report in PDF format.
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.ContentType = contentType;
                    Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename + "." + extension);
                    Response.BinaryWrite(bytes);
                    Response.Flush();
                    Response.End();
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}