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
        private string TenantId
        {
            get
            {
                EnsurePayrollSession();

                return Session["TenantID"].ToString();
            }
        }
        private string UserId
        {
            get
            {
                EnsurePayrollSession();

                return Session["UserID"].ToString();
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            EnsurePayrollSession();
            RegisterPayrollSessionStorageScript();

            if (!IsPostBack)
            {
                Default();
            }
        }
        private void Default()
        {
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            ddlEmployee.DataSource = clsALLEmpLoan.Loan_List_Tenantwise_Employee_Wise_All(TenantId);
            ddlEmployee.DataTextField = "EmployeeName";
            ddlEmployee.DataValueField = "IDEmployee";
            ddlEmployee.DataBind();
        }
        public void SessionOut()
        {
            EnsurePayrollSession();
        }

        private void EnsurePayrollSession()
        {
            if (Session["UserName"] == null ||
                Session["EmployeeNo"] == null ||
                Session["TenantID"] == null ||
                Session["UserID"] == null ||
                string.IsNullOrWhiteSpace(Session["UserName"].ToString()) ||
                string.IsNullOrWhiteSpace(Session["EmployeeNo"].ToString()) ||
                string.IsNullOrWhiteSpace(Session["TenantID"].ToString()) ||
                string.IsNullOrWhiteSpace(Session["UserID"].ToString()))
            {
                string restoreUrl = ResolveUrl("~/Login/RestoreSession") + "?returnUrl=" + Server.UrlEncode(Request.RawUrl);
                Response.Redirect(restoreUrl, true);
            }
        }

        private void RegisterPayrollSessionStorageScript()
        {
            if (ClientScript.IsStartupScriptRegistered("PayrollSessionStorage"))
            {
                return;
            }

            string script = string.Format(@"
                (function () {{
                    var sessionData = {{
                        RoleID: '{0}',
                        UserName: '{1}',
                        UserID: '{2}',
                        EmployeeNo: '{3}',
                        UserEmail: '{4}',
                        TenantID: '{5}'
                    }};

                    if (sessionData.UserName && sessionData.EmployeeNo && sessionData.TenantID) {{
                        localStorage.setItem('PayrollSession', JSON.stringify(sessionData));
                    }}
                }})();",
                HttpUtility.JavaScriptStringEncode(Convert.ToString(Session["RoleID"])),
                HttpUtility.JavaScriptStringEncode(Convert.ToString(Session["UserName"])),
                HttpUtility.JavaScriptStringEncode(Convert.ToString(Session["UserId"])),
                HttpUtility.JavaScriptStringEncode(Convert.ToString(Session["EmployeeNo"])),
                HttpUtility.JavaScriptStringEncode(Convert.ToString(Session["UserEmail"])),
                HttpUtility.JavaScriptStringEncode(Convert.ToString(Session["TenantID"])));

            ClientScript.RegisterStartupScript(GetType(), "PayrollSessionStorage", script, true);
        }

        private void ShowDetails()
        {
            string IDEmp = ddlEmployee.SelectedValue;
            string con = ConfigurationManager.ConnectionStrings["Admin"].ConnectionString;
            DataTable DT = clsALLEmpLoan.Employee_Loan_Ledger_TenantWise(TenantId, IDEmp);
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
