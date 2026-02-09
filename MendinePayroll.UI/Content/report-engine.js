/* ========================================================================
   ReportEngine.js - Global Engine for ieHRMS Reports
   Handles Filters + Dynamic Exporting + Visibility Control
   ======================================================================== */

var ReportEngine = {
    currentData: [],
    currentHeaders: [],
    currentUrl: '',

    // Generic Report Header Info (Injected from Page)
    reportTitle: '',
    reportSubTitle: '', 

    /* ================= INIT ================= */
    init: function (dataUrl, options, reportInfo) {
        var self = this;
        self.currentUrl = dataUrl;

        // Optional Report Header Info
        if (reportInfo) {
            self.reportTitle = reportInfo.title || '';
            self.reportSubTitle = reportInfo.subTitle || '';
        }

        var settings = $.extend({
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
            .on('change', function () { self.loadEmployees(); });

        $('.filter-trigger')
            .on('change', function () { self.loadData(self.currentUrl); });

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
        var self = this;

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
        }, function (res) {
            const ddl = $('#ddlEmployee').empty();
            res.forEach(x => ddl.append(new Option(x.EmployeeName, x.EmployeeID)));
            ddl.multiselect('rebuild');
        });
    },

    /* ================= DATA ================= */

    loadData: function (url) {
        var self = this;

        $('#gridBody').html(
            '<tr><td colspan="100" class="text-center" style="padding:40px;">' +
            '<i class="fa fa-spinner fa-spin"></i> Generating Report...</td></tr>'
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
                    self.renderTable();
                } else {
                    $('#gridBody').html('<tr><td colspan="100" class="text-center">No records found</td></tr>');
                }
            }
        });
    },

    renderTable: function () {
        var h = '<tr><th>SL</th>';
        this.currentHeaders.forEach(x => h += `<th>${x}</th>`);
        $('#ctcHead').html(h + '</tr>');

        var b = '';
        this.currentData.forEach((r, i) => {
            b += `<tr><td>${i + 1}</td>`;
            this.currentHeaders.forEach(h => b += `<td>${r[h] ?? ''}</td>`);
            b += '</tr>';
        });
        $('#gridBody').html(b);
    },

    /* ================= EXPORT ================= */

    buildExcelHeader: function () {
        const lines = [];
        const now = new Date().toLocaleString();

        // Add custom titles from reportInfo
        if (this.reportTitle) lines.push(this.reportTitle.toUpperCase());
        if (this.reportSubTitle) lines.push(this.reportSubTitle);
        lines.push(`Generated On : ${now}`);
        lines.push(''); // Spacing

        return lines.join('\r\n');
    },

    //exportExcel: function () {
    //    if (this.currentData.length === 0) {
    //        alert("No data to export");
    //        return;
    //    }

    //    // IMPORTANT: tell Excel explicitly it's TAB-separated
    //    //const content = "sep=\t\r\n" + this.generateFileContent("\t");
    //    const headerInfo = this.buildExcelHeader();
    //    const content = "sep=\t\r\n" + headerInfo + this.generateFileContent("\t");
    //    const blob = new Blob(
    //        ["\uFEFF", content],
    //        { type: "application/vnd.ms-excel;charset=utf-16le;" }
    //    );

    //    this.downloadFile(blob, "xls");
    //},

    exportExcel: function () {
        const self = this;
        if (!self.currentData.length) {
            alert("No data to export");
            return;
        }

        const headers = ["SL"].concat(self.currentHeaders);
        const colCount = headers.length;

        // 1. Prepare the Array of Arrays (AOA) for SheetJS
        const sheetData = [];

        // 2. Add Merged Header Information
        sheetData.push([self.reportTitle.toUpperCase()]); // Row 0
        sheetData.push([self.reportSubTitle]);           // Row 1
        sheetData.push([`Generated On: ${new Date().toLocaleString()}`]); // Row 2
        sheetData.push([]); // Spacer Row 3

        // 3. Add Table Column Headers
        sheetData.push(headers); // Row 4

        // 4. Add Body Data
        self.currentData.forEach((r, i) => {
            const row = headers.map(h => {
                if (h === 'SL') return i + 1;
                let val = r[h] ?? '';
                // Convert to number for Excel calculations, removing any display commas
                return isNaN(val) || val === '' ? val : parseFloat(val.toString().replace(/,/g, ""));
            });
            sheetData.push(row);
        });

        // 5. Create Worksheet
        const worksheet = XLSX.utils.aoa_to_sheet(sheetData);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Salary Register");

        // 6. Define Merges (s = start, e = end, r = row, c = column)
        worksheet['!merges'] = [
            { s: { r: 0, c: 0 }, e: { r: 0, c: colCount - 1 } }, // Merge Title across all columns
            { s: { r: 1, c: 0 }, e: { r: 1, c: colCount - 1 } }, // Merge Subtitle
            { s: { r: 2, c: 0 }, e: { r: 2, c: colCount - 1 } }  // Merge Date
        ];

        // 7. Auto-Size Columns (Approximate)
        worksheet['!cols'] = headers.map(h => ({ wch: Math.max(h.length, 12) }));

        // 8. Trigger File Download
        const m = $('#ddlMonth option:selected').text();
        const y = $('#ddlYear').val();
        XLSX.writeFile(workbook, `Salary_Register_${m}_${y}.xlsx`);
    },
    exportCSV: function () {
        if (!this.currentData.length) {
            alert('No data to export');
            return;
        }
        // 1. Build Header info + separator instruction + table content
        const headerInfo = this.buildExcelHeader();
        const content = 'sep=,\r\n' + headerInfo + this.generateFileContent(',');
        //const content = 'sep=,\r\n' + this.generateFileContent(',');
        const blob = new Blob(['\uFEFF', content], { type: 'text/csv;charset=utf-8;' });
        this.downloadFile(blob, 'csv');
    },

    generateFileContent: function (sep) {
        const rows = [];
        const headers = ['SL'].concat(this.currentHeaders);

        rows.push(headers.join(sep));

        this.currentData.forEach((r, i) => {
            const row = headers.map(h => h === 'SL' ? i + 1 : (r[h] ?? ''));
            rows.push(row.join(sep));
        });

        return rows.join('\r\n');
    },

    downloadFile: function (blob, ext) {
        const m = $('#ddlMonth option:selected').text();
        const y = $('#ddlYear').val();
        const a = document.createElement('a');

        a.href = URL.createObjectURL(blob);
        a.download = `Salary_Register_${m}_${y}.${ext}`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    },

    exportPDF: function () { this.printReport(); },

    printReport: function () {
        const m = $('#ddlMonth option:selected').text();
        const y = $('#ddlYear').val();
        const w = window.open('', '', 'width=900,height=700');

        w.document.write(
            `<html><head><title>${this.reportTitle}</title>
             <style>
               table{width:100%;border-collapse:collapse}
               th,td{border:1px solid #ccc;padding:6px;font-size:10px}
               th{background:#f4f4f4}
             </style></head>
             <body><h2>${this.reportTitle}</h2>
             <h4>${this.reportSubTitle}</h4>
             ${document.getElementById('reportContainer').innerHTML}
             </body></html>`
        );

        w.document.close();
        setTimeout(() => { w.print(); w.close(); }, 500);
    }
};
