$(function () {
    let idconfig = 1;
    defaultLoad();
    function defaultLoad() {
        // Month List 
        let url = "/SalaryConfiguration/ConfigurationDetail";
        let param = {
            "IDConfiguration": idconfig
        };
        $.get(url, param, function (data) {
            let newdata = JSON.parse(data);
            console.log(newdata);
            $("#hdIDConfiguration").val(idconfig);
            $("#txtUserName").val(newdata[0].UserName);
            $("#txtPassword").val(newdata[0].Password);
            $("#txtRetype").val(newdata[0].Password);
        });

    }
    // Save Button
    $("#btnSave").click(function () {
        if (validation() == false) { return false;}
        let url = "/SalaryConfiguration/ConfigurationSave";
        let param = {
            "IDConfiguration": idconfig,
            "UserName": $("#txtUserName").val(),
            "Password": $("#txtPassword").val()
        };
        $.ajax({
            url: url,
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(param),
            success: function (res) {
                if (res.success == "") {
                    alert("Configuration successfully saved....");
                    window.location.href = "SalaryConfiguration";
                }
                else {
                    alert(res.result);
                }
            },
        });
    });
    function validation() {
        let password = $("#txtPassword");
        let retype = $("#txtRetype");
        if (password.val() != retype.val()) {
            alert("Password mismatched....")
            password.val("");
            retype.val("")
            password.focus();
            return false
        }
        return true ;
    }
});
