$(function () {
    defaultLoad();
    function defaultLoad() {
        // Application NO 
        let url = "/Loan/LoanApplicationNo";
        $.get(url, function (data) {
            let newdata = JSON.parse(data);
            $("#txtAppNo").val(newdata[0].Appno)
        });
        // APplicaiton Date 
        // Employee List 
        url = "/Loan/AutoCompleteEmpDeatils";
        $.get(url, function (data) {
            let str = '';
            $("#ddlEmployee").empty()
            $("#ddlEmployee").append("<option>Select</option>");
            data.map(function (value) {
                $("#ddlEmployee").append("<option>" + value + "</option>");
            });
        });
    }
    $("#ddlEmployee").on('change', function () {
        let empDetails = $("#ddlEmployee :selected").val();
        let nameArray = empDetails.split("-");
        let Empno = nameArray[1];
        $("#txtEmpno").val(Empno);
    });
    function ClearEntry() {
        window.location.href = "LoanApplication";
    }
    function ShowEntry() {
        $("#Entry").css("display", "block");
        $("#List").css("display", "none");
    }
    function ShowList() {
        $("#Entry").css("display", "none");
        $("#List").css("display", "block");
    }
    $("#btnAdd").on('click', function () {
        ShowEntry();
        ClearEntry();
    });
    $("#btnList").on('click', function () {
        ShowList();
        let url = "/Loan/LoanApplicationList";
        let obj = {
            "idemployee": 0
        }
        $.get(url, obj, function (data) {
            let newdata = JSON.parse(data);
            $("#tblBody").empty();
            newdata.map(function (obj) {
                var row = '';
                row = row + '<tr>';
                row = row + '<td><label id="lblAppDate"> ' + obj.AppDate + '</label></td>';
                row = row + '<td><label id="lblAppNo"> ' + obj.AppNo + '</td>';
                row = row + '<td><label id="lblEmpNo"> ' + obj.EmployeeNo + '</td>';
                row = row + '<td><label id="lblEmpName"> ' + obj.EmployeeName + '</td>';
                row = row + '<td><label id="lblAmount"> ' + obj.Amount + '</td>';
                row = row + '<td><label id="lblInstallment"> ' + obj.Installment + '</td>';
                row = row + '<td><label id="lblSelf"> ' + obj.SelfApproval + '</td>';
                row = row + '<td><label id="lblApproval1"> ' + obj.Approval1 + '</td>';
                row = row + '<td><label id="lblApproval2"> ' + obj.Approval2 + '</td>';
                row = row + '<td><label id="lblApproval3"> ' + obj.Approval3 + '</td>';
                row = row + '<td><input type="hidden" id="hdIDApplcation" value="' + obj.IDApplication + '"/>';
                row = row + '<input type="button" id="btnEdit" class="btn btn-sm btn-success" value="Edit">';
                row = row + '<input type="button" id="btnPreview" class="btn btn-sm btn-warning" value="Preview">';
                row = row + '</tr>';
                $("#tblBody").append(row);
            });
        });
    });
    $("#btnSave").on('click', function () {
        if (Valudation() == true) {
            let url = "/Loan/LoanApplicationSave";
            let obj = {
                "IDApplicatioon": $("#hdIDApplication").val() === "" ? 0 : $("#hdIDApplication").val(),
                "AppDate": $("#txtAppDate").val(),
                "EmployeeNo": $("#txtEmpno").val(),
                "LoanAmount": $("#txtAmount").val(),
                "Installment": $("#txtInstallment").val(),
                "Reason": $("#txtReason").val(),
                "User": $("#hdUserEmail").val()
            };
            $.ajax({
                type: "POST",
                url: url,
                data: JSON.stringify(obj),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    alert(data.Result == "" ? "Record successfully saved...." : Result);
                    ClearEntry();
                }
            });
        }
    });
    function Valudation() {
        let appno = $("#txtAppNo").val();
        let appdate = $("#txtAppDate").val();
        let EmployeeNo = $("#txtEmpno").val();
        let LoanAmount = $("#txtAmount").val();
        let Installment = $("#txtInstallment").val();
        let Reason = $("#txtReason").val();

        if (appno === "") {
            alert("Loan application no is missing....");
            $("#txtAppNo").focus();
            return false;
        }
        if (appdate === "") {
            alert("Loan application date no is missing....");
            $("#txtAppDate").focus();
            return false;
        }
        if (EmployeeNo === "") {
            alert("Employee no is missing....");
            $("#txtEmpno").focus();
            return false;
        }
        if (LoanAmount === "") {
            alert("Loan amount is missing....");
            $("#txtAmount").focus();
            return false;
        }
        if (Installment === "") {
            alert("Loan installment is missing....");
            $("#txtInstallment").focus();
            return false;
        }
        if (Reason === "") {
            alert("Loan reason is missing....");
            $("#txtReason").fodu();
            return false;
        }
        return true;
    }

    $("#tblBody").on('click', "#btnEdit", function () {
        let IDApp = $(this).closest("tr").find('#hdIDApplcation').val();
        let obj = {
            "idapplication": IDApp
        };
        let url = "/Loan/LoanApplicationDetail";
        $.get(url, obj, function (data) {
            let newdata = JSON.parse(data);
            $("#hdIDApplication").val(newdata[0].IDApplication);
            $("#txtAppNo").val(newdata[0].AppNo);
            $("#txtAppDate").val(newdata[0].AppDate);
            $("#txtAmount").val(newdata[0].Amount);
            $("#txtInstallment").val(newdata[0].Installment);
            $("#txtReason").val(newdata[0].Reason);
            $("#txtEmpno").val(newdata[0].EmployeeNo);
            $("#ddlEmployee").val(newdata[0].EmployeeName + '-' + newdata[0].EmployeeNo);
            ShowEntry();
        });
    });
    $("#tblBody").on('click', "#btnPreview", function () {
        let IDApp = $(this).closest("tr").find('#hdIDApplcation').val();
        let obj = {
            "idapplication": IDApp
        };
        let url = "/Loan/LoanApplicationDetail";
        let str = '';
        $.get(url, obj, function (data) {
            let newdata = JSON.parse(data);
            //$("#hdIDApplication").val(newdata[0].IDApplication);
            //$("#txtAppNo").val(newdata[0].AppNo);
            //$("#txtAppDate").val(newdata[0].AppDate);
            //$("#txtAmount").val(newdata[0].Amount);
            //$("#txtInstallment").val(newdata[0].Installment);
            //$("#txtReason").val(newdata[0].Reason);
            //$("#txtEmpno").val(newdata[0].EmployeeNo);
            //$("#ddlEmployee").val(newdata[0].EmployeeName + '-' + newdata[0].EmployeeNo);


            // Modal Title 
            $('#modalTitle').text("Laon No:" + newdata[0].AppNo);

            // Preview data
            str += '<div class="row">'
            str += '<div class="col-md-12">'
            str += '<p>To <p>';
            str += '<p>The Managing Director <p>';
            str += '<p>To Mendine Pharmaceutical Pvt. Ltd. <p>';
            str += '<p>36 A and B Alipore Road, Kolkata-27 <p>';
            str += '<br>';
            str += '<br>';
            str += '<br>';
            str += '<p> Dear Sir/Madam </p> ';
            str += '<p> I  must request you for an loan of Rs. <b>' + newdata[0].Amount + '</b> for the purpose of ' + newdata[0].Reason + ' </p> ';
            str += '<p> I would exteremly grateful if teh above mentioned loan is sanctioned.All required details are providing below :' + '</p>';
            str += '<p> Thank you <p>';
            str += '<br>';
            str += '<br>';
            str += '<br>';
            str += '<p> Yours Faithfully </p>';
            str += '<p><b> '+ newdata[0].EmployeeName +'</b></p>';

            str += '<br>';
            str += '<br>';
            str += '<br>';

            str += '<table class="table table-sm table-bordered small">';
            str += '<thead>';
            str += '<tr>';
            str += '<th>NO </th>';
            str += '<th>NAME </th>';
            str += '<th>LOAN AMOUNT </th>';
            str += '<th>INSTALLMENT AMOUNT </th>';
            str += '</tr>';
            str += '</thead>';
            str += '<tbody>';
            str += '<tr>';
            str += '<td><label>' + newdata[0].EmployeeNo+ '</label></td>';
            str += '<td><label>' + newdata[0].EmployeeName + '</label></td>';
            str += '<td><label>' + newdata[0].Amount + '</label></td>';
            str += '<td><label>' + newdata[0].Installment + '</label></td>';

            str += '</tr>';
            str += '</tbody>';
            str += '</table>';

            str += '<br>';
            str += '<br>';
            str += '<br>';
            str += '<br>';
            str += '<br>';

            str += '<div class="row">';
            str += '<div class="col-md-3 border-right"><label>SIGNATURE</label><br>(' + newdata[0].EmployeeName  +')</div>';
            str += '<div class="col-md-3"><label>SIGNATURE</label><br> (APPROVED BY) </div>';
            str += '<div class="col-md-3"><label>SIGNATURE</label><br>(FORWARDED BY) </div>';
            str += '<div class="col-md-3"><label>SIGNATURE</label><br>(SANCTIONED BY) </div>';
            str += '</div>';


            str += '</div>';
            str += '</div>';


            $('#divPrint').append(str);
            $("#modalPreview").modal('show');
        });
        

    });



});