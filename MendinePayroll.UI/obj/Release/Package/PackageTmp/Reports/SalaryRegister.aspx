<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SalaryRegister.aspx.cs" Inherits="MendinePayroll.UI.Reports.SalaryRegister" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Payroll | Salary Register</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC" crossorigin="anonymous" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <div class="container-fluid">
            <div class="row d-flex justify-content-center small">
                <div class="col-md-11">
                    <div class="card mt-3">
                        <div class="card-header bg-white d-flex justify-content-between">
                            <div>
                                <h5>Salary Register</h5>
                            </div>
                            <div>
                                <asp:Button ID="Button1" runat="server" CssClass="btn btn-sm btn-primary" Text="Show" OnClick="btnShow_Click" />
                                <asp:Button ID="btnBack" runat="server" CssClass="btn btn-sm btn-primary" Text="Back" OnClick="btnBack_Click" />
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row mb-1">
                                <div class="col-md-1">
                                    <label>Start Month</label>
                                </div>
                                <div class="col-md-2">
                                    <asp:DropDownList ID="ddlStartMonth" runat="server" CssClass="form-select form-select-sm">
                                        <asp:ListItem Value="0" Text="Select" Selected></asp:ListItem>
                                        <asp:ListItem Value="1" Text="January"></asp:ListItem>
                                        <asp:ListItem Value="2" Text="February"></asp:ListItem>
                                        <asp:ListItem Value="3" Text="March"></asp:ListItem>
                                        <asp:ListItem Value="4" Text="April"></asp:ListItem>
                                        <asp:ListItem Value="5" Text="May"></asp:ListItem>
                                        <asp:ListItem Value="6" Text="June"></asp:ListItem>
                                        <asp:ListItem Value="7" Text="July"></asp:ListItem>
                                        <asp:ListItem Value="8" Text="August"></asp:ListItem>
                                        <asp:ListItem Value="9" Text="September"></asp:ListItem>
                                        <asp:ListItem Value="10" Text="October"></asp:ListItem>
                                        <asp:ListItem Value="11" Text="November"></asp:ListItem>
                                        <asp:ListItem Value="12" Text="December"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-1">
                                    <label>Start Year </label>
                                </div>
                                <div class="col-md-2">
                                    <asp:DropDownList ID="ddlStartYear" runat="server" CssClass="form-select form-select-sm">
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-1">
                                    <label>End Month</label>
                                </div>
                                <div class="col-md-2">
                                    <asp:DropDownList ID="ddlEndMonth" runat="server" CssClass="form-select form-select-sm">
                                        <asp:ListItem Value="0" Text="Select" Selected></asp:ListItem>
                                        <asp:ListItem Value="1" Text="January"></asp:ListItem>
                                        <asp:ListItem Value="2" Text="February"></asp:ListItem>
                                        <asp:ListItem Value="3" Text="March"></asp:ListItem>
                                        <asp:ListItem Value="4" Text="April"></asp:ListItem>
                                        <asp:ListItem Value="5" Text="May"></asp:ListItem>
                                        <asp:ListItem Value="6" Text="June"></asp:ListItem>
                                        <asp:ListItem Value="7" Text="July"></asp:ListItem>
                                        <asp:ListItem Value="8" Text="August"></asp:ListItem>
                                        <asp:ListItem Value="9" Text="September"></asp:ListItem>
                                        <asp:ListItem Value="10" Text="October"></asp:ListItem>
                                        <asp:ListItem Value="11" Text="November"></asp:ListItem>
                                        <asp:ListItem Value="12" Text="December"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-1">
                                    <label>End Year </label>
                                </div>
                                <div class="col-md-2">
                                    <asp:DropDownList ID="ddlEndYear" runat="server" CssClass="form-select form-select-sm">
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="row mb-1">
                                <div class="col-md-1">
                                    <label>Employee</label>
                                </div>
                                <div class="col-md-5">
                                    <asp:DropDownList ID="ddlEmployee" runat="server" CssClass="form-select form-select-sm"></asp:DropDownList>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <rsweb:ReportViewer ID="RVViewer" runat="server" Width="100%" ZoomPercent="100" Height="100%">
                                </rsweb:ReportViewer>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
