<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BonusReport.aspx.cs" Inherits="MendinePayroll.UI.Reports.BonusReport" %>


<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Payroll | Bonus Register</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC" crossorigin="anonymous" />

    <script src="../Contents/js/jquery-3.3.1.js"></script>
    <link href="../Contents/css/sumoselect.css" rel="stylesheet" type="text/css" />
    <script src="../Contents/js/jquery.sumoselect.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            setallsumo();
            function setallsumo() {

                $(<%=LstDepartment.ClientID%>).SumoSelect({ selectAll: true, okCancelInMulti: true, placeholder: 'SELECT DEPARTMENT' });


            }

        });
    </script>
    <style type="text/css">
        .SumoSelect {
            width: 100%;
        }

            .SumoSelect .select-all {
                height: 40px;
            }

            .SumoSelect > .CaptionCont > span.placeholder {
                font-style: normal;
                text-transform: uppercase;
                font-size: 13px;
                color: #666;
            }

            .SumoSelect > .CaptionCont > span {
                padding-right: 10px;
            }

            .SumoSelect > .CaptionCont {
                border: 1px solid #ced4da;
                border-radius: 0.2rem;
            }

            .SumoSelect .select-all.partial > span i, .SumoSelect .select-all.selected > span i, .SumoSelect > .optWrapper.multiple > .options li.opt.selected span i {
                background-color: #1C1854;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>
        <div class="container-fluid">
            <div class="row d-flex justify-content-center small">
                <div class="col-md-11">
                    <div class="card mt-3 mb-3">
                        <div class="card-header bg-white d-flex justify-content-between">
                            <div>
                                <h5>Salary Register</h5>
                            </div>
                            <div>
                                <asp:Button ID="btnShow" runat="server" CssClass="btn btn-sm btn-primary" Text="Show" OnClick="btnShow_Click" />
                                <asp:Button ID="btnBack" runat="server" CssClass="btn btn-sm btn-primary" Text="Back" OnClick="btnBack_Click" />
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row mb-1">
                                <div class="col-md-2">
                                    <label>Financial Year </label>
                                </div>
                                <div class="col-md-2">
                                    <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-select form-select-sm">
                                    </asp:DropDownList>
                                </div>

                                <div class="col-md-1">
                                    <label>Department </label>
                                </div>
                                <div class="col-md-3">
                                    <asp:ListBox ID="LstDepartment" runat="server" SelectionMode="Multiple" CssClass="form-control form-control-sm"></asp:ListBox>
                                </div>
                                <div class="col-md-1">
                                    <label>Designation </label>
                                </div>
                                <div class="col-md-3">
                                    <asp:DropDownList ID="ddlDesignation" runat="server" CssClass="form-select form-select-sm">
                                    </asp:DropDownList>
                                </div>

                                <div class="col-md-1 mt-2">
                                    <label>Location </label>
                                </div>
                                <div class="col-md-3 mt-2">
                                    <asp:DropDownList ID="ddlLocation" runat="server" CssClass="form-select form-select-sm">
                                    </asp:DropDownList>
                                </div>

                                <div class="col-md-1 mt-2">
                                    <label>Company </label>
                                </div>
                                <div class="col-md-3 mt-2">
                                    <asp:DropDownList ID="ddlCompany" runat="server" CssClass="form-select form-select-sm">
                                    </asp:DropDownList>
                                </div>


                                <div class="col-md-1 mt-2">
                                    <label>Employee</label>
                                </div>
                                <div class="col-md-3 mt-2">
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

