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
        let info = {"EmployeeNo":$("#txtempno").val()};
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
                    row = row + '<td> ' + obj.LoanDate + '</td>' ;
                    row = row + '<td id="IdLoan"> ' + obj.LoanNo + '</td>';
                    row = row + '<td id="EmpName">' + obj.EmpName + '</td>';
                    row = row + '<td>' + obj.LoanType + '</td>';
                    row = row + '<td> ' + obj.LoanAmount + '</td>';
                    /*
                    row = row + '<td> ' + obj.TotalReceivedAmount + '</td >';
                    row = row + '<td> ' + obj.CompleteYN + '</td >';
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
        let info = { "LoanNo": IdLoan };
        $("#hdIDLoan").val(IdLoan);
        $("#modalTitle").html(modalTitle);
        $.ajax({
            type: "GET",
            url: "LoanDetail",
            data: info,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var parsed = $.parseJSON(msg);
                $("#tblModalLoanDetailsBody").empty();
                $("#ddlMonth").empty();
                $.each(parsed, function (index, obj) {
                    var row = '';
                    let salary = obj.SalaryProcessed == true ? "YES" : "NO";
                    row = row + '<tr>';
                    row = row + '<td id="IDDetail" style="display:none">' + obj.IDDetail + '</td>';
                    row = row + '<td id="IDLoan" style = "display:none">' + obj.IDLoan + '</td>';
                    row = row + '<td>' + obj.OpeningAmount + '</td >';
                    row = row + '<td id="IDInstallment">' + obj.InstallmentAmount + '</td>';
                    row = row + '<td id="IDInstallmentInterest">' + obj.InterestAmount + '</td>';
                    row = row + '<td>'+ obj.ClosingAmount + '</td>';
                    row = row + '<td id="IDDetailSalaryProcessed">' + salary + '</td>';
                    row = row + '<td>' + obj.InstallmentMonth + '</td>';
                    row = row + '<td>' + obj.InstallmentYear + '</td>';
                    if (obj.Processed == true) {
                        row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed" checked></td>';
                    }
                    else {
                        row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed"></td>';
                    }
                    row = row + '<td>' + obj.ProcessDate + '</td>';
                    row = row + '<td id="IDReceivedAmount"> <input type="text" class="form-control small form-control-sm border-0 text-right" style="height:25px" id="IDRAmount" value=' + obj.ReceivedAmount + ' disabled></td>'
                    row = row + '<td id="useraction" style="display:none" >' + salary + '</td>';
                    row = row + '</tr>';
                    $("#tblModalLoanDetailsBody").append(row);

                    // Month List 
                    $("#ddlMonth").append('<option>' + obj.InstallmentMonth + '</option>');
                });
            }
        });
        $('#modalEmpLoanDtls').modal('show');
    });
    //Loan Processed Amount Changes Checked Or Unchecked
    $("#tblModalLoanDetailsBody").on('change', ".chbProcessed", function () {
        let chkValue = $(this).closest("tr").find("#chkProcessed").is(":checked")
        let textBox = $(this).closest("tr").find("#IDRAmount");
        let installment = $(this).closest("tr").find("#IDInstallment").html();
        //let useraction = $(this).closest("tr").find("#useraction");
        let installmentInterest = $(this).closest("tr").find("#IDInstallmentInterest").html();
        let totalAmount = parseFloat(installment) + parseFloat(installmentInterest);
        //useraction = useraction.html("USERACTION");
        $(textBox).val(0);
        if (chkValue == true) {
            $(textBox).val(totalAmount);
        }
    });
    $("#btnSave").on('click', function () {
        let param = [];
        $.each($("#tblModalLoanDetailsBody").find("tr"), function () {
            let chkSalaryProcessed = $(this).find("#IDDetailSalaryProcessed").html();
            let IDDetail = $(this).closest("tr").find("#IDDetail").html();
            let IDLoan = $(this).closest("tr").find("#IDLoan").html();
            let RecAmount = $(this).closest("tr").find("#IDRAmount").val();
            let useraction = $(this).closest("tr").find("#useraction").html();
            if (chkSalaryProcessed === "NO") {
                param.push({
                    "IDDetail": IDDetail,
                    "IDLoan": IDLoan,
                    "ReceivedAmount": RecAmount
                });
            }
        });
        $.ajax({
            type: "POST",
            url: "LoanProcessed",
            data: JSON.stringify(param),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                alert(data.Result == "" ? "Record successfully saved...." : Result);
            }
        });
    });
    // Change Loan 
    $("#btnChangeLoan").click(function () {
        let info = {
            "LoanNo": $("#hdIDLoan").val(),
            "InsAmount": $("#txtInsAmount").val(),
            "ChangeMonth": $("#ddlMonth :selected").val()
        };
        $.ajax({
            type: "POST",
            url: "ChangeLoan",
            data: JSON.stringify(info),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                $("#tblModalLoanDetailsBody").empty();
                let parsed = $.parseJSON(data);
                $.each(parsed, function (index, obj) {
                    var row = '';
                    let salary = obj.SalaryProcessed == true ? "YES" : "NO";
                    let processdate = obj.ProcessDate == null ? "" : new Date(obj.ProcessDate).toDateString();
                    let amount = obj.ReceivedAmount == null ? 0 : obj.ReceivedAmount;
                    row = row + '<tr>';
                    row = row + '<td id="IDDetail" style="display:none">' + obj.IDDetail + '</td>';
                    row = row + '<td id="IDLoan" style = "display:none">' + obj.IDLoan + '</td>';
                    row = row + '<td>' + obj.OpeningAmount + '</td>';
                    row = row + '<td id="IDInstallment">' + obj.InstallmentAmount + '</td>';
                    row = row + '<td id="IDInstallmentInterest">' + obj.InterestAmount + '</td>';
                    row = row + '<td>' + obj.ClosingAmount + '</td>';
                    row = row + '<td id="IDDetailSalaryProcessed">' + salary + '</td>';
                    row = row + '<td>' + obj.InstallmentMonth + '</td>';
                    row = row + '<td>' + obj.InstallmentYear + '</td>';
                    if (obj.Processed == true) {
                        row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed" checked></td>';
                    }
                    else {
                        row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed"></td>';
                    }
                    row = row + '<td>' + processdate + '</td>';
                    row = row + '<td id="IDReceivedAmount"> <input type="text" class="form-control small form-control-sm border-0 text-right" style="height:25px" id="IDRAmount" value=' + amount + ' disabled></td>'
                    row = row + '<td id="useraction" style="display:none" >' + salary + '</td>';
                    row = row + '</tr>';
                    $("#tblModalLoanDetailsBody").append(row);

                });

            }
        });
    });
});