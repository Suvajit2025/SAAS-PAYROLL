using MendinePayroll.UI.Models;
using Rotativa;
using System.Web.Mvc;

public class PayslipPdfService
{
    public byte[] GeneratePayslipPdf(
        ControllerContext context,
        PayslipViewModel model,
        string fileName)
    {
        var pdf = new ViewAsPdf("~/Views/Payslip/Index.cshtml", model)
        {
            FileName = fileName,
            PageSize = Rotativa.Options.Size.A4,
            PageOrientation = Rotativa.Options.Orientation.Portrait
        };

        return pdf.BuildFile(context);
    }
}
