$(function () {
    defaultLoad();
    function defaultLoad() {
        let url = "/Loan/AutoCompleteEmpDeatils";
        $.get(url, function (data) {
            $("#txtempname").autocomplete({
                source: data
            });
        });
    }
    $("#txtempname").on('change', function () {
        let empDetails = $("#txtempname").val();
        let nameArray = empDetails.split("-");
        let Empno = nameArray[1];
        $("#txtempno").val(Empno);
    });
    $("#btnSearch").on('click', function () {
        let info = { "EmployeeNo": $("#txtempno").val() };
        $.ajax({
            type: "GET",
            url: "LoanList",
            data: info,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var parsed = $.parseJSON(msg);
                $("#listEmployeeLoanList").empty();
                $.each(parsed, function (index, obj) {
                    var row = '';
                    row = row + '<tr>';
                    row = row + '<td> ' + obj.LoanDate + '</td>';
                    row = row + '<td id="IdLoan"> ' + obj.LoanNo + '</td>';
                    row = row + '<td id="EmpName">' + obj.EmpName + '</td>';
                    row = row + '<td>' + obj.LoanType + '</td>';
                    row = row + '<td> ' + obj.LoanAmount + '</td>';
                    row = row + '<td> ' + obj.TotalReceivedAmount + '</td >';
                    row = row + '<td> ' + obj.ClosingAmount + '</td >';
                    row = row + '<td> ' + obj.LoanStatus + '</td >';
                    /*
                    row = row + '<td> ' + obj.Reason + '</td >';
                    */
                    row = row + '<td> <input type="button" id="btnDetails" value="Details" class="mb-sm mt-xs mr-xs btn btn-sm btn-tertiary text-sm text-weight-semibold" ></td> </tr > ';
                    row = row + '</tr>';
                    $("#listEmployeeLoanList").append(row);
                });
            }
        });
    });
    $("#listEmployeeLoanList").on('click', "#btnDetails", function () {
        let IdLoan = $(this).closest("tr").find("#IdLoan").html();
        let EmpName = $(this).closest("tr").find("#EmpName").html();
        let LoanTitle = "Loan";
        let LoanTitleEmpName = LoanTitle.concat(" ", EmpName);
        let modalTitle = LoanTitleEmpName.concat(" ", IdLoan);
        let info = { "LoanNo": $.trim(IdLoan) };
        $("#hdIDLoan").val(IdLoan);
        $("#modalTitle").html(modalTitle);
        $.ajax({
            type: "GET",
            url: "LoanRevisedDetail",
            data: info,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var parsed = $.parseJSON(msg);
                $("#tblModalLoanDetailsBody").empty();
                $("#ddlMonth").empty();
                $("#ddlYear").empty();

                // Showing data
                ShowData(parsed);
                // ON Success Loan Month 
                url = "/Loan/LoanRevisedMonthList";
                $.get(url, info, function (res) {
                    $("#ddlMonth").empty();
                    var newres = $.parseJSON(res);
                    newres.map(function (x) {
                        $("#ddlMonth").append('<option value=' + x.Months + '>' + x.Months + '</option>');
                    });
                });
            }
        });
        $('#modalEmpLoanDtls').modal('show');
    });
    function ShowData(data) {
        $("#tblModalLoanDetailsBody").empty();
        $.each(data, function (index, obj) {
            row = '';
            row = row + '<tr>';
            row = row + '<td>';
            row = row + '<label id="lblIDDetail" style="display:none">' + obj.IDDetail + '</label> ';
            row = row + '<label id="lblIDLoan"  style="display:none">' + obj.IDLoan + '</label> ';
            row = row + '<label id="lblMonth">' + obj.InstallmentMonth + '</label>';
            row = row + '</td>';
            row = row + '<td><label id="lblYear">' + obj.InstallmentYear + '</label></td>';
            row = row + '<td><label id="lblOP">' + obj.OpeningAmount + '</label></td >';
            row = row + '<td><label id="lblIns">' + obj.InstallmentAmount + '</label></td>';
            row = row + '<td><label id="lblInterest">' + obj.InterestAmount + '</label></td>';
            row = row + '<td><label id="lblCL">' + obj.ClosingAmount + '</label></td>';
            row = row + '<td><label id="lblLoanProcessed">' + obj.LoanProcessed + '</label></td>';
            row = row + '<td><label id="lblProcessData">' + obj.ProcessDate + '</label></td>';
            row = row + '<td><label id="lblSalaryProcessed">' + obj.SalaryProcessed + '</label></td>';
            row = row + '</tr>';
            $("#tblModalLoanDetailsBody").append(row);
        });

    }
    $("#btnChangeLoan").click(function () {
        
        let txtInsAmount = $("#txtInsAmount").val();
        let arry = $("#ddlMonth :selected").val().split("-");
        let monthname =arry[0];
        let yearvalue = arry[1];
        if (arry.length === 0) {
            alert("Revised month is missing....");
            return false
        }
        if (txtInsAmount === '') {
            alert("Revised amount is missing....");
            return false
        }
        if (confirm("Are you sure to revise the loan installment....?") == true) {
            let info = {
                "LoanNo": $("#hdIDLoan").val(),
                "InsAmount": $("#txtInsAmount").val(),
                "ChangeMonth": monthname,
                "ChangeYear": yearvalue,
            };
            $.ajax({
                type: "POST",
                url: "RevisedLoan",
                data: JSON.stringify(info),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    let parsed = $.parseJSON(data);
                    ShowData(parsed);
                }
            });
        }
    });
});