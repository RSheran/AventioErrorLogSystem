
var isActiveForAll = false; //variable to determine whether for active all or inactive all

$(document).ready(function () {

    $("#btnReset").click(ResetControls);
    $("#btnSave").click(SaveDomain);
    $("#btnSearchDomain").click(ShowDomainIndexPopUp);
    $("#txtDomainCode").keyup(AutoCompleteForDomainCode);
    $("#txtDomainCode").blur(GetDataForDomainCode);

    $("#btnResetPopup").click(ResetPopUp);
    $("#btnChangeStatus").click(UpdateActiveStatus);
    $("#btnActiveAll").click(ActiveAllDomains);
    $("#btnInactiveAll").click(InactiveAllDomains);

    $("#txtSearchError").keyup(FilterBootstrapGridBySearch);


});

/**For Domain selection PopUp***/

var updateStatArr = []; //To store status update values

function LoadDomainsForIndexPopup() {

    var param = JSON.stringify({ 'isActive': parseInt(0) });
    $.ajax({
        url: '/Domain/LoadDomains',
        type: 'post',
        data: param,
        contentType: 'application/json',
        success: function (inputParam) {

            LoadDomainsToBootstrapGrid(inputParam);

        }

    });

}


function LoadDomainsToBootstrapGrid(domainList) {

    var table = $("#tblDomain tbody");


    table.empty();
    if (domainList != null && typeof (domainList) !== 'undefined' && domainList.length > 0) {

        $('.hidecol').show();

        $.each(domainList, function (idx, elem) {

            getPagination('#tblDomain');

            if (elem.IsActive == true) {

                table.append("<tr class='success'><td>" + elem.DomainCode +
                            "</td><td>" + elem.DomainName +
                            "</td><td>" + elem.DomainDescription +
                             "</td><td class='hidetd'>" + elem.IsActive +
                            "</td><td><input type='checkbox' class='chkbox' checked='checked' value='' />" +
                             "</td></tr>");
            }
            else {
                table.append("<tr class='danger'><td>" + elem.DomainCode +
                           "</td><td>" + elem.DomainName +
                           "</td><td>" + elem.DomainDescription +
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

    $('#tblDomain tbody tr').on('click', function () {


        var domainCode = $(this).closest("tr").find('td:eq(0)').text();
        var domainName = $(this).closest("tr").find('td:eq(1)').text();
        var domainDescription = $(this).closest("tr").find('td:eq(2)').text();
        var isActive = $(this).closest("tr").find('td:eq(3)').text();


        var $chkObj = $(this).find(':checkbox');

        if (event.target.type != 'checkbox') {
            $("#txtDomainCode").val(domainCode);
            $("#txtDomainName").val(domainName);
            $("#txtDomainDescription").val(domainDescription);


            if (isActive == 'true') {
                $("#chkIsDomainActive").attr('checked', true);
            }
            else {
                $("#chkIsDomainActive").attr('checked', false);
            }

            swal({
                title: "",
                text: "Domain Code selected successfully! ",
                type: "success",
                timer: 1000,
                showConfirmButton: false
            });

            $("#txtDomainCode").parent().addClass("form-group has-success");

            var dialog = $("#domainPopUpModal").dialog();
            dialog.dialog("close");

        }
        else {


            var $chkObj = $(this).find(':checkbox');

            if ($chkObj.prop('checked')) {


                isActive = true;

                var objArr = {};
                objArr["DomainCode"] = domainCode;
                objArr["IsActive"] = isActive;

                RemoveExistingType(domainCode);

                updateStatArr.push(objArr);


            }
            else {

                isActive = false;

                var objArr = {};
                objArr["DomainCode"] = domainCode;
                objArr["IsActive"] = isActive;

                RemoveExistingType(domainCode);

                updateStatArr.push(objArr);

            }


        }


    });

    //var domainCode = $(this).closest("tr").find('td:eq(0)').text();

    //$("#txtDomainCode").val(domainCode);

    //$("#txtDomainCode").focus();

    //var dialog = $("#domainPopUpModal").dialog();
    //dialog.dialog("close");




}

function FilterBootstrapGridBySearch() {
    var input, filter, table, tr, td, i;
    input = document.getElementById("txtSearchError");
    filter = input.value.toUpperCase();
    table = document.getElementById("tblDomain");
    tr = table.getElementsByTagName("tr");
    var x;
    for (i = 0; i < tr.length; i++) {



        tdDomainCode = tr[i].getElementsByTagName("td")[0];
        tdDomainName = tr[i].getElementsByTagName("td")[1];
        tdDomainDescription = tr[i].getElementsByTagName("td")[2];

        if (tdDomainCode && tdDomainName && tdDomainDescription) {

            indexOfFilterDomainCode = tdDomainCode.innerHTML.toUpperCase().indexOf(filter);
            indexOfFilterDomainName = tdDomainName.innerHTML.toUpperCase().indexOf(filter);
            indexOfFilterDomainDescription = tdDomainDescription.innerHTML.toUpperCase().indexOf(filter);



            if ((indexOfFilterDomainCode > -1) || (indexOfFilterDomainName > -1) || (indexOfFilterDomainDescription > -1)) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }

    }
}


//Getting Details for domain code blur function
function GetDataForDomainCode(data, event) {


    var domainCode = $("#txtDomainCode").val();

    if (domainCode != "") {
        var dataObject = JSON.stringify({ 'domainCode': domainCode });


        $.ajax({
            url: '/Domain/GetDomainDetailsForCode',
            type: 'post',
            data: dataObject,
            contentType: 'application/json',
            success: function (data) {

                if (data == null || data.length == 0) {
                    $("#txtDomainName").val("");
                    $("#txtDomainDescription").val(0);
                    $("#chkIsDomainActive").attr('checked', true);
                    $("#btnSave").html('Save');
                    $("#txtDomainCode").parent().removeClass("form-group has-error");
                    $("#txtDomainName").parent().removeClass("form-group has-success");
                    $("#txtDomainName").parent().removeClass("form-group has-error");
                    $("#txtDomainDescription").parent().removeClass("form-group has-success");
                    $("#txtDomainDescription").parent().removeClass("form-group has-error");
                    $("#txtDomainDescription").val('');

                } else {
                    $("#txtDomainName").val(data[0].DomainName);
                    $("#txtDomainDescription").val(data[0].DomainDescription);
                    $("#btnSave").html('Update');
                    $("#txtDomainCode").parent().removeClass("form-group has-error");
                    $("#txtDomainName").parent().removeClass("form-group has-success");
                    $("#txtDomainName").parent().removeClass("form-group has-error");
                    $("#txtDomainDescription").parent().removeClass("form-group has-success");
                    $("#txtDomainDescription").parent().removeClass("form-group has-error");

                    if (data[0].IsActive == true) {
                        $("#chkIsDomainActive").prop('checked', true);
                    }
                    else {
                        $("#chkIsDomainActive").prop('checked', false);
                    }
                }
            },


        });
    }
    else {
        $("#txtDomainName").parent().addClass("form-group has-error");
        $("#txtDomainName").parent().removeClass("form-group has-success");
        $("#txtDomainName").parent().removeClass("form-group has-warning");
    }

}




function RemoveExistingType(domainCode) {
    for (var i = 0; i < updateStatArr.length; i++) {
        if (updateStatArr[i].DomainCode == domainCode) {
            updateStatArr.splice(i, 1);
            break;
        }
        else {
            continue;
        }
    }
}


function ShowDomainIndexPopUp() {

    LoadDomainsForIndexPopup();

    dialog = $("#domainPopUpModal").dialog({
        autoOpen: false,
        title: "Domain Index",
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


/***For domain code auto suggestion***/

function AutoCompleteForDomainCode() {

    var domainCode = $("#txtDomainCode").val();

    var dataObject = JSON.stringify({ 'domainCode': domainCode });

    $("#txtDomainCode").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "/Domain/GetDomainCodesForAutoComplete",
                data: dataObject,
                dataType: "json",
                success: function (data) {

                    response($.map(data, function (item) {

                        return {
                            label: item.DomainCode + "-" + item.DomainName,
                            value: item.DomainCode//,
                            //id: item.ProductID

                        }
                    }))
                },
                select: function (event, ui) {
                    $("#txtDomainCode").val(ui.item.value);

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

/***To save active status of domain****************/

function UpdateActiveStatus() {


    if (updateStatArr.length > 0) {
        var objParam = JSON.stringify({ 'domainObj': updateStatArr });

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
        url: '/Domain/UpdateStatusOfSelectedDomains',
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
                LoadDomainsForIndexPopup();
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

function ActiveAllDomains() {
    isActiveForAll = true;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all domains as active..?",
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
            url: '/Domain/ChangeStatusForAllDomains',
            type: 'post',
            data: objParam,
            contentType: 'application/json',
            success: function (inputParam) {

                swal({
                    title: "",
                    text: "All domains changed as active successfully! ",
                    type: "success",
                    timer: 3000,
                    showConfirmButton: false
                });

                isActiveForAll = false;
                LoadDomainsForIndexPopup();

            }

        });

    });

}

function InactiveAllDomains() {
    isActiveForAll = false;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all domains as inactive..?",
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
           url: '/Domain/ChangeStatusForAllDomains',
           type: 'post',
           data: objParam,
           contentType: 'application/json',
           success: function (inputParam) {

               swal({
                   title: "",
                   text: "All domains changed as inactive successfully! ",
                   type: "success",
                   timer: 3000,
                   showConfirmButton: false
               });

               isActiveForAll = false;
               LoadDomainsForIndexPopup();

           }

       });

   });
}

/*************************************************/


function SaveDomain() {
    var domainCode = $("#txtDomainCode").val();
    var domainName = $("#txtDomainName").val();
    var domainDescription = $("#txtDomainDescription").val();
    var isDomainActive = $("#chkIsDomainActive").prop('checked');


    if (domainCode.length > 0) {

        $("#txtDomainCode").parent().removeClass("form-group has-error");
        $("#txtDomainCode").parent().addClass("form-group has-success");

        if (domainName.length > 0) {
            $("#txtDomainName").parent().removeClass("form-group has-error");
            $("#txtDomainName").parent().addClass("form-group has-success");


            var domainObj = {};
            domainObj['DomainCode'] = domainCode;
            domainObj['DomainName'] = domainName;
            domainObj['DomainDescription'] = domainDescription.length > 0 ? domainDescription : "";
            domainObj['IsActive'] = isDomainActive;

            var dataObject = JSON.stringify({ 'domainObj': domainObj });

            $.ajax({
                url: '/Domain/AddOrUpdateDomain',
                type: 'post',
                data: dataObject,
                contentType: 'application/json',
                success: function (data) {
                    if (data) {
                        swal({
                            title: "",
                            text: "Domain saved successfully! ",
                            type: "success",
                            timer: 3000,
                            showConfirmButton: false
                        });

                        ResetControls();
                    }
                    else {
                        swal({
                            title: "",
                            text: "Oops..Unable to save the domain data",
                            type: "error",
                            timer: 3000,
                            showConfirmButton: false
                        });

                    }
                },


            });

        }
        else {
            $("#txtDomainName").parent().addClass("form-group has-error");
            $("#txtDomainName").parent().removeClass("form-group has-success");

        }
    }
    else {

        $("#txtDomainCode").parent().addClass("form-group has-error");
        $("#txtDomainCode").parent().removeClass("form-group has-success");
    }
}

function ResetControls() {
    $(':input').val('');

    $("#chkIsDomainActive").attr('checked', false);

    $("#txtDomainCode").parent().removeClass("form-group has-success");
    $("#txtDomainCode").parent().removeClass("form-group has-error");
    $("#txtDomainName").parent().removeClass("form-group has-success");
    $("#txtDomainName").parent().removeClass("form-group has-error");
    $("#txtDomainDescription").parent().removeClass("form-group has-success");
    $("#txtDomainDescription").parent().removeClass("form-group has-error");

}


function ResetPopUp() {
    isActiveForAll = false;
    updateStatArr = [];
    LoadDomainsForIndexPopup();

}





