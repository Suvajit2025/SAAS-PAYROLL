$(function () {
    defaultLoad();
    function defaultLoad() {
        let url = "/Annexure/AllEmployeeList";
        $.get(url, function (data) {
            $("#ddlEmployee").empty();
            data.map(function (z) {
                $("#ddlEmployee").append('<option value=' + z.IDEmployee + '>' + z.EmployeeName + '</option>');
            });
            $('#ddlEmployee').multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                enableCaseInsensitiveFiltering:true,
                maxHeight: 250,
            });
        });
        url = "/Loan/SalaryYearsList";
        $.get(url, function (data) {
            $("#ddlYear").empty();
            data.map(function (z) {
                if (z.Selected == true) {
                    $("#ddlYear").append('<option value="' + z.Name + '" selected>' + z.Name + '</option>');
                }
                else {
                    $("#ddlYear").append('<option value=' + z.Name + '>' + z.Name + '</option>');
                }
            });
        });
        url = "/Loan/SalaryMonthList";
        $.get(url, function (data) {
            $("#ddlMonth").empty();
            data.map(function (z) {
                if (z.Selected == true) {
                    $("#ddlMonth").append('<option value="' + z.Name + '" selected>' + z.Name + '</option>');
                }
                else {
                    $("#ddlMonth").append('<option value=' + z.Name + '>' + z.Name + '</option>');
                }

            });
        });

    }
    $("#btnShow").on('click', function () {
        //alert(selected);
        if (Validation() == true) {
            let url = "/Annexure/Annexure92BData";

            // Multiselect Employee
            let empIDS = $('#ddlEmployee :selected').toArray().map(item => item.value);
            let IDS = "";
            empIDS.map((item) => {
                IDS += item +","
            })
            info = {
                "EmployeeIDS": IDS,
                "Month": $("#ddlMonth :selected").val(),
                "Year": $("#ddlYear :selected").val()
            }
            $.get(url, info, function (data) {
                $("#tblAnnexureBody").empty();
                data.map(function (obj) {
                    var row = '';
                    row = row + '<tr>';
                    row = row + '<td>'
                    row = row + '<input type="hidden" id="hdIDAnnexure" value="' + obj.IDAnnexure + '"' + '/> ';
                    row = row + '<input type="hidden" id="hdIDEmployee" value="' + obj.IDEmployee + '"' + '/> ';
                    row = row + '<input type="hidden" id="hdEmpMonth" value="' + obj.EmpMonth + '"' + '/> ';
                    row = row + '<input type="hidden" id="hdEmpYear" value="' + obj.EmpYear + '"' + '/> ';
                    row = row + '<input type="text" id="txtChallanNo" class="form-control form-control-sm" value="' + obj.ChallanNo + '"' + '/></td>';
                    row = row + '<td><label id="lblEmployeeName">' + obj.EmployeeName + '</label></td>';
                    row = row + '<td><label id="lblPANNCard">' + obj.PANNCard + '</label></td>';
                    row = row + '<td><label id="lblSectionCode">' + obj.SectionCode + '</label></td>';
                    row = row + '<td><label id="lblPaymentAmount">' + obj.PaymentAmount + '</label></td>';
                    row = row + '<td><label id="lblTaxDeductionDate">' + obj.TaxDeductionDate + '</label></td>';
                    row = row + '<td><label id="lblAmountPaidDate">' + obj.AmountPaidDate + '</label></td>';
                    row = row + '<td><label id="lblTDSAmount">' + obj.TDSAmount + '</label></td>';
                    row = row + '<td><label id="lblSurchargeAmount" >' + obj.SurchargeAmount + '</label></td>';
                    row = row + '<td><label id="lblHealthECAmount">' + obj.HealthECAmount + '</label></td>';
                    row = row + '<td><label id="lblSHECAmount">' + obj.SHECAmount + '</label></td>';
                    row = row + '<td><label id="lblTotalAmount">' + obj.TotalAmount + '</label></td>';
                    row = row + '<td><label id="lblTotalTaxDeductionAmount">' + obj.TotalAmount + '</label></td>';
                    row = row + '<td><input type="text" id="txtReason" class="form-control form-control-sm" value="' + obj.Reason + '"' + '/></td>';
                    row = row + '<td><input type="text" id="txtCertificateno" class="form-control form-control-sm" value="' + obj.Certificateno + '"' + '/></td>';
                    if (obj.IDAnnexure != 0) {
                        row = row + '<td><input type="button" id="btnSave" value="Save" class="btn btn-sm btn-success"></td>';
                    }
                    else {
                        row = row + '<td><input type="button" id="btnSave" value="Save" class="btn btn-sm btn-primary"></td>';
                    }
                    row = row + '</tr>';
                    $("#tblAnnexureBody").append(row);
                });
            });
        }
    });
    function Validation() {
        let Year = $("#ddlYear :selected").val();
        let Month = $("#ddlMonth :selected").val();
        let Employee = $("#ddlEmployee :selected").val();
        if (Year === "") {
            alert("Year is missing....");
            $("#ddlYear").focus();
            return false;
        }
        if (Month === "") {
            alert("Month is missing....");
            $("#ddlMonth").focus();
            return false;
        }
        //if (Employee === "0") {
        //    alert("Employee is missing....");
        //    $("#ddlEmployee").focus();
        //    return false;
        //}

        return true;
    }
    $("#chkAll").on('click', function () {
        let value = this.checked;
        let ctl = null;
        console.log(value);
        $("#LoanProcessBody tr").each(function () {
            $(this).find("#chkProcess").prop('checked', value);
        });
    });
    $("#tblAnnexureBody").on('click', "#btnSave", function () {
        let records = [];
        let obj = {};
        let IDAnnexure = $(this).closest("tr").find("#hdIDAnnexure").val();
        let IDEmployee = $(this).closest("tr").find("#hdIDEmployee").val();
        let EmpMonth = $(this).closest("tr").find("#hdEmpMonth").val();
        let EmpYear = $(this).closest("tr").find("#hdEmpYear").val();
        let ChallanNo = $(this).closest("tr").find("#txtChallanNo").val();
        let PANNCard = $(this).closest("tr").find("#lblPANNCard").html();
        let PaymentAmount = $(this).closest("tr").find("#lblPaymentAmount").html();
        let SectionCode = $(this).closest("tr").find("#lblSectionCode").html();
        let TaxDeductionDate = $(this).closest("tr").find("#lblTaxDeductionDate").html();
        let AmountPaidDate = $(this).closest("tr").find("#lblAmountPaidDate").html();
        let TDSAmount = $(this).closest("tr").find("#lblTDSAmount").html();
        let SurchargeAmount = $(this).closest("tr").find("#lblSurchargeAmount").html();
        let HealthECAmount = $(this).closest("tr").find("#lblHealthECAmount").html();
        let SHECAmount = $(this).closest("tr").find("#lblSHECAmount").html();
        let TotalAmount = $(this).closest("tr").find("#lblTotalAmount").html();
        let TotalTaxDeductionAmount = $(this).closest("tr").find("#lblTotalTaxDeductionAmount").html();
        let Reason = $(this).closest("tr").find("#txtReason").val();
        let Certificateno = $(this).closest("tr").find("#txtCertificateno").val();

        obj = {
            "IDAnnexure": IDAnnexure,
            "IDEmployee": IDEmployee,
            "EmpMonth": EmpMonth,
            "EmpYear": EmpYear,
            "ChallanNo": ChallanNo,
            "PANNCard": PANNCard,
            "PaymentAmount": PaymentAmount,
            "SectionCode": SectionCode,
            "TaxDeductionDate": TaxDeductionDate,
            "AmountPaidDate": AmountPaidDate,
            "TDSAmount": TDSAmount,
            "SurchargeAmount": SurchargeAmount,
            "HealthECAmount": HealthECAmount,
            "SHECAmount": SHECAmount,
            "TotalAmount": TotalAmount,
            "TotalTaxDeductionAmount": TotalTaxDeductionAmount,
            "Reason": Reason,
            "Certificateno": Certificateno
        };
        records.push(obj);
        let url = "/Annexure/Annexure92BSave";
        $.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(records),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                alert(data.success == "" ? "Record successfully saved...." : Result);
            }
        });
    });
});