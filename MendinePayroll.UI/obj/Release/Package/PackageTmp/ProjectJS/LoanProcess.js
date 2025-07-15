$(function () {
    defaultLoad();
    function defaultLoad() {
        let url = "/Loan/LoanEmployeeList";
        $.get(url, function (data) {
            $("#ddlEmployee").empty();
            $("#ddlEmployee").append('<option value="0">Select</option>');
            data.map(function (z) {
                $("#ddlEmployee").append('<option value=' + z.IDEmployee + '>' + z.EmployeeName + '</option>');
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
        if (Validation() == true) {
            let url = "/Loan/LoanProcessData";
            info = {
                "IDEmployee": $("#ddlEmployee").val() === "0" ? "0" : $("#ddlEmployee").val()
            }
            $.get(url, info, function (data) {
                let newdata = JSON.parse(data);
                $("#LoanProcessBody").empty();
                newdata.map(function (obj) {
                    var row = '';
                    row = row + '<tr>';
                    row = row + '<td><label id="lblEmployeeno"> ' + obj.EmployeeNo + '</label></td>';
                    row = row + '<td><label id="lblEmployeename"> ' + obj.EmployeeName + '</td>';
                    row = row + '<td><label id="lblLondate"> ' + obj.LoanDate + '</td>';
                    row = row + '<td><label id="lblLoanno"> ' + obj.LoanNo + '</td>';
                    row = row + '<td><label id="lblInsMonth"> ' + obj.InstallmentMonth + '</td>';
                    row = row + '<td><label id="lblInsYear"> ' + obj.InstallmentYear + '</td>';
                    row = row + '<td><label id="lblMonthly"> ' + obj.MonthlyLoan + '</td>';
                    if (obj.Processed == true) {
                        row = row + '<td><input type="checkbox" id="chkProcess" checked></td>';
                    }
                    else {
                        row = row + '<td><input type="checkbox" id="chkProcess"></td>';
                    }
                    row = row + '<td><input type="hidden" id="hdIDLoan" value=' + obj.IDLoan + ' />';
                    row = row + '<input type="hidden" id="hdIDSRL" value=' + obj.SRL + ' />';
                    row = row + '<input type="hidden" id="hdIDDetail" value=' + obj.IDDetail + ' /></td>';
                    row = row + '</tr>';
                    $("#LoanProcessBody").append(row);
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
        if (Employee === "0") {
            alert("Employee is missing....");
            $("#ddlEmployee").focus();
            return false;
        }

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

    // Show Loan Sanctioned, Loan Realised And Loan Due
    $('body').on('click', '#chkProcess', function () {
        let value = this.checked;
        //console.log(value);
        //alert("Pallab");
        if (value) {
            $("#IDLoanSanctionedDueRealised").css({ display: "block" });
            let url = "/Loan/Sanctioned_Realised_Due_Loan";
            info = {
                "IDEmployee": $("#ddlEmployee").val() === "0" ? "0" : $("#ddlEmployee").val()
            }
            $.get(url, info, function (data) {
                let newdata = JSON.parse(data);
                $("#listLoanSanctionedDueRealised").empty();
                newdata.map(function (obj) {
                    var row = '';
                    row = row + '<tr>';
                    row = row + '<td><label id="lblEmployeeno"> ' + obj.IDEmployee + '</label></td>';
                    /* row = row + '<td><label id="lblEmployeename"> ' + obj.EmployeeName + '</td>';*/
                    row = row + '<td><label id="lblLondate"> ' + obj.SanctionedLoanAmount + '</td>';
                    row = row + '<td><label id="lblLoanno"> ' + obj.LoanRealized + '</td>';
                    row = row + '<td><label id="lblInsMonth"> ' + obj.DueLoanAmount + '</td>';

                    row = row + '</tr>';
                    $("#listLoanSanctionedDueRealised").append(row);
                });

            });
        }
        else {
            $("#IDLoanSanctionedDueRealised").css({ display: "none" });
        }
    });

    $("#btnProcess").on('click', function () {
        if (ProcessValidation() == true) {
            let records = [];
            let obj = {};
            $("#LoanProcessBody tr").each(function () {
                obj = {
                    "SRL": $(this).find("#hdIDSRL").val(),
                    "IDDetail": $(this).find("#hdIDDetail").val(),
                    "IDLoan": $(this).find("#hdIDLoan").val(),
                    "ReceivedAmount": $(this).find("#chkProcess").prop("checked") == true ? $(this).find("#lblMonthly").text().trim() : "0",
                    "EmployeeName": $(this).find("#lblEmployeename").text().trim(),
                    "EmployeeNo": $(this).find("#lblEmployeeno").text().trim()
                };
                records.push(obj);
            });

            // Distinct  Loan No 
            let distinctloan = [...new Set(records.map(x => x.IDLoan))];
            distinctloan.map(function (value) {
                let insdata = records.filter(obj => obj.IDLoan === value);
                let emploan = [];
                let total = 0;
                insdata.map(function (z) {
                    let obj = {
                        "SRL": z.SRL,
                        "IDDetail": z.IDDetail,
                        "IDLoan": z.IDLoan,
                        "ReceivedAmount": z.ReceivedAmount
                    };
                    total = total + parseFloat(z.ReceivedAmount);
                    emploan.push(obj);
                });
                console.log(total);
                if (total > 0) {
                    let url = "/Loan/LoanProcessed";
                    $.ajax({
                        type: "POST",
                        url: url,
                        data: JSON.stringify(emploan),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (data) {
                            alert(data.Result == "" ? "Record successfully saved...." : Result);
                        }
                    });
                }
            });
        }
        $("#imgLoader").hide();
    });
    function ProcessValidation() {
        let value = false;
        $("#LoanProcessBody tr").each(function () {
            if ($(this).find("#chkProcess").prop('checked') === true) {
                value = true;
                return;
            }
        });
        if (value === false) {
            alert("No selection found....");
            return false;
        }
        return value;
    }

    //$("#txtempname").on('change', function () {
    //    let empDetails = $("#txtempname").val();
    //    let nameArray = empDetails.split("-");
    //    let Empno = nameArray[1];
    //    $("#txtempno").val(Empno);
    //});
    //$("#btnSearch").on('click', function () {
    //    let info = { "EmployeeNo": $("#txtempno").val() };
    //    $.ajax({
    //        type: "GET",
    //        url: "LoanList",
    //        data: info,
    //        contentType: "application/json; charset=utf-8",
    //        dataType: "json",
    //        success: function (msg) {
    //            var parsed = $.parseJSON(msg);
    //            $("#listEmployeeLoanList").empty();
    //            $.each(parsed, function (index, obj) {
    //                var row = '';
    //                row = row + '<tr>';
    //                row = row + '<td> ' + obj.LoanDate + '</td>';
    //                row = row + '<td id="IdLoan"> ' + obj.LoanNo + '</td>';
    //                row = row + '<td id="EmpName">' + obj.EmpName + '</td>';
    //                row = row + '<td>' + obj.LoanType + '</td>';
    //                row = row + '<td> ' + obj.LoanAmount + '</td>';
    //                row = row + '<td> ' + obj.TotalReceivedAmount + '</td >';
    //                row = row + '<td> ' + obj.ClosingAmount + '</td >';
    //                row = row + '<td> ' + obj.LoanStatus + '</td >';
    //                /*
    //                row = row + '<td> ' + obj.Reason + '</td >';
    //                */
    //                row = row + '<td> <input type="button" id="btnDetails" value="Details" class="mb-sm mt-xs mr-xs btn btn-sm btn-tertiary text-sm text-weight-semibold" ></td> </tr > ';
    //                row = row + '</tr>';
    //                $("#listEmployeeLoanList").append(row);
    //            });
    //        }
    //    });
    //});
    //$("#listEmployeeLoanList").on('click', "#btnDetails", function () {
    //    let IdLoan = $(this).closest("tr").find("#IdLoan").html();
    //    let EmpName = $(this).closest("tr").find("#EmpName").html();
    //    let LoanTitle = "Loan";
    //    let LoanTitleEmpName = LoanTitle.concat(" ", EmpName);
    //    let modalTitle = LoanTitleEmpName.concat(" ", IdLoan);
    //    let info = { "LoanNo": $.trim(IdLoan) };
    //    $("#hdIDLoan").val(IdLoan);
    //    $("#modalTitle").html(modalTitle);
    //    $.ajax({
    //        type: "GET",
    //        url: "LoanDetail",
    //        data: info,
    //        contentType: "application/json; charset=utf-8",
    //        dataType: "json",
    //        success: function (msg) {
    //            var parsed = $.parseJSON(msg);
    //            $("#tblModalLoanDetailsBody").empty();
    //            $("#ddlMonth").empty();
    //            let processed = "";
    //            let salaryprocessed = ""
    //            let row = '';
    //            $.each(parsed, function (index, obj) {

    //                row = '';
    //                processed = obj.Processed == true ? "YES" : "NO";
    //                salaryprocessed = obj.SalaryProcessed == true ? "YES" : "NO";
    //                row = row + '<tr>';
    //                row = row + '<td id="IDSRL">' + obj.SRL + '</td>';
    //                row = row + '<td id="IDDetail" style="display:none">' + obj.IDDetail + '</td>';
    //                row = row + '<td id="IDLoan" style = "display:none">' + obj.IDLoan + '</td>';
    //                row = row + '<td id="IDActiveRow" style = "display:none">' + obj.RowActive + '</td>';
    //                row = row + '<td>' + obj.OpeningAmount + '</td >';
    //                row = row + '<td id="IDInstallment">' + obj.InstallmentAmount + '</td>';
    //                row = row + '<td id="IDInstallmentInterest">' + obj.InterestAmount + '</td>';
    //                row = row + '<td>' + obj.ClosingAmount + '</td>';
    //                row = row + '<td>' + obj.InstallmentMonth + '</td>';
    //                row = row + '<td>' + obj.InstallmentYear + '</td>';
    //                // IN case Freeze is there 
    //                if (obj.Freeze == true || obj.RowActive == 'N') {
    //                    row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed" disabled></td>';
    //                }
    //                else if (processed == "YES" || obj.SalaryProcessed == "YES") {
    //                    row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed" checked disabled></td>';
    //                }
    //                else {
    //                    row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed"></td>';
    //                }
    //                row = row + '<td>' + obj.ProcessDate + '</td>';
    //                row = row + '<td id="IDReceivedAmount"> <input type="text" class="form-control small form-control-sm border-0 text-right" style="height:25px" id="IDRAmount" value=' + obj.ReceivedAmount + ' disabled></td>'
    //                row = row + '<td id="useraction" style="display:none" >' + salaryprocessed + '</td>';
    //                row = row + '<td id="IDDetailSalaryProcessed">' + salaryprocessed + '</td>';
    //                row = row + '</tr>';
    //                $("#tblModalLoanDetailsBody").append(row);
    //            });

    //            // ON Success 
    //            // Salary Month 
    //            url = "/Loan/SalaryMonthsList"
    //            $.get(url, info, function (res) {
    //                $("#ddlMonth").empty();
    //                res.map(function (x) {
    //                    $("#ddlMonth").append('<option value=' + x.Name + '>' + x.Name + '</option>');
    //                });
    //                // ON Success 
    //                // Salary Year
    //                url = "/Loan/SalaryYearList"
    //                $.get(url, info, function (data) {
    //                    $("#ddlYear").empty();
    //                    data.map(function (z) {
    //                        $("#ddlYear").append('<option value=' + z.Name + '>' + z.Name + '</option>');
    //                    });
    //                });
    //            });
    //        }
    //    });
    //    $('#modalEmpLoanDtls').modal('show');
    //});
    ////Loan Processed Amount Changes Checked Or Unchecked
    //$("#tblModalLoanDetailsBody").on('change', ".chbProcessed", function () {
    //    let chkValue = $(this).closest("tr").find("#chkProcessed").is(":checked")
    //    let textBox = $(this).closest("tr").find("#IDRAmount");
    //    let installment = $(this).closest("tr").find("#IDInstallment").html();
    //    //let useraction = $(this).closest("tr").find("#useraction");
    //    let installmentInterest = $(this).closest("tr").find("#IDInstallmentInterest").html();
    //    let totalAmount = parseFloat(installment) + parseFloat(installmentInterest);
    //    //useraction = useraction.html("USERACTION");
    //    $(textBox).val(0);
    //    if (chkValue == true) {
    //        $(textBox).val(totalAmount);
    //    }
    //});
    //$("#btnSave").on('click', function () {
    //    let param = [];
    //    let ActiveRow = "";
    //    let IDSRL = "";
    //    let IDDetail = "";
    //    let IDLoan = "";
    //    let RecAmount = "";
    //    $.each($("#tblModalLoanDetailsBody").find("tr"), function () {
    //        ActiveRow = $(this).closest("tr").find("#IDActiveRow").html();
    //        IDSRL = $(this).closest("tr").find("#IDSRL").html();
    //        IDDetail = $(this).closest("tr").find("#IDDetail").html();
    //        IDLoan = $(this).closest("tr").find("#IDLoan").html();
    //        RecAmount = $(this).closest("tr").find("#IDRAmount").val();
    //        if (ActiveRow === "Y") {
    //            param.push({
    //                "SRL": IDSRL,
    //                "IDDetail": IDDetail,
    //                "IDLoan": IDLoan,
    //                "ReceivedAmount": RecAmount
    //            });
    //        }
    //        ActiveRow = "";
    //        IDSRL = "";
    //        IDDetail = "";
    //        IDLoan = "";
    //        RecAmount = "";
    //    });
    //    $.ajax({
    //        type: "POST",
    //        url: "LoanProcessed",
    //        data: JSON.stringify(param),
    //        contentType: "application/json; charset=utf-8",
    //        dataType: "json",
    //        success: function (data) {
    //            alert(data.Result == "" ? "Record successfully saved...." : Result);
    //        }
    //    });
    //});
    //// Change Loan 
    //$("#btnChangeLoan").click(function () {
    //    //Valuatation
    //    let txtInsAmount = $("#txtInsAmount").val();
    //    let ddlMonth = $("#ddlMonth :selected").val();
    //    let ddlYear = $("#ddlYear :selected").val();
    //    let validRevised = "";
    //    if (txtInsAmount === '') {
    //        alert("Revised amount is missing");
    //        return false
    //    }
    //    //let url = "/Loan/RevisedValueChecking"
    //    //let para = { "LoanNo": $.trim($("#hdIDLoan").val()), "Month": ddlMonth, "Year": ddlYear };
    //    //$.get(url, para, function (data) {
    //    //    // need to discuss
    //    //});

    //    if (confirm("Are you sure to revise the loan installment....?") == true) {
    //        let info = {
    //            "LoanNo": $("#hdIDLoan").val(),
    //            "InsAmount": $("#txtInsAmount").val(),
    //            "ChangeMonth": $("#ddlMonth :selected").val(),
    //            "ChangeYear": $("#ddlYear :selected").val(),
    //        };
    //        $.ajax({
    //            type: "POST",
    //            url: "ChangeLoan",
    //            data: JSON.stringify(info),
    //            contentType: "application/json; charset=utf-8",
    //            dataType: "json",
    //            success: function (data) {
    //                console.log(data);
    //                $("#tblModalLoanDetailsBody").empty();
    //                let parsed = $.parseJSON(data);
    //                $.each(parsed, function (index, obj) {
    //                    var row = '';
    //                    let salary = obj.SalaryProcessed == true ? "YES" : "NO";
    //                    let processdate = obj.ProcessDate == null ? "" : obj.ProcessDate;
    //                    let amount = obj.ReceivedAmount == null ? 0 : obj.ReceivedAmount;
    //                    row = row + '<tr>';
    //                    row = row + '<td id="IDDetail" style="display:none">' + obj.IDDetail + '</td>';
    //                    row = row + '<td id="IDLoan" style = "display:none">' + obj.IDLoan + '</td>';
    //                    row = row + '<td>' + obj.OpeningAmount + '</td>';
    //                    row = row + '<td id="IDInstallment">' + obj.InstallmentAmount + '</td>';
    //                    row = row + '<td id="IDInstallmentInterest">' + obj.InterestAmount + '</td>';
    //                    row = row + '<td>' + obj.ClosingAmount + '</td>';
    //                    row = row + '<td id="IDDetailSalaryProcessed">' + salary + '</td>';
    //                    row = row + '<td>' + obj.InstallmentMonth + '</td>';
    //                    row = row + '<td>' + obj.InstallmentYear + '</td>';

    //                    if (obj.Processed == true) {
    //                        if (obj.SalaryProcessed == true) {
    //                            row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed" checked disable></td>';
    //                        }
    //                        else {
    //                            row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed" checked ></td>';
    //                        }
    //                    }
    //                    else {
    //                        row = row + '<td><input type="checkbox" id="chkProcessed" class="text-center chbProcessed"></td>';
    //                    }
    //                    row = row + '<td>' + processdate + '</td>';
    //                    row = row + '<td id="IDReceivedAmount"> <input type="text" class="form-control small form-control-sm border-0 text-right" style="height:25px" id="IDRAmount" value=' + amount + ' disabled></td>'
    //                    row = row + '<td id="useraction" style="display:none" >' + salary + '</td>';
    //                    row = row + '</tr>';
    //                    $("#tblModalLoanDetailsBody").append(row);

    //                });
    //            }
    //        });
    //    }
    //});
});