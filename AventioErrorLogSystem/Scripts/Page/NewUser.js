
$(document).ready(function () {
    
    LoadUserGroups();
    AutoSelectCurrentUserType();
    $("#errorStrip").hide();
    $("#btnSaveUser").click(AddNewUserDetails);
    $("#btnResetUser").click(ResetNewUserDetails);
    $("#txtUsername").blur(ValidateUsername);
    $("#txtUserEmail").blur(ValidateUserEmail);
    $("#drpUserGroup").change(ValidateUserGroup);
    $("#txtFullName").blur(ValidateFullName);
    $("#txtPassword").blur(ValidatePassword);
    $("#txtConPassword").blur(ValidateConPassword);

    GetCurrentUser();

});

/****Data Loading Section*****/

var userGroupArr = []; //Array to get user types at the tie of binding
function LoadUserGroups() {

    var param = JSON.stringify({ 'isActive': parseInt(1) });
    $.ajax({
        url: '/UserGroup/LoadUserGroups',
        type: 'post',
        data: param,
        async:false,
        contentType: 'application/json',
        success: function (inputParam) {

            BindUserGroupList(inputParam);

        }

    });

}

function BindUserGroupList(userGroupList)
{
    $("#drpUserGroup").children().empty();
    $("#drpUserGroup").empty().append($("<option>").val(0).html(""));

    if (userGroupList != null) {

        userGroupArr = userGroupList;


        $.each(userGroupList, function (idx, elem) {

            var val = elem.UserGroupId;
            var text = elem.UserGroupName;
            $("#drpUserGroup").append($("<option>").val(val).html(text));

        });



    }


   
   
}

function AutoSelectCurrentUserType()
{
    var hdnUserType = $("#hdnUserType").val();

    for (var i = 0; i < userGroupArr.length; i++)
    {
        if (userGroupArr[i].UserGroupName.toLowerCase() == hdnUserType.toLowerCase()) {
            $("#drpUserGroup").val(userGroupArr[i].UserGroupId);
            break;
        }
        else {
            continue;
        }
    }
}

var currentUser; //Variable to get current user
function GetCurrentUser()
{
   
    $.ajax({
        url: '/NewUser/GetCurrentUser',
        type: 'post',
        async: false,
        contentType: 'application/json',
        success: function (inputParam) {
           
            currentUser = inputParam;

        }

    });

}

/****************************/

/****Validation Section********/


function ValidateUsername() {
    var x = $("#txtUsername").val();
    var success = true;

    if (x.length > 0) {

        var dataObject = JSON.stringify({ 'chkUserName': x });
        $.ajax({
            url: '/NewUser/IsUsernameExisting',
            type: 'post',
            data: dataObject,
            async: false,
            contentType: 'application/json',
            success: function (data) {
                if (data) {
                    $("#txtUsername").parent().addClass("form-group has-error");
                    $("#txtUsername").parent().removeClass("form-group has-success");

                    $("#errorStrip").show();
                    $("#userAlert").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp; Oops..This username is already existing...');
                    $("#userAlert").show().delay(3000).fadeOut();
                    success = false;
                }
                else {
                    $("#txtUsername").parent().removeClass("form-group has-error");
                    $("#txtUsername").parent().addClass("form-group has-success");

                    $("#errorStrip").hide();
                    success = true;

                }

            },


        });


        return success;


    }
    else {
        $("#txtUsername").parent().addClass("form-group has-error");
        $("#txtUsername").parent().removeClass("form-group has-success");
        $("#spanUName").html('');
        return false;
    }
}

function ValidateUserGroup()
{
    var x = $("#drpUserGroup").val();
   
    if (x > 0) {
        $("#drpUserGroup").parent().removeClass("form-group has-error");
        $("#drpUserGroup").parent().addClass("form-group has-success");
        $("#spanUserGroup").html('');

        return true;
        
    }
    else {
        $("#drpUserGroup").parent().addClass("form-group has-error");
        $("#drpUserGroup").parent().removeClass("form-group has-success");
        $("#spanUserGroup").html('');
        return false;
    }
}


function ValidateFullName() {
    var x = $("#txtFullName").val();

    if (x.length > 0) {
        $("#txtFullName").parent().removeClass("form-group has-error");
        $("#txtFullName").parent().addClass("form-group has-success");
        return true;
    }
    else {
        $("#txtFullName").parent().addClass("form-group has-error");
        $("#txtFullName").parent().removeClass("form-group has-success");
        return false;
    }

}

//Regular expression email validation
function IsValEmail(sEmail) {
    var filter = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
    if (filter.test(sEmail)) {
        return true;
    }
    else {
        return false;
    }
}

function ValidateUserEmail() {

    var userMail = $("#txtUserEmail").val();
    var success = true;

    if (userMail.length > 0) {
        if (IsValEmail(userMail) == true) {

            var dataObject = JSON.stringify({ 'chkUserEmail': userMail });
            $.ajax({
                url: '/NewUser/IsUserEmailExisting',
                type: 'post',
                data: dataObject,
                async: false,
                contentType: 'application/json',
                success: function (data) {
                    if (data) {
                        $("#txtUserEmail").parent().addClass("form-group has-error");
                        $("#txtUserEmail").parent().removeClass("form-group has-success");

                        $("#errorStrip").show();
                        $("#userAlert").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp; Oops..This email is already existing...');
                        $("#userAlert").show().delay(3000).fadeOut();
                        success = false;
                    }
                    else {
                        $("#txtUserEmail").parent().removeClass("form-group has-error");
                        $("#txtUserEmail").parent().addClass("form-group has-success");

                        $("#errorStrip").hide();
                        success = true;

                    }

                },


            });


            return success;

        }
        else {
            $("#txtUserEmail").parent().addClass("form-group has-error");
            $("#txtUserEmail").parent().removeClass("form-group has-success");

            $("#errorStrip").show();
            $("#userAlert").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp;Invalid E-mail');
            $("#userAlert").show().delay(3000).fadeOut();
        }




    }
    else {
        $("#txtUserEmail").parent().addClass("form-group has-error");
        $("#txtUserEmail").parent().removeClass("form-group has-success");
       
        return false;
    }
}

function ValidatePassword(data, event)
{
    var x = $("#txtPassword").val();

    if (x.length > 0) {
        $("#txtPassword").parent().removeClass("form-group has-error");
        $("#txtPassword").parent().addClass("form-group has-success");
        return true;
    }
    else {
        $("#txtPassword").parent().addClass("form-group has-error");
        $("#txtPassword").parent().removeClass("form-group has-success");
        return false;
    }
}

function ValidateConPassword()
{
    var x = $("#txtPassword").val();
    var y = $("#txtConPassword").val();

    if (y.length > 0) {
        if (x == y) {
            $("#txtConPassword").parent().removeClass("form-group has-error");
            $("#txtConPassword").parent().addClass("form-group has-success");
            $("#spanConPassword").html('');
            return true;
        }
        else {
            $("#txtConPassword").parent().addClass("form-group has-error");
            $("#txtConPassword").parent().removeClass("form-group has-success");

            $("#errorStrip").show();
            $("#userAlert").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp;Passwords do not match');
            $("#userAlert").show().delay(3000).fadeOut();
         
            return false;
        }
    }
    else {
        $("#txtConPassword").parent().addClass("form-group has-error");
        $("#txtConPassword").parent().removeClass("form-group has-success");
        $("#spanConPassword").html('');
        return false;
    }
}

function AddNewUserDetails()
{
    if (ValidateUsername() && ValidateUserGroup() && ValidateFullName() &&
        ValidateUserEmail() && ValidatePassword() && ValidateConPassword())
    {
        var userName = $("#txtUsername").val();
        var usergroupId = $("#drpUserGroup option:selected").val();
        var fullName = $("#txtFullName").val();
        var callingName = $("#txtCallingName").val();
        var userMail = $("#txtUserEmail").val();
        var password = $("#txtPassword").val();
        var isActive = $("#chkIsUserActive").prop('checked');

        var systemUser = {};

        systemUser["UserGroupId"] = usergroupId;
        systemUser["Username"] = userName;
        systemUser["Password"] = password;
        systemUser["CallingName"] = callingName;
        systemUser["FullName"] = fullName;
        systemUser["Email"] = userMail;
        systemUser["ProfilePicURL"] = "";
        systemUser["IsActive"] = true;
       
        var dataObject = JSON.stringify({ 'systemUser': systemUser });

        $.ajax({
            url: '/NewUser/AddOrUpdateSystemUser',
            type: 'post',
            data: dataObject,
            async: false,
            contentType: 'application/json',
            success: function (data) {
                if (data) {

                    var resultText = currentUser==""?"You have been added as a user successfully!..Log in with your credentials..":"The user has been created successfully.";
                    swal({

                        title: "",
                        text: resultText,
                        type: "success",
                        showCancelButton: false,
                        //confirmButtonColor: "#DD6B55",
                        confirmButtonText: "OK",
                        closeOnConfirm: true
                    }, function ()
                    {
                        ResetNewUserDetails();

                        if (currentUser == "")
                        {
                            window.location = "/Login/Index";
                        }
                       
                    });

                    //var dialog = $("#newUserModal").dialog();
                    //dialog.dialog("close");


                }
                else {
                    swal({
                        title: "",
                        text: "Oops..An error occured while saving...!",
                        type: "error",
                        timer: 3000,
                        showConfirmButton: false
                    });

                }

            },


        });



    }

}


function ResetNewUserDetails()
{
    $(':input').val('');
    $("#drpUserGroup").val(0);
        
    $("#txtUsername").parent().removeClass("form-group has-error");
    $("#txtUsername").parent().removeClass("form-group has-success");

    $("#drpUserGroup").parent().removeClass("form-group has-error");
    $("#drpUserGroup").parent().removeClass("form-group has-success");

    $("#txtFullName").parent().removeClass("form-group has-error");
    $("#txtFullName").parent().removeClass("form-group has-success");

    $("#txtCallingName").parent().removeClass("form-group has-error");
    $("#txtCallingName").parent().removeClass("form-group has-success");

    $("#txtUserEmail").parent().removeClass("form-group has-error");
    $("#txtUserEmail").parent().removeClass("form-group has-success");

    $("#txtPassword").parent().removeClass("form-group has-error");
    $("#txtPassword").parent().removeClass("form-group has-success");
}


/**************************************/