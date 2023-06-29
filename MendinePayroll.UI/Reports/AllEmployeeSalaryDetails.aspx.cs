using System;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MendinePayroll.UI.BLL.Reports;
using Microsoft.Reporting.WebForms;
using Common.Utility;

namespace MendinePayroll.UI.Reports
{
    public partial class AllEmployeeSalaryDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionOut();
            if (!IsPostBack)
            {
               // Response.Cache.SetCacheability(HttpCacheability.NoCache);
                string monthname = Request.QueryString["Month"];
                int year = int.Parse(Request.QueryString["Year"]);
                string empno = "";
                string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
                DataSet DS = clsDatabase.fnDataSet("PRC_ExcelReport_GetAll", year, empno, monthname);

                RVViewer.ProcessingMode = Microsoft.Reporting.WebForms.ProcessingMode.Local;
                RVViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/RDLC/AllEMPSalaryList.rdlc");
                ReportDataSource SourceEmpSalaryList = new ReportDataSource("AllEmpSalaryDatasets", DS.Tables[0]);
                RVViewer.LocalReport.DataSources.Clear();
                RVViewer.LocalReport.DataSources.Add(SourceEmpSalaryList);
            }

        }
        public void SessionOut()
        {
            if (Session["UserName"] == null)
            {
                Response.Redirect(ConfigurationManager.AppSettings["PayrollLogout"].ToString());
            }
        }

        //protected void btnBack_Click(object sender, EventArgs e)
        //{
        //    Response.Redirect(ConfigurationManager.AppSettings["DashboardURL"].ToString());
        //}
    }
}