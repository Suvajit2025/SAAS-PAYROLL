$(function () {
    defaultLoad();
    function defaultLoad() {
        // Pay Group
        let url = "/SalaryProcessValidation/PayGroupList";
        $.get(url, function (data) {
            $("#ddlPayGroup").empty();
            $("#ddlPayGroup").append('<option value=select>Select</option>');
            data.map(x => $("#ddlPayGroup").append('<option value=' + x.IDPaygroup + '>' + x.Name + '</option>'));
        });

        // Month 
        url = "/SalaryProcessValidation/PayrollMonthList";
        $.get(url, function (data) {
            $("#ddlMonth").empty();
            $("#ddlMonth").append('<option value=select>Select</option>');
            data.map(x => $("#ddlMonth").append('<option  value=' + x.Month +'>' + x.Month + '</option>'));
        });

        // Year 
        url = "/SalaryProcessValidation/PayrollYearList";
        $.get(url, function (data) {
            $("#ddlYear").empty();
            $("#ddlYear").append('<option>Select</option>');
            data.map(x => $("#ddlYear").append('<option value=' + x.Year + '>' + x.Year + '</option>'));
        });

    }
    $("#btnShow").on('click', function () {
        // Table 
        url = "/SalaryProcessValidation/PayrollProcessList";
        let param = {
            IDPayGroup: $("#ddlPayGroup").val(),
            Month: $("#ddlMonth").val(),
            Year: $("#ddlYear").val()
        };

        $.get(url, param, function (data) {
            $("#ProcessSalary").empty();
            let row = '';
            let index = 1;
            data.map(function (x) {
                row = '<tr>';
                row = row + '<td>';
                row = row + '<label>' + index + '</label>';
                row = row + '</td> ';
                row = row + '<td>';
                row = row + '<input type=hidden id="hdidPayroll" value=' + x.IDPayroll + ' > ';
                row = row + '<input type=hidden id="hdidEmployee" value=' + x.IDEmployee + ' > ';
                row = row + '<label>' + x.EmployeeNo + '</label>'
                row = row + '</td> ';
                row = row + '<td><label>' + x.EmployeeName + '</label></td>';
                row = row + '<td><label>' + x.Department + '</label></td>';
                row = row + '<td><label>' + x.PayGroup + '</label></td>';
                row = row + '<td><label id="lblStatus"></label></td>';
                row = row + '</tr>';
                $("#ProcessSalary").append(row);
                index += 1;
            });
        });
    });
    $("#btnValidate").on('click', function () {
        alert("Mayukh");
        let param = {};
        //url = "/SalaryProcessValidation/PayrollProcessValidation";
        let status = '';
        $("#tblSalary tr").each(function (x) {
            let row = $(this)
            let idEmployee = row.find("#hdidEmployee").val();
            let idPayroll = row.find("#hdidPayroll").val();
            let month = $("#ddlMonth").val();
            let year = $("#ddlYear").val();
            param = {
                IDPayroll: idPayroll,
                IDEmployee: idEmployee,
                Month: month,
                Year: year
            };
            // Data Validation
            $.ajax({
                type: "GET",
                url: "SalaryProcessValidation/PayrollProcessValidation",
                data: param,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                async: false,
                success: function (data) {
                    row.find("#lblStatus").html(data);
                }
            });
        });
    });


});