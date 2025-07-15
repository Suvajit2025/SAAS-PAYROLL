$(function () {
    $("#btnShow").on('click', function () {
        let url = "/Loan/ApprovalList";
        $.get(url, function (data) {
            let newdata = JSON.parse(data);
            $("#tblDataBody").empty();
            console.log(newdata);
            newdata.map(function (obj) {
                var row = '';
                row = row + '<tr>';
                row = row + '<td><label id="lblAppDate"> ' + obj.AppDate + '</label></td>';
                row = row + '<td><label id="lblAppNo"> ' + obj.AppNo + '</td>';
                row = row + '<td><label id="lblEmpNo"> ' + obj.EmployeeNo + '</td>';
                row = row + '<td><label id="lblEmpName"> ' + obj.EmployeeName + '</td>';
                row = row + '<td><label id="lblEmpName"> ' + obj.LoanNo + '</td>';
                row = row + '<td><label id="lblRequestAmount"> ' + obj.RequestAmount + '</td>';
                row = row + '<td><label id="lblApprovedAmount"> ' + obj.ApprovedAmount + '</td>';
                row = row + '<td><input type="hidden" id="hdPurpose" value="' + obj.Purpose + '"/>';

                if (obj.Reason === '') {
                    row = row + '<input type="hidden" id="hdReason" value=""/>';
                }
                else {
                    row = row + '<input type="hidden" id="hdReason" value="' + obj.Reason + '"/>';
                }
                if (obj.RejectReason === '') {
                    row = row + '<input type="hidden" id="hdRejectReason" value=""/>';
                }
                else {
                    row = row + '<input type="hidden" id="hdRejectReason" value="' + obj.RejectReason + '"/>';
                }
                row = row + '<input type="hidden" id="hdApproved" value="' + obj.Approved + '"/>';
                row = row + '<input type="hidden" id="hdRejected" value="' + obj.Rejected + '"/>';
                row = row + '<input type="hidden" id="hdIDApplication" value="' + obj.IDApplication + '"/>';
                row = row + '<input type="hidden" id="hdIDIDLoan" value="' + obj.IDLoan + '"/>';
                if (obj.IDLoan === 0) {
                    row = row + '<input type="button" id="btnApproved" class="btn btn-sm btn-primary mr-2" value="Approve/Reject">';
                    row = row + '<input type="button" id="btnLoan" class="btn btn-sm btn-primary" style="margin-left:10px" value="Loan"></td>';
                }
                row = row + '</tr>';
                $("#tblDataBody").append(row);
            });
        });
    });

    $("#btnApprovedLoan").on('click', function () {
        if (ApprovalValidation() === true) {

            let Approved = $("#ddlApproval :selected").val() === "APPROVED" ? true : false;
            let Rejected = $("#ddlApproval :selected").val() === "REJECTED" ? true : false;
            let ApprovedAmount = $("#txtApprovedAmount").val();
            let RejectReason = $("#txtRejectReason").val();
            let IDApplication = $("#hdIDApplication").val();
            let user = $("#hdUserEmail").val();
            let url = "/Loan/LoanApproval";
            let data = {
                "Approval": Approved,
                "ApprovedAmount": ApprovedAmount,
                "Rejected": Rejected,
                "RejectReason": RejectReason,
                "IDApplication": IDApplication,
                "User": user
            };
            console.log(data);
            $.get(url, data, function (data) {
                alert(data.Result == "" ? "Record successfully saved...." : Result);
            });
        }
    });

    function ApprovalValidation() {
        let Approved = $("#ddlApproval :selected").val();
        let RequestAmount = $("#txtRequestedAmount").val();
        let ApprovedAmount = $("#txtApprovedAmount").val();
        let RejectReason = $("#txtRejectReason").val();

        if (Approved === 'APPROVED' && (ApprovedAmount === "" || ApprovedAmount === "0")) {
            alert("Approval selected but approved amount is missing....");
            $("#txtApprovedAmount").focus();
            return false;
        }
        if (Approved === 'APPROVED' && (parseFloat(ApprovedAmount) > parseFloat(RequestAmount))) {
            alert("Approval amount can not be more than requested amount....");
            $("#txtApprovedAmount").val(0);
            $("#txtApprovedAmount").focus();
            return false;
        }
        if (Approved === 'REJECTED' && RejectReason === "") {
            alert("Rejection selected but rejection reason is missing....");
            $("#txtRejectReason").focus();
            return false;
        }
        if (Approved === 'APPROVED' && RejectReason != "") {
            alert("Rejection selected is missing....");
            $("#ddlApproval").focus();
            return false ;
        }
        return true;

    }
    $("#tblDataBody").on('click', "#btnLoan", function () {
        let Empno = $(this).closest('tr').find("#lblEmpNo").text().trim();
        let Empname = $(this).closest('tr').find("#lblEmpName").text().trim();
        let ApprovedAmount = $(this).closest('tr').find("#lblApprovedAmount").text().trim();
        let Appno = $(this).closest('tr').find("#lblAppNo").text().trim();
        let IDApplication = $(this).closest('tr').find("#hdIDApplication").val().trim();
        let user = $("#hdUserEmail").val();

        $("#txtLoanEmpno").val(Empno);
        $("#txtLoanEmpname").val(Empname);
        $("#txtLoanAmount").val(ApprovedAmount);
        $("#txtLoanInstallment").val(0);
        $("#txtLoanTenure").val(0);
        $("#txtLoanSDate").val("");
        $("#txtLoanEDate").val("");
        $("#hdIDApplication").val(IDApplication);
        $("#hdAppno").val(Appno);
        $("#hdUser").val(user);
        

        // Date         
        $("#txtLoanSDate").datepicker({ dateFormat:'dd/M/yy' });
        $("#txtLoanEDate").datepicker({ dateFormat: 'dd/M/yy' });

        // Loan No 
        let url = "/Loan/AutoLoanNo"
        let type = "TRANSACTION";
        let LoanNo=""
        $.get(url, { Type: type }, function (data) {
            LoanNo = data;
            $("#txtLoanNo").val(LoanNo);
            $("#txtLoanAppno").val(Appno);
            $("#modalLoanTitle").text(Empname);
            $("#modalLoan").modal('show');
        });
    });
    // Loan Tenure calculation
    $("#txtLoanInstallment").change(function () {
        let txtloanamount = $("#txtLoanAmount").val();
        let txtmonthlyinstallment = $("#txtLoanInstallment").val();
        let tenure = Math.ceil(txtloanamount / txtmonthlyinstallment);
        $("#txtLoanTenure").val(tenure);
    });


    // Calculate End Date
    function addMonths(date, month) {
        var d = date.getDate();
        date.setMonth(date.getMonth() + month);
        if (date.getDate() != d) {
            date.setDate(0);
        }
        return date;
    }

    // Convert date string into date
    function convertDate(str) {
        const mnames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        ];
        let date = new Date(str);
        let monthno = ("0" + (date.getMonth() + 1)).slice(-2);
        let day = ("0" + date.getDate()).slice(-2);
        let index = parseInt(monthno);
        let monthname = mnames[index];
        let dd = day + '/' + monthname + '/' + date.getFullYear();
        return dd;  //ay+ monthname, date.getFullYear()].join("/");
    }

    // Add months To End Date Based on Tenure
    $("#txtLoanSDate,#txttenure").change(function () {
        let tenure = $("#txtLoanTenure").val();
        let loanstartdate = $("#txtLoanSDate").val();
        let loanenddate = addMonths(new Date(loanstartdate), tenure).toString();
        $("#txtLoanEDate").val(convertDate(loanenddate));
    });
    $("#tblDataBody").on('click', "#btnApproved", function () {
        let Empno = $(this).closest('tr').find("#lblEmpNo").text().trim();;
        let Empname = $(this).closest('tr').find("#lblEmpName").text().trim();;
        let RequestedAmount = $(this).closest('tr').find("#lblRequestAmount").text().trim();;
        let ApprovedAmount = $(this).closest('tr').find("#lblApprovedAmount").text().trim();;
        let Purpose = $(this).closest('tr').find("#hdPurpose").val();
        let Reason = $(this).closest('tr').find("#hdReason").val();
        let RejectReason = $(this).closest('tr').find("#hdRejectReason").val();
        let IDApplication = $(this).closest('tr').find("#hdIDApplication").val();
        let user = $("#hdUserEmail").val();
        let Appno = Empname + "(" + $("#lblAppNo").text() + ")";

        $("#txtEmpno").val(Empno);
        $("#txtEmpname").val(Empname);
        $("#txtRequestedAmount").val(RequestedAmount);
        $("#txtApprovedAmount").val(ApprovedAmount);
        $("#txtPurpose").val(Purpose);
        $("#txtReason").val(Reason);
        $("#txtRejectReason").val(RejectReason);
        $("#hdIDApplication").val(IDApplication);
        $("#hdUser").val(user);
        $("#modalLoanTitle").text(Appno);
        $("#modalLoanApp").modal('show');
    });

    // Save Validation 
    function Validation() {
        let txtLoanInstallment = $("#txtLoanInstallment");
        let txtLoanTenure = $("#txtLoanTenure");
        let txtLoanSDate = $("#txtLoanSDate");
        let txtLoanEDate = $("#txtLoanEDate");


        if (txtLoanInstallment.val() == '' || txtLoanInstallment.val()=='0') {
            alert("Loan installment amount is missing....");
            txtLoanInstallment.focus();
            return false;
        }
        if (txtLoanTenure.val() == '' || txtLoanInstallment.val() == '0') {
            alert("Loan tenure value is missing....");
            txtLoanTenure.focus();
            return false;
        }
        if (txtLoanSDate.val() == '') {
            alert("Loan start date value is missing....");
            txtLoanSDate.focus();
            return false;
        }
        if (txtLoanEDate.val() == '') {
            alert("Loan end date value is missing....");
            txtLoanEDate.focus();
            return false;
        }
        return true;
    }

    // Save Button
    $("#btnLoanCreation").click(function () {
        if (Validation() == true) {
            let info = {
                "IDLoan": 0,
                "LoanType":'TRANSACTION',
                "LoanNo": $("#txtLoanNo").val(),
                "EmpNo": $("#txtLoanEmpno").val(),
                "LoanDate": $("#txtLoanSDate").val(),
                "EndDate": $("#txtLoanEDate").val(),
                "RefNo": $("#hdAppno").val(),
                "RefDate": $("#txtLoanSDate").val(),
                "LoanAmount": $("#txtLoanAmount").val(),
                "LoanInterestPcn":0,
                "LoanTenure": $("#txtLoanTenure").val(),
                "MonthlyInstallmentAmount": $("#txtLoanInstallment").val(),
                "MonthlyInterestAmount": 0,
                "MonthlyLoan": $("#txtLoanInstallment").val(),
                "TotalInterestAmount": 0,
                "TotalLoanAmount": $("#txtLoanAmount").val()
            };

            let url = "/Loan/SaveLoan";
            $.ajax({
                url: url,
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(info),
                success: function (res) {
                    if (res.success == "") {
                        alert("Loan successfully created....");
                        window.location.href = "Approval";
                    }
                    else {
                        alert(res.result);
                    }
                },
            });
        }
    });

});