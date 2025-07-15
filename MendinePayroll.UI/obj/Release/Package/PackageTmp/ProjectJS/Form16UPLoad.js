$(function () {
    defaultLoad();
    function defaultLoad() {
        let url = "/Annexure/YearList";
        $.get(url, function (data) {
            // Year List 
            $("#ddlYear").empty();
            $("#ddlYear").append('<option value="">Select</option>');
            data.map(function (z) {
                $("#ddlYear").append('<option value=' + z.Name + '>' + z.Name + '</option>');
            });

            // Company List 
            url = "/Annexure/CompanyList";
            $("#ddlCompany").empty();
            $("#ddlCompany").append('<option value="">Select</option>');
            $.get(url, function (data) {
                data.map(function (z) {
                    $("#ddlCompany").append('<option value=' + z.Code + '>' + z.Name + '</option>');
                });
            });
        });
    }
    $("#ddlCompany").on('change', function () {
        let value = $(this).val();
        let param = { "Companycode": value };
        url = "/Annexure/EmployeeList";
        $("#ddlEmployee").empty();
        $("#ddlEmployee").append('<option value="0">Select</option>');
        $.get(url, param, function (data) {
            data.map(function (z) {
                $("#ddlEmployee").append('<option value="' + z.EmployeeNo + '">' + z.EmployeeName + '</option>');
            });
        });
    });

    function SaveValidation() {
        let Year = $("#ddlYear :selected").val();
        let Month = $("#ddlMonth :selected").val();
        let Employee = $("#ddlEmployee :selected").val();
        let filename = $("#txtFile").prop('files')[0].name;

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
        if (Employee === "") {
            alert("Employee is missing....");
            $("#ddlEmployee").focus();
            return false;
        }
        if (filename === "") {
            alert("Form 16 file is missing....");
            $("#txtFile").focus();
            return false;
        }
        return true;
    }

    //$("#btnSave").on('click', function () {
    //    //let idemp = $("#ddlEmployee :selected").val();
    //    //let actualfile = $('#txtFile').prop('files')[0];
    //    let filename = $('#txtFile').prop('files')[0].name;
    //    /*let year = $("#ddlYear :selected").val();*/

    //    if (filename === "") {
    //        alert("upload file is missing....");
    //        $(this).closest("tr").find("#txtFile").focus();
    //        return;
    //    }
    //    else {
    //        let url = "/Annexure/Form16Upload";
    //        let param = {
    //            "PostedFile": actualfile,
    //            "IDEmp": idemp,
    //            "Filename": filename,
    //            "EmpYear": year
    //        }
    //            $.ajax({
    //            type: "POST",
    //            url: url,
    //            data: JSON.stringify(param),
    //            contentType: "application/json; charset=utf-8",
    //            dataType: "json",
    //            cache: false,
    //            processData: false,
    //            success: function (data) {
    //                //alert(data.success == "" ? "Record successfully saved...." : Result);
    //            }
    //        });
    //    }
    //});
    function ShowValidation() {
        let Year = $("#ddlYear :selected").val();
        let Company = $("#ddlCompany:selected").val();
        if (Year === "Select") {
            alert("Year is missing....");
            $("#ddlYear").focus();
            return false;
        }
        if (Company === "Select") {
            alert("Company is missing....");
            $("#ddlCompany").focus();
            return false;
        }
        return true;
    }
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
});

//if (filename === "") {
        //    alert("upload file is missing....");
        //    $(this).closest("tr").find("#txtFile").focus();
        //    return;
        //}
        //else {
        //    let url = "/Annexure/Form16Upload";
        //    let param = {
        //        "file": actualfile,
        //        "Empno": empno,
        //        "Filename": filename,
        //        "EmpYear": year
        //    }
        //    $.ajax({
        //        type: "POST",
        //        url: url,
        //        data: JSON.stringify(param),
        //        contentType: "application/json; charset=utf-8",
        //        //dataType: "json",
        //        //cache: false,
        //        //processData: false,
        //        success: function (data) {
        //            alert(data.success == "" ? "Record successfully saved...." : Result);
        //        }
        //    });
        //}

        //if (ShowValidation() == true) {

        //    var row = '';
        //    $.get(url, param, function (data) {
        //        $("#tbltblForm16Body").empty();
        //        data.map(function (obj) {
        //            row = '';
        //            row = row + '<tr>';
        //            row = row + '<td>'
        //            row = row + '<input type="hidden" id="hdIDForm" value="' + obj.IDForm + '"' + '/> ';
        //            row = row + '<input type="hidden" id="hdIDEmployee" value="' + obj.Employee.IDEmployee + '"' + '/> ';
        //            row = row + '<label id="lblEmployeeNo">' + obj.Employee.EmployeeNo + '</label></td>';
        //            row = row + '<td><label id="lblEmployeeName">' + obj.Employee.EmployeeName + '</label></td>';
        //            row = row + '<td><label id="lblStatus">' + obj.Status + '</label></td>';
        //            row = row + '<td><input type="file" id="txtFile" class="form-control form-control-sm" /></td> ';
        //            if (obj.Status != '') {
        //                row = row + '<td><input type="button" id="btnUpload" class="btn btn-sm btn-success" value="Upload"/></td> ';
        //            }
        //            else {
        //                row = row + '<td><input type="button" id="btnUpload" class="btn btn-sm btn-primary" value="Upload"/></td> ';
        //            }
        //            row = row + '</tr>';
        //            $("#tbltblForm16Body").append(row);
        //        });
        //    });
        //}
//$("#tblForm16Body").on('click', "#btnUpload", function () {
//    let empno = $(this).closest("tr").find("#lblEmployeeNo").html();
//    let actualfile = $(this).closest("tr").find("#txtFile").prop('files')[0];
//    let filename = $(this).closest("tr").find("#txtFile").prop('files')[0].name;
//    let year = $("#ddlYear :selected").val();


//    console.log(empno);
//    console.log(actualfile);
//    console.log(filename);
//    console.log(year);

//    if (filename === "") {
//        alert("upload file is missing....");
//        $(this).closest("tr").find("#txtFile").focus();
//        return;
//    }
//    else {
//        let url = "/Annexure/Form16Upload";
//        let param = {
//            "file": actualfile,
//            "Empno": empno,
//            "Filename": filename,
//            "EmpYear": year
//        }
//        $.ajax({
//            type: "POST",
//            url: url,
//            data: JSON.stringify(param),
//            contentType: "application/json; charset=utf-8",
//            //dataType: "json",
//            //cache: false,
//            //processData: false,
//            success: function (data) {
//                alert(data.success == "" ? "Record successfully saved...." : Result);
//            }
//        });
//    }

//    //if (ShowValidation() == true) {

//    //    var row = '';
//    //    $.get(url, param, function (data) {
//    //        $("#tbltblForm16Body").empty();
//    //        data.map(function (obj) {
//    //            row = '';
//    //            row = row + '<tr>';
//    //            row = row + '<td>'
//    //            row = row + '<input type="hidden" id="hdIDForm" value="' + obj.IDForm + '"' + '/> ';
//    //            row = row + '<input type="hidden" id="hdIDEmployee" value="' + obj.Employee.IDEmployee + '"' + '/> ';
//    //            row = row + '<label id="lblEmployeeNo">' + obj.Employee.EmployeeNo + '</label></td>';
//    //            row = row + '<td><label id="lblEmployeeName">' + obj.Employee.EmployeeName + '</label></td>';
//    //            row = row + '<td><label id="lblStatus">' + obj.Status + '</label></td>';
//    //            row = row + '<td><input type="file" id="txtFile" class="form-control form-control-sm" /></td> ';
//    //            if (obj.Status != '') {
//    //                row = row + '<td><input type="button" id="btnUpload" class="btn btn-sm btn-success" value="Upload"/></td> ';
//    //            }
//    //            else {
//    //                row = row + '<td><input type="button" id="btnUpload" class="btn btn-sm btn-primary" value="Upload"/></td> ';
//    //            }
//    //            row = row + '</tr>';
//    //            $("#tbltblForm16Body").append(row);
//    //        });
//    //    });
//    //}
//});

    //$("#btnShow").on('click', function () {
    //    if (ShowValidation() == true) {
    //        let url = "/Annexure/Form16ist";
    //        let param = {
    //            "Companycode": $("#ddlCompany :selected").val(),
    //            "Year": $("#ddlYear :selected").val(),
    //            "IDEmployee": $("#ddlEmployee :selected").val() == "" ? "" : $("#ddlEmployee :selected").val()
    //        }
    //        var row = '';
    //        $.get(url, param, function (data) {
    //            $("#tblForm16Body").empty();
    //            data.map(function (obj) {
    //                row = '';
    //                row = row + '<tr>';
    //                row = row + '<td>'
    //                row = row + '<input type="hidden" id="hdIDForm" value="' + obj.IDForm + '"' + '/> ';
    //                row = row + '<input type="hidden" id="hdIDEmployee" value="' + obj.Employee.IDEmployee + '"' + '/> ';
    //                row = row + '<input type="hidden" id="hdFileName" value="' + obj.Employee.IDEmployee + '"' + '/> ';
    //                row = row + '<label id="lblEmployeeNo">' + obj.Employee.EmployeeNo + '</label></td>';
    //                row = row + '<td><label id="lblEmployeeName">' + obj.Employee.EmployeeName + '</label></td>';
    //                row = row + '<td><label id="lblStatus">' + obj.Status + '</label></td>';
    //                row = row + '<td><input type="file" id="txtFile" class="form-control form-control-sm" /></td> ';
    //                if (obj.Status != '') {
    //                    row = row + '<td><input type="button" id="btnUpload" class="btn btn-sm btn-success" value="Upload"/></td> ';
    //                   // row = row + '<td><button id="btnUpload" class="btn btn-sm btn-success">UPLoad</Button></td>';
    //                }
    //                else {
    //                    row = row + '<td><input type="button" id="btnUpload" class="btn btn-sm btn-primary" value="Upload"/></td> ';
    //                    //row = row + '<td><button id="btnUpload" class="btn btn-sm btn-primary">UPLoad</Button></td>';
    //                }
    //                row = row + '</tr>';
    //                $("#tblForm16Body").append(row);
    //            });
    //        });
    //    }
    //});
    //function getfilename(e) {
    //    var filename = e.target.files[0].name;
    //    alert(filename);
    //}

    //$("#tbltblForm16Body").on('change', "#txtFile", function () {
    //    var filename = $(this).closest("tr").find("#txtFile").files[0].name;
    //    alert(filename);
    //});
    //$("#btnShow").on('click', function () {
    //    //alert(selected);
    //    if (Validation() == true) {
    //        let url = "/Annexure/Annexure92BData";

    //        // Multiselect Employee
    //        let empIDS = $('#ddlEmployee :selected').toArray().map(item => item.value);
    //        let IDS = "";
    //        empIDS.map((item) => {
    //            IDS += item +","
    //        })
    //        info = {
    //            "EmployeeIDS": IDS,
    //            "Month": $("#ddlMonth :selected").val(),
    //            "Year": $("#ddlYear :selected").val()
    //        }
    //        $.get(url, info, function (data) {
    //            $("#tblAnnexureBody").empty();
    //            data.map(function (obj) {
    //                var row = '';
    //                row = row + '<tr>';
    //                row = row + '<td>'
    //                row = row + '<input type="hidden" id="hdIDAnnexure" value="' + obj.IDAnnexure + '"' + '/> ';
    //                row = row + '<input type="hidden" id="hdIDEmployee" value="' + obj.IDEmployee + '"' + '/> ';
    //                row = row + '<input type="hidden" id="hdEmpMonth" value="' + obj.EmpMonth + '"' + '/> ';
    //                row = row + '<input type="hidden" id="hdEmpYear" value="' + obj.EmpYear + '"' + '/> ';
    //                row = row + '<input type="text" id="txtChallanNo" class="form-control form-control-sm" value="' + obj.ChallanNo + '"' + '/></td>';
    //                row = row + '<td><label id="lblEmployeeName">' + obj.EmployeeName + '</label></td>';
    //                row = row + '<td><label id="lblPANNCard">' + obj.PANNCard + '</label></td>';
    //                row = row + '<td><label id="lblSectionCode">' + obj.SectionCode + '</label></td>';
    //                row = row + '<td><label id="lblPaymentAmount">' + obj.PaymentAmount + '</label></td>';
    //                row = row + '<td><label id="lblTaxDeductionDate">' + obj.TaxDeductionDate + '</label></td>';
    //                row = row + '<td><label id="lblAmountPaidDate">' + obj.AmountPaidDate + '</label></td>';
    //                row = row + '<td><label id="lblTDSAmount">' + obj.TDSAmount + '</label></td>';
    //                row = row + '<td><label id="lblSurchargeAmount" >' + obj.SurchargeAmount + '</label></td>';
    //                row = row + '<td><label id="lblHealthECAmount">' + obj.HealthECAmount + '</label></td>';
    //                row = row + '<td><label id="lblSHECAmount">' + obj.SHECAmount + '</label></td>';
    //                row = row + '<td><label id="lblTotalAmount">' + obj.TotalAmount + '</label></td>';
    //                row = row + '<td><label id="lblTotalTaxDeductionAmount">' + obj.TotalAmount + '</label></td>';
    //                row = row + '<td><input type="text" id="txtReason" class="form-control form-control-sm" value="' + obj.Reason + '"' + '/></td>';
    //                row = row + '<td><input type="text" id="txtCertificateno" class="form-control form-control-sm" value="' + obj.Certificateno + '"' + '/></td>';
    //                if (obj.IDAnnexure != 0) {
    //                    row = row + '<td><input type="button" id="btnSave" value="Save" class="btn btn-sm btn-success"></td>';
    //                }
    //                else {
    //                    row = row + '<td><input type="button" id="btnSave" value="Save" class="btn btn-sm btn-primary"></td>';
    //                }
    //                row = row + '</tr>';
    //                $("#tblAnnexureBody").append(row);
    //            });
    //        });
    //    }
    //});
    //function Validation() {
    //    let Year = $("#ddlYear :selected").val();
    //    let Month = $("#ddlMonth :selected").val();
    //    let Employee = $("#ddlEmployee :selected").val();
    //    if (Year === "") {
    //        alert("Year is missing....");
    //        $("#ddlYear").focus();
    //        return false;
    //    }
    //    if (Month === "") {
    //        alert("Month is missing....");
    //        $("#ddlMonth").focus();
    //        return false;
    //    }
    //    //if (Employee === "0") {
    //    //    alert("Employee is missing....");
    //    //    $("#ddlEmployee").focus();
    //    //    return false;
    //    //}

    //    return true;
    //}
    //$("#chkAll").on('click', function () {
    //    let value = this.checked;
    //    let ctl = null;
    //    console.log(value);
    //    $("#LoanProcessBody tr").each(function () {
    //        $(this).find("#chkProcess").prop('checked', value);
    //    });
    //});
    //$("#tblAnnexureBody").on('click', "#btnSave", function () {
    //    let records = [];
    //    let obj = {};
    //    let IDAnnexure = $(this).closest("tr").find("#hdIDAnnexure").val();
    //    let IDEmployee = $(this).closest("tr").find("#hdIDEmployee").val();
    //    let EmpMonth = $(this).closest("tr").find("#hdEmpMonth").val();
    //    let EmpYear = $(this).closest("tr").find("#hdEmpYear").val();
    //    let ChallanNo = $(this).closest("tr").find("#txtChallanNo").val();
    //    let PANNCard = $(this).closest("tr").find("#lblPANNCard").html();
    //    let PaymentAmount = $(this).closest("tr").find("#lblPaymentAmount").html();
    //    let SectionCode = $(this).closest("tr").find("#lblSectionCode").html();
    //    let TaxDeductionDate = $(this).closest("tr").find("#lblTaxDeductionDate").html();
    //    let AmountPaidDate = $(this).closest("tr").find("#lblAmountPaidDate").html();
    //    let TDSAmount = $(this).closest("tr").find("#lblTDSAmount").html();
    //    let SurchargeAmount = $(this).closest("tr").find("#lblSurchargeAmount").html();
    //    let HealthECAmount = $(this).closest("tr").find("#lblHealthECAmount").html();
    //    let SHECAmount = $(this).closest("tr").find("#lblSHECAmount").html();
    //    let TotalAmount = $(this).closest("tr").find("#lblTotalAmount").html();
    //    let TotalTaxDeductionAmount = $(this).closest("tr").find("#lblTotalTaxDeductionAmount").html();
    //    let Reason = $(this).closest("tr").find("#txtReason").val();
    //    let Certificateno = $(this).closest("tr").find("#txtCertificateno").val();

    //    obj = {
    //        "IDAnnexure": IDAnnexure,
    //        "IDEmployee": IDEmployee,
    //        "EmpMonth": EmpMonth,
    //        "EmpYear": EmpYear,
    //        "ChallanNo": ChallanNo,
    //        "PANNCard": PANNCard,
    //        "PaymentAmount": PaymentAmount,
    //        "SectionCode": SectionCode,
    //        "TaxDeductionDate": TaxDeductionDate,
    //        "AmountPaidDate": AmountPaidDate,
    //        "TDSAmount": TDSAmount,
    //        "SurchargeAmount": SurchargeAmount,
    //        "HealthECAmount": HealthECAmount,
    //        "SHECAmount": SHECAmount,
    //        "TotalAmount": TotalAmount,
    //        "TotalTaxDeductionAmount": TotalTaxDeductionAmount,
    //        "Reason": Reason,
    //        "Certificateno": Certificateno
    //    };
    //    records.push(obj);
    //    let url = "/Annexure/Annexure92BSave";
    //    $.ajax({
    //        type: "POST",
    //        url: url,
    //        data: JSON.stringify(records),
    //        contentType: "application/json; charset=utf-8",
    //        dataType: "json",
    //        success: function (data) {
    //            alert(data.success == "" ? "Record successfully saved...." : Result);
    //        }
    //    });
    //});
