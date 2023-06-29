<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PaySlip.aspx.cs" Inherits="MendinePayroll.UI.Reports.PaySlip" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845DCD8080CC91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<!DOCTYPE html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <!--<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" />
<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css" />
<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>-->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@4.6.1/dist/css/bootstrap.min.css" integrity="sha384-zCbKRCUGaJDkqS1kPbPd7TveP5iyJE0EjAuZQTgFLD2ylzuqKfdKlfG/eSrtxUkn" crossorigin="anonymous">
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
                                <h3 style="font-weight: bold; font-style: inherit">Salary Slip</h3>
                            </div>
                            <div class="col-md-4 text-right">
                             
                                <select class="btn btn-primary" runat="server" id="ddlDownloadItem">
                                    <option value="SELECT" selected>SELECT</option>
                                    <option class="bg-light text-dark" value="PDF">PDF</option>
                                    <option class="bg-light text-dark" value="WORD">WORD</option>
                                    <option class="bg-light text-dark" value="EXCEL">EXCEL</option>
                                </select>
                                <asp:Button ID="btnDownload" runat="server" CssClass="btn btn-sm btn-info active" Text="DOWNLOAD" Style="padding: 6px 13px;" OnClick="btnDownload_Click" />
                            </div>
                        </div>
                        <hr />
                        <div class="form-group row mb-2" style="align-content: center">
                            <div class="col-md-12">

                                <rsweb:ReportViewer ID="RVViewer" runat="server" Width="100%" ZoomPercent="100" Height="100%" ShowToolBar="false">
                                </rsweb:ReportViewer>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </form>
</body>
