$(function () {

    // ---------------- TOASTR CONFIG ----------------
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: "3000",
        extendedTimeOut: "1000",
        showMethod: "fadeIn",
        hideMethod: "fadeOut"
    };

    // ---------- INITIAL LOAD ----------
    defaultLoad();

    function defaultLoad() {

        // Load Employee List
        $.get("/Loan/GetEmpDetails", function (data) {
            $("#ddlEmployee").empty();
            data.map(function (z) {
                $("#ddlEmployee").append('<option value="' + z.id + '">' + z.value + '</option>');
            });

            $('#ddlEmployee').multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                enableCaseInsensitiveFiltering: true,
                filterPlaceholder: 'Search here...',
                nonSelectedText: 'Select Employees',
                buttonWidth: '100%',
                buttonClass: 'btn btn-default btn-sm dropdown-toggle',
                maxHeight: 300,
                selectAllText: 'Select All',
                allSelectedText: 'All Selected',
                nSelectedText: 'Selected',
                numberDisplayed: 2,
                templates: {
                    ul: '<ul class="multiselect-container dropdown-menu" style="min-width:250px; max-height:300px; overflow:auto;"></ul>'
                }
            });
        });

        // Year dropdown
        $.get("/Loan/SalaryYearsList", function (data) {
            $("#ddlYear").empty();
            data.map(function (z) {
                $("#ddlYear").append('<option value="' + z.Name + '"' + (z.Selected ? ' selected' : '') + '>' + z.Name + '</option>');
            });
        });

        // Month dropdown
        $.get("/Loan/SalaryMonthList", function (data) {
            $("#ddlMonth").empty();
            data.map(function (z) {
                $("#ddlMonth").append('<option value="' + z.Name + '"' + (z.Selected ? ' selected' : '') + '>' + z.Name + '</option>');
            });
        });
    }

    // ---------- SHOW DATA ----------
    $("#btnShow").on('click', function () {

        if (!Validation()) return;

        let empList = $("#ddlEmployee").val() || [];
        if (empList.length === 0) {
            toastr.warning("Please select at least one employee", "Validation");
            return;
        }

        $("#LoanProcessBody").empty();
        let allRows = "";

        let joinedIds = empList.join(',');

        $.get("/Loan/BulkLoanProcessData", {
            EmpIds: joinedIds,
            Month: $("#ddlMonth").val(),
            Year: $("#ddlYear").val()
        }, function (data) {

            let newdata = JSON.parse(data);

            if (!newdata.length) {
                $("#LoanProcessBody").html('<tr><td colspan="10" class="text-center text-muted">No records found</td></tr>');
                return;
            }

            newdata.forEach(function (obj) {

                allRows += `
                    <tr>
                        <td><input type="checkbox" class="chkProcess" value="${obj.IDLoan}"></td>
                        <td>${obj.EmployeeCode}</td>
                        <td>${obj.EmployeeName}</td>
                        <td>${obj.LoanDate}</td>
                        <td>${obj.LoanNo}</td>
                        <td>${obj.InstallmentMonthName}</td>
                        <td>${obj.InstallmentYear}</td>
                        <td class="text-centre">${parseFloat(obj.PrincipalComponent).toFixed(2)}</td>
                        <td class="text-centre">${parseFloat(obj.InterestComponent).toFixed(2)}</td>
                        <td class="text-centre text-primary"><strong>${parseFloat(obj.AmountToBePaid).toFixed(2)}</strong></td>

                        <input type="hidden" class="hdIDLoan" value="${obj.IDLoan}">
                        <input type="hidden" class="hdEmployeeId" value="${obj.IDEmployee}">
                        <input type="hidden" class="hdMonth" value="${obj.InstallmentMonth}">
                        <input type="hidden" class="hdYear" value="${obj.InstallmentYear}">
                        <input type="hidden" class="hdBalanceBefore" value="${obj.BalanceBefore}">
                        <input type="hidden" class="hdBalanceAfter" value="${obj.BalanceAfter}">
                        <input type="hidden" class="hdPrincipal" value="${obj.PrincipalComponent}">
                        <input type="hidden" class="hdInterest" value="${obj.InterestComponent}">
                        <input type="hidden" class="hdWaiverAmount" value="${obj.WaiverAmount}">
                        <input type="hidden" class="hdWaiverType" value="${obj.WaiverType}">
                    </tr>`;
            });

            $("#LoanProcessBody").html(allRows);
        });
    });

    // ---------- CHECK ALL ----------
    $("#chkAll").on('change', function () {
        $(".chkProcess").prop('checked', this.checked);
    });

    // ---------- PROCESS SELECTED ----------
    $("#btnProcess").on("click", function () {

        if (!ProcessValidation()) return;

        let records = [];

        $("#LoanProcessBody tr").each(function () {

            const isChecked = $(this).find(".chkProcess").prop("checked");
            if (!isChecked) return;

            const loanId = $(this).find(".hdIDLoan").val();
            const empId = $(this).find(".hdEmployeeId").val();
            const month = $(this).find(".hdMonth").val();
            const year = $(this).find(".hdYear").val();
            const loanStatus = ($(this).find(".hdLoanStatus").val() || "").toUpperCase();

            if (!loanId || !empId) return;

            if (loanStatus === "CLOSED") {
                toastr.error("Loan already closed for " + $(this).find("td:eq(2)").text().trim(), "Processing Error");
                return false;
            }

            const duplicate = records.some(r =>
                r.IDLoan === loanId && r.InstallmentMonth === month && r.InstallmentYear === year
            );

            if (duplicate) {
                toastr.warning("Duplicate month detected for " + $(this).find("td:eq(2)").text().trim(), "Validation");
                return false;
            }

            records.push({
                IDLoan: loanId,
                IDEmployee: empId,
                InstallmentMonth: month,
                InstallmentYear: year,
                BalanceBefore: parseFloat($(this).find(".hdBalanceBefore").val()) || 0,
                PrincipalComponent: parseFloat($(this).find(".hdPrincipal").val()) || 0,
                InterestComponent: parseFloat($(this).find(".hdInterest").val()) || 0,
                AmountPaid: parseFloat($(this).find("td:eq(9)").text().trim()) || 0,
                InterestPaid: parseFloat($(this).find(".hdInterest").val()) || 0,
                WaiverAmount: $(this).find(".hdWaiverAmount").val() || 0,
                WaiverType: $(this).find(".hdWaiverType").val() || "",
                BalanceAfter: parseFloat($(this).find(".hdBalanceAfter").val()) || 0,
                EmployeeNo: $(this).find("td:eq(1)").text().trim(),
                EmployeeName: $(this).find("td:eq(2)").text().trim()
            });
        });

        if (records.length === 0) {
            toastr.warning("No employees selected for processing", "Validation");
            return;
        }

        const payload = {
            JsonData: JSON.stringify(records),
            Month: $("#ddlMonth").val(),
            Year: $("#ddlYear").val(),
            UserName: $("#hdnUserName").val() || "SystemUser"
        };

        $.ajax({
            type: "POST",
            url: "/Loan/ProcessBulkLoans",
            data: JSON.stringify(payload),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            beforeSend: function () {
                $("#btnProcess").prop("disabled", true)
                    .html('<i class="fas fa-spinner fa-spin"></i> Processing...');
            },
            success: function (res) {

                $("#btnProcess").prop("disabled", false).text("Process Selected Loans");

                if (res.Code === 1) {
                    toastr.success(res.Message, "Success");
                    setTimeout(function () {
                        location.reload();
                    }, 1500);
                } else {
                    toastr.error(res.Message, "Error");
                }
            },
            error: function (xhr) {
                $("#btnProcess").prop("disabled", false).text("Process Selected Loans");
                toastr.error("Communication Error: " + xhr.statusText, "Failed");
            }
        });
    });

    // ---------- VALIDATIONS ----------
    function Validation() {

        if (!$("#ddlYear").val()) {
            toastr.warning("Please select Year", "Validation");
            $("#ddlYear").focus();
            return false;
        }

        if (!$("#ddlMonth").val()) {
            toastr.warning("Please select Month", "Validation");
            $("#ddlMonth").focus();
            return false;
        }

        return true;
    }

    function ProcessValidation() {

        if (!$("#ddlMonth").val()) {
            toastr.warning("Please select a Month", "Validation");
            return false;
        }

        if (!$("#ddlYear").val()) {
            toastr.warning("Please select a Year", "Validation");
            return false;
        }

        const hasChecked = $("#LoanProcessBody .chkProcess:checked").length > 0;
        if (!hasChecked) {
            toastr.error("Select at least one employee to process", "Validation");
            return false;
        }

        return true;
    }

});
