<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AllEmployeeSalaryDetails.aspx.cs" Inherits="MendinePayroll.UI.Reports.AllEmployeeSalaryDetails" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <script src="~/Scripts/jquery-3.6.0.js"></script>
    <title></title>
</head>
<body>
      <form id="form1" runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <div class="d-flex justify-content-center">
            <div class="col-md-11">
                <div class="card mt-3 p-2 border-1">
                    <div class="card-body">
                        <div class="form-group row">
                            <div class="col-md-8">
                                <h4>Salary List</h4>
                            </div>
                        </div>
                     
                        <div class="form-group row mb-2">
                            <div class="col-md-12" >
                                <rsweb:ReportViewer ID="RVViewer" runat="server" Width="100%" ZoomPercent="100" Height="100%">
                                </rsweb:ReportViewer>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div>
        </div>
    </form>
</body>
</html>
