<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EmpWiseLoanDetails.aspx.cs" Inherits="MendinePayroll.UI.Report.Loan.EmpWiseLoanDetails" %>


<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <title>Employee Loan Register </title>
    <style>
        .heading {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: -15px;
        }

        .form-card {
            margin: 10px 20px;
            border: 1px solid lightgrey;
            border-radius: 3px;
            height: auto !important;
            padding-bottom: 20px;
            position: relative;
        }

        .btn-success {
            margin: 0 5px;
            margin-top: 10px;
        }

        .d-flex {
            display: flex;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <div class="container-fluid form-card">
            <div class="heading">
                <h3>Employee Loan Register</h3>
                <div class="d-flex">
                    <asp:Button ID="btnShow" runat="server" CssClass="btn btn-sm btn-success" Text="Show" OnClick="btnShow_Click" />
                    <asp:Button ID="btnBack" runat="server" CssClass="btn btn-sm btn-success" Text="Back" OnClick="btnBack_Click" />
                </div>
            </div>
            <hr />
            <div class="row" style="margin-top: 30px;">
                <div class="col-md-12">
                    <div class="form-group row mb-2">
                        <div class="col-md-1">
                            <label>Emp Name</label>
                        </div>
                        <div class="col-md-2">
                            <asp:DropDownList ID="ddlEMPID" runat="server" CssClass="form-control" Style="margin: -7px;">
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-1">
                            <label>Type</label>
                        </div>
                        <div class="col-md-2">
                            <asp:DropDownList ID="ddlLoantype" runat="server" CssClass="form-control" Style="margin: -7px;">
                                <asp:ListItem Value="Select Type">Select Type</asp:ListItem>
                                <asp:ListItem Value="OPENING">OPENING</asp:ListItem>
                                <asp:ListItem Value="TRANSACTION">TRANSACTION</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-1">
                            <label>From Date</label>
                        </div>
                        <div class="col-md-2">
                            <input type="date" id="txtFromDate" class="form-control form-control-sm bg-white" runat="server" style="margin: -7px;" />
                        </div>
                        <div class="col-md-1">
                            <label>To Date</label>
                        </div>
                        <div class="col-md-2">
                            <input type="date" id="txtToDate" class="form-control form-control-sm bg-white" runat="server" style="margin: -7px;" />
                        </div>
                        <div class="col-md-12 mt-5">
                            <rsweb:ReportViewer ID="RVViewer" runat="server" Width="100%" ZoomPercent="100" Height="100%">
                            </rsweb:ReportViewer>
                        </div>
                    </div>
                </div>
            </div>
            <%-- <hr />
            <div class="form-group row mb-2">
                
            </div>--%>
        </div>

    </form>

</body>
</html>
