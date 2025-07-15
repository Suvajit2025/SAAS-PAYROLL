$(function () {
    defaultLoad();
    function defaultLoad() {
        // Employee Auto Complete
        let url = "/Loan/AutoCompleteEmpDeatils";
        $.get(url, function (data) {
            $("#txtEmployee").autocomplete({
                source: data
            });
        });
        //Month List 
        url = "/Modification/SalaryMonthsList";
        $.get(url, function (data) {
            data.map(function (x) {
                $("#ddlMonth").append('<option value=' + x.Name + '>' + x.Name+ '</option>');
            });
        });
        // Year List 
        url = "/Modification/SalaryYearList";
        $.get(url, function (data) {
            data.map(function (x) {
                $("#ddlYear").append('<option value=' + x.Name + '>' + x.Name + '</option>');
            });
        });

    }
    $("#txtEmployee").on('change', function () {
        let empDetails = $("#txtEmployee").val();
        let nameArray = empDetails.split("-");
        let Empno = nameArray[1];
        $("#txtEmpno").val(Empno);
    });


      // Show Button
    $("#btnShow").click(function () {
        if (ShowValidation() == true) {
            let info = {
                "Employeeno": $("#txtEmpno").val(),
                "MonthName": $("#ddlMonth :selected").val(),
                "Year": $("#ddlYear :selected").val()
            };

            let url = "/Modification/SalaryDetailList";
            $.ajax({
                url: url,
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(info),
                success: function (res) {
                    $("#txtESIOld").val('');
                    $("#txtESINew").val('');
                    if (res.length > 0) {
                        $("#txtESIOld").val(res[0].DESI);
                        $("#hdIDSalary").val(res[0].RowID);
                    }
                    else {
                        alert("No Data found....");
                    }
                },
            });
        }
    });
    // Show Validation 
    function ShowValidation() {
        let txtEmployee = $("#txtEmployee");
        let Month = $("#ddlMonth :selected").val();
        let Year = $("#ddlYear :selected").val();

        if (txtEmployee.val() == '') {
            alert("Employee name is missing....");
            txtEmployee.focus();
            return false;
        }
        if (Month == '') {
            alert("Month is missing....");
            $("#ddlMonth").focus();
            return false;
        }
        if (Year  == '') {
            alert("Year is missing....");
            $("#ddlYear").focus();
            return false;
        }
        return true;
    }

    // Save Button
    $("#btnSave").click(function () {
        if (SaveValidation() == true) {
            let info = {
                "RowID": $("#hdIDSalary").val(),
                "EmpNo": $("#txtEmpno").val(),
                "DESI": $("#txtESINew").val()
            };

            let url = "/Modification/SalaryESIModification";
            $.ajax({
                url: url,
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(info),
                success: function (res) {

                    if (res.success == "") {
                        alert("Record successfully saved....");
                        window.location.href = "Salary";
                    }
                    else {
                        alert(res.result);
                    }
                },
            });
        }
    });

    // Save Validation
    function SaveValidation() {
        let txtEmployee = $("#txtEmployee");
        let Month = $("#ddlMonth :selected").val();
        let Year = $("#ddlYear :selected").val();
        let txtESIOld = $("#txtESIOld");
        let txtESINew = $("#txtESINew");


        if (txtEmployee.val() == '') {
            alert("Employee name is missing....");
            txtEmployee.focus();
            return false;
        }
        if (Month == '') {
            alert("Month is missing....");
            $("#ddlMonth").focus();
            return false;
        }
        if (Year == '') {
            alert("Year is missing....");
            $("#ddlYear").focus();
            return false;
        }
        if (txtESIOld.val() == '' && txtESINew.val() =='' ) {
            alert("Both OLd and New ESI value can not be 0....");
            txtESINew.focus();
            return false;
        }
        return true;
    }

});



//$("#txtempname").on('change',function () {
    //    let empDetails = $("#txtempname").val();
    //    let nameArray = empDetails.split("-");
    //    let Empno = nameArray[1];
    //    $("#txtempno").val(Empno);
    //});

    //$("#txtloandate").datepicker();
    //$("#txtenddate").datepicker();
    //$("#txtrefdate").datepicker();

    //$("#txttype").change(function () {
    //    let type = $("#txttype").val();
    //    $.get("/Loan/AutoLoanNo", { Type: type }, function (data) {
    //        let LoanNo = data;
    //        $("#txtloanno").val(LoanNo);
    //    });
    //});

    //// Calculate Tenure
    //$("#txtmonthlyinstallment").change(function () {
    //    let txtloanamount = $("#txtloanamount").val();
    //    let txtmonthlyinstallment = $("#txtmonthlyinstallment").val();
    //    let tenure = Math.ceil(txtloanamount / txtmonthlyinstallment);
    //    $("#txttenure").val(tenure);
    //    $("txttotalmonthlyloan").val(txtmonthlyinstallment);
    //});

    //// Calculate End Date
    //function addMonths(date, month) {
    //    var d = date.getDate();
    //    date.setMonth(date.getMonth() + +month);
    //    if (date.getDate() != d) {
    //        date.setDate(0);
    //    }
    //    return date;
    //}

    //// Convert date string into date
    //function convertDate(str) {
    //    var date = new Date(str),
    //        month = ("0" + (date.getMonth() + 1)).slice(-2),
    //        day = ("0" + date.getDate()).slice(-2);
    //    return [month,day,date.getFullYear()].join("/");
    //}

    //// Add months To End Date Based on Tenure
    //$("#txtloandate,#txttenure").change(function () {
    //    let tenure = $("#txttenure").val();
    //    let loandate = $("#txtloandate").val();
    //    let loanenddate = addMonths(new Date(loandate), tenure).toString();
    //    $("#txtenddate").val(convertDate(loanenddate));
    //});

    //// Monthly Loan == Total Loan First without Interest
    //$("#txtmonthlyinstallment").change(function () {
    //    //$("#txttotalloan").val($("#txtmonthlyinstallment").val());
    //    $("#txttotalloan").val($("#txtloanamount").val());

    //});
    //// Interest Pcn
    //$("#txtinterestpcn").change(function () {
    //    let interestpcn = $("#txtinterestpcn").val() == '' ? "0" : parseFloat($("#txtinterestpcn").val());
    //    let loanamount = $("#txtloanamount").val() == '' ? "0" : parseFloat($("#txtloanamount").val());
    //    let installmentamount = $("#txtmonthlyinstallment").val() == '' ? "0" : parseFloat($("#txtmonthlyinstallment").val());
    //    let tenure = $("#txttenure").val() == '' ? "0" : parseFloat($("#txttenure").val());
    //    let monthlyinterest = ((installmentamount * interestpcn) / 100);
    //    let loaninterest = (monthlyinterest * tenure);
    //    $("#txtinterestmonthly").val(parseFloat(monthlyinterest).toFixed(2));
    //    $("#txtinterest").val(parseFloat(loaninterest).toFixed(2));
    //    let totalMonthlyLoan = parseFloat((installmentamount + monthlyinterest).toFixed(2));
    //    let totalLoan = parseFloat((loanamount + loaninterest).toFixed(2));
    //    $("#txttotalmonthlyloan").val(totalMonthlyLoan);
    //    $("#txttotalloan").val(totalLoan);
    //});
