$(function () {
    /*  var text = '';*/
    Default();
    function Default() {
        

        $.ajax({
            type: "GET", url: "/PTaxSlab/State_List", dataType: "json", contentType: "application/json", success: function (res) {
                $.each(res.d, function (data, value) {
                    //alert(value.Name)
                    $("#ddlState").append($("<option></option>").val(value.IDState).html(value.Name));
                })
            }

        });


        $("#btnNewRow").click(function () {
            let rows = '';
            rows += '<tr>'
            rows += '<td><input type="text" id="txtStart" class="form-control form-control-sm RangeStart" max="5" /></td>'
            rows += '<td><input type="text" id="txtEnd" class="form-control form-control-sm RangerEnd" max="5" /></td>'
            rows += '<td><input type="text" id="txtAmount" class="form-control form-control-sm Amount" max="5" /></td>'
            rows += '</tr>'
            $("#bodyPTaxSlab").append(rows);
        });
    }
    //$("#btnSave").click(function () {
    //    alert("Mayukh");

    //});
    function fnClear() {
        $("#txtYear").val('');
        $("#tblPTaxSlab tr").remove();
      }
    function Validation() {
        let Detail = [];
        $.each($("#tblPTaxSlab").find("#bodyPTaxSlab").find("tr"), function () {
            Detail.push({
                "RangeFrom": $(this).find("#txtStart").val(),
                "RangeToEnd": $(this).find("#txtEnd").val(),
                "Amount": $(this).find("#txtAmount").val(),

            });
            /*  alert("test");*/
        });
       /* let rows = ''=$("#bodyPTaxSlab").append(rows);*/
        let txtYear = $("#txtYear");
        let txtStart = $("#txtStart");
        let txtEnd = $("#txtEnd");
        let txtAmount = $("#txtAmount");
        if (txtYear.val() == '') {
            alert("Year Code is Missing....");
            return false;
        }
        if (txtStart.val() == '') {
            
            alert("Add New Row is Missing Textbox Range From....");
            return false;
        }
        if (txtEnd.val() == '') {

            alert("Add New Row is Missing Textbox Range To....");
            return false;
        }
        if (txtAmount.val() == '') {

            alert("Add New Row is Missing Textbox Amount....");
            return false;
        }

        return true;
    }


    $("#btnSave").click(function () {
        
      /*  debugger*/
        if (Validation() == true) {
           
            let Detail = [];
            let mUser = $("#ddlState").val();
            /*  let mUser = $("#txtYear").val();*/

            $.each($("#tblPTaxSlab").find("#bodyPTaxSlab").find("tr"), function () {
                Detail.push({
                    "IDState": $("#ddlState").val(),
                    "Year": $("#txtYear").val(),
                    "RangeFrom": $(this).find("#txtStart").val(),
                    "RangeToEnd": $(this).find("#txtEnd").val(),
                    "Amount": $(this).find("#txtAmount").val(),

                });
                /*  alert("test");*/
            });
            /* console.log(Detail);*/
            /* jQuery.ajaxSettings.traditional = true*/

            $.post("/PTaxSlab/PTax_Save", { Info: Detail }, function (data) {

                let Result = data.d;
                /* alert("test");*/
                if (Result == '') {
                    alert("Record Successfully Saved....");
                    fnClear();
                }
                /*alert("test");*/
            });

            //$("#txtYear").click(function () {
            //    alert("test");
            //});

            //$(document).ready(function selectyear () {
            //    $('#txtYear').DataTable({
            //        "ajax": {
            //            "url": "/PTaxSlab/PTax_Detail",
            //            "type": "GET",
            //            "datatype": "json"
            //        },
                    
            //    });
            //});


            //function RowDis() {
            //    alert("test");
            //}
           
          
        }
    });

    //function selectyear() {
    //    /*$('#txtYear').DataTable({*/
    //    //"ajax": {
    //    //    "url": "/PTaxSlab/PTax_Detail",
    //    //    "type": "GET",
    //    //    "datatype": "json"
    //    //},

    //    // });
    //    alert('ok')
    //    $.ajax({
    //        type: "GET", url: "/PTaxSlab/PTax_Detail", dataType: "json", contentType: "application/json", success: function (res) {
    //            $.each(res.d, function (data, value) {
    //                //alert(value.Name)

    //            })
    //        }

    //    });



        //function RowDis() {
        //    alert("test");
        //}

    $("#btnSearch").click(function () {
      

        if (Validation() == true) {
            
         /* alert('ok')*/
            $.ajax({
               
                type: "GET", url: "/PTaxSlab/PTax_Detail", data: { 'IDState': $("#ddlState").val(), 'Year': $("#txtYear").val() }, "datatype": "json",success: function (res) {
                    $.each(res.d, function (data, value) {
                        
                        //alert(value);
                        //alert(res.d)
                        let rows = '';
                        rows += '<tr>'
                        rows += '<td><input type="text" id="txtStart" class="form-control form-control-sm RangeStart" max="5" value="' + value.RangeFrom + '"/></td>'
                        rows += '<td><input type="text" id="txtEnd" class="form-control form-control-sm RangerEnd" max="5" value="' + value.RangeToEnd +'"/> </td>'
                        rows += '<td><input type="text" id="txtAmount" class="form-control form-control-sm Amount" max="5" value="' + value.Amount +'"/></td>'
                        rows += '</tr>'
                        $("#bodyPTaxSlab").append(rows);
                    })
                }

            });


        }
    });
    
});

