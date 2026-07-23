using Common.Utility;
using MendinePayroll.UI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;

public class PayslipPdfService
{
    private static readonly string DefaultBrandColor = "#167d80";

    public byte[] GeneratePayslipPdf(
        ControllerContext context,
        string viewName,
        PayslipViewModel model,
        string fileName)
    {
        string normalizedViewName = NormalizeViewName(viewName);
        string viewPath = "~/Views/Payslip/" + normalizedViewName + ".cshtml";
        string physicalViewPath = HostingEnvironment.MapPath(viewPath);

        try
        {
            clsDatabase.fnErrorLog(
                "PayslipPdfServiceStart",
                $"FileName={fileName}; ViewName={normalizedViewName}; ViewExists={File.Exists(physicalViewPath)}; EmpId={model?.EmpId}; EmpCode={model?.EmpCode}; Month={model?.Month}; Year={model?.Year}; TenantId={model?.TenantId}");

            byte[] logoBytes = TryDownloadLogo(model.CompanyLogoUrl);
            bool isInicio = IsInicioView(normalizedViewName);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    if (model.EmpId == 0 || model.NetPay == 0)
                    {
                        page.Margin(10, Unit.Millimetre);
                        page.Content().AlignMiddle().AlignCenter().Text(
                            !string.IsNullOrWhiteSpace(model.UnavailableMessage)
                                ? model.UnavailableMessage
                                : $"No payslip generated for {model.Month} {model.Year}")
                            .FontSize(12).Bold().FontColor(Colors.Red.Darken2);
                        return;
                    }

                    if (isInicio)
                    {
                        ComposeInicioPage(page, model, logoBytes);
                    }
                    else
                    {
                        ComposeDefaultPage(page, model, logoBytes);
                    }
                });
            });

            byte[] bytes = document.GeneratePdf();

            if (bytes == null || bytes.Length == 0)
                throw new Exception("QuestPDF produced an empty PDF.");

            clsDatabase.fnErrorLog(
                "PayslipPdfServiceSuccess",
                $"FileName={fileName}; ViewName={normalizedViewName}; Bytes={bytes.Length}; EmpId={model?.EmpId}; EmpCode={model?.EmpCode}; Month={model?.Month}; Year={model?.Year}");

            return bytes;
        }
        catch (Exception ex)
        {
            clsDatabase.fnErrorLog(
                "PayslipPdfServiceError",
                $"FileName={fileName}; ViewName={normalizedViewName}; ViewExists={File.Exists(physicalViewPath)}; EmpId={model?.EmpId}; EmpCode={model?.EmpCode}; Month={model?.Month}; Year={model?.Year}; TenantId={model?.TenantId}; ErrorType={ex.GetType().FullName}; Message={ex.Message}; Stack={ex}");
            throw;
        }
    }

    private string NormalizeViewName(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            return "Index";

        viewName = viewName.Replace("\\", "/").Trim();

        if (viewName.StartsWith("~/Views/Payslip/", StringComparison.OrdinalIgnoreCase))
            viewName = viewName.Substring("~/Views/Payslip/".Length);

        if (viewName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
            viewName = viewName.Substring(0, viewName.Length - ".cshtml".Length);

        int slashIndex = viewName.LastIndexOf('/');
        if (slashIndex >= 0)
            viewName = viewName.Substring(slashIndex + 1);

        return string.IsNullOrWhiteSpace(viewName) ? "Index" : viewName;
    }

    private bool IsInicioView(string viewName)
    {
        return string.Equals(viewName, "Index_Inicio", StringComparison.OrdinalIgnoreCase)
            || string.Equals(viewName, "Inicio_Index", StringComparison.OrdinalIgnoreCase);
    }

    private void ComposeDefaultPage(PageDescriptor page, PayslipViewModel model, byte[] logoBytes)
    {
        page.Margin(4, Unit.Millimetre);
        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));
        page.Header().Element(c => ComposeDefaultHeader(c, model, logoBytes));
        page.Content().Element(c => ComposeDefaultContent(c, model));
        page.Footer().Element(ComposeDefaultFooter);
    }

    private void ComposeDefaultHeader(IContainer container, PayslipViewModel model, byte[] logoBytes)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Column(inner =>
            {
                if (logoBytes != null)
                    inner.Item().AlignCenter().Width(70).Image(logoBytes).FitWidth();

                inner.Item().AlignCenter().PaddingTop(2).Text(model.CompanyName ?? "")
                    .FontSize(12).Bold();

                if (!string.IsNullOrWhiteSpace(model.CompanyAddressLine1))
                    inner.Item().AlignCenter().Text(model.CompanyAddressLine1).FontSize(7);

                if (!string.IsNullOrWhiteSpace(model.CompanyAddressLine2))
                    inner.Item().AlignCenter().Text(model.CompanyAddressLine2).FontSize(7);
            });

            column.Item().PaddingTop(3).BorderBottom(1).BorderColor(DefaultBrandColor);

            column.Item().PaddingTop(3).Background(DefaultBrandColor).Padding(3)
                .AlignCenter()
                .Text($"Payslip for {model.Month} {model.Year} (Confidential)")
                .FontColor(Colors.White).Bold().FontSize(9);
        });
    }

    private void ComposeDefaultContent(IContainer container, PayslipViewModel model)
    {
        container.PaddingTop(5).Column(column =>
        {
            column.Spacing(5);

            column.Item().Element(c => DefaultSectionTitle(c, "Employee Details"));
            column.Item().Element(c => DefaultEmployeeDetailsTable(c, model));

            column.Item().Element(c => DefaultSectionTitle(c, "Attendance Summary"));
            column.Item().Element(c => DefaultAttendanceTable(c, model));

            if (model.LeaveBalances != null && model.LeaveBalances.Any())
            {
                column.Item().Element(c => DefaultSectionTitle(c, "Leave Balance"));
                column.Item().Element(c => LeaveBalanceTable(c, model));
            }

            if (model.LoanInstallment > 0 || model.OpeningLoan > 0 || model.ClosingLoan > 0)
            {
                column.Item().Element(c => DefaultSectionTitle(c, "Loan Details"));
                column.Item().Element(c => LoanTable(c, model, true));
            }

            column.Item().Element(c => DefaultSectionTitle(c, "Salary Details"));
            column.Item().Element(c => DefaultSalaryTable(c, model));

            if (model.ShowNetPay)
            {
                column.Item().Element(c => DefaultSectionTitle(c, "Net Salary"));
                column.Item().Element(c => DefaultNetPayTable(c, model));
            }
        });
    }

    private void DefaultSectionTitle(IContainer container, string title)
    {
        container.Background(DefaultBrandColor).Padding(3)
            .Text(title).FontColor(Colors.White).Bold().FontSize(8);
    }

    private void DefaultEmployeeDetailsTable(IContainer container, PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1.2f);
                c.RelativeColumn(1.5f);
                c.RelativeColumn(1.2f);
                c.RelativeColumn(1.5f);
            });

            AddDefaultRow(table, "Employee Name", model.EmpName, "Employee Code", model.EmpCode);
            AddDefaultRow(table, "Designation", model.Designation, "Department", model.Department);
            AddDefaultRow(table, "Date of Joining", model.DOJ.ToString("dd-MMM-yyyy"), "Bank A/C", model.BankAccount);
            AddDefaultRow(table, "Bank Name", model.BankName, "IFSC Code", model.IFSCCode);
            AddDefaultRow(table, "PAN", model.PAN, "UAN", model.UAN);
            AddDefaultRow(table, "PF No", model.PFNo, "ESIC No", model.ESICNo);
        });
    }

    private void AddDefaultRow(TableDescriptor table, string label1, string value1, string label2, string value2)
    {
        table.Cell().Element(DefaultLabelCell).Text(label1 ?? "");
        table.Cell().Element(DefaultValueCell).Text(value1 ?? "");
        table.Cell().Element(DefaultLabelCell).Text(label2 ?? "");
        table.Cell().Element(DefaultValueCell).Text(value2 ?? "");
    }

    private void DefaultAttendanceTable(IContainer container, PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                for (int i = 0; i < 5; i++) c.RelativeColumn();
            });

            foreach (var h in new[] { "Working Days", "Weekly Off/Holiday", "Leave Taken", "Pay Days", "LWP" })
                table.Cell().Element(DefaultHeaderCell).AlignCenter().Text(h);

            foreach (var v in new[] { model.WorkedDays, model.WeeklyOffHoliday, model.LeaveTaken, model.PayableDays, model.LOPDays })
                table.Cell().Element(DefaultValueCell).AlignCenter().Text(v.ToString("N2"));
        });
    }

    private void DefaultSalaryTable(IContainer container, PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1.5f);
                c.RelativeColumn(1f);
                c.RelativeColumn(1.5f);
                c.RelativeColumn(1f);
            });

            foreach (var h in new[] { "Earnings", "Amount (Rs.)", "Deductions", "Amount (Rs.)" })
                table.Cell().Element(DefaultHeaderCell).Text(h).Bold();

            int rows = Math.Max(model.Earnings.Count, model.Deductions.Count);
            for (int i = 0; i < rows; i++)
            {
                table.Cell().Element(DefaultValueCell).Text(i < model.Earnings.Count ? model.Earnings[i].PayConfigName : "");
                table.Cell().Element(DefaultValueCell).AlignRight().Text(i < model.Earnings.Count ? model.Earnings[i].Amount.ToString("N2") : "");
                table.Cell().Element(DefaultValueCell).Text(i < model.Deductions.Count ? model.Deductions[i].PayConfigName : "");
                table.Cell().Element(DefaultValueCell).AlignRight().Text(i < model.Deductions.Count ? model.Deductions[i].Amount.ToString("N2") : "");
            }

            table.Cell().Element(DefaultValueCell).Text("Gross Earnings").Bold();
            table.Cell().Element(DefaultValueCell).AlignRight().Text(model.GrossEarnings.ToString("N2")).Bold();
            table.Cell().Element(DefaultValueCell).Text("Total Deductions").Bold();
            table.Cell().Element(DefaultValueCell).AlignRight().Text(model.TotalDeductions.ToString("N2")).Bold();
        });
    }

    private void DefaultNetPayTable(IContainer container, PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
            });

            table.Cell().Element(DefaultValueCell).Text("Net Pay (Take Home)").FontColor(DefaultBrandColor).Bold().FontSize(10);
            table.Cell().Element(DefaultValueCell).AlignRight().Text($"Rs. {model.NetPay:N2}").FontColor(DefaultBrandColor).Bold().FontSize(10);
            table.Cell().Element(DefaultValueCell).Text("Amount in Words");
            table.Cell().Element(DefaultValueCell).Text($"{model.NetPayWords} Only");
        });
    }

    private void ComposeDefaultFooter(IContainer container)
    {
        container.PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Lighten1)
            .PaddingTop(2).AlignCenter().Text("Confidential: This is a system generated payslip and does not require signature. Figures rounded to nearest rupee.")
            .FontSize(6).FontColor(Colors.Grey.Darken1);
    }

    private void ComposeInicioPage(PageDescriptor page, PayslipViewModel model, byte[] logoBytes)
    {
        page.Margin(0);
        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));
        page.Content().Element(c => ComposeInicioPayslip(c, model, logoBytes));
    }

    private void ComposeInicioPayslip(IContainer container, PayslipViewModel model, byte[] logoBytes)
    {
        container
            .Padding(10, Unit.Millimetre)
            .Border(1.5f)
            .BorderColor(Colors.Black)
            .PaddingHorizontal(8, Unit.Millimetre)
            .PaddingVertical(6, Unit.Millimetre)
            .Column(column =>
            {
                column.Item().Element(c => ComposeInicioHeader(c, model, logoBytes));
                column.Item().PaddingTop(8).Element(c => InicioInfoTable(c, model));

                if (model.LeaveBalances != null && model.LeaveBalances.Any())
                    column.Item().PaddingTop(8).Element(c => LeaveBalanceTable(c, model));

                if (model.LoanInstallment > 0 || model.OpeningLoan > 0 || model.ClosingLoan > 0)
                    column.Item().PaddingTop(8).Element(c => LoanTable(c, model, false));

                column.Item().PaddingTop(8).Element(c => InicioSalaryTable(c, model));

                if (model.ShowNetPay)
                    column.Item().PaddingTop(8).Element(c => InicioNetPayBlock(c, model));

                column.Item().ExtendVertical().AlignBottom().Element(ComposeInicioFooter);
            });
    }

    private void ComposeInicioHeader(IContainer container, PayslipViewModel model, byte[] logoBytes)
    {
        container.BorderBottom(1.5f).BorderColor(Colors.Black).PaddingBottom(6).Row(row =>
        {
            row.ConstantItem(115).AlignMiddle().Element(c =>
            {
                if (logoBytes != null)
                    c.MaxWidth(105).MaxHeight(88).Image(logoBytes).FitArea();
            });

            row.RelativeItem().AlignCenter().Column(inner =>
            {
                inner.Item().AlignCenter().Text(model.CompanyName ?? "").FontSize(19).Bold();

                if (!string.IsNullOrWhiteSpace(model.CompanyAddressLine1))
                    inner.Item().AlignCenter().Text(model.CompanyAddressLine1).FontSize(10.5f).Bold();

                if (!string.IsNullOrWhiteSpace(model.CompanyAddressLine2))
                    inner.Item().AlignCenter().Text(model.CompanyAddressLine2).FontSize(10.5f).Bold();

                inner.Item().AlignCenter().PaddingTop(6)
                    .Text($"Salary Slip for the month of {model.Month}-{model.Year}")
                    .FontSize(13).Bold();
            });

            row.ConstantItem(115);
        });
    }

    private void InicioInfoTable(IContainer container, PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1.05f);
                c.RelativeColumn(1.65f);
                c.RelativeColumn(1.05f);
                c.RelativeColumn(1.65f);
                c.RelativeColumn(1.05f);
                c.RelativeColumn(1.65f);
            });

            AddInicioInfoRow(table, "Name :", model.EmpName, "Emp Code :", model.EmpCode, "Designation :", model.Designation);
            AddInicioInfoRow(table, "Department:", model.Department, "DOJ :", model.DOJ.ToString("dd-MM-yyyy"), "PAN :", model.PAN);
            AddInicioInfoRow(table, "PF No :", model.PFNo, "UAN No. :", model.UAN, "ESIC No :", model.ESICNo);
            AddInicioInfoRow(table, "A/C No :", model.BankAccount, "Bank Name:", model.BankName, "IFSC Code:", model.IFSCCode);
            AddInicioInfoRow(table, "Total Days:", model.TotalDays.ToString("N2"), "Worked Days :", model.WorkedDays.ToString("N2"), "Payable Days :", model.PayableDays.ToString("N2"));
            AddInicioInfoRow(table, "Weekly Off/Hol:", model.WeeklyOffHoliday.ToString("N2"), "LWP :", model.LOPDays.ToString("N2"), "Leave Taken :", model.LeaveTaken.ToString("N2"));
        });
    }

    private void AddInicioInfoRow(
        TableDescriptor table,
        string label1,
        string value1,
        string label2,
        string value2,
        string label3,
        string value3)
    {
        table.Cell().Element(InicioLabelCell).Text(label1 ?? "");
        table.Cell().Element(InicioValueCell).Text(value1 ?? "");
        table.Cell().Element(InicioLabelCell).Text(label2 ?? "");
        table.Cell().Element(InicioValueCell).Text(value2 ?? "");
        table.Cell().Element(InicioLabelCell).Text(label3 ?? "");
        table.Cell().Element(InicioValueCell).Text(value3 ?? "");
    }

    private void InicioSalaryTable(IContainer container, PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(1.55f);
                c.RelativeColumn(1f);
                c.RelativeColumn(1f);
                c.RelativeColumn(1.55f);
                c.RelativeColumn(1f);
                c.RelativeColumn(1f);
            });

            table.Cell().ColumnSpan(3).Element(InicioHeaderCell).AlignCenter().Text("Earning").Bold();
            table.Cell().ColumnSpan(3).Element(InicioHeaderCell).AlignCenter().Text("Deductions").Bold();

            foreach (var h in new[] { "Head", "Amount", "Payable", "Head", "Amount", "Deductions" })
                table.Cell().Element(InicioHeaderCell).AlignCenter().Text(h);

            int rows = Math.Max(model.Earnings.Count, model.Deductions.Count);
            for (int i = 0; i < rows; i++)
            {
                table.Cell().Element(InicioValueCell).Text(i < model.Earnings.Count ? model.Earnings[i].PayConfigName : "");
                table.Cell().Element(InicioValueCell).AlignRight().Text(i < model.Earnings.Count ? model.Earnings[i].ActualAmount.ToString("N2") : "");
                table.Cell().Element(InicioValueCell).AlignRight().Text(i < model.Earnings.Count ? model.Earnings[i].Amount.ToString("N2") : "");
                table.Cell().Element(InicioValueCell).Text(i < model.Deductions.Count ? model.Deductions[i].PayConfigName : "");
                table.Cell().Element(InicioValueCell).AlignRight().Text(i < model.Deductions.Count ? model.Deductions[i].ActualAmount.ToString("N2") : "");
                table.Cell().Element(InicioValueCell).AlignRight().Text(i < model.Deductions.Count ? model.Deductions[i].Amount.ToString("N2") : "");
            }

            table.Cell().Element(InicioValueCell).Text("Total Earnings").Bold();
            table.Cell().Element(InicioValueCell).Text("").Bold();
            table.Cell().Element(InicioValueCell).AlignRight().Text(model.GrossEarnings.ToString("N2")).Bold();
            table.Cell().Element(InicioValueCell).Text("Total Deductions").Bold();
            table.Cell().Element(InicioValueCell).Text("").Bold();
            table.Cell().Element(InicioValueCell).AlignRight().Text(model.TotalDeductions.ToString("N2")).Bold();
        });
    }

    private void InicioNetPayBlock(IContainer container, PayslipViewModel model)
    {
        container.Column(column =>
        {
            column.Item().Text(text =>
            {
                text.Span("Net Payble (INR)   ").FontSize(12);
                text.Span(model.NetPay.ToString("N2")).FontSize(13).Bold();
            });
            column.Item().PaddingTop(4).Text(text =>
            {
                text.Span("Net Payble INR (In Words):  ").FontSize(11);
                text.Span($"{model.NetPayWords} Only").FontSize(11).Bold();
            });
        });
    }

    private void ComposeInicioFooter(IContainer container)
    {
        container.AlignCenter().Text("This is system generated Document signature not require.")
            .FontSize(10.5f);
    }

    private void LeaveBalanceTable(IContainer container, PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                foreach (var _ in model.LeaveBalances) c.RelativeColumn();
            });

            foreach (var lb in model.LeaveBalances)
                table.Cell().Element(InicioHeaderCell).AlignCenter().Text(lb.CodeType);

            foreach (var lb in model.LeaveBalances)
                table.Cell().Element(InicioValueCell).AlignCenter().Text(lb.Balance.ToString("N2"));
        });
    }

    private void LoanTable(IContainer container, PayslipViewModel model, bool showCurrency)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
                c.RelativeColumn();
            });

            foreach (var h in new[] { "Opening Loan Balance", "Current Installment", "Closing Loan Balance" })
                table.Cell().Element(InicioHeaderCell).AlignCenter().Text(h);

            string prefix = showCurrency ? "Rs. " : "";
            table.Cell().Element(InicioValueCell).AlignCenter().Text(prefix + model.OpeningLoan.ToString("N2"));
            table.Cell().Element(InicioValueCell).AlignCenter().Text(prefix + model.LoanInstallment.ToString("N2"));
            table.Cell().Element(InicioValueCell).AlignCenter().Text(prefix + model.ClosingLoan.ToString("N2"));
        });
    }

    private static IContainer DefaultLabelCell(IContainer c) =>
        c.Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Padding(2);

    private static IContainer DefaultValueCell(IContainer c) =>
        c.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(2);

    private static IContainer DefaultHeaderCell(IContainer c) =>
        c.Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).Padding(2);

    private static IContainer InicioLabelCell(IContainer c) =>
        c.Border(1).BorderColor(Colors.Black).PaddingHorizontal(6).PaddingVertical(3);

    private static IContainer InicioValueCell(IContainer c) =>
        c.Border(1).BorderColor(Colors.Black).PaddingHorizontal(6).PaddingVertical(3);

    private static IContainer InicioHeaderCell(IContainer c) =>
        c.Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten4).PaddingHorizontal(6).PaddingVertical(3);

    private byte[] TryDownloadLogo(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        try
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(url);
            }
        }
        catch
        {
            return null;
        }
    }
}
