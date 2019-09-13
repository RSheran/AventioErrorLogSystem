
var isActiveForAll = false; //variable to determine whether for active all or inactive all

$(document).ready(function () {

    $("#btnReset").click(ResetControls);
    $("#btnSave").click(SaveField);
    $("#btnSearchField").click(ShowFieldIndexPopUp);
    $("#txtFieldCode").keyup(AutoCompleteForFieldCode);
    $("#txtFieldCode").blur(GetDataForFieldCode);

    $("#btnResetPopup").click(ResetPopUp);
    $("#btnChangeStatus").click(UpdateActiveStatus);
    $("#btnActiveAll").click(ActiveAllFields);
    $("#btnInactiveAll").click(InactiveAllFields);

    $("#txtSearchError").keyup(FilterBootstrapGridBySearch);


});

/**For Field selection PopUp***/

var updateStatArr = []; //To store status update values

function LoadFieldsForIndexPopup() {

    var param = JSON.stringify({ 'isActive': parseInt(0) });
    $.ajax({
        url: '/Field/LoadFields',
        type: 'post',
        data: param,
        contentType: 'application/json',
        success: function (inputParam) {

            LoadFieldsToBootstrapGrid(inputParam);

        }

    });

}


function LoadFieldsToBootstrapGrid(FieldList) {

    var table = $("#tblField tbody");


    table.empty();
    if (FieldList != null && typeof (FieldList) !== 'undefined' && FieldList.length > 0) {

        $('.hidecol').show();

        $.each(FieldList, function (idx, elem) {

            getPagination('#tblField');

            if (elem.IsActive == true) {

                table.append("<tr class='success'><td>" + elem.FieldCode +
                            "</td><td>" + elem.FieldName +
                            "</td><td>" + elem.FieldDescription +
                             "</td><td class='hidetd'>" + elem.IsActive +
                            "</td><td><input type='checkbox' class='chkbox' checked='checked' value='' />" +
                             "</td></tr>");
            }
            else {
                table.append("<tr class='danger'><td>" + elem.FieldCode +
                           "</td><td>" + elem.FieldName +
                           "</td><td>" + elem.FieldDescription +
                           "</td><td class='hidetd'>" + elem.IsActive +
                           "</td><td><input type='checkbox' class='chkbox' value='' />" +
                            "</td></tr>");
            }

        });

        $('.hidecol').hide();
        $('.hidetd').hide();


        $('.no-records-found').hide();


    }
    else {
        $('.no-records-found').show();
    }

    $('#tblField tbody tr').on('click', function () {


        var FieldCode = $(this).closest("tr").find('td:eq(0)').text();
        var FieldName = $(this).closest("tr").find('td:eq(1)').text();
        var FieldDescription = $(this).closest("tr").find('td:eq(2)').text();
        var isActive = $(this).closest("tr").find('td:eq(3)').text();


        var $chkObj = $(this).find(':checkbox');

        if (event.target.type != 'checkbox') {
            $("#txtFieldCode").val(FieldCode);
            $("#txtFieldName").val(FieldName);
            $("#txtFieldDescription").val(FieldDescription);


            if (isActive == 'true') {
                $("#chkIsFieldActive").attr('checked', true);
            }
            else {
                $("#chkIsFieldActive").attr('checked', false);
            }

            swal({
                title: "",
                text: "Field Code selected successfully! ",
                type: "success",
                timer: 1000,
                showConfirmButton: false
            });

            $("#txtFieldCode").parent().addClass("form-group has-success");

            var dialog = $("#FieldPopUpModal").dialog();
            dialog.dialog("close");

        }
        else {


            var $chkObj = $(this).find(':checkbox');

            if ($chkObj.prop('checked')) {


                isActive = true;

                var objArr = {};
                objArr["FieldCode"] = FieldCode;
                objArr["IsActive"] = isActive;

                RemoveExistingType(FieldCode);

                updateStatArr.push(objArr);


            }
            else {

                isActive = false;

                var objArr = {};
                objArr["FieldCode"] = FieldCode;
                objArr["IsActive"] = isActive;

                RemoveExistingType(FieldCode);

                updateStatArr.push(objArr);

            }


        }


    });

    //var FieldCode = $(this).closest("tr").find('td:eq(0)').text();

    //$("#txtFieldCode").val(FieldCode);

    //$("#txtFieldCode").focus();

    //var dialog = $("#FieldPopUpModal").dialog();
    //dialog.dialog("close");




}

function FilterBootstrapGridBySearch() {
    var input, filter, table, tr, td, i;
    input = document.getElementById("txtSearchError");
    filter = input.value.toUpperCase();
    table = document.getElementById("tblField");
    tr = table.getElementsByTagName("tr");
    var x;
    for (i = 0; i < tr.length; i++) {



        tdFieldCode = tr[i].getElementsByTagName("td")[0];
        tdFieldName = tr[i].getElementsByTagName("td")[1];
        tdFieldDescription = tr[i].getElementsByTagName("td")[2];

        if (tdFieldCode && tdFieldName && tdFieldDescription) {

            indexOfFilterFieldCode = tdFieldCode.innerHTML.toUpperCase().indexOf(filter);
            indexOfFilterFieldName = tdFieldName.innerHTML.toUpperCase().indexOf(filter);
            indexOfFilterFieldDescription = tdFieldDescription.innerHTML.toUpperCase().indexOf(filter);



            if ((indexOfFilterFieldCode > -1) || (indexOfFilterFieldName > -1) || (indexOfFilterFieldDescription > -1)) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }

    }
}


//Getting Details for Field code blur function
function GetDataForFieldCode(data, event) {


    var FieldCode = $("#txtFieldCode").val();

    if (FieldCode != "") {
        var dataObject = JSON.stringify({ 'FieldCode': FieldCode });


        $.ajax({
            url: '/Field/GetFieldDetailsForCode',
            type: 'post',
            data: dataObject,
            contentType: 'application/json',
            success: function (data) {

                if (data == null || data.length == 0) {
                    $("#txtFieldName").val("");
                    $("#txtFieldDescription").val(0);
                    $("#chkIsFieldActive").attr('checked', true);
                    $("#btnSave").html('Save');
                    $("#txtFieldCode").parent().removeClass("form-group has-error");
                    $("#txtFieldName").parent().removeClass("form-group has-success");
                    $("#txtFieldName").parent().removeClass("form-group has-error");
                    $("#txtFieldDescription").parent().removeClass("form-group has-success");
                    $("#txtFieldDescription").parent().removeClass("form-group has-error");
                    $("#txtFieldDescription").val('');

                } else {
                    $("#txtFieldName").val(data[0].FieldName);
                    $("#txtFieldDescription").val(data[0].FieldDescription);
                    $("#btnSave").html('Update');
                    $("#txtFieldCode").parent().removeClass("form-group has-error");
                    $("#txtFieldName").parent().removeClass("form-group has-success");
                    $("#txtFieldName").parent().removeClass("form-group has-error");
                    $("#txtFieldDescription").parent().removeClass("form-group has-success");
                    $("#txtFieldDescription").parent().removeClass("form-group has-error");

                    if (data[0].IsActive == true) {
                        $("#chkIsFieldActive").prop('checked', true);
                    }
                    else {
                        $("#chkIsFieldActive").prop('checked', false);
                    }
                }
            },


        });
    }
    else {
        $("#txtFieldName").parent().addClass("form-group has-error");
        $("#txtFieldName").parent().removeClass("form-group has-success");
        $("#txtFieldName").parent().removeClass("form-group has-warning");
    }

}




function RemoveExistingType(FieldCode) {
    for (var i = 0; i < updateStatArr.length; i++) {
        if (updateStatArr[i].FieldCode == FieldCode) {
            updateStatArr.splice(i, 1);
            break;
        }
        else {
            continue;
        }
    }
}


function ShowFieldIndexPopUp() {

    LoadFieldsForIndexPopup();

    dialog = $("#FieldPopUpModal").dialog({
        autoOpen: false,
        title: "Field Index",
        width: 800,
        modal: true,
        closeOnEscape: false,
        open: function (event, ui) { $(".ui-dialog-titlebar-close", ui.dialog || ui).hide(); },
        buttons: {
            //Save: function () {
            //    validateSetSaving();
            //    // dialog.dialog("close");
            //},
            //Cancel: function () {
            //    resetAllSetDetails();
            //    //dialog.dialog("close");
            //}
        },
        close: function () {
        }
    });

    dialog.dialog("open");
}

/********************************/


/***For Field code auto suggestion***/

function AutoCompleteForFieldCode() {

    var FieldCode = $("#txtFieldCode").val();

    var dataObject = JSON.stringify({ 'FieldCode': FieldCode });

    $("#txtFieldCode").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "/Field/GetFieldCodesForAutoComplete",
                data: dataObject,
                dataType: "json",
                success: function (data) {

                    response($.map(data, function (item) {

                        return {
                            label: item.FieldCode + "-" + item.FieldName,
                            value: item.FieldCode//,
                            //id: item.ProductID

                        }
                    }))
                },
                select: function (event, ui) {
                    $("#txtFieldCode").val(ui.item.value);

                    return false;
                },
                error: function (result) {
                    alert("No Match");
                }
            });
        }
    });
}


/**************************************/

/***To save active status of Field****************/

function UpdateActiveStatus() {


    if (updateStatArr.length > 0) {
        var objParam = JSON.stringify({ 'FieldObj': updateStatArr });

        swal({
            title: "Are you sure to commit the status change(s)..?",
            //text: "You will not be able to recover this imaginary file!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#01DF74",
            confirmButtonText: "Yes,save..",
            showLoaderOnConfirm: true,
            closeOnConfirm: false,
            height: "100px",
            width: "100px"
        },
function () {
    $.ajax({
        url: '/Field/UpdateStatusOfSelectedFields',
        type: 'post',
        data: objParam,
        contentType: 'application/json',
        success: function (inputParam) {

            if (inputParam == true) {
                swal({
                    title: "",
                    text: "Status change updated successfully! ",
                    type: "success",
                    timer: 3000,
                    showConfirmButton: false
                });

                isActiveForAll = false;
                LoadFieldsForIndexPopup();
            }
            else {
                swal({
                    title: "",
                    text: "Oops!..An error occured during the status update.",
                    type: "error",
                    timer: 3000,
                    showConfirmButton: false
                });
            }


        }

    });

});


    }
    else {
        swal({
            title: "",
            text: "No change(s) commited to update..",
            type: "error",
            timer: 3000,
            showConfirmButton: false
        });
    }


}

function ActiveAllFields() {
    isActiveForAll = true;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all Fields as active..?",
        //text: "You will not be able to recover this imaginary file!",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#01DF74",
        confirmButtonText: "Yes,save..",
        showLoaderOnConfirm: true,
        closeOnConfirm: false,
        height: "100px",
        width: "100px"
    },
    function () {

        $.ajax({
            url: '/Field/ChangeStatusForAllFields',
            type: 'post',
            data: objParam,
            contentType: 'application/json',
            success: function (inputParam) {

                swal({
                    title: "",
                    text: "All Fields changed as active successfully! ",
                    type: "success",
                    timer: 3000,
                    showConfirmButton: false
                });

                isActiveForAll = false;
                LoadFieldsForIndexPopup();

            }

        });

    });

}

function InactiveAllFields() {
    isActiveForAll = false;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all Fields as inactive..?",
        //text: "You will not be able to recover this imaginary file!",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#01DF74",
        confirmButtonText: "Yes,save..",
        showLoaderOnConfirm: true,
        closeOnConfirm: false,
        height: "100px",
        width: "100px"
    },
   function () {

       $.ajax({
           url: '/Field/ChangeStatusForAllFields',
           type: 'post',
           data: objParam,
           contentType: 'application/json',
           success: function (inputParam) {

               swal({
                   title: "",
                   text: "All Fields changed as inactive successfully! ",
                   type: "success",
                   timer: 3000,
                   showConfirmButton: false
               });

               isActiveForAll = false;
               LoadFieldsForIndexPopup();

           }

       });

   });
}

/*************************************************/


function SaveField() {
    var FieldCode = $("#txtFieldCode").val();
    var FieldName = $("#txtFieldName").val();
    var FieldDescription = $("#txtFieldDescription").val();
    var isFieldActive = $("#chkIsFieldActive").prop('checked');


    if (FieldCode.length > 0) {

        $("#txtFieldCode").parent().removeClass("form-group has-error");
        $("#txtFieldCode").parent().addClass("form-group has-success");

        if (FieldName.length > 0) {
            $("#txtFieldName").parent().removeClass("form-group has-error");
            $("#txtFieldName").parent().addClass("form-group has-success");


            var FieldObj = {};
            FieldObj['FieldCode'] = FieldCode;
            FieldObj['FieldName'] = FieldName;
            FieldObj['FieldDescription'] = FieldDescription.length > 0 ? FieldDescription : "";
            FieldObj['IsActive'] = isFieldActive;

            var dataObject = JSON.stringify({ 'FieldObj': FieldObj });

            $.ajax({
                url: '/Field/AddOrUpdateField',
                type: 'post',
                data: dataObject,
                contentType: 'application/json',
                success: function (data) {
                    if (data) {
                        swal({
                            title: "",
                            text: "Field saved successfully! ",
                            type: "success",
                            timer: 3000,
                            showConfirmButton: false
                        });

                        ResetControls();
                    }
                    else {
                        swal({
                            title: "",
                            text: "Oops..Unable to save the Field data",
                            type: "error",
                            timer: 3000,
                            showConfirmButton: false
                        });

                    }
                },


            });

        }
        else {
            $("#txtFieldName").parent().addClass("form-group has-error");
            $("#txtFieldName").parent().removeClass("form-group has-success");

        }
    }
    else {

        $("#txtFieldCode").parent().addClass("form-group has-error");
        $("#txtFieldCode").parent().removeClass("form-group has-success");
    }
}

function ResetControls() {
    $(':input').val('');

    $("#chkIsFieldActive").attr('checked', false);

    $("#txtFieldCode").parent().removeClass("form-group has-success");
    $("#txtFieldCode").parent().removeClass("form-group has-error");
    $("#txtFieldName").parent().removeClass("form-group has-success");
    $("#txtFieldName").parent().removeClass("form-group has-error");
    $("#txtFieldDescription").parent().removeClass("form-group has-success");
    $("#txtFieldDescription").parent().removeClass("form-group has-error");

}


function ResetPopUp() {
    isActiveForAll = false;
    updateStatArr = [];
    LoadUserGroupsForIndexPopup();

}





