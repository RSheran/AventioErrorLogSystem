var isActiveForAll = false; //variable to determine whether for active all or inactive all

$(document).ready(function () {
    
    $("#btnReset").click(ResetControls);
    $("#btnSave").click(SaveUserGroup);
    $("#btnSearchUserGroup").click(ShowUserGroupIndexPopUp);
    $("#txtUserGroupName").keyup(AutoCompleteForUserGroupName);
    $("#txtUserGroupName").blur(GetDataForUserGoupName);

    $("#btnResetPopup").click(ResetPopUp);
    $("#btnChangeStatus").click(UpdateActiveStatus);
    $("#btnActiveAll").click(ActiveAllUserGroups);
    $("#btnInactiveAll").click(InactiveAllUserGroups);
});

/**Data Loading Section*****/

/**For User Group selection PopUp***/

var updateStatArr = []; //To store status update values

function ShowUserGroupIndexPopUp() {

    LoadUserGroupsForIndexPopup();

    dialog = $("#userGroupPopUpModal").dialog({
        autoOpen: false,
        title: "User Group Index",
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

function LoadUserGroupsForIndexPopup() {

    var param = JSON.stringify({ 'isActive': parseInt(0) });
    $.ajax({
        url: '/UserGroup/LoadUserGroups',
        type: 'post',
        data: param,
        contentType: 'application/json',
        success: function (inputParam) {

            LoadUserGroupsToBootstrapGrid(inputParam);

        }

    });

}

function LoadUserGroupsToBootstrapGrid(userGroupList) {

    var table = $("#tblUserGroup tbody");


    table.empty();
    if (userGroupList != null && typeof (userGroupList) !== 'undefined' && userGroupList.length > 0) {

        $('.hidecol').show();

        $.each(userGroupList, function (idx, elem) {

            if (elem.IsActive == true) {

                table.append("<tr class='success'>" +
                    "<td>" + elem.UserGroupName +
                    "</td><td>" + elem.UserGroupDescription +
                     "</td><td class='hidetd'>" + elem.IsActive +
                    "</td><td><input type='checkbox' class='chkbox' checked='checked' value='' />" +
                     "</td></tr>");
            }
            else {
                table.append("<tr class='danger'>" +
                   "<td>" + elem.UserGroupName +
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

    $('#tblUserGroup tbody tr').on('click', function () {


       
        var userGroupName = $(this).closest("tr").find('td:eq(0)').text();
        var userGroupDescription = $(this).closest("tr").find('td:eq(1)').text();
        var isActive = $(this).closest("tr").find('td:eq(2)').text();


        var $chkObj = $(this).find(':checkbox');

        if (event.target.type != 'checkbox') {
            $("#txtUserGroupName").val(userGroupName);
            $("#txtUserGroupDescription").val(userGroupDescription);
       

            if (isActive == 'true') {
                $("#chkIsUserGroupActive").attr('checked', true);
            }
            else {
                $("#chkIsUserGroupActive").attr('checked', false);
            }

            swal({
                title: "",
                text: "User Group Name selected successfully! ",
                type: "success",
                timer: 1000,
                showConfirmButton: false
            });

            $("#txtUserGroupName").parent().addClass("form-group has-success");

            var dialog = $("#userGroupPopUpModal").dialog();
            dialog.dialog("close");

        }
        else {


            var $chkObj = $(this).find(':checkbox');

            if ($chkObj.prop('checked')) {


                isActive = true;

                var objArr = {};
                objArr["UserGroupName"] = userGroupName;
                objArr["IsActive"] = isActive;

                RemoveExistingElement(userGroupName);

                updateStatArr.push(objArr);


            }
            else {

                isActive = false;

                var objArr = {};
                objArr["UserGroupName"] = userGroupName;
                objArr["IsActive"] = isActive;

                RemoveExistingElement(userGroupName);

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

//Getting Details for domain code blur function
function GetDataForUserGoupName(data, event) {


    var userGroupName = $("#txtUserGroupName").val();
      

    if (userGroupName != "") {
        var dataObject = JSON.stringify({ 'userGroupName': userGroupName });


        $.ajax({
            url: '/UserGroup/GetUserGroupDetails',
            type: 'post',
            data: dataObject,
            contentType: 'application/json',
            success: function (data) {

                if (data == null || data.length == 0) {
                    $("#txtUserGroupDescription").val("");
                    $("#btnSave").html('Save');
                    $("#txtUserGroupName").parent().removeClass("form-group has-error");
                    $("#txtUserGroupName").parent().addClass("form-group has-success");
                    $("#txtUserGroupDescription").parent().removeClass("form-group has-success");
                    $("#txtUserGroupDescription").parent().removeClass("form-group has-error");
                    $("#txtUserGroupDescription").val('');
                    $("#chkIsUserGroupActive").attr('checked', true);

                } else {
                    $("#txtUserGroupDescription").val(data[0].UserGroupDescription);
                    $("#btnSave").html('Update');
                    $("#txtUserGroupName").parent().removeClass("form-group has-error");
                    $("#txtUserGroupName").parent().addClass("form-group has-success");
                    $("#txtUserGroupDescription").parent().removeClass("form-group has-error");
                    $("#txtUserGroupDescription").parent().removeClass("form-group has-success");
                   

                    if (data[0].IsActive == true) {
                        $("#chkIsUserGroupActive").prop('checked', true);
                    }
                    else {
                        $("#chkIsUserGroupActive").prop('checked', false);
                    }
                }
            },


        });
    }
    else {
        $("#txtUserGroupName").parent().addClass("form-group has-error");
        $("#txtUserGroupName").parent().removeClass("form-group has-success");
        $("#txtUserGroupName").parent().removeClass("form-group has-warning");
    }

}


function RemoveExistingElement(userGroupName) {
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

/*********************************************************/



/***For domain code auto suggestion***/

function AutoCompleteForUserGroupName() {

    var userGroupName = $("#txtUserGroupName").val();

    var dataObject = JSON.stringify({ 'userGroupName': userGroupName });

    $("#txtUserGroupName").autocomplete({
        source: function (request, response) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "/UserGroup/GetUserGroupNamesForAutoComplete",
                data: dataObject,
                dataType: "json",
                success: function (data) {

                    response($.map(data, function (item) {

                        return {
                            label: item.UserGroupName,
                            value: item.UserGroupName//,
                            //id: item.ProductID

                        }
                    }))
                },
                select: function (event, ui) {
                    $("#txtUserGroupName").val(ui.item.value);

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


/********************************************************/


/***Data Saving Section*********/



/***To save active status of domain****************/

function UpdateActiveStatus() {


    if (updateStatArr.length > 0) {
        var objParam = JSON.stringify({ 'userGroupObj': updateStatArr });

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
        url: '/UserGroup/UpdateStatusOfSelectedGroups',
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
                LoadUserGroupsForIndexPopup();
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

function ActiveAllUserGroups() {
    isActiveForAll = true;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all user groups as active..?",
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
            url: '/UserGroup/ChangeStatusForAllGroups',
            type: 'post',
            data: objParam,
            contentType: 'application/json',
            success: function (inputParam) {

                swal({
                    title: "",
                    text: "All user groups changed as active successfully! ",
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

function InactiveAllUserGroups() {
    isActiveForAll = false;

    var objParam = JSON.stringify({ 'isActiveForAll': isActiveForAll });

    swal({
        title: "Are you sure to change all user groups as inactive..?",
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
           url: '/UserGroup/ChangeStatusForAllGroups',
           type: 'post',
           data: objParam,
           contentType: 'application/json',
           success: function (inputParam) {

               swal({
                   title: "",
                   text: "All user groups changed as inactive successfully! ",
                   type: "success",
                   timer: 3000,
                   showConfirmButton: false
               });

               isActiveForAll = false;
               LoadUserGroupsForIndexPopup();

           }

       });

   });
}

/*************************************************/

//Save Main Details
function SaveUserGroup() {
    var userGroupName = $("#txtUserGroupName").val();
    var userGroupDescription = $("#txtUserGroupDescription").val();
    var isUserGroupActive = $("#chkIsUserGroupActive").prop('checked');


    if (userGroupName.length > 0)
    {

            $("#txtUserGroupName").parent().removeClass("form-group has-error");
            $("#txtUserGroupName").parent().addClass("form-group has-success");
        

            var userGroupObj = {};
            userGroupObj['UserGroupName'] = userGroupName;
            userGroupObj['UserGroupDescription'] = userGroupDescription.length > 0 ? userGroupDescription : "";
            userGroupObj['IsActive'] = isUserGroupActive;

            var dataObject = JSON.stringify({ 'systemUserGroupObj': userGroupObj });

            $.ajax({
                url: '/UserGroup/AddOrUpdateUserGroup',
                type: 'post',
                data: dataObject,
                contentType: 'application/json',
                success: function (data) {
                    if (data) {
                        swal({
                            title: "",
                            text: "User Group saved successfully! ",
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

        $("#txtUserGroupName").parent().addClass("form-group has-error");
        $("#txtUserGroupName").parent().removeClass("form-group has-success");
    }
}

function ResetControls() {
    $(':input').val('');

    $("#chkIsUserGroupActive").attr('checked', false);

    $("#txtUserGroupName").parent().removeClass("form-group has-success");
    $("#txtUserGroupName").parent().removeClass("form-group has-error");
    $("#txtUserGroupDescription").parent().removeClass("form-group has-success");
    $("#txtUserGroupDescription").parent().removeClass("form-group has-error");

}

function ResetPopUp()
{
    isActiveForAll = false;
    updateStatArr = [];
    LoadUserGroupsForIndexPopup();
    
}

/********************************/