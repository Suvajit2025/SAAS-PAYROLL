$(function () {
    defaultLoad();
    function defaultLoad() {
        let url = "/Annexure/YearList";
        $.get(url, function (data) {
            // Year List 
            $("#ddlYear").empty();
            $("#ddlYear").append('<option value="0">Select</option>');
            data.map(function (z) {
                $("#ddlYear").append('<option value=' + z.Name + '>' + z.Name + '</option>');
            });

            // Company List 
            url = "/Annexure/CompanyList";
            $("#ddlCompany").empty();
            $("#ddlCompany").append('<option value="0">Select</option>');
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
    $("#btnShow").on('click', function () {
        if (ShowValidation() === true) {
            let url = "/Annexure/Form16List";
            let param = {
                "Companycode": $("#ddlCompany :selected").val(),
                "Year": $("#ddlYear :selected").val(),
                "Employeeno": $("#ddlEmployee :selected").val() == "" ? "0" : $("#ddlEmployee :selected").val()
            }
            var row = '';
            $.get(url, param, function (data) {
                $("#tblForm16Body").empty();
                data.map(function (obj) {
                    row = '';
                    row = row + '<tr>';
                    row = row + '<td>';
                    row = row + '<input type="hidden" id="hdIDForm" value="' + obj.IDForm + '"' + '/> ';
                    row = row + '<input type="hidden" id="hdIDEmployee" value="' + obj.Employee.IDEmployee + '"' + '/> ';
                    row = row + '<label id="lblEmployeeNo">' + obj.Employee.EmployeeNo + '</label>';
                    row = row + '</td>';
                    row = row + '<td>';
                    row = row + '<label id="lblEmployeeName">' + obj.Employee.EmployeeName + '</label></td>';
                    row = row + '</td>';
                    row = row + '<td>';
                    row = row + '<label id="lblPAN">' + obj.Employee.PAN + '</label></td>';
                    row = row + '</td>';
                    row = row + '<td>';
                    row = row + '<label id="lblStatusA">' + obj.StatusA + '</label>';
                    row = row + '</td>';
                    row = row + '<td>';
                    row = row + '<input type="file" id="txtFileA" class="form-control form-control-sm" />';
                    row = row + '</td>';
                    row = row + '<td>';
                    row = row + '<label id="lblStatusB">' + obj.StatusB + '</label>';
                    row = row + '</td>';
                    row = row + '<td>';
                    row = row + '<input type="file" id="txtFileB" class="form-control form-control-sm small" />';
                    row = row + '</td>';
                    row = row + '<td>';
                    if (obj.StatusA != '') {
                        row = row + '<input type="button" id="btnUpload" class="btn btn-sm btn-success" value="Upload"/> ';
                    }
                    else {
                        row = row + '<input type="button" id="btnUpload" class="btn btn-sm btn-primary" value="Upload"/> ';
                    }
                    row = row + '</td>';
                    row = row + '</tr>';
                    $("#tblForm16Body").append(row);
                });
            });
        }
    });
    $("#tblForm16Body").on('click', "#btnUpload", function () {
        var mdata = new FormData();
        mdata.append("empno", $(this).closest("tr").find("#lblEmployeeNo").html());
        mdata.append("postedfileA", $(this).closest("tr").find("#txtFileA").prop('files')[0]);
        mdata.append("postedfileB", $(this).closest("tr").find("#txtFileB").prop('files')[0]);
        mdata.append("year", $("#ddlYear :selected").val());
        mdata.append("idform", $(this).closest("tr").find("#hdIDForm").val());
        mdata.append("companycode", $("#ddlCompany :selected").val());

        let fileA = $(this).closest("tr").find("#txtFileA").val();
        let fileB = $(this).closest("tr").find("#txtFileB").val();

        if (fileA === "") {
            alert("Part A file is missing....");
            $(this).closest("tr").find("#txtfileA").focus();
            return;
        }
        else if (fileB  === "") {
            alert("Part B file is missing....");
            $(this).closest("tr").find("#txtfileB").focus();
            return;
        }
        else {
            let url = "/annexure/form16upload";
            $.ajax({
                url: url,
                contentType: false,
                processData: false,
                type: "post",
                datatype: 'json',
                data: mdata, 
                success: function (data) {
                    alert(data.success == "" ? "record successfully saved...." : data.error);
                }
            });
        }
    });


    function ShowValidation() {
        let Year = $("#ddlYear :selected").val();
        let Company = $("#ddlCompany :selected").val();
        if (Year === "0") {
            alert("Year is missing....");
            $("#ddlYear").focus();
            return false;
        }
        if (Company === "0") {
            alert("Company is missing....");
            $("#ddlCompany").focus();
            return false;
        }
        return true;
    }
});