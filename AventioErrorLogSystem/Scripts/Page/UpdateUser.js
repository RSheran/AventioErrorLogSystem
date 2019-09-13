/**AWS S3 bucket coniguration variables***/
var bucket; //AWS s3 object
var bucketName;
var bucketRegion;
var bucketStartURL;
/*******************************/

$(document).ready(function () {

    LoadUserGroups();
    InitialSettings();
    InitAWSConfigurations();
    GetLoggedInUserDetails();
    $("#txtUserEmail").blur(ValidateUserEmail);
    $("#txtFullName").blur(ValidateFullName);
    $("#txtOldPassword").blur(ValidateOldPassword);
    $("#txtPassword").blur(ValidatePassword);
    $("#txtConPassword").blur(ValidateConPassword);

    $("#btnEditMode").click(EnableControls);

    $("#btnUpdateUser").click(UpdateUserDetails);
    $("#btnUpdatePassword").click(UpdatePassword);

    $("#btnCancelUpload").click(CancelUpload);
    $("#btnUploadPhoto").click(UploadPhoto);
    $("#btnSaveProfilePic").click(SavePhoto);
    GetCurrentUserDetails();

});

//To initialize AWS Bucket Configurations
function InitAWSConfigurations()
{
    bucketName = $("#hdnBucketName").val();

    bucketStartURL = $("#hdnBucketStartURL").val();

    AWS.config.update({
        accessKeyId: $("#hdnAWSAccessKey").val(),
        secretAccessKey: $("#hdnAWSSecretKey").val(),
        region: $("#hdnBucketRegion").val()

    });


    bucket = new AWS.S3({
        params: { Bucket: bucketName }
    });
    // s3 = new AWS.S3();

    /****Image upload path initializations****/

    userProfileImagePath = $("#hdnUserProfileImagePath").val();
    otherUserProfileFilePath = $("#hdnOtherUserProfileFilePath").val();


    /***************************************/

}

function LoadUserGroups()
{

    var param = JSON.stringify({ 'isActive': parseInt(1) });
    $.ajax({
        url: '/UserGroup/LoadUserGroups',
        type: 'post',
        data: param,
        async: false,
        contentType: 'application/json',
        success: function (inputParam) {

            BindUserGroupList(inputParam);

        }

    });

}

function BindUserGroupList(userGroupList) {
    $("#drpUserType").children().empty();
    $("#drpUserType").empty().append($("<option>").val(0).html(""));

    if (userGroupList != null) {

        userGroupArr = userGroupList;


        $.each(userGroupList, function (idx, elem) {

            var val = elem.UserGroupId;
            var text = elem.UserGroupName;
            $("#drpUserType").append($("<option>").val(val).html(text));

        });


       }

  }



function InitialSettings () {
    HideFileSelector();
    $("#errorStrip").hide();
    $("#errorStripPwd").hide();

}

function ShowFileSelector()
{
    $("#imgUploader").show();
    $("#uploadButton").hide();
}

function HideFileSelector()
{
    $("#imgUploader").hide();
    $("#uploadButton").show();
}

function DisableControls()
{

    $("#txtFullName").attr("disabled", true);
    $("#txtCallingName").attr("disabled", true);
    $("#txtPassword").attr("disabled", true);
    $("#txtConPassword").attr("disabled", true);
    $("#btnUpdateUser").attr("disabled", true);
    $("#txtUserEmail").attr("disabled", true);
    $("#chkIsUserActive").attr("disabled", true);
}

function EnableControls()
{
    $("#txtFullName").attr("disabled", false);
    $("#txtCallingName").attr("disabled", false);
    $("#txtPassword").attr("disabled", false);
    $("#txtConPassword").attr("disabled", false);
    $("#btnUpdateUser").attr("disabled", false);
    $("#txtUserEmail").attr("disabled", false);
    $("#chkIsUserActive").attr("disabled", false);
}


function GetCurrentUserDetails() {

    $.ajax({
        // data: dataObject,
        url: '/UpdateUser/GetUserDetails',
        type: 'post',
        contentType: 'application/json',
        success: function (inputParam) {
          
            $("#userSalutation").html('Welcome, ' + inputParam[0].Username);

            //picture loading
            if (inputParam[0].ProfilePicURL != null && inputParam[0].ProfilePicURL != "") {
                $("#userImageMenu").attr("src", inputParam[0].ProfilePicURL);



            } else {
                $("#userImageMenu").attr("src", "../../Images/BlankProfilePic.png");

            }

        }


    });
}

function GetLoggedInUserDetails()
{

    $.ajax({
        // data: dataObject,
        url: '/UpdateUser/GetRedirectedUserDetails',
        type: 'post',
        contentType: 'application/json',
        async:false,
        success: function (inputParam) {

            $("#txtUsername").val(inputParam[0].Username);
            $("#logUserEditName").html(inputParam[0].FullName);
            $("#drpUserType").val(inputParam[0].UserGroupId);
            $("#txtFullName").val(inputParam[0].FullName);
            $("#txtCallingName").val(inputParam[0].CallingName);
            $("#txtUserEmail").val(inputParam[0].Email);

            //User active status
            $("#chkIsUserActive").attr('checked', inputParam[0].IsActive);
            

            //picture loading
            if (inputParam[0].ProfilePicURL != null && inputParam[0].ProfilePicURL != "") {
                $("#userImageEdit").attr("src", inputParam[0].ProfilePicURL);



            } else {
                $("#userImageEdit").attr("src", "../../Images/BlankProfilePic.png");

            }

        }


    });
}

function ValidateFullName()
{
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
function IsValEmail(sEmail)
{
    var filter = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
    if (filter.test(sEmail)) {
        return true;
    }
    else {
        return false;
    }
}

function ValidateUserEmail()
{

    var userMail = $("#txtUserEmail").val();
    var success = true;

    if (userMail.length > 0) {
        if (IsValEmail(userMail) == true) {

            $("#txtUserEmail").parent().removeClass("form-group has-error");
            $("#txtUserEmail").parent().addClass("form-group has-success");

            $("#errorStrip").hide();
            return true;
            //var dataObject = JSON.stringify({ 'chkUserEmail': userMail });
            //$.ajax({
            //    url: '/NewUser/IsUserEmailExisting',
            //    type: 'post',
            //    data: dataObject,
            //    async: false,
            //    contentType: 'application/json',
            //    success: function (data) {
            //        if (data) {
            //            $("#txtUserEmail").parent().addClass("form-group has-error");
            //            $("#txtUserEmail").parent().removeClass("form-group has-success");

            //            $("#errorStrip").show();
            //            $("#userAlert").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp; Oops..This email is already existing...');
            //            $("#userAlert").show().delay(3000).fadeOut();
            //            success = false;
            //        }
            //        else {
            //            $("#txtUserEmail").parent().removeClass("form-group has-error");
            //            $("#txtUserEmail").parent().addClass("form-group has-success");

            //            $("#errorStrip").hide();
            //            success = true;

            //        }

            //    },


            //});


            //return success;

        }
        else {
            $("#txtUserEmail").parent().addClass("form-group has-error");
            $("#txtUserEmail").parent().removeClass("form-group has-success");

            $("#errorStrip").show();
            $("#userAlert").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp;Invalid E-mail');
            $("#userAlert").show().delay(3000).fadeOut();

            return false;
        }




    }
    else {
        $("#txtUserEmail").parent().addClass("form-group has-error");
        $("#txtUserEmail").parent().removeClass("form-group has-success");
        $("#spanEmail").html('');
        return false;
    }
}

function ValidateOldPassword() {
    var chkPassword = $("#txtOldPassword").val();
    var success = true;

    if (chkPassword.length > 0) {

        var dataObject = JSON.stringify({ 'chkPassword': chkPassword });
        $.ajax({
            url: '/UpdateUser/IsOldPwdValid',
            type: 'post',
            data: dataObject,
            async: false,
            contentType: 'application/json',
            success: function (data) {
                if (data) {
                    $("#txtOldPassword").parent().removeClass("form-group has-error");
                    $("#txtOldPassword").parent().addClass("form-group has-success");

                    $("#errorStripPwd").hide();

                    success = true;
                }
                else {
                    $("#txtOldPassword").parent().addClass("form-group has-error");
                    $("#txtOldPassword").parent().removeClass("form-group has-success");

                    $("#errorStripPwd").show();
                    $("#userAlertPwd").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp;Old password is incorrect.');
                    $("#userAlertPwd").show().delay(3000).fadeOut();
                    success = false;

                }

            },


        });


        return success;


    }
    else {
        $("#txtOldPassword").parent().addClass("form-group has-error");
        $("#txtOldPassword").parent().removeClass("form-group has-success");
        $("#spanOldPassword").html('');
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


function ValidateConPassword(data, event)
{
    var x = $("#txtPassword").val();
    var y = $("#txtConPassword").val();

    if (y.length > 0) {
        $("#errorStripPwd").hide();
        if (x == y) {
            $("#errorStripPwd").hide();
            $("#txtConPassword").parent().removeClass("form-group has-error");
            $("#txtConPassword").parent().addClass("form-group has-success");

            $("#txtPassword").parent().removeClass("form-group has-error");
            $("#txtPassword").parent().addClass("form-group has-success");

            $("#spanConPassword").html('');
            return true;
        }
        else {
            $("#txtConPassword").parent().addClass("form-group has-error");
            $("#txtConPassword").parent().removeClass("form-group has-success");

            $("#txtPassword").parent().addClass("form-group has-error");
            $("#txtPassword").parent().removeClass("form-group has-success");

            $("#errorStripPwd").show();
            $("#userAlertPwd").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp;Password and Confirm Password do not match.');
            $("#userAlertPwd").show().delay(3000).fadeOut();

            return false;
        }
    }
    else {

        $("#txtConPassword").parent().addClass("form-group has-error");
        $("#txtConPassword").parent().removeClass("form-group has-success");
        $("#Span3").html('');

        return false;
    }
}

function UpdateUserDetails(data, event) {

    var userName = $("#txtUsername").val();
    var fullName = $("#txtFullName").val();
    var callingName = $("#txtCallingName").val();
    var userMail = $("#txtUserEmail").val();
    var password = $("#txtPassword").val();
    var userTypeId = $("#drpUserType option:selected").val();
    var isActive = $("#chkIsUserActive").prop('checked');

   

    if (ValidateFullName() && ValidateUserEmail()) {
        var systemUser = {};

        systemUser["Username"] = userName;
        systemUser["CallingName"] = callingName;
        systemUser["UserGroupId"] = userTypeId;
        systemUser["FullName"] = fullName;
        systemUser["EMail"] = userMail;
        systemUser["IsActive"] = isActive;

        var dataObject = JSON.stringify({ 'systemUser': systemUser });
        
        $.ajax({
            url: '/UpdateUser/UpdateSystemUser',
            type: 'post',
            data: dataObject,
            async: false,
            contentType: 'application/json',
            success: function (data) {
                if (data) {
                    swal({

                        title: "",
                        text: "Updated Succesfully.",
                        type: "success",
                        showCancelButton: false,
                        //confirmButtonColor: "#DD6B55",
                        confirmButtonText: "OK",
                        closeOnConfirm: true
                    }, function () {


                    });

                    //var dialog = $("#newUserModal").dialog();
                    //dialog.dialog("close");


                }
                else {
                    swal({
                        title: "",
                        text: "Oops..An error occured while updating...!",
                        type: "error",
                        timer: 3000,
                        showConfirmButton: false
                    });

                }

            },


        });
    }

}

function UpdatePassword() {

    var newPassword = $("#txtPassword").val();

    if (ValidateOldPassword() && ValidatePassword() && ValidateConPassword()) {

        var dataObject = JSON.stringify({ 'newPassword': newPassword });

        $.ajax({
            url: '/UpdateUser/UpdatePassword',
            type: 'post',
            data: dataObject,
            async: false,
            contentType: 'application/json',
            success: function (data) {
                if (data) {
                    swal({

                        title: "",
                        text: "Password updated.",
                        type: "success",
                        showCancelButton: false,
                        //confirmButtonColor: "#DD6B55",
                        confirmButtonText: "OK",
                        closeOnConfirm: true
                    }, function () {
                        // self.resetNewUserDetails();
                        $("#txtOldPassword").val("");
                        $("#txtPassword").val("");
                        $("#txtConPassword").val("");

                        $("#txtOldPassword").parent().removeClass("form-group has-error");
                        $("#txtOldPassword").parent().removeClass("form-group has-success");

                        $("#txtPassword").parent().removeClass("form-group has-error");
                        $("#txtPassword").parent().removeClass("form-group has-success");

                        $("#txtConPassword").parent().removeClass("form-group has-error");
                        $("#txtConPassword").parent().removeClass("form-group has-success");

                    });

                    //var dialog = $("#newUserModal").dialog();
                    //dialog.dialog("close");


                }
                else {
                    swal({
                        title: "",
                        text: "Oops..An error occured while updating password...!",
                        type: "error",
                        timer: 3000,
                        showConfirmButton: false
                    });

                }

            },


        });
    }
}
/****Section to handle profile picture uploading and saving in AWS Bucket****/
function CancelUpload()
{
    HideFileSelector();

}

function UploadPhoto()
{
   ShowFileSelector();
}

function UploadImageToS3Bucket(path, username, uploadFileArr) {

    var status = true;
    var file;
    var fileName;
    var albumPhotosKey, imageType, imageKey, photoKey;

    if (username.length > 0) {

        if (uploadFileArr.length > 0) {

            for (var i = 0; i < uploadFileArr.length; i++) {

                //If it's an already available file,then savning it is not required.
                if (typeof (uploadFileArr[i].File) === 'undefined' || uploadFileArr[i].File == null) {

                    continue;
                }
                else {

                    file = uploadFileArr[i].File;
                    fileName = uploadFileArr[i].MediaName;//file.name;
                    imageType = GetFileExtension(fileName);
                    imageType = imageType.toLowerCase();
                    imageKey = path + '/'; //encodeURIComponent(folderName) + '/';

                    photoKey = imageKey + fileName;
                    bucket.upload({
                        Key: photoKey,
                        Body: file,
                        ContentType: 'image/' + imageType,
                        ACL: 'public-read'
                    }, function (err, data) {
                        if (err) {
                            DisplayErrorMsg('Error occured when saving images.' + err);
                            return false;
                            //return alert('There was an error uploading your photo: ', err.message);
                        }
                        // DisplaySuccessMsg('Product details saved successfully.');
                        return true;
                        //viewAlbum(albumName);
                    });
                }

            }
        }



        return true;

    }
    else {
        DisplayErrorMsg('Error Code cannot be empty.');
        return false;
    }

    // DisplaySuccessMsg('Product details saved successfully.');

    return true;


}


function SavePhoto()
{
    var fileURL = "";
    var currentUserName = $("#hdnCurrentUserName").val();
    var uploadfiles = $("#imgNew").get(0);
    var uploadedfiles = uploadfiles.files;
    var uploadStatus = true;
    if (uploadedfiles.length > 0)
    {
        swal({
            title: "Are you sure to update your profile picture?",
            //text: "You will not be able to recover this imaginary file!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#01DF74",
            confirmButtonText: "Yes, update..",
            showLoaderOnConfirm: true,
            closeOnConfirm: false,
            height: "100px",
            width: "100px"
        },
   function () {
       
       var file = uploadedfiles[0];
       var fileName =currentUserName+"_"+uploadedfiles[0].name;//file.name;
       var imageType = GetFileExtension(fileName);
       imageType = imageType.toLowerCase();
       var imageKey = userProfileImagePath; //encodeURIComponent(folderName) + '/';

       fileURL = bucketStartURL + bucketName + "/" + userProfileImagePath + fileName;

       photoKey = imageKey + fileName;
       
      
       bucket.upload({
           Key: photoKey,
           Body: file,
           ContentType: 'image/' + imageType,
           ACL: 'public-read'
       }, function (err, data) {
           if (err) {
               //DisplayErrorMsg('Error occured when saving the image.' + err);
               uploadStatus = false;
               //return alert('There was an error uploading your photo: ', err.message);
           }
           // DisplaySuccessMsg('Product details saved successfully.');
           uploadStatus=true;
           //viewAlbum(albumName);
       });

     
       if (uploadStatus == true) {

           var dataObject = JSON.stringify({ 'imageURL': fileURL });

           $.ajax({
               url: '/UpdateUser/SaveImageWithTheUserID',
               type: 'post',
               data: dataObject,
               async: false,
               contentType: 'application/json',
               success: function (data) {
                   if (data) {
                   
                       HideFileSelector();

                       swal({
                           title: "",
                           text: "User Profile picture updated successfully! ",
                           type: "success",
                           timer: 2000,
                           showConfirmButton: false
                       });

                       uploadedfiles = null;

                       $("#imgNew").val("");

                       GetLoggedInUserDetails();

                   }
                   else {
                       swal({
                           title: "",
                           text: "Oops..Unable to update the Profile picture.. ",
                           type: "error",
                           timer: 2000,
                           showConfirmButton: false
                       });

                   }

               },


           });

     
       }
       else
       {
           swal({
               title: "",
               text: "Oops..Unable to update the Profile picture.. ",
               type: "error",
               timer: 2000,
               showConfirmButton: false
           });
       }
      
             

       });

        // event.preventDefault();
    }
    else {
        swal({
            title: "",
            text: "Oops..No picture selected to update.. ",
            type: "error",
            timer: 2000,
            showConfirmButton: false
        });
    }
}

function GetFileExtension(filename) {
    var parts = filename.split('.');
    return parts[parts.length - 1];
}

/************************************************************************/

function ResetControls()
{
   GetLoggedInUserDetails();
   DisableControls();

}