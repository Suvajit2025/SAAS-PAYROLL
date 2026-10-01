using Common.Utility;
using MendinePayroll.UI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;

public class PayslipPdfService
{
    private static readonly string DefaultBrandColor = "#167d80";


    // ============================================================
    // GENERATE PDF
    // ============================================================

    public byte[] GeneratePayslipPdf(
        ControllerContext context,
        string viewName,
        PayslipViewModel model,
        string fileName)
    {
        string normalizedViewName =
            NormalizeViewName(viewName);

        string viewPath =
            "~/Views/Payslip/" +
            normalizedViewName +
            ".cshtml";

        string physicalViewPath =
            HostingEnvironment.MapPath(viewPath);

        try
        {
            clsDatabase.fnErrorLog(
                "PayslipPdfServiceStart",
                $"FileName={fileName}; " +
                $"ViewName={normalizedViewName}; " +
                $"ViewExists={File.Exists(physicalViewPath)}; " +
                $"EmpId={model?.EmpId}; " +
                $"EmpCode={model?.EmpCode}; " +
                $"Month={model?.Month}; " +
                $"Year={model?.Year}; " +
                $"TenantId={model?.TenantId}"
            );

            byte[] logoBytes =
                TryDownloadLogo(
                    model?.CompanyLogoUrl);

            bool isInicio =
                IsInicioView(
                    normalizedViewName);

            var document =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);

                        page.DefaultTextStyle(
                            x => x.FontFamily("Arial"));

                        if (model == null ||
                            model.EmpId == 0 ||
                            model.NetPay == 0)
                        {
                            page.Margin(
                                10,
                                Unit.Millimetre);

                            page.Content()
                                .AlignMiddle()
                                .AlignCenter()
                                .Text(
                                    !string.IsNullOrWhiteSpace(
                                        model?.UnavailableMessage)
                                        ? model.UnavailableMessage
                                        : $"No payslip generated for {model?.Month} {model?.Year}"
                                )
                                .FontSize(12)
                                .Bold()
                                .FontColor(
                                    Colors.Red.Darken2);

                            return;
                        }

                        if (isInicio)
                        {
                            ComposeInicioPage(
                                page,
                                model,
                                logoBytes);
                        }
                        else
                        {
                            ComposeDefaultPage(
                                page,
                                model,
                                logoBytes);
                        }
                    });
                });

            byte[] pdfBytes =
                document.GeneratePdf();

            if (pdfBytes == null ||
                pdfBytes.Length == 0)
            {
                throw new Exception(
                    "QuestPDF produced an empty PDF.");
            }

            clsDatabase.fnErrorLog(
                "PayslipPdfServiceSuccess",
                $"FileName={fileName}; " +
                $"ViewName={normalizedViewName}; " +
                $"Bytes={pdfBytes.Length}; " +
                $"EmpId={model?.EmpId}; " +
                $"EmpCode={model?.EmpCode}"
            );

            return pdfBytes;
        }
        catch (Exception ex)
        {
            clsDatabase.fnErrorLog(
                "PayslipPdfServiceError",
                $"FileName={fileName}; " +
                $"ViewName={normalizedViewName}; " +
                $"EmpId={model?.EmpId}; " +
                $"EmpCode={model?.EmpCode}; " +
                $"Error={ex}"
            );

            throw;
        }
    }



    // ============================================================
    // VIEW NAME
    // ============================================================

    private string NormalizeViewName(
        string viewName)
    {
        if (string.IsNullOrWhiteSpace(
            viewName))
        {
            return "Index";
        }

        viewName =
            viewName
                .Replace("\\", "/")
                .Trim();

        if (viewName.StartsWith(
            "~/Views/Payslip/",
            StringComparison.OrdinalIgnoreCase))
        {
            viewName =
                viewName.Substring(
                    "~/Views/Payslip/".Length);
        }

        if (viewName.EndsWith(
            ".cshtml",
            StringComparison.OrdinalIgnoreCase))
        {
            viewName =
                viewName.Substring(
                    0,
                    viewName.Length -
                    ".cshtml".Length);
        }

        int slashIndex =
            viewName.LastIndexOf('/');

        if (slashIndex >= 0)
        {
            viewName =
                viewName.Substring(
                    slashIndex + 1);
        }

        return
            string.IsNullOrWhiteSpace(
                viewName)
                ? "Index"
                : viewName;
    }


    private bool IsInicioView(
        string viewName)
    {
        return
            string.Equals(
                viewName,
                "Index_Inicio",
                StringComparison.OrdinalIgnoreCase)

            ||

            string.Equals(
                viewName,
                "Inicio_Index",
                StringComparison.OrdinalIgnoreCase);
    }



    // ============================================================
    // DEFAULT PAYSLIP
    // ============================================================

    //private void ComposeDefaultPage(
    //    PageDescriptor page,
    //    PayslipViewModel model,
    //    byte[] logoBytes)
    //{
    //    page.Margin(
    //        4,
    //        Unit.Millimetre);

    //    page.DefaultTextStyle(
    //        x => x
    //            .FontSize(8)
    //            .FontFamily("Arial"));

    //    page.Header()
    //        .Element(
    //            c =>
    //                ComposeDefaultHeader(
    //                    c,
    //                    model,
    //                    logoBytes));

    //    page.Content()
    //        .Element(
    //            c =>
    //                ComposeDefaultContent(
    //                    c,
    //                    model));

    //    page.Footer()
    //        .Element(
    //            ComposeDefaultFooter);
    //}
    private void ComposeDefaultPage(
    PageDescriptor page,
    PayslipViewModel model,
    byte[] logoBytes)
    {
        page.Size(PageSizes.A4);

        // Razor print version uses approximately 5mm page padding
        page.Margin(5, Unit.Millimetre);

        page.DefaultTextStyle(x => x
            .FontFamily("Arial")
            .FontSize(8.6f)
            .FontColor("#333333"));

        page.Content()
            .Column(column =>
            {
                column.Spacing(0);

                // HEADER
                column.Item()
                    .Element(c =>
                        ComposeDefaultHeader(
                            c,
                            model,
                            logoBytes));

                // EMPLOYEE DETAILS
                column.Item()
                    .PaddingTop(6)
                    .Element(c =>
                        DefaultSectionTitle(
                            c,
                            "Employee Details"));

                column.Item()
                    .PaddingTop(3)
                    .Element(c =>
                        DefaultEmployeeDetailsTable(
                            c,
                            model));

                // ATTENDANCE
                column.Item()
                    .PaddingTop(6)
                    .Element(c =>
                        DefaultSectionTitle(
                            c,
                            "Attendance Summary"));

                column.Item()
                    .PaddingTop(3)
                    .Element(c =>
                        DefaultAttendanceTable(
                            c,
                            model));

                // LEAVE
                if (model.LeaveBalances != null &&
                    model.LeaveBalances.Any())
                {
                    column.Item()
                        .PaddingTop(6)
                        .Element(c =>
                            DefaultSectionTitle(
                                c,
                                "Leave Balance"));

                    column.Item()
                        .PaddingTop(3)
                        .Element(c =>
                            DefaultLeaveBalanceTable(
                                c,
                                model));
                }

                // LOAN
                if (model.LoanInstallment > 0 ||
                    model.OpeningLoan > 0 ||
                    model.ClosingLoan > 0)
                {
                    column.Item()
                        .PaddingTop(6)
                        .Element(c =>
                            DefaultSectionTitle(
                                c,
                                "Loan Details"));

                    column.Item()
                        .PaddingTop(3)
                        .Element(c =>
                            DefaultLoanTable(
                                c,
                                model));
                }

                // SALARY
                column.Item()
                    .PaddingTop(6)
                    .Element(c =>
                        DefaultSectionTitle(
                            c,
                            "Salary Details"));

                column.Item()
                    .PaddingTop(3)
                    .Element(c =>
                        DefaultSalaryTable(
                            c,
                            model));

                // NET SALARY
                if (model.ShowNetPay)
                {
                    column.Item()
                        .PaddingTop(6)
                        .Element(c =>
                            DefaultSectionTitle(
                                c,
                                "Net Salary"));

                    column.Item()
                        .PaddingTop(3)
                        .Element(c =>
                            DefaultNetPayTable(
                                c,
                                model));
                }

                // FOOTER
                column.Item()
                    .PaddingTop(8)
                    .Element(ComposeDefaultFooter);
            });
    }
    private void DefaultLeaveBalanceTable(
    IContainer container,
    PayslipViewModel model)
    {
        if (model.LeaveBalances == null ||
            !model.LeaveBalances.Any())
        {
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var leave in model.LeaveBalances)
                {
                    columns.RelativeColumn();
                }
            });

            // HEADER
            foreach (var leave in model.LeaveBalances)
            {
                table.Cell()
                    .Element(DefaultHeaderCell)
                    .AlignCenter()
                    .Text(leave.CodeType ?? "")
                    .SemiBold();
            }

            // BALANCE
            foreach (var leave in model.LeaveBalances)
            {
                table.Cell()
                    .Element(DefaultValueCell)
                    .AlignCenter()
                    .Text(leave.Balance.ToString("N2"));
            }
        });
    }
    private void DefaultLoanTable(
    IContainer container,
    PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            foreach (var header in new[]
            {
            "Opening Loan Balance",
            "Current Installment",
            "Closing Loan Balance"
        })
            {
                table.Cell()
                    .Element(DefaultHeaderCell)
                    .AlignCenter()
                    .Text(header)
                    .SemiBold();
            }

            table.Cell()
                .Element(DefaultValueCell)
                .Text($"₹ {model.OpeningLoan:N2}");

            table.Cell()
                .Element(DefaultValueCell)
                .Text($"₹ {model.LoanInstallment:N2}");

            table.Cell()
                .Element(DefaultValueCell)
                .Text($"₹ {model.ClosingLoan:N2}");
        });
    }
    //private void ComposeDefaultHeader(
    //    IContainer container,
    //    PayslipViewModel model,
    //    byte[] logoBytes)
    //{
    //    container.Column(column =>
    //    {
    //        column.Item()
    //            .AlignCenter()
    //            .Column(inner =>
    //            {
    //                if (logoBytes != null)
    //                {
    //                    inner.Item()
    //                        .AlignCenter()
    //                        .Width(70)
    //                        .Image(logoBytes)
    //                        .FitWidth();
    //                }

    //                inner.Item()
    //                    .AlignCenter()
    //                    .PaddingTop(2)
    //                    .Text(
    //                        model.CompanyName ?? "")
    //                    .FontSize(12)
    //                    .Bold();

    //                if (!string.IsNullOrWhiteSpace(
    //                    model.CompanyAddressLine1))
    //                {
    //                    inner.Item()
    //                        .AlignCenter()
    //                        .Text(
    //                            model.CompanyAddressLine1)
    //                        .FontSize(7);
    //                }

    //                if (!string.IsNullOrWhiteSpace(
    //                    model.CompanyAddressLine2))
    //                {
    //                    inner.Item()
    //                        .AlignCenter()
    //                        .Text(
    //                            model.CompanyAddressLine2)
    //                        .FontSize(7);
    //                }
    //            });

    //        column.Item()
    //            .PaddingTop(3)
    //            .BorderBottom(1)
    //            .BorderColor(
    //                DefaultBrandColor);

    //        column.Item()
    //            .PaddingTop(3)
    //            .Background(
    //                DefaultBrandColor)
    //            .Padding(3)
    //            .AlignCenter()
    //            .Text(
    //                $"Payslip for {model.Month} {model.Year} (Confidential)")
    //            .FontColor(
    //                Colors.White)
    //            .Bold()
    //            .FontSize(9);
    //    });
    //}

    private void ComposeDefaultHeader(IContainer container,PayslipViewModel model,byte[] logoBytes)
    {
        container.Column(column =>
        {
            column.Spacing(0);

            // LOGO
            if (logoBytes != null &&
                logoBytes.Length > 0)
            {
                column.Item()
                    .AlignCenter()
                    .Width(120)
                    .MaxHeight(55)
                    .Image(logoBytes)
                    .FitArea();
            }

            // COMPANY NAME
            column.Item()
                .PaddingTop(3)
                .AlignCenter()
                .Text(model.CompanyName ?? "")
                .FontFamily("Arial")
                .FontSize(14)
                .Bold()
                .FontColor("#333333");

            // ADDRESS 1
            if (!string.IsNullOrWhiteSpace(
                model.CompanyAddressLine1))
            {
                column.Item()
                    .PaddingTop(2)
                    .AlignCenter()
                    .Text(
                        CleanAddress(
                            model.CompanyAddressLine1))
                    .FontSize(9)
                    .FontColor("#333333");
            }

            // ADDRESS 2
            if (!string.IsNullOrWhiteSpace(
                model.CompanyAddressLine2))
            {
                column.Item()
                    .PaddingTop(1)
                    .AlignCenter()
                    .Text(
                        CleanAddress(
                            model.CompanyAddressLine2))
                    .FontSize(9)
                    .FontColor("#333333");
            }

            // Teal line — same as Razor .header border-bottom
            column.Item()
                .PaddingTop(5)
                .BorderBottom(1.5f)
                .BorderColor(DefaultBrandColor);

            // PAYSLIP TITLE
            column.Item()
                .PaddingTop(5)
                .Background(DefaultBrandColor)
                .PaddingVertical(4)
                .PaddingHorizontal(5)
                .AlignCenter()
                .Text(
                    $"Payslip for {model.Month} {model.Year} (Confidential)")
                .FontSize(9.5f)
                .Bold()
                .FontColor(Colors.White);
        });
    }
    private void ComposeDefaultContent(
        IContainer container,
        PayslipViewModel model)
    {
        container
            .PaddingTop(5)
            .Column(column =>
            {
                column.Spacing(5);

                column.Item()
                    .Element(
                        c =>
                            DefaultSectionTitle(
                                c,
                                "Employee Details"));

                column.Item()
                    .Element(
                        c =>
                            DefaultEmployeeDetailsTable(
                                c,
                                model));

                column.Item()
                    .Element(
                        c =>
                            DefaultSectionTitle(
                                c,
                                "Attendance Summary"));

                column.Item()
                    .Element(
                        c =>
                            DefaultAttendanceTable(
                                c,
                                model));

                if (model.LeaveBalances != null &&
                    model.LeaveBalances.Any())
                {
                    column.Item()
                        .Element(
                            c =>
                                DefaultSectionTitle(
                                    c,
                                    "Leave Balance"));

                    column.Item()
                        .Element(
                            c =>
                                LeaveBalanceTable(
                                    c,
                                    model));
                }

                if (model.LoanInstallment > 0 ||
                    model.OpeningLoan > 0 ||
                    model.ClosingLoan > 0)
                {
                    column.Item()
                        .Element(
                            c =>
                                DefaultSectionTitle(
                                    c,
                                    "Loan Details"));

                    column.Item()
                        .Element(
                            c =>
                                LoanTable(
                                    c,
                                    model,
                                    true));
                }

                column.Item()
                    .Element(
                        c =>
                            DefaultSectionTitle(
                                c,
                                "Salary Details"));

                column.Item()
                    .Element(
                        c =>
                            DefaultSalaryTable(
                                c,
                                model));

                if (model.ShowNetPay)
                {
                    column.Item()
                        .Element(
                            c =>
                                DefaultSectionTitle(
                                    c,
                                    "Net Salary"));

                    column.Item()
                        .Element(
                            c =>
                                DefaultNetPayTable(
                                    c,
                                    model));
                }
            });
    }


    //private void DefaultSectionTitle(
    //    IContainer container,
    //    string title)
    //{
    //    container
    //        .Background(
    //            DefaultBrandColor)
    //        .Padding(3)
    //        .Text(title)
    //        .FontColor(
    //            Colors.White)
    //        .Bold()
    //        .FontSize(8);
    //}
    private void DefaultSectionTitle(
    IContainer container,
    string title)
    {
        container
            .Background(DefaultBrandColor)
            .PaddingVertical(3.5f)
            .PaddingHorizontal(5)
            .Text(title)
            .FontFamily("Arial")
            .FontSize(9)
            .Bold()
            .FontColor(Colors.White);
    }

    //private void DefaultEmployeeDetailsTable(
    //    IContainer container,
    //    PayslipViewModel model)
    //{
    //    container.Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            c.RelativeColumn(1.2f);
    //            c.RelativeColumn(1.5f);
    //            c.RelativeColumn(1.2f);
    //            c.RelativeColumn(1.5f);
    //        });

    //        AddDefaultRow(
    //            table,
    //            "Employee Name",
    //            model.EmpName,
    //            "Employee Code",
    //            model.EmpCode);

    //        AddDefaultRow(
    //            table,
    //            "Designation",
    //            model.Designation,
    //            "Department",
    //            model.Department);

    //        AddDefaultRow(
    //            table,
    //            "Date of Joining",
    //            model.DOJ.ToString(
    //                "dd-MMM-yyyy"),
    //            "Bank A/C",
    //            model.BankAccount);

    //        AddDefaultRow(
    //            table,
    //            "Bank Name",
    //            model.BankName,
    //            "IFSC Code",
    //            model.IFSCCode);

    //        AddDefaultRow(
    //            table,
    //            "PAN",
    //            model.PAN,
    //            "UAN",
    //            model.UAN);

    //        AddDefaultRow(
    //            table,
    //            "PF No",
    //            model.PFNo,
    //            "ESIC No",
    //            model.ESICNo);
    //    });
    //}
    private void DefaultEmployeeDetailsTable(
    IContainer container,
    PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.25f);
                columns.RelativeColumn(1.75f);

                columns.RelativeColumn(1.25f);
                columns.RelativeColumn(1.75f);
            });

            AddDefaultRow(
                table,
                "Employee Name",
                model.EmpName,
                "Employee Code",
                model.EmpCode);

            AddDefaultRow(
                table,
                "Designation",
                model.Designation,
                "Department",
                model.Department);

            AddDefaultRow(
                table,
                "Date of Joining",
                model.DOJ.ToString("dd-MMM-yyyy"),
                "Bank A/C",
                model.BankAccount);

            AddDefaultRow(
                table,
                "Bank Name",
                model.BankName,
                "IFSC Code",
                model.IFSCCode);

            AddDefaultRow(
                table,
                "PAN",
                model.PAN,
                "UAN",
                model.UAN);

            AddDefaultRow(
                table,
                "PF No",
                model.PFNo,
                "ESIC No",
                model.ESICNo);
        });
    }

    private void AddDefaultRow(
        TableDescriptor table,
        string label1,
        string value1,
        string label2,
        string value2)
    {
        table.Cell()
            .Element(DefaultLabelCell)
            .Text(label1 ?? "");

        table.Cell()
            .Element(DefaultValueCell)
            .Text(value1 ?? "");

        table.Cell()
            .Element(DefaultLabelCell)
            .Text(label2 ?? "");

        table.Cell()
            .Element(DefaultValueCell)
            .Text(value2 ?? "");
    }


    //private void DefaultAttendanceTable(
    //    IContainer container,
    //    PayslipViewModel model)
    //{
    //    container.Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            for (int i = 0; i < 5; i++)
    //                c.RelativeColumn();
    //        });

    //        foreach (var h in new[]
    //        {
    //            "Working Days",
    //            "Weekly Off/Holiday",
    //            "Leave Taken",
    //            "Pay Days",
    //            "LWP"
    //        })
    //        {
    //            table.Cell()
    //                .Element(DefaultHeaderCell)
    //                .AlignCenter()
    //                .Text(h);
    //        }

    //        foreach (var v in new[]
    //        {
    //            model.WorkedDays,
    //            model.WeeklyOffHoliday,
    //            model.LeaveTaken,
    //            model.PayableDays,
    //            model.LOPDays
    //        })
    //        {
    //            table.Cell()
    //                .Element(DefaultValueCell)
    //                .AlignCenter()
    //                .Text(v.ToString("N2"));
    //        }
    //    });
    //}

    private void DefaultAttendanceTable(
    IContainer container,
    PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            string[] headers =
            {
            "Working Days",
            "Weekly Off/Holiday",
            "Leave Taken",
            "Pay Days",
            "LWP"
        };

            foreach (var header in headers)
            {
                table.Cell()
                    .Element(DefaultHeaderCell)
                    .AlignCenter()
                    .Text(header);
            }

            string[] values =
            {
            model.WorkedDays.ToString("N2"),
            model.WeeklyOffHoliday.ToString("N2"),
            model.LeaveTaken.ToString("N2"),
            model.PayableDays.ToString("N2"),
            model.LOPDays.ToString("N2")
        };

            foreach (var value in values)
            {
                table.Cell()
                    .Element(DefaultValueCell)
                    .AlignCenter()
                    .Text(value);
            }
        });
    }
    //private void DefaultSalaryTable(
    //    IContainer container,
    //    PayslipViewModel model)
    //{
    //    container.Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            c.RelativeColumn(1.5f);
    //            c.RelativeColumn(1f);
    //            c.RelativeColumn(1.5f);
    //            c.RelativeColumn(1f);
    //        });

    //        foreach (var h in new[]
    //        {
    //            "Earnings",
    //            "Amount (Rs.)",
    //            "Deductions",
    //            "Amount (Rs.)"
    //        })
    //        {
    //            table.Cell()
    //                .Element(DefaultHeaderCell)
    //                .Text(h)
    //                .Bold();
    //        }

    //        int rows =
    //            Math.Max(
    //                model.Earnings.Count,
    //                model.Deductions.Count);

    //        for (int i = 0; i < rows; i++)
    //        {
    //            table.Cell()
    //                .Element(DefaultValueCell)
    //                .Text(
    //                    i < model.Earnings.Count
    //                        ? FormatSalaryHead(
    //                            model.Earnings[i]
    //                                .PayConfigName)
    //                        : "");

    //            table.Cell()
    //                .Element(DefaultValueCell)
    //                .AlignRight()
    //                .Text(
    //                    i < model.Earnings.Count
    //                        ? model.Earnings[i]
    //                            .Amount
    //                            .ToString("N2")
    //                        : "");

    //            table.Cell()
    //                .Element(DefaultValueCell)
    //                .Text(
    //                    i < model.Deductions.Count
    //                        ? FormatSalaryHead(
    //                            model.Deductions[i]
    //                                .PayConfigName)
    //                        : "");

    //            table.Cell()
    //                .Element(DefaultValueCell)
    //                .AlignRight()
    //                .Text(
    //                    i < model.Deductions.Count
    //                        ? model.Deductions[i]
    //                            .Amount
    //                            .ToString("N2")
    //                        : "");
    //        }

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .Text("Gross Earnings")
    //            .Bold();

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .AlignRight()
    //            .Text(
    //                model.GrossEarnings.ToString("N2"))
    //            .Bold();

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .Text("Total Deductions")
    //            .Bold();

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .AlignRight()
    //            .Text(
    //                model.TotalDeductions.ToString("N2"))
    //            .Bold();
    //    });
    //}

    private void DefaultSalaryTable(
    IContainer container,
    PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.55f);
                columns.RelativeColumn(1f);

                columns.RelativeColumn(1.55f);
                columns.RelativeColumn(1f);
            });

            string[] headers =
            {
            "Earnings",
            "Amount (₹)",
            "Deductions",
            "Amount (₹)"
        };

            foreach (var header in headers)
            {
                table.Cell()
                    .Element(DefaultHeaderCell)
                    .Text(header)
                    .SemiBold();
            }

            int rows = Math.Max(
                model.Earnings?.Count ?? 0,
                model.Deductions?.Count ?? 0);

            for (int i = 0; i < rows; i++)
            {
                // Earnings Head
                table.Cell()
                    .Element(DefaultValueCell)
                    .Text(
                        i < model.Earnings.Count
                            ? model.Earnings[i].PayConfigName ?? ""
                            : "");

                // Earnings Amount
                table.Cell()
                    .Element(DefaultValueCell)
                    .Text(
                        i < model.Earnings.Count
                            ? model.Earnings[i].Amount.ToString("N2")
                            : "");

                // Deduction Head
                table.Cell()
                    .Element(DefaultValueCell)
                    .Text(
                        i < model.Deductions.Count
                            ? model.Deductions[i].PayConfigName ?? ""
                            : "");

                // Deduction Amount
                table.Cell()
                    .Element(DefaultValueCell)
                    .Text(
                        i < model.Deductions.Count
                            ? model.Deductions[i].Amount.ToString("N2")
                            : "");
            }

            // TOTAL ROW
            table.Cell()
                .Element(DefaultValueCell)
                .Text("Gross Earnings")
                .Bold();

            table.Cell()
                .Element(DefaultValueCell)
                .Text(model.GrossEarnings.ToString("N2"))
                .Bold();

            table.Cell()
                .Element(DefaultValueCell)
                .Text("Total Deductions")
                .Bold();

            table.Cell()
                .Element(DefaultValueCell)
                .Text(model.TotalDeductions.ToString("N2"))
                .Bold();
        });
    }
    //private void DefaultNetPayTable(
    //    IContainer container,
    //    PayslipViewModel model)
    //{
    //    container.Table(table =>
    //    {
    //        table.ColumnsDefinition(c =>
    //        {
    //            c.RelativeColumn();
    //            c.RelativeColumn();
    //        });

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .Text("Net Pay (Take Home)")
    //            .FontColor(
    //                DefaultBrandColor)
    //            .Bold()
    //            .FontSize(10);

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .AlignRight()
    //            .Text(
    //                $"Rs. {model.NetPay:N2}")
    //            .FontColor(
    //                DefaultBrandColor)
    //            .Bold()
    //            .FontSize(10);

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .Text("Amount in Words");

    //        table.Cell()
    //            .Element(DefaultValueCell)
    //            .Text(
    //                $"{model.NetPayWords} Only");
    //    });
    //}

    private void DefaultNetPayTable(
    IContainer container,
    PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell()
                .Element(DefaultValueCell)
                .Text("Net Pay (Take Home)")
                .FontSize(10)
                .Bold()
                .FontColor(DefaultBrandColor);

            table.Cell()
                .Element(DefaultValueCell)
                .Text($"₹ {model.NetPay:N2}")
                .FontSize(10)
                .Bold()
                .FontColor(DefaultBrandColor);

            table.Cell()
                .Element(DefaultValueCell)
                .Text("Amount in Words");

            table.Cell()
                .Element(DefaultValueCell)
                .Text($"{model.NetPayWords} Only");
        });
    }
    //private void ComposeDefaultFooter(
    //    IContainer container)
    //{
    //    container
    //        .PaddingTop(4)
    //        .BorderTop(1)
    //        .BorderColor(
    //            Colors.Grey.Lighten1)
    //        .PaddingTop(2)
    //        .AlignCenter()
    //        .Text(
    //            "Confidential: This is a system generated payslip and does not require signature. Figures rounded to nearest rupee.")
    //        .FontSize(6)
    //        .FontColor(
    //            Colors.Grey.Darken1);
    //}

    private void ComposeDefaultFooter(
    IContainer container)
    {
        container
            .BorderTop(0.7f)
            .BorderColor("#CCCCCC")
            .PaddingTop(6)
            .AlignCenter()
            .Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .Text(
                        "Confidential: This is a system generated payslip and does not require signature.")
                    .FontSize(7)
                    .FontColor("#555555");

                column.Item()
                    .PaddingTop(1)
                    .AlignCenter()
                    .Text(
                        "Figures rounded to nearest rupee.")
                    .FontSize(7)
                    .FontColor("#555555");
            });
    }

    // ============================================================
    // INICIO PAGE
    // ============================================================

    private void ComposeInicioPage(
        PageDescriptor page,
        PayslipViewModel model,
        byte[] logoBytes)
    {
        page.Margin(0);

        page.DefaultTextStyle(
            x => x
                .FontSize(8)
                .FontFamily("Arial"));

        page.Content()
            .Element(
                c =>
                    ComposeInicioPayslip(
                        c,
                        model,
                        logoBytes));
    }



    // ============================================================
    // INICIO MAIN LAYOUT
    // ============================================================

    private void ComposeInicioPayslip(
        IContainer container,
        PayslipViewModel model,
        byte[] logoBytes)
    {
        container
            .Padding(
                10,
                Unit.Millimetre)

            .Border(1.5f)
            .BorderColor(
                Colors.Black)

            .PaddingHorizontal(
                8,
                Unit.Millimetre)

            .PaddingVertical(
                6,
                Unit.Millimetre)

            .Column(column =>
            {
                // --------------------------------------------
                // HEADER
                // --------------------------------------------
                column.Item()
                    .Element(
                        c =>
                            ComposeInicioHeader(
                                c,
                                model,
                                logoBytes));


                // --------------------------------------------
                // EMPLOYEE DETAILS
                // --------------------------------------------
                column.Item()
                    .PaddingTop(8)
                    .Element(
                        c =>
                            InicioInfoTable(
                                c,
                                model));


                // --------------------------------------------
                // ATTENDANCE
                // Exact same logic as Razor
                // --------------------------------------------

                bool showAttendance =
                    model.TotalDays > 0 ||
                    model.WorkedDays > 0 ||
                    model.PayableDays > 0 ||
                    model.WeeklyOffHoliday > 0 ||
                    model.LOPDays > 0 ||
                    model.LeaveTaken > 0;

                if (showAttendance)
                {
                    column.Item()
                        .PaddingTop(8)
                        .Element(
                            c =>
                                InicioAttendanceTable(
                                    c,
                                    model));
                }


                // --------------------------------------------
                // LEAVE
                // --------------------------------------------

                if (model.LeaveBalances != null &&
                    model.LeaveBalances.Any())
                {
                    column.Item()
                        .PaddingTop(8)
                        .Element(
                            c =>
                                LeaveBalanceTable(
                                    c,
                                    model));
                }


                // --------------------------------------------
                // LOAN
                // --------------------------------------------

                if (model.LoanInstallment > 0 ||
                    model.OpeningLoan > 0 ||
                    model.ClosingLoan > 0)
                {
                    column.Item()
                        .PaddingTop(8)
                        .Element(
                            c =>
                                LoanTable(
                                    c,
                                    model,
                                    false));
                }


                // --------------------------------------------
                // SALARY
                // --------------------------------------------

                column.Item()
                    .PaddingTop(8)
                    .Element(
                        c =>
                            InicioSalaryTable(
                                c,
                                model));


                // --------------------------------------------
                // NET PAY
                // --------------------------------------------

                if (model.ShowNetPay)
                {
                    column.Item()
                        .PaddingTop(8)
                        .Element(
                            c =>
                                InicioNetPayBlock(
                                    c,
                                    model));
                }


                // --------------------------------------------
                // FOOTER
                //
                // IMPORTANT:
                // no ExtendVertical()
                // --------------------------------------------

                column.Item()
                    .PaddingTop(35)
                    .AlignCenter()
                    .Element(
                        ComposeInicioFooter);
            });
    }



    // ============================================================
    // INICIO HEADER
    // ============================================================

    private void ComposeInicioHeader(
        IContainer container,
        PayslipViewModel model,
        byte[] logoBytes)
    {
        try
        {
            string localLogoPath = null;
            if (HostingEnvironment.IsHosted)
            {
                localLogoPath = HostingEnvironment.MapPath("~/Contents/images/inicio_logo.png");
            }
            if (string.IsNullOrEmpty(localLogoPath) || !File.Exists(localLogoPath))
            {
                localLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Contents", "images", "inicio_logo.png");
            }
            if (File.Exists(localLogoPath))
            {
                logoBytes = File.ReadAllBytes(localLogoPath);
            }
        }
        catch { }

        container
            .MinHeight(78)
            .PaddingBottom(6)
            .BorderBottom(1.5f)
            .BorderColor(
                Colors.Black)

            .Row(row =>
            {
                // --------------------------------------------
                // LOGO
                // --------------------------------------------

                row.ConstantItem(110)
                    .AlignMiddle()
                    .AlignLeft()
                    .Element(c =>
                    {
                        if (logoBytes != null)
                        {
                            c
                                .Width(105)
                                .Height(55)
                                .AlignMiddle()
                                .AlignLeft()
                                .Image(logoBytes)
                                .FitArea();
                        }
                    });


                // --------------------------------------------
                // COMPANY
                // --------------------------------------------

                row.RelativeItem()
                    .AlignMiddle()
                    .Column(inner =>
                    {
                        inner.Spacing(1);

                        inner.Item()
                            .AlignCenter()
                            .Text("INICIO LABORATORIES PVT LTD")
                            .FontSize(13.5f)
                            .Bold();

                        inner.Item()
                            .AlignCenter()
                            .Text("Cin No.: U24304GJ2022PTC137549")
                            .FontSize(7f)
                            .Bold();

                        inner.Item()
                            .AlignCenter()
                            .PaddingHorizontal(2)
                            .Text("Regd. Office: 3rd Floor Chinubhai House, 7-B Amrutbaug colony, opp. Sardar Patel Stadium, Near Hindu Colony, Navrangpura Ahmadabad GJ 380014.")
                            .FontSize(7f)
                            .Bold()
                            .LineHeight(1.1f);

                        inner.Item()
                            .AlignCenter()
                            .PaddingHorizontal(2)
                            .Text("Corp. Office: 58/1, Halder Para Lane,Salkia, Howrah- 711106")
                            .FontSize(7f)
                            .Bold()
                            .LineHeight(1.1f);

                        inner.Item()
                            .AlignCenter()
                            .PaddingHorizontal(2)
                            .Text("Mail id: hr@iniciolabs.com Mob: 8637078150 website: www.iniciolabs.com")
                            .FontSize(7f)
                            .Bold()
                            .LineHeight(1.1f);

                        inner.Item()
                            .PaddingTop(3)
                            .AlignCenter()
                            .Text(
                                $"Salary Slip for the month of {model?.Month}-{model?.Year}")
                            .FontSize(9.5f)
                            .Bold();
                    });


                // --------------------------------------------
                // RIGHT BALANCE
                // --------------------------------------------

                row.ConstantItem(110);
            });
    }



    // ============================================================
    // ADDRESS CLEANING
    // ============================================================

    private string CleanAddress(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return string.Join(
            " ",
            value
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Split(
                    new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries));
    }



    // ============================================================
    // EMPLOYEE INFO TABLE
    // Exactly same 4 rows as Razor
    // ============================================================

    private void InicioInfoTable(
        IContainer container,
        PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(0.90f);
                c.RelativeColumn(1.65f);

                c.RelativeColumn(0.90f);
                c.RelativeColumn(1.45f);

                c.RelativeColumn(1.00f);
                c.RelativeColumn(1.75f);
            });


            AddInicioInfoRow(
                table,
                "Name :",
                model.EmpName,
                "Emp Code :",
                model.EmpCode,
                "Designation :",
                model.Designation);


            AddInicioInfoRow(
                table,
                "Department :",
                model.Department,
                "DOJ :",
                model.DOJ.ToString(
                    "dd-MM-yyyy"),
                "PAN :",
                model.PAN);


            AddInicioInfoRow(
                table,
                "PF No :",
                model.PFNo,
                "UAN No. :",
                model.UAN,
                "ESIC No :",
                model.ESICNo);


            AddInicioInfoRow(
                table,
                "A/C No :",
                model.BankAccount,
                "Bank Name :",
                model.BankName,
                "IFSC Code :",
                model.IFSCCode);
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
        table.Cell()
            .Element(
                InicioLabelCell)
            .Text(
                label1 ?? "");

        table.Cell()
            .Element(
                InicioValueCell)
            .Text(
                value1 ?? "");


        table.Cell()
            .Element(
                InicioLabelCell)
            .Text(
                label2 ?? "");

        table.Cell()
            .Element(
                InicioValueCell)
            .Text(
                value2 ?? "");


        table.Cell()
            .Element(
                InicioLabelCell)
            .Text(
                label3 ?? "");

        table.Cell()
            .Element(
                InicioValueCell)
            .Text(
                value3 ?? "");
    }



    // ============================================================
    // ATTENDANCE TABLE
    // ============================================================

    private void InicioAttendanceTable(
        IContainer container,
        PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                for (int i = 0; i < 6; i++)
                    c.RelativeColumn();
            });


            string[] headers =
            {
                "Total Days",
                "Worked Days",
                "Payable Days",
                "Weekly Off/Hol",
                "LWP",
                "Leave Taken"
            };


            foreach (string header in headers)
            {
                table.Cell()
                    .Element(
                        InicioHeaderCell)
                    .AlignCenter()
                    .Text(header)
                    .FontSize(7.5f)
                    .Bold();
            }


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    model.TotalDays
                        .ToString("N2"));


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    model.WorkedDays
                        .ToString("N2"));


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    model.PayableDays
                        .ToString("N2"));


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    model.WeeklyOffHoliday
                        .ToString("N2"));


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    model.LOPDays
                        .ToString("N2"));


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    model.LeaveTaken
                        .ToString("N2"));
        });
    }



    // ============================================================
    // LEAVE
    // ============================================================

    private void LeaveBalanceTable(
        IContainer container,
        PayslipViewModel model)
    {
        if (model.LeaveBalances == null ||
            !model.LeaveBalances.Any())
        {
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                foreach (var _ in model.LeaveBalances)
                {
                    c.RelativeColumn();
                }
            });


            foreach (var leave in
                model.LeaveBalances)
            {
                table.Cell()
                    .Border(1)
                    .BorderColor(
                        Colors.Black)
                    .Background(
                        Colors.Grey.Lighten4)
                    .PaddingHorizontal(2)
                    .PaddingVertical(3)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(
                        FormatLeaveName(
                            leave.CodeType))
                    .FontSize(7.2f)
                    .Bold();
            }


            foreach (var leave in
                model.LeaveBalances)
            {
                table.Cell()
                    .Border(1)
                    .BorderColor(
                        Colors.Black)
                    .PaddingVertical(2)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(
                        leave.Balance
                            .ToString("N2"))
                    .FontSize(8);
            }
        });
    }



    private string FormatLeaveName(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
            value))
        {
            return "";
        }

        string result =
            value.Trim();

        int bracketStart =
            result.LastIndexOf('(');

        if (bracketStart > 0 &&
            result.EndsWith(")"))
        {
            string description =
                result
                    .Substring(
                        0,
                        bracketStart)
                    .Trim();

            string code =
                result
                    .Substring(
                        bracketStart + 1,
                        result.Length -
                        bracketStart -
                        2)
                    .Trim()
                    .ToUpperInvariant();

            description =
                CultureTitleCase(
                    description);

            return
                $"{description}({code})";
        }

        return
            CultureTitleCase(
                result);
    }



    private string CultureTitleCase(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        return
            CultureInfo
                .InvariantCulture
                .TextInfo
                .ToTitleCase(
                    value.ToLowerInvariant());
    }



    // ============================================================
    // SALARY TABLE
    // ============================================================

    private void InicioSalaryTable(
        IContainer container,
        PayslipViewModel model)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2.10f);
                c.RelativeColumn(0.90f);
                c.RelativeColumn(0.90f);

                c.RelativeColumn(1.90f);
                c.RelativeColumn(0.85f);
                c.RelativeColumn(0.95f);
            });


            table.Cell()
                .ColumnSpan(3)
                .Element(
                    InicioHeaderCell)
                .AlignCenter()
                .Text("Earning")
                .Bold();


            table.Cell()
                .ColumnSpan(3)
                .Element(
                    InicioHeaderCell)
                .AlignCenter()
                .Text("Deductions")
                .Bold();


            foreach (var header in new[]
            {
                "Head",
                "Amount",
                "Payable",
                "Head",
                "Amount",
                "Deductions"
            })
            {
                table.Cell()
                    .Element(
                        InicioHeaderCell)
                    .AlignCenter()
                    .Text(header)
                    .FontSize(7.5f)
                    .Bold();
            }


            int rows =
                Math.Max(
                    model.Earnings.Count,
                    model.Deductions.Count);


            for (int i = 0;
                i < rows;
                i++)
            {
                string earningName =
                    i < model.Earnings.Count
                        ? FormatSalaryHead(
                            model.Earnings[i]
                                .PayConfigName)
                        : "";


                string deductionName =
                    i < model.Deductions.Count
                        ? FormatSalaryHead(
                            model.Deductions[i]
                                .PayConfigName)
                        : "";


                table.Cell()
                    .Element(
                        InicioSalaryHeadCell)
                    .Text(
                        earningName);


                table.Cell()
                    .Element(
                        InicioSalaryAmountCell)
                    .AlignRight()
                    .Text(
                        i < model.Earnings.Count
                            ? model.Earnings[i]
                                .ActualAmount
                                .ToString("N2")
                            : "");


                table.Cell()
                    .Element(
                        InicioSalaryAmountCell)
                    .AlignRight()
                    .Text(
                        i < model.Earnings.Count
                            ? model.Earnings[i]
                                .Amount
                                .ToString("N2")
                            : "");


                table.Cell()
                    .Element(
                        InicioSalaryHeadCell)
                    .Text(
                        deductionName);


                table.Cell()
                    .Element(
                        InicioSalaryAmountCell)
                    .AlignRight()
                    .Text(
                        i < model.Deductions.Count
                            ? model.Deductions[i]
                                .ActualAmount
                                .ToString("N2")
                            : "");


                table.Cell()
                    .Element(
                        InicioSalaryAmountCell)
                    .AlignRight()
                    .Text(
                        i < model.Deductions.Count
                            ? model.Deductions[i]
                                .Amount
                                .ToString("N2")
                            : "");
            }


            // --------------------------------------------
            // TOTAL
            // --------------------------------------------

            table.Cell()
                .Element(
                    InicioTotalCell)
                .Text(
                    "Total Earnings")
                .Bold();

            table.Cell()
                .Element(
                    InicioTotalCell)
                .Text("");

            table.Cell()
                .Element(
                    InicioTotalCell)
                .AlignRight()
                .Text(
                    model.GrossEarnings
                        .ToString("N2"))
                .Bold();


            table.Cell()
                .Element(
                    InicioTotalCell)
                .Text(
                    "Total Deductions")
                .Bold();

            table.Cell()
                .Element(
                    InicioTotalCell)
                .Text("");

            table.Cell()
                .Element(
                    InicioTotalCell)
                .AlignRight()
                .Text(
                    model.TotalDeductions
                        .ToString("N2"))
                .Bold();
        });
    }



    // ============================================================
    // SALARY HEAD FORMAT
    // ============================================================

    private string FormatSalaryHead(
        string name)
    {
        if (string.IsNullOrWhiteSpace(
            name))
        {
            return "";
        }

        string value =
            name.Trim();


        if (value.Equals(
            "Hra",
            StringComparison.OrdinalIgnoreCase))
        {
            return "HRA";
        }


        if (value.Equals(
            "Lta",
            StringComparison.OrdinalIgnoreCase))
        {
            return "LTA";
        }


        if (
            value.Equals(
                "P Tex",
                StringComparison.OrdinalIgnoreCase)

            ||

            value.Equals(
                "P Tax",
                StringComparison.OrdinalIgnoreCase)

            ||

            value.Equals(
                "PTax",
                StringComparison.OrdinalIgnoreCase)

            ||

            value.Equals(
                "Professional Tax",
                StringComparison.OrdinalIgnoreCase))
        {
            return "P Tax";
        }


        return value;
    }



    // ============================================================
    // NET PAY
    // ============================================================

    private void InicioNetPayBlock(
        IContainer container,
        PayslipViewModel model)
    {
        container
            .Column(column =>
            {
                column.Spacing(3);


                column.Item()
                    .Row(row =>
                    {
                        row.ConstantItem(105)
                            .Text(
                                "Net Payable (INR)")
                            .FontSize(9)
                            .Bold();


                        row.RelativeItem()
                            .Text(
                                model.NetPay
                                    .ToString("N2"))
                            .FontSize(10)
                            .Bold();
                    });


                column.Item()
                    .Row(row =>
                    {
                        row.ConstantItem(145)
                            .Text(
                                "Net Payable INR (In Words):")
                            .FontSize(8);


                        row.RelativeItem()
                            .Text(
                                $"{model.NetPayWords} Only")
                            .FontSize(8)
                            .Bold();
                    });
            });
    }



    // ============================================================
    // LOAN
    // ============================================================

    private void LoanTable(
        IContainer container,
        PayslipViewModel model,
        bool showCurrency)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn();
                c.RelativeColumn();
                c.RelativeColumn();
            });


            foreach (var header in new[]
            {
                "Opening Loan Balance",
                "Current Installment",
                "Closing Loan Balance"
            })
            {
                table.Cell()
                    .Element(
                        InicioHeaderCell)
                    .AlignCenter()
                    .Text(header)
                    .FontSize(7.5f)
                    .Bold();
            }


            string prefix =
                showCurrency
                    ? "Rs. "
                    : "";


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    prefix +
                    model.OpeningLoan
                        .ToString("N2"));


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    prefix +
                    model.LoanInstallment
                        .ToString("N2"));


            table.Cell()
                .Element(
                    InicioValueCell)
                .AlignCenter()
                .Text(
                    prefix +
                    model.ClosingLoan
                        .ToString("N2"));
        });
    }



    // ============================================================
    // FOOTER
    // ============================================================

    private void ComposeInicioFooter(
        IContainer container)
    {
        container
            .AlignCenter()
            .Text(
                "This is system generated document signature not require.")
            .FontSize(8);
    }



    // ============================================================
    // DEFAULT CELL STYLE
    // ============================================================

    //private static IContainer DefaultLabelCell(
    //    IContainer c)
    //{
    //    return c
    //        .Border(1)
    //        .BorderColor(
    //            Colors.Grey.Lighten1)
    //        .Background(
    //            Colors.Grey.Lighten4)
    //        .Padding(2);
    //}

    private static IContainer DefaultLabelCell(
    IContainer container)
    {
        return container
            .Border(0.7f)
            .BorderColor("#CCCCCC")
            .Background("#F1F1F1")
            .PaddingVertical(4)
            .PaddingHorizontal(5)
            .AlignMiddle()
            .DefaultTextStyle(x => x
                .FontFamily("Arial")
                .FontSize(8.5f)
                .SemiBold()
                .FontColor("#333333"));
    }

    private static IContainer DefaultValueCell(
        IContainer container)
    {
        return container
            .Border(0.7f)
            .BorderColor("#CCCCCC")
            .PaddingVertical(4)
            .PaddingHorizontal(5)
            .AlignMiddle()
            .DefaultTextStyle(x => x
                .FontFamily("Arial")
                .FontSize(8.5f)
                .FontColor("#333333"));
    }

    private static IContainer DefaultHeaderCell(
        IContainer container)
    {
        return container
            .Border(0.7f)
            .BorderColor("#CCCCCC")
            .Background("#F1F1F1")
            .PaddingVertical(4)
            .PaddingHorizontal(5)
            .AlignMiddle()
            .DefaultTextStyle(x => x
                .FontFamily("Arial")
                .FontSize(8.5f)
                .SemiBold()
                .FontColor("#333333"));
    }
    //private static IContainer DefaultValueCell(
    //    IContainer c)
    //{
    //    return c
    //        .Border(1)
    //        .BorderColor(
    //            Colors.Grey.Lighten1)
    //        .Padding(2);
    //}


    //private static IContainer DefaultHeaderCell(
    //    IContainer c)
    //{
    //    return c
    //        .Border(1)
    //        .BorderColor(
    //            Colors.Grey.Lighten1)
    //        .Background(
    //            Colors.Grey.Lighten3)
    //        .Padding(2);
    //}



    // ============================================================
    // INICIO CELL STYLE
    // ============================================================

    private static IContainer InicioLabelCell(
        IContainer c)
    {
        return c
            .Border(1)
            .BorderColor(
                Colors.Black)
            .PaddingHorizontal(3)
            .PaddingVertical(2.2f)
            .AlignMiddle()
            .DefaultTextStyle(
                x => x
                    .FontSize(7.5f)
                    .Bold());
    }


    private static IContainer InicioValueCell(
        IContainer c)
    {
        return c
            .Border(1)
            .BorderColor(
                Colors.Black)
            .PaddingHorizontal(3)
            .PaddingVertical(2.2f)
            .AlignMiddle()
            .DefaultTextStyle(
                x => x.FontSize(7.5f));
    }


    private static IContainer InicioHeaderCell(
        IContainer c)
    {
        return c
            .Border(1)
            .BorderColor(
                Colors.Black)
            .Background(
                Colors.Grey.Lighten4)
            .PaddingHorizontal(3)
            .PaddingVertical(2.5f)
            .AlignMiddle()
            .DefaultTextStyle(
                x => x.FontSize(7.5f));
    }


    private static IContainer InicioSalaryHeadCell(
        IContainer c)
    {
        return c
            .Border(1)
            .BorderColor(
                Colors.Black)
            .PaddingHorizontal(3)
            .PaddingVertical(2)
            .AlignMiddle()
            .DefaultTextStyle(
                x => x.FontSize(7.5f));
    }


    private static IContainer InicioSalaryAmountCell(
        IContainer c)
    {
        return c
            .Border(1)
            .BorderColor(
                Colors.Black)
            .PaddingHorizontal(3)
            .PaddingVertical(2)
            .AlignMiddle()
            .DefaultTextStyle(
                x => x.FontSize(7.5f));
    }


    private static IContainer InicioTotalCell(
        IContainer c)
    {
        return c
            .Border(1)
            .BorderColor(
                Colors.Black)
            .PaddingHorizontal(3)
            .PaddingVertical(2.5f)
            .AlignMiddle()
            .DefaultTextStyle(
                x => x
                    .FontSize(7.5f)
                    .Bold());
    }



    // ============================================================
    // LOGO
    // ============================================================

    private byte[] TryDownloadLogo(
        string url)
    {
        if (string.IsNullOrWhiteSpace(
            url))
        {
            return null;
        }

        try
        {
            using (var client =
                new WebClient())
            {
                return
                    client.DownloadData(
                        url);
            }
        }
        catch (Exception ex)
        {
            clsDatabase.fnErrorLog(
                "PayslipLogoDownloadError",
                $"Url={url}; Error={ex.Message}");

            return null;
        }
    }
}