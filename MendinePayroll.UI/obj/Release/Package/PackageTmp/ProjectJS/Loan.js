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

    $("#txtempname").on('change',function () {
        let empDetails = $("#txtempname").val();
        let nameArray = empDetails.split("-");
        let Empno = nameArray[1];
        $("#txtempno").val(Empno);
    });

    $("#txtloandate").datepicker();
    $("#txtenddate").datepicker();
    $("#txtrefdate").datepicker();

    $("#txttype").change(function () {
        let type = $("#txttype").val();
        $.get("/Loan/AutoLoanNo", { Type: type }, function (data) {
            let LoanNo = data;
            $("#txtloanno").val(LoanNo);
        });
    });

    // Calculate Tenure
    $("#txtmonthlyinstallment").change(function () {
        let txtloanamount = $("#txtloanamount").val();
        let txtmonthlyinstallment = $("#txtmonthlyinstallment").val();
        let tenure = Math.ceil(txtloanamount / txtmonthlyinstallment);
        $("#txttenure").val(tenure);
        console.log(txtmonthlyinstallment);
        $("txttotalmonthlyloan").val(txtmonthlyinstallment);
    });
    
    // Calculate End Date
    function addMonths(date, month) {
        var d = date.getDate();
        date.setMonth(date.getMonth() + +month);
        if (date.getDate() != d) {
            date.setDate(0);
        }
        return date;
    }

    // Convert date string into date
    function convertDate(str) {
        var date = new Date(str),
            month = ("0" + (date.getMonth() + 1)).slice(-2),
            day = ("0" + date.getDate()).slice(-2);
        return [month,day,date.getFullYear()].join("/");
    }

    // Add months To End Date Based on Tenure
    $("#txtloandate,#txttenure").change(function () {
        let tenure = $("#txttenure").val();
        let loandate = $("#txtloandate").val();
        let loanenddate = addMonths(new Date(loandate), tenure).toString();
        $("#txtenddate").val(convertDate(loanenddate));
    });

    // Monthly Loan == Total Loan First without Interest
    $("#txtmonthlyinstallment").change(function () {
        //$("#txttotalloan").val($("#txtmonthlyinstallment").val());
        $("#txttotalloan").val($("#txtloanamount").val());
        
    });
    // Interest Pcn
    $("#txtinterestpcn").change(function () {
        let interestpcn = $("#txtinterestpcn").val() == '' ? "0" : parseFloat($("#txtinterestpcn").val());
        let loanamount = $("#txtloanamount").val() == '' ? "0" : parseFloat($("#txtloanamount").val());
        let installmentamount = $("#txtmonthlyinstallment").val() == '' ? "0" : parseFloat($("#txtmonthlyinstallment").val());
        let tenure = $("#txttenure").val() == '' ? "0" : parseFloat($("#txttenure").val());
        let monthlyinterest = ((installmentamount * interestpcn) / 100);
        let loaninterest = (monthlyinterest * tenure);
        $("#txtinterestmonthly").val(parseFloat(monthlyinterest).toFixed(2));
        $("#txtinterest").val(parseFloat(loaninterest).toFixed(2));
        let totalMonthlyLoan = parseFloat((installmentamount + monthlyinterest).toFixed(2));
        let totalLoan = parseFloat((loanamount + loaninterest).toFixed(2));
        $("#txttotalmonthlyloan").val(totalMonthlyLoan);
        $("#txttotalloan").val(totalLoan);
    });
    
    

    // Save Button
    $("#btnSave").click(function () {
        if (Validation() == true) {
            let info = {
                "IDLoan": 0,
                "LoanType": $("#txttype").val(),
                "LoanNo": $("#txtloanno").val(),
                "EmpNo": $("#txtempno").val(),
                "LoanDate": $("#txtloandate").val(),
                "EndDate": $("#txtenddate").val(),
                "RefNo": $("#txtrefno").val(),
                "RefDate": $("#txtrefdate").val(),
                "LoanAmount": $("#txtloanamount").val(),
                "LoanInterestPcn": $("#txtinterestpcn",).val(),
                "LoanTenure": $("#txttenure").val(),
                "MonthlyInstallmentAmount": $("#txtmonthlyinstallment").val(),
                "MonthlyInterestAmount": $("#txtinterestmonthly").val(),
                "MonthlyLoan": $("#txttotalmonthlyloan").val(),
                "TotalInterestAmount": $("#txtinterest").val(),
                "TotalLoanAmount": $("#txttotalloan").val()
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
                        alert("Record successfully saved....");
                        window.location.href = "Index";
                    }
                    else {
                        alert(res.result);
                    }
                },
            });
        }
    });

    function Validation() {
        let txttype = $("#txttype");
        let txtempname = $("#txtempname");
        let txtloanamount = $("#txtloanamount");
        let txtmonthlyinstallment = $("#txtmonthlyinstallment");
        let txttenure = $("#txttenure");
        let txtloandate = $("#txtloandate");
        let txtenddate = $("#txtenddate");


        if (txttype.val() == 'Select Type') {

            alert("Please Select Type");
            return false;
        }
        if (txtempname.val() == '') {

            alert("Please Select Employee Name");
            return false;
        }
        if (txtloanamount.val() == '') {

            alert("Please Select Loan Amount");
            return false;
        }
        if (txtmonthlyinstallment.val() == '') {

            alert("Please Select Monthly Installment");
            return false;
        }
        if (txttenure.val() == '') {

            alert("Please Select Tenure");
            return false;
        }

        if (txtloandate.val() == '') {

            alert("Please Select loan date");
            return false;
        }

        if (txtenddate.val() == '') {

            alert("Please Select end date");
            return false;
        }
        //let ins = txtmonthlyinstallment.val();
        //let tenure = txttenure.val();
        //let amount = (ins * tenure);
        //let loanamount = txtloanamount.val();
        //if (amount != loanamount) {
        //    alert("Loan amount is mismatch....");
        //    return false;
        //}

        return true;
    }

});