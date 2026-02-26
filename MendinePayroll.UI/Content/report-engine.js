/* ========================================================================
   ReportEngine.js - Global Engine for ieHRMS Reports
   Filters + Client Pagination + Search + Export
   ======================================================================== */

var ReportEngine = {

    /* ================= STATE ================= */
    currentData: [],
    currentHeaders: [],
    currentUrl: '',

    currentPage: 1,
    pageSize: 10,

    reportTitle: '',
    reportSubTitle: '',

    /* ================= INIT ================= */
    init: function (dataUrl, options, reportInfo) {
        const self = this;
        self.currentUrl = dataUrl;

        if (reportInfo) {
            self.reportTitle = reportInfo.title || '';
            self.reportSubTitle = reportInfo.subTitle || '';
        }

        const settings = $.extend({
            showExcel: true,
            showPDF: true,
            showCSV: true,
            showPrint: true
        }, options);

        $('#btnExpExcel').toggle(settings.showExcel);
        $('#btnExpPDF').toggle(settings.showPDF);
        $('#btnExpCSV').toggle(settings.showCSV);
        $('#btnExpPrint').toggle(settings.showPrint);

        $('.multi-select').multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            enableCaseInsensitiveFiltering: true,
            filterPlaceholder: 'Search here...',
            nonSelectedText: 'Select Options',
            buttonWidth: '100%',
            buttonClass: 'btn btn-default btn-sm dropdown-toggle',
            maxHeight: 300
        });

        self.populateYearMonth();
        self.loadStaticLookups();

        $('#ddlCompany, #ddlDepartment, #ddlCategory, #ddlstatus')
            .on('change', () => self.loadEmployees());

        $('.filter-trigger').on('change', () => self.loadData(self.currentUrl));

        $('#ddlPageSize').on('change', function () {
            self.pageSize = parseInt(this.value);
            self.currentPage = 1;
            self.renderTable();
        });

        $('#tableSearch').on('keyup', function () {
            self.currentPage = 1;
            self.renderTable();
        });

        $('#btnPrev').on('click', () => self.prevPage());
        $('#btnNext').on('click', () => self.nextPage());

        $('#btnClearFilters').on('click', function () {
            $('.multi-select').val(null);
            $('#ddlstatus').val('ACTIVE');

            const now = new Date();
            $('#ddlYear').val(now.getFullYear());
            $('#ddlMonth').val(now.getMonth() + 1);

            $('.multi-select').multiselect('rebuild');
            self.loadData(self.currentUrl);
        });

        self.loadData(self.currentUrl);
    },

    /* ================= LOOKUPS ================= */

    populateYearMonth: function () {
        const y = $('#ddlYear');
        if (y.children().length) return;

        const year = new Date().getFullYear();
        for (let i = 0; i < 5; i++)
            y.append(new Option(year - i, year - i));

        const m = $('#ddlMonth');
        const months = ["January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"];
        months.forEach((x, i) => m.append(new Option(x, i + 1)));
        m.val(new Date().getMonth() + 1);
    },

    loadStaticLookups: function () {
        const self = this;

        $.get('/PayrollReports/GetCompanyList', res => {
            $('#ddlCompany').empty();
            res.forEach(x => $('#ddlCompany').append(new Option(x.CompanyName, x.CompanyId)));
            $('#ddlCompany').multiselect('rebuild');
        });

        $.get('/PayrollReports/GetDepartmentList', res => {
            $('#ddlDepartment').empty();
            res.forEach(x => $('#ddlDepartment').append(new Option(x.DepartmentName, x.DepartmentID)));
            $('#ddlDepartment').multiselect('rebuild');
        });

        $.get('/PayrollReports/GetCategoryList', res => {
            $('#ddlCategory').empty();
            res.forEach(x => $('#ddlCategory').append(new Option(x.CategoryName, x.CategoryID)));
            $('#ddlCategory').multiselect('rebuild');
        });

        $.get('/PayrollReports/GetPayGroupList', res => {
            $('#ddlpaygroup').empty();
            res.forEach(x => $('#ddlpaygroup').append(new Option(x.PayGroupName, x.PayGroupID)));
            $('#ddlpaygroup').multiselect('rebuild');
            self.loadEmployees();
        });
    },

    loadEmployees: function () {
        $.get('/PayrollReports/GetEmployeeList', {
            companyIds: ($('#ddlCompany').val() || []).join(','),
            deptIds: ($('#ddlDepartment').val() || []).join(','),
            categoryIds: ($('#ddlCategory').val() || []).join(','),
            status: $('#ddlstatus').val()
        }, res => {
            const ddl = $('#ddlEmployee').empty();
            res.forEach(x => ddl.append(new Option(x.EmployeeName, x.EmployeeID)));
            ddl.multiselect('rebuild');
        });
    },

    /* ================= DATA ================= */

    loadData: function (url) {
        const self = this;

        $('#gridBody').html(
            `<tr><td colspan="100" class="text-center" style="padding:40px;">
             <i class="fa fa-spinner fa-spin"></i> Generating Report...</td></tr>`
        );

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                year: $('#ddlYear').val(),
                month: $('#ddlMonth').val(),
                companyIds: $('#ddlCompany').val() || [],
                deptIds: $('#ddlDepartment').val() || [],
                categoryIds: $('#ddlCategory').val() || [],
                paygroupIds: $('#ddlpaygroup').val() || [],
                empIds: $('#ddlEmployee').val() || [],
                status: $('#ddlstatus').val()
            }),
            success: function (res) {
                if (res.success && res.Data) {
                    self.currentData = res.Data;
                    self.currentHeaders = res.Headers;
                    self.currentPage = 1;
                    self.renderTable();
                } else {
                    $('#gridBody').html('<tr><td colspan="100" class="text-center">No records found</td></tr>');
                }
            }
        });
    },

    /* ================= TABLE + PAGINATION ================= */

    renderTable: function () {
        const self = this;
        const search = $('#tableSearch').val().toLowerCase();

        const filtered = self.currentData.filter(r =>
            self.currentHeaders.some(h =>
                (r[h] ?? '').toString().toLowerCase().includes(search)
            )
        );

        const total = filtered.length;
        const totalPages = Math.ceil(total / self.pageSize) || 1;
        if (self.currentPage > totalPages) self.currentPage = totalPages;

        const start = (self.currentPage - 1) * self.pageSize;
        const pageData = filtered.slice(start, start + self.pageSize);

        /* Header */
        let h = '<tr><th>SL</th>';
        self.currentHeaders.forEach(x => h += `<th>${x}</th>`);
        $('#ctcHead').html(h + '</tr>');

        /* Body */
        let b = '';
        pageData.forEach((r, i) => {
            b += `<tr><td>${start + i + 1}</td>`;
            self.currentHeaders.forEach(h => b += `<td>${r[h] ?? ''}</td>`);
            b += '</tr>';
        });
        $('#gridBody').html(b || '<tr><td colspan="100" class="text-center">No data</td></tr>');

        /* Info */
        $('#paginationInfo').html(
            `Showing <b>${total ? start + 1 : 0}</b>
             to <b>${Math.min(start + self.pageSize, total)}</b>
             of <b>${total}</b> entries`
        );

        /* Buttons */
        $('#btnPrev').prop('disabled', self.currentPage === 1);
        $('#btnNext').prop('disabled', self.currentPage === totalPages);

        /* Page Numbers */
        let p = '';
        for (let i = 1; i <= totalPages; i++) {
            p += `<button class="btn-page ${i === self.currentPage ? 'active' : ''}"
                   onclick="ReportEngine.gotoPage(${i})">${i}</button>`;
        }
        $('#pageNumbers').html(p);
    },

    gotoPage: function (p) {
        this.currentPage = p;
        this.renderTable();
    },
    nextPage: function () {
        this.currentPage++;
        this.renderTable();
    },
    prevPage: function () {
        this.currentPage--;
        this.renderTable();
    },

    /* ================= EXPORT (UNCHANGED) ================= */

    buildExcelHeader: function () {
        const lines = [];
        if (this.reportTitle) lines.push(this.reportTitle.toUpperCase());
        if (this.reportSubTitle) lines.push(this.reportSubTitle);
        lines.push(`Generated On : ${new Date().toLocaleString()}`);
        lines.push('');
        return lines.join('\r\n');
    },

    exportExcel: function () {
        if (!this.currentData.length) return alert("No data to export");

        const headers = ["SL"].concat(this.currentHeaders);
        const sheetData = [];

        sheetData.push([this.reportTitle.toUpperCase()]);
        sheetData.push([this.reportSubTitle]);
        sheetData.push([`Generated On: ${new Date().toLocaleString()}`]);
        sheetData.push([]);
        sheetData.push(headers);

        this.currentData.forEach((r, i) => {
            sheetData.push(headers.map(h =>
                h === 'SL' ? i + 1 :
                    isNaN(r[h]) ? r[h] : parseFloat(r[h].toString().replace(/,/g, ''))
            ));
        });

        const ws = XLSX.utils.aoa_to_sheet(sheetData);
        ws['!merges'] = [
            { s: { r: 0, c: 0 }, e: { r: 0, c: headers.length - 1 } },
            { s: { r: 1, c: 0 }, e: { r: 1, c: headers.length - 1 } },
            { s: { r: 2, c: 0 }, e: { r: 2, c: headers.length - 1 } }
        ];

        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, "Salary Register");

        XLSX.writeFile(
            wb,
            `Salary_Register_${$('#ddlMonth option:selected').text()}_${$('#ddlYear').val()}.xlsx`
        );
    },

    exportCSV: function () {
        if (!this.currentData.length) return alert("No data to export");

        const headers = ['SL'].concat(this.currentHeaders);
        const rows = [headers.join(',')];

        this.currentData.forEach((r, i) => {
            rows.push(headers.map(h => h === 'SL' ? i + 1 : (r[h] ?? '')).join(','));
        });

        const blob = new Blob(['\uFEFF', rows.join('\r\n')], { type: 'text/csv' });
        this.downloadFile(blob, 'csv');
    },

    downloadFile: function (blob, ext) {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = `Salary_Register_${$('#ddlMonth option:selected').text()}_${$('#ddlYear').val()}.${ext}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    },

    exportPDF: function () { this.printReport(); },

    printReport: function () {
        const w = window.open('', '', 'width=900,height=700');
        w.document.write(`<html><head><title>${this.reportTitle}</title></head>
            <body>${document.getElementById('reportContainer').innerHTML}</body></html>`);
        w.document.close();
        setTimeout(() => { w.print(); w.close(); }, 500);
    }
};
