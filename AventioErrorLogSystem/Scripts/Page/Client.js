
var isActiveForAll = false; //variable to determine whether for active all or inactive all

$(document).ready(function () {

    $("#btnReset").click(ResetControls);
    $("#btnSave").click(SaveClient);
    $("#btnSearchClient").click(ShowClientIndexPopUp);
    $("#txtClientCode").keyup(AutoCompleteForClientCode);
    $("#txtClientCode").blur(GetDataForClientCode);

    $("#btnResetPopup").click(ResetPopUp);
    $("#btnChangeStatus").click(UpdateActiveStatus);
    $("#btnActiveAll").click(ActiveAllClients);
    $("#btnInactiveAll").click(InactiveAllClients);

    $("#txtSearchError").keyup(FilterBootstrapGridBySearch);


});

/**For Client selection PopUp***/

var updateStatArr = []; //To store status update values

function LoadClientsForIndexPopup() {

    var param = JSON.stringify({ 'isActive': parseInt(0) });
    $.ajax({
        url: '/Client/LoadClients',
        type: 'post',
        data: param,
        contentType: 'application/json',
        success: function (inputParam) {

            LoadClientsToBootstrapGrid(inputParam);

        }

    });

}


function LoadClientsToBootstrapGrid(ClientList) {

    var table = $("#tblClient tbody");


    table.empty();
    if (ClientList != null && typeof (ClientList) !== 'undefined' && ClientList.length > 0) {

        $('.hidecol').show();

        $.each(ClientList, function (idx, elem) {

            getPagination('#tblClient');

            if (elem.IsActive == true) {

                table.append("<tr class='success'><td>" + elem.ClientCode +
                            "</td><td>" + elem.ClientName +
                            "</td><td>" + elem.ClientDescription +
                             "</td><td class='hidetd'>" + elem.IsActive +
                            "</td><td><input type='checkbox' class='chkbox' checked='checked' value='' />" +
                             "</td></tr>");
            }
            else {
                table.append("<tr class='danger'><td>" + elem.ClientCode +
                           "</td><td>" + elem.ClientName +
                           "</td><td>" + elem.ClientDescription +
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

    $('#tblClient tbody tr').on('click', function () {


        var ClientCode = $(this).closest("tr").find('td:eq(0)').text();
        var ClientName = $(this).closest("tr").find('td:eq(1)').text();
        var ClientDescription = $(this).closest("tr").find('td:eq(2)').text();
        var isActive = $(this).closest("tr").find('td:eq(3)').text();


        var $chkObj = $(this).find(':checkbox');

        if (event.target.type != 'checkbox') {
            $("#txtClientCode").val(ClientCode);
            $("#txtClientName").val(ClientName);
            $("#txtClientDescription").val(ClientDescription);


            if (isActive == 'true') {
                $("#chkIsClientActive").attr('checked', true);
            }
            else {
                $("#chkIsClientActive").attr('checked', false);
            }

            swal({
                title: "",
                text: "Client Code selected successfully! ",
                type: "success",
                timer: 1000,
                showConfirmButton: false
            });

            $("#txtClientCode").parent().addClass("form-group has-success");

            var dialog = $("#ClientPopUpModal").dialog();
            dialog.dialog("close");

        }
        else {


            var $chkObj = $(this).find(':checkbox');

            if ($chkObj.prop('checked')) {


                isActive = true;

                var objArr = {};
                objArr["ClientCode"] = ClientCode;
                objArr["IsActive"] = isActive;

                RemoveExistingType(ClientCode);

                updateStatArr.push(objArr);


            }
            else {

                isActive = false;

                var objArr = {};
                objArr["ClientCode"] = ClientCode;
                objArr["IsActive"] = isActive;

                RemoveExistingType(ClientCode);

                updateStatArr.push(objArr);

            }


        }


    });

    //var ClientCode = $(this).closest("tr").find('td:eq(0)').text();

    //$("#txtClientCode").val(ClientCode);

    //$("#txtClientCode").focus();

    //var dialog = $("#ClientPopUpModal").dialog();
    //dialog.dialog("close");




}

function FilterBootstrapGridBySearch() {
    var input, filter, table, tr, td, i;
    input = document.getElementById("txtSearchError");
    filter = input.value.toUpperCase();
    table = document.getElementById("tblClient");
    tr = table.getElementsByTagName("tr");
    var x;
    for (i = 0; i < tr.length; i++) {



        tdClientCode = tr[i].getElementsByTagName("td")[0];
        tdClientName = tr[i].getElementsByTagName("td")[1];
        tdClientDescription = tr[i].getElementsByTagName("td")[2];

        if (tdClientCode && tdClientName && tdClientDescription) {

            indexOfFilterClientCode = tdClientCode.innerHTML.toUpperCase().indexOf(filter);
            indexOfFilterClientName = tdClientName.innerHTML.toUpperCase().indexOf(filter);
            indexOfFilterClientDescription = tdClientDescription.innerHTML.toUpperCase().indexOf(filter);



            if ((indexOfFilterClientCode > -1) || (indexOfFilterClientName > -1) || (indexOfFilterClientDescription > -1)) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }

    }
}


//Getting Details for Client code blur function
function GetDataForClientCode(data, event) {


    var ClientCode = $("#txtClientCode").val();

    if (ClientCode != "") {
        var dataObject = JSON.stringify({ 'ClientCode': ClientCode });


        $.ajax({
            url: '/Client/GetClientDetailsForCode',
            type: 'post',
            data: dataObject,
            contentType: 'application/json',
            success: function (data) {

                if (data == null || data.length == 0) {
                    $("#txtClientName").val("");
                    $("#txtClientDescription").val(0);
                    $("#chkIsClientActive").attr('checked', true);
                    $("#btnSave").html('Save');
                    $("#txtClientCode").parent().removeClass("form-group has-error");
                    $("#txtClientName").parent().removeClass("form-group has-success");
                    $("#txtClientName").parent().removeClass("form-group has-error");
                    $("#txtClientDescription").parent().removeClass("form-group has-success");
                    $("#txtClientDescription").parent().removeClass("form-group has-error");
                    $("#txtClientDescription").val('');

                } else {
                    $("#txtClientName").val(data[0].ClientName);
                    $("#txtClientDescription").val(data[0].ClientDescription);
                    $("#btnSave").html('Update');
                    $("#txtClientCode").parent().removeClass("form-group has-error");
                    $("#txtClientName").parent().removeClass("form-group has-success");
                    $("#txtClientName").parent().removeClass("form-group has-error");
                    $("#txtClientDescription").parent().removeClass("form-group has-success");
                    $("#txtClientDescription").parent().removeClass("form-group has-error");

                    if (data[0].IsActive == true) {
                        $("#chkIsClientActive").prop('checked', true);
                    }
                    else {
                        $("#chkIsClientActive").prop('checked', false);
                    }
                }
            },


        });
    }
    else {
        $("#txtClientName").parent().addClass("form-group has-error");
        $("#txtClientName").parent().removeClass("form-group has-success");
        $("#txtClientName").parent().removeClass("form-group has-warning");
    }

}





function RemoveExistingType(ClientCode) {
    for (var i = 0; i < updateStatArr.length; i++) {
        if (updateStatArr[i].ClientCode == ClientCode) {
            updateStatArr.splice(i, 1);
            break;
        }
        else {
            continue;
        }
    }
}


function ShowClientIndexPopUp() {

    LoadClientsForIndexPopup();

    dialog = $("#ClientPopUpModal").dialog({
        autoOpen: false,
        title: "Client Index",
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


/***For Client code auto suggestion***/

function AutoCompleteForClientCode() {

    var ClientCode = $("#txtClientCode").val();

    var dataObject = JSON.stringify({ 'ClientCode': ClientCode });

    $("#txtClientCode").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "/Client/GetClientCodesForAutoComplete",
                data: dataObject,
                dataType: "json",
                success: function (data) {

                    response($.map(data, function (item) {

                        return {
                            label: item.ClientCode + "-" + item.ClientName,
                            value: item.ClientCode//,
                            //id: item.ProductID

                        }
                    }))
                },
                select: function (event, ui) {
                    $("#txtClientCode").val(ui.item.value);

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

/***To save active status of Client****************/

function UpdateActiveStatus() {


    if (updateStatArr.length > 0) {
        var objParam = JSON.stringify({ 'ClientObj': updateStatArr });

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
        url: '/Client/UpdateStatusOfSelectedClients',
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
                LoadClientsForIndexPopup();
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

function ActiveAllClients() {
    isActiveForAll = true;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all Clients as active..?",
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
            url: '/Client/ChangeStatusForAllClients',
            type: 'post',
            data: objParam,
            contentType: 'application/json',
            success: function (inputParam) {

                swal({
                    title: "",
                    text: "All Clients changed as active successfully! ",
                    type: "success",
                    timer: 3000,
                    showConfirmButton: false
                });

                isActiveForAll = false;
                LoadClientsForIndexPopup();

            }

        });

    });

}

function InactiveAllClients() {
    isActiveForAll = false;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all Clients as inactive..?",
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
           url: '/Client/ChangeStatusForAllClients',
           type: 'post',
           data: objParam,
           contentType: 'application/json',
           success: function (inputParam) {

               swal({
                   title: "",
                   text: "All Clients changed as inactive successfully! ",
                   type: "success",
                   timer: 3000,
                   showConfirmButton: false
               });

               isActiveForAll = false;
               LoadClientsForIndexPopup();

           }

       });

   });
}

/*************************************************/


function SaveClient() {
    var ClientCode = $("#txtClientCode").val();
    var ClientName = $("#txtClientName").val();
    var ClientDescription = $("#txtClientDescription").val();
    var isClientActive = $("#chkIsClientActive").prop('checked');


    if (ClientCode.length > 0) {

        $("#txtClientCode").parent().removeClass("form-group has-error");
        $("#txtClientCode").parent().addClass("form-group has-success");

        if (ClientName.length > 0) {
            $("#txtClientName").parent().removeClass("form-group has-error");
            $("#txtClientName").parent().addClass("form-group has-success");


            var ClientObj = {};
            ClientObj['ClientCode'] = ClientCode;
            ClientObj['ClientName'] = ClientName;
            ClientObj['ClientDescription'] = ClientDescription.length > 0 ? ClientDescription : "";
            ClientObj['IsActive'] = isClientActive;

            var dataObject = JSON.stringify({ 'ClientObj': ClientObj });

            $.ajax({
                url: '/Client/AddOrUpdateClient',
                type: 'post',
                data: dataObject,
                contentType: 'application/json',
                success: function (data) {
                    if (data) {
                        swal({
                            title: "",
                            text: "Client saved successfully! ",
                            type: "success",
                            timer: 3000,
                            showConfirmButton: false
                        });

                        ResetControls();
                    }
                    else {
                        swal({
                            title: "",
                            text: "Oops..Unable to save the Client data",
                            type: "error",
                            timer: 3000,
                            showConfirmButton: false
                        });

                    }
                },


            });

        }
        else {
            $("#txtClientName").parent().addClass("form-group has-error");
            $("#txtClientName").parent().removeClass("form-group has-success");

        }
    }
    else {

        $("#txtClientCode").parent().addClass("form-group has-error");
        $("#txtClientCode").parent().removeClass("form-group has-success");
    }
}

function ResetControls() {
    $(':input').val('');

    $("#chkIsClientActive").attr('checked', false);

    $("#txtClientCode").parent().removeClass("form-group has-success");
    $("#txtClientCode").parent().removeClass("form-group has-error");
    $("#txtClientName").parent().removeClass("form-group has-success");
    $("#txtClientName").parent().removeClass("form-group has-error");
    $("#txtClientDescription").parent().removeClass("form-group has-success");
    $("#txtClientDescription").parent().removeClass("form-group has-error");

}


function ResetPopUp() {
    isActiveForAll = false;
    updateStatArr = [];
    LoadUserGroupsForIndexPopup();

}





