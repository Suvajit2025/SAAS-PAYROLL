/**
 * Payroll Processing Module
 */

// Global State
let selectedEmployees = [];
let attendanceData = {};
let empDtInstance = null;
let dtInstance = null;
let editingEmpId = null;
let pivotData = null; // Stores the server response for recalc

$(document).ready(function () {
    // Initialize defaults if needed
});

/* --- VIEW NAVIGATION --- */
function switchMainView(view) {
    $('.view-toggle-btn').removeClass('active');
    if (view === 'group') {
        $('#btnViewGroup').addClass('active');
        $('#view_group_wise').removeClass('hidden');
        $('#view_employee_wise').addClass('hidden');
        $('#filterContainer').addClass('hidden');
        if (empDtInstance) { empDtInstance.destroy(); empDtInstance = null; }
    } else {
        $('#btnViewEmp').addClass('active');
        $('#view_group_wise').addClass('hidden');
        $('#view_employee_wise').removeClass('hidden');
        $('#filterContainer').removeClass('hidden');
        // loadEmployeeGrid(); // Uncomment if using local employeesDB
    }
}

/* --- GENERATION MODAL HANDLERS --- */
function openGenModal() {
    $("#genRefNo").val('');
    $("#txtComments").val('');
    document.getElementById('genRefDate').valueAsDate = new Date();
    setCurrentMonthYear();
    setLastMonthRange();
    $("#genModal").fadeIn(200);
    loadCompanyList();
    handleTypeChange();
}

function handleTypeChange() {
    const type = $("#ddlType").val();
    if (type === '1') {
        $("#pnlMonthly").removeClass('hidden');
        $("#pnlRange").addClass('hidden');
    } else {
        $("#pnlMonthly").addClass('hidden');
        $("#pnlRange").removeClass('hidden');
    }
    loadPayGroups(type);
}

function setCurrentMonthYear() {
    const now = new Date();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const year = now.getFullYear();
    document.getElementById('txtMonthYear').value = `${year}-${month}`;
}

function setLastMonthRange() {
    const today = new Date();
    const toDate = new Date(today);
    const fromDate = new Date(today);
    fromDate.setMonth(fromDate.getMonth() - 1);
    document.getElementById('txtFromDate').valueAsDate = fromDate;
    document.getElementById('txtToDate').valueAsDate = toDate;
}

/* --- SERVER AJAX CALLS --- */
function loadCompanyList() {
    const ddl = $("#ddlCompany").empty();
    $.getJSON('/bulkSalaryProcess/GetCompanies', function (companies) {
        ddl.append(`<option value="">-- Select Company --</option>`);
        companies.forEach(c => {
            ddl.append(`<option value="${c.value}" data-code="${c.code}">${c.text}</option>`);
        });
    });
}

function loadPayGroups(type) {
    const ddl = $("#ddlGroup").empty();
    $.getJSON('/bulkSalaryProcess/GetPayGroups', { payrollType: type }, function (groups) {
        ddl.append(`<option value="">-- Select Group --</option>`);
        groups.forEach(g => {
            ddl.append(`<option value="${g.value}">${g.text}</option>`);
        });
    });
}

function tryGenerateRefNo() {
    const companyCode = $("#ddlCompany option:selected").data("code");
    const groupId = $("#ddlGroup").val();
    const type = $("#ddlType").val();
    if (!companyCode || !groupId || !type) return;

    let payload = {
        companyCode: companyCode,
        payGroupId: parseInt(groupId),
        payrollType: type
    };

    if (type === '1') {
        const ym = $("#txtMonthYear").val();
        if (!ym) return;
        const [year, month] = ym.split('-');
        payload.processMonth = parseInt(month);
        payload.processYear = parseInt(year);
    } else {
        payload.fromDate = $("#txtFromDate").val();
        payload.toDate = $("#txtToDate").val();
    }

    $.getJSON('/bulkSalaryProcess/GenerateRefNo', payload).done(function (res) {
        $("#genRefNo").val(res.refNo);
    });
}

/* --- ATTENDANCE GRID --- */
function openAttendanceModal() {
    const payGroupId = $("#ddlGroup").val();
    const payrollType = $("#ddlType").val();

    selectedEmployees = $(".chk-emp:checked").map(function () { return $(this).val(); }).get();
    if (selectedEmployees.length === 0) {
        toastr.error("Please select at least one employee");
        return;
    }

    $.getJSON('/bulkSalaryProcess/GetAttendanceComponents', { payGroupId, payrollType }, function (components) {
        closeModal('genModal');
        $("#attModal").fadeIn(200);
        // Logic to render component table and load employees
        loadAttendanceEmployees(payGroupId, payrollType);
    });
}

function renderAttendanceGrid(res) {
    const columns = res.columns || [];
    const rows = res.data || [];
    const head = $("#attTableHead").empty();
    const body = $("#attTableBody").empty();

    let header = "<tr>";
    columns.forEach(col => { header += `<th>${col}</th>`; });
    header += "</tr>";
    head.html(header);

    rows.forEach(r => {
        let tr = `<tr data-employeeid="${r.EmployeeId}">`;
        columns.forEach(col => {
            if (col === "PaidUnits" || col === "PaidDays") {
                tr += `<td class="text-right"><input type="number" class="grid-input paid-days" value="${r[col] ?? 0}"></td>`;
            } else {
                tr += `<td>${r[col] ?? ""}</td>`;
            }
        });
        tr += "</tr>";
        body.append(tr);
    });
}

/* --- FINAL CALCULATION & PIVOT RENDER --- */
function processFinalPayroll() {
    const payrollType = $("#ddlType").val();
    let employees = [];

    $("#attTableBody tr").each(function () {
        const employeeId = parseInt($(this).data("employeeid"), 10);
        if (!employeeId) return;
        const paidDays = parseInt($(this).find("input.paid-days").val(), 10) || 0;
        employees.push({ EmployeeId: employeeId, PaidDays: paidDays });
    });

    let payload = { PayrollType: payrollType, Employees: employees };
    if (payrollType === '1') {
        const [year, month] = $("#txtMonthYear").val().split("-");
        payload.ProcessMonth = parseInt(month);
        payload.ProcessYear = parseInt(year);
    }

    $.ajax({
        url: '/bulkSalaryProcess/CalculateSalary',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            pivotData = res;
            closeModal('attModal');
            openDetailModal(false, res);
            toastr.success("Calculated successfully");
        }
    });
}

function renderSalaryGrid(pivot) {
    if (!pivot || !pivot.Rows) return;
    const rows = pivot.Rows;
    const columns = pivot.Columns;

    let lastDeductionIndex = -1;
    columns.forEach((col, idx) => {
        if ((col.PayType || '').toUpperCase() === 'DEDUCTION') lastDeductionIndex = idx;
    });

    let html = `<thead><tr><th class="sticky-col left">Employee</th><th class="sticky-col-2 text-center">Paid Days</th>`;
    columns.forEach((c, idx) => {
        const type = (c.PayType || '').toUpperCase();
        const cls = type === 'MANUAL' ? 'header-manual' : (type === 'DEDUCTION' ? 'header-deduction' : '');
        html += `<th class="text-right ${cls}">${c.PayConfigName}</th>`;
        if (idx === lastDeductionIndex) html += `<th class="text-right header-net">Net Salary</th>`;
    });
    html += `<th class="text-right header-ctc">Monthly CTC</th></tr></thead><tbody>`;

    rows.forEach(row => {
        html += `<tr><td class="sticky-col left">Emp ${row.EmployeeId}</td><td class="sticky-col-2 text-center">${row.PaidDays}</td>`;
        columns.forEach((col, idx) => {
            const cell = row.Cells.find(x => x.PayConfigId === col.PayConfigId);
            const amt = cell ? cell.Amount : 0;
            const type = (col.PayType || '').toUpperCase();
            const cellClass = type === 'MANUAL' ? 'cell-manual' : (type === 'DEDUCTION' ? 'cell-deduction' : '');

            if (col.IsOther && col.OtherType === "LOANRECOVERY") {
                html += `<td class="text-right"><span style="font-weight:600;color:#991b1b">${amt.toLocaleString()}</span><br><label style="font-size:10px"><input type="checkbox" ${cell.IsWaived ? 'checked' : ''} onchange="toggleLoanWaiver(${row.EmployeeId}, ${col.PayConfigId})"> Waive</label></td>`;
            } else {
                html += `<td class="text-right ${cellClass}">${amt.toLocaleString()}</td>`;
            }
            if (idx === lastDeductionIndex) html += `<td class="text-right cell-net">₹ ${row.NetPay.toLocaleString()}</td>`;
        });
        html += `<td class="text-right" style="background:#f8fafc">₹ ${(row.MonthlyCTC || 0).toLocaleString()}</td></tr>`;
    });

    $("#salaryTable").html(html);
    updateStatsFromPivot(rows);
}

/* --- RECALCULATION LOGIC --- */
function toggleLoanWaiver(empId, payConfigId) {
    const row = pivotData.Rows.find(r => r.EmployeeId === empId);
    if (!row) return;
    const cell = row.Cells.find(c => c.PayConfigId === payConfigId);
    if (!cell) return;

    if (cell.OriginalAmount == null) cell.OriginalAmount = cell.Amount;
    cell.IsWaived = !cell.IsWaived;
    cell.Amount = cell.IsWaived ? 0 : cell.OriginalAmount;

    recalcRowNet(row);
    renderSalaryGrid(pivotData);
}

function recalcRowNet(row) {
    let totalDeductions = 0;
    let waivedLoan = 0;
    row.Cells.forEach(c => {
        const col = pivotData.Columns.find(x => x.PayConfigId === c.PayConfigId);
        if (!col) return;
        if ((col.PayType || '').toUpperCase() === "DEDUCTION") totalDeductions += c.Amount;
        if (col.IsOther && col.OtherType === "LOANRECOVERY" && c.IsWaived) waivedLoan += c.OriginalAmount;
    });
    row.TotalDeductions = totalDeductions;
    row.NetPay = (row.TotalEarnings - totalDeductions) + waivedLoan;
}

function updateStatsFromPivot(rows) {
    let tEarn = 0, tDed = 0, tNet = 0;
    rows.forEach(r => {
        tEarn += (r.TotalEarnings || 0);
        tDed += (r.TotalDeductions || 0);
        tNet += (r.NetPay || 0);
    });
    $("#lblEarn").text("₹ " + tEarn.toLocaleString());
    $("#lblDed").text("₹ " + tDed.toLocaleString());
    $("#lblNet").text("₹ " + tNet.toLocaleString());
}

/* --- UI HELPERS --- */
function toggleAccordion(id) {
    $("#" + id).toggleClass("hidden");
    $("#icon-" + id).toggleClass("acc-open");
}

function closeModal(id) {
    $(`#${id}`).fadeOut(200);
    if (id === 'detailModal' && dtInstance) { dtInstance.destroy(); dtInstance = null; }
}

function openDetailModal(isViewMode, serverResult) {
    $("#detailModal").fadeIn(200);
    $("#lblRef").text($("#genRefNo").val() || 'VIEW');
    renderSalaryGrid(serverResult);
}