$(function () {
    defaultLoad();
    function defaultLoad() {
        // Month List 
        let url = "/Bonus/Monthlist";
        $.get(url, function (data) {
            $("#ddlMonth").empty();
            data.map(function (x) {
                $("#ddlMonth").append('<option value=' + x.month + '>' + x.month + '</option>');
            });
        });
        // Current Year 
        url = "/Bonus/CurrentYear";
        $.get(url, function (data) {
            $("#txtYear").val(data);
        });
        // Bonus Date
        $("#txtDate").datepicker(
            { dateFormat: 'dd-M-yy' }
        );
        // Emplyee List 
        url = "/Bonus/EmployeeList";
        $.get(url, function (result) {
            data = $.parseJSON(result);
            $("#ddlEmployee").empty();
            data.map(function (x) {
                $("#ddlEmployee").append('<option value=' + x.empID + '>' + x.EmpDetails + '</option>');
            });
        });

    }
    // Save Button
    $("#btnSave").click(function () {
        if (Validation() == true) {
            let info = {
                "IDBonus": $("#hdIDBonus").val() == '' ? "0" : $("#hdIDBonus").val(),
                "IDEmployee": $("#ddlEmployee").val(),
                "BonusDate": $("#txtDate").val(),
                "Amount": $("#txtAmount").val(),
                "BonusMonth": $("#ddlMonth").val(),
                "BonusYear": $("#txtYear").val()
            };

            let url = "/Bonus/BonusSave";
            $.ajax({
                url: url,
                type: "POST",
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify(info),
                success: function (res) {
                    if (res.success == "") {
                        alert("Record successfully saved....");
                        window.location.href = "Bonus";
                    }
                    else {
                        alert(res.result);
                    }
                },
            });
        }
    });
    function Validation() {
        let txtDate = $("#txtDate");
        let txtAmount = $("#txtAmount");
        if (txtDate.val() == '') {
            alert("Bonus date is missing");
            txtDate.focus();
            return false;
        }
        if (txtAmount.val() == '' || txtAmount.val() == '0') {
            txtAmount.focus();
            alert("Bonus amount is missing");
            return false;
        }
        return true;
    }
    // List Button 
    $("#btnList").click(function () {
        let url = "/Bonus/BonusList";
        $.get(url, function (data) {
            //console.log(data);
            let newdata = JSON.parse(data);
            $("#tblBody").empty();
            var row = '';
            newdata.map(function (x) {
                row = row + '<tr>';
                row = row + '<td>' + x.BonusDate;
                row = row + '<input type="hidden" id="hdIDBonus" value="' + x.IDBonus + '" /></td>';
                row = row + '<td>' + x.EmployeeName + '</td>';
                row = row + '<td>' + x.Amount + '</td>';
                row = row + '<td>' + x.BonusMonth + '</td>';
                row = row + '<td>' + x.BonusYear + '</td>';
                row = row + '<td> <input type="button" id="btnShow" value="Details" class="mb-sm mt-xs mr-xs btn btn-sm btn-tertiary text-sm text-weight-semibold" >';
                row = row + '</tr>';
            });
            $("#tblBody").append(row);
            $(".divList").removeAttr('style');
            $(".divEntry").css('display', 'none');
            $("#btnSave").prop('disabled', true);
        });
    });
    // List Button 
    $("#btnAdd").click(function () {
        window.location.href = "Bonus";
    });
    $("#tblBody").on("click", "#btnShow", function () {

        let bonus = $(this).closest("tr").find("#hdIDBonus").val();
        let param = {
            "IDBonus": bonus
        };
        let url = "/Bonus/BonusDetail";
        $.get(url, param, function (data) {
            let newdata = JSON.parse(data);
            console.log(newdata[0].IDBonus);
            $("#hdIDBonus").val(newdata[0].IDBonus);
            $("#ddlEmployee").val(newdata[0].IDEmployee);
            $("#txtDate").val(newdata[0].BonusDate);
            $("#txtAmount").val(newdata[0].Amount);
            $("#ddlMonth").val(newdata[0].BonusMonth);
            $("#txtYear").val(newdata[0].BonusYear);
        });
        $(".divEntry").removeAttr('style');
        $(".divList").css('display', 'none');
        $("#btnSave").prop('disabled', false);
    });

});
