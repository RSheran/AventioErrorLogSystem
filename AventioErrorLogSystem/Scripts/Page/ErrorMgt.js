var isErrorOnly = true; isErrorWithSolution = false, isSolutionAddForError = false;

/**AWS S3 bucket coniguration variables***/
var bucket; //AWS s3 object
var bucketName;
var bucketRegion;
var bucketStartURL;
/*******************************/

/*Image upload path variables*****/
var errorImagePath, otherErrorFilePath, solutionImagePath, otherSolutionFilePath;



/*******************************/

$(document).ready(function () {

    InitAWSConfigurations();
    $("#drpDomain").select2();
    $("#drpField").select2();
    $("#drpClient").select2();

    GetNewErrorCode();
    GetNewSolutionCode();
    LoadDomainsForDropDown();
    LoadFieldsForDropDown();
    LoadClientsForDropDown();


    ChangeErrorAdditionMode();

    $("#btnErrorOnly").click(function(){
        isErrorOnly = true;
        isErrorWithSolution = false;
        isSolutionAddForError = false;
        GetNewErrorCode();
        GetNewSolutionCode();
        ChangeErrorAdditionMode();
      
    });

    $("#btnErrorWithSol").click(function(){
        
        GetNewErrorCode();
        GetNewSolutionCode();
        isErrorOnly = false;
        isErrorWithSolution = true;
        isSolutionAddForError = false;
        ChangeErrorAdditionMode();
    
    });

    $("#btnSolution").click(function(){
        isErrorOnly = false;
        isErrorWithSolution = false;
        isSolutionAddForError = true;
        ChangeErrorAdditionMode();
        $("#txtErrorCode").val('');
    });

    $("#dvShowlabelMeanings").click(ShowLabelMeaningPopUp);
    $("#fileUpErrorAttachment").change(ErrorImageFileUploading);
    $("#fileUpSolutionAttachment").change(SolutionImageFileUploading);
    $("#btnSearchError").click(ShowSearchErrorPopUp);
    $("#btnSave").click(SaveMainErrorDetails);
    HideErrorMsg();
    //$("#btnReset").click(ResetControls);
    //$("#btnSave").click(SaveDomain);
    //$("#dvShowDomains").click(ShowDomainIndexPopUp);
    //$("#txtDomainCode").keyup(AutoCompleteForDomainCode);
    //$("#txtDomainCode").blur(GetDataForDomainCode);
   // GetAllErrors();
    $("#txtSearchError").keyup(FilterErrorListBySearchQuery);

    SetErrorForSolution();


});

function DisplayErrorMsg(message) {
    //HideSuccessMsg();
    $("#ErrorMessage").text(message);
    $("#showError").css('display', 'block');
    $(window).scrollTop($('#showError').offset().top);
}

function HideErrorMsg() {
    $("#ErrorMessage").text('');
    $("#showError").css('display', 'none');
}


/**Image upload section***/

//Arrays to get uploaded image/video files
var errorImageFileArr = [], solutionImageFileArr=[],videoFileArr = [];


function ErrorImageFileUploading() {
    var uploadfiles = $("#fileUpErrorAttachment").get(0);
    var uploadedfiles = uploadfiles.files;
    var errorCode = $("#txtErrorCode").val();

    if (errorCode.length == 0) {
        DisplayErrorMsg('Error Code must be available to upload an image.');
    }
    else {

        if (uploadedfiles.length == 0) {
            //imageFileArr = null;
            //imageFileArr = [];

        } else {

            for (var i = 0; i < uploadedfiles.length; i++) {
                if (CheckErrorImageFileExisting(uploadedfiles[i].name)) {


                    swal({
                        title: "",
                        text: "The File " + uploadedfiles[i].name + " " + "has already been uploaded",
                        type: "error",
                        timer: 3000,
                        showConfirmButton: false
                    });

                }
                else {

                    for (var i = 0; i < uploadedfiles.length; i++) {
                        var file = uploadedfiles[i];
                        // var fileName = file.name;
                        var mimeType = file['type'];

                        //Choose the mediaURL and mediaType based on the file type
                        var mediaURL, mediaType,mediaPath;
                        if (mimeType.split('/')[0] == 'image')
                        {
                            mediaURL = bucketStartURL + bucketName + "/"+errorImagePath + errorCode + "_" + "Error" + "_" + file.name;
                            mediaType = 'Image';
                            mediaPath = errorImagePath;
                        }
                        else
                        {
                            mediaURL = bucketStartURL + bucketName + "/"+otherErrorFilePath + errorCode + "_" + "Error" + "_" + file.name;
                            mediaType = 'Other';
                            mediaPath = otherErrorFilePath;
                        }

                        imgData = {};
                        imgData["MediaType"] = mediaType; //'Image';
                        imgData["MediaName"] = errorCode + "_" + "Error" +"_"+file.name;
                        imgData["MediaURL"] = mediaURL;//bucketStartURL + bucketName + errorImagePath+ errorCode+"_"+"Error" + "_" + file.name;
                        imgData["IsErrorAttachment"] = true;
                        imgData["MediaMIMEType"] = mimeType;
                        imgData["MediaPath"] = mediaPath;
                        imgData["File"] = file;

                        errorImageFileArr.push(imgData);

                    }




                }
            }
        }
    }


}

//Function to check already uploaded image files...
function CheckErrorImageFileExisting(name) {
    var isExist = false;
    for (var i = 0; i < errorImageFileArr.length; i++) {
        if (errorImageFileArr[i].MediaName == name) {
            isExist = true;
            break;
        }
    }
    return isExist;
}


function SolutionImageFileUploading() {
    var uploadfiles = $("#fileUpSolutionAttachment").get(0);
    var uploadedfiles = uploadfiles.files;
    var errorCode = $("#txtErrorCode").val();
    var solutionCode = $("#txtSolutionCode").val();

    if (errorCode.length == 0) {
        DisplayErrorMsg('Error Code must be available to upload an image.');
    }
    else {

        if (uploadedfiles.length == 0) {
            //imageFileArr = null;
            //imageFileArr = [];

        } else {

            for (var i = 0; i < uploadedfiles.length; i++) {
                if (CheckSolutionImageFileExisting(uploadedfiles[i].name)) {


                    swal({
                        title: "",
                        text: "The File " + uploadedfiles[i].name + " " + "has already been uploaded",
                        type: "error",
                        timer: 3000,
                        showConfirmButton: false
                    });

                }
                else {

                    for (var i = 0; i < uploadedfiles.length; i++) {
                        var file = uploadedfiles[i];
                        // var fileName = file.name;

                        var mimeType = file['type'];

                        //Choose the mediaURL and mediaType based on the file type
                        var mediaURL, mediaType,mediaPath;
                        if (mimeType.split('/')[0] == 'image') {
                            mediaURL = bucketStartURL + bucketName + "/"+solutionImagePath + errorCode + "_" + solutionCode + "_" + "Solution" + "_" + file.name;
                            mediaType = 'Image';
                            mediaPath = solutionImagePath;
                        }
                        else {
                            mediaURL = bucketStartURL + bucketName + "/"+ otherSolutionFilePath + errorCode  + "_" + solutionCode +  "_" + "Solution" + "_" + file.name;
                            mediaType = 'Other';
                            mediaPath = otherSolutionFilePath;
                        }

                        imgData = {};
                        imgData["MediaType"] = mediaType;//'Image';
                        imgData["MediaName"] = errorCode + "_" + solutionCode + "_" + "Solution" + "_" + file.name;
                        imgData["MediaURL"] = mediaURL;//bucketStartURL + bucketName + "/Images/SolutionImages/" + errorCode +"_"+"Solution" + "_" + file.name;
                        imgData["IsErrorAttachment"] = false;
                        imgData["MediaMIMEType"] = mimeType;
                        imgData["MediaPath"] = mediaPath;
                        imgData["File"] = file;
                          

                        solutionImageFileArr.push(imgData);

                    }




                }
            }
        }
    }


}


function CheckSolutionImageFileExisting(name) {
    var isExist = false;
    for (var i = 0; i < solutionImageFileArr.length; i++) {
        if (solutionImageFileArr[i].MediaName == name) {
            isExist = true;
            break;
        }
    }
    return isExist;
}

/***************************/

/***AWS configurations anf image saving section***/

function InitAWSConfigurations()
{
    /***AWS Config Initializations***/
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
    /*************************************/


    /****Image upload path initializations****/

    errorImagePath = $("#hdnErrorImagePath").val();
    otherErrorFilePath = $("#hdnOtherErrorFilePath").val();
    solutionImagePath = $("#hdnSolutionImagePath").val();
    otherSolutionFilePath = $("#hdnOtherSolutionFilePath").val();

    /***************************************/

}


function UploadFileToS3Bucket(errorCode, uploadFileArr)
{

    var status = true;
    var file,fileName,imageKey;
   
    if (errorCode.length>0) {

        if (uploadFileArr.length > 0) {

            for (var i = 0; i < uploadFileArr.length; i++) {

                //If it's an already available file,then savning it is not required.
                if (typeof (uploadFileArr[i].File) === 'undefined' || uploadFileArr[i].File == null) {

                    continue;
                }
                else {

                    file = uploadFileArr[i].File;
                    fileName = uploadFileArr[i].MediaName;//file.name;

                    var mimeType = file['type'];
                    var mainFileType= mimeType.split('/')[0];
                    //imageType = GetFileExtension(fileName);
                   // imageType = imageType.toLowerCase();
                   // imageKey = path+'/'; //encodeURIComponent(folderName) + '/';

                    imageKey = uploadFileArr[i].MediaPath+fileName;
                    bucket.upload({
                        Key: imageKey,
                        Body: file,
                        ContentType:mainFileType+'/'+GetFileExtension(fileName),//uploadFileArr[i].MediaMIMEType, //'image/' + imageType,
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



function GetFileExtension(filename) {
    var parts = filename.split('.');
    return parts[parts.length - 1];
}

/*****************************/



function ChangeErrorAdditionMode()
{
    if (isErrorWithSolution==true||isSolutionAddForError==true)
    {
        $("#dvSolution").show();
    }
    else
    {
        $("#dvSolution").hide();
    }

    if (isSolutionAddForError == true)
    {
        $("#txtErrorCaption").attr('disabled', true);
        $("#txtErrorDescription").attr('disabled', true);
        $("#dvSearchErrorCaption").show();
        $("#dvErrorAttachment").hide();
        $("#drpDomain").attr('disabled', true);
        $("#drpField").attr('disabled', true);
        $("#drpClient").attr('disabled', true);
    }
    else
    {
        $("#txtErrorCaption").attr('disabled', false);
        $("#txtErrorDescription").attr('disabled', false);
        $("#dvSearchErrorCaption").hide();
        $("#dvErrorAttachment").show();
        $("#drpDomain").attr('disabled', false);
        $("#drpField").attr('disabled', false);
        $("#drpClient").attr('disabled', false);
    }

}


function ShowLabelMeaningPopUp() {

    dialog = $("#errorLabelMeaningModal").dialog({
        autoOpen: false,
        title: "Error Management Index",
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

function ShowSearchErrorPopUp() {

    GetAllErrors();

    dialog = $("#searchErrorModal").dialog({
        autoOpen: false,
        title: "Search Error",
        width: 1000,
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


/**Data Loading section****/

function GetNewErrorCode() {
    $.ajax({
        url: '/ErrorMgt/GetNewErrorCode',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            $("#txtErrorCode").val(inputParam);

        }

    });

}

function GetNewSolutionCode() {
    $.ajax({
        url: '/ErrorMgt/GetNewSolutionCode',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            $("#txtSolutionCode").val(inputParam);

        }

    });

}

function LoadDomainsForDropDown() {

    var param = JSON.stringify({ 'isActive': 1 });

    $.ajax({
        url: '/Domain/LoadDomains',
        type: 'post',
        contentType: 'application/json',
        data:param,
        async:false,
        success: function (inputParam) {

            $("#drpDomain").children().empty();
            $("#drpDomain").empty().append($("<option>").val(0).html(""));

            if (inputParam != null) {
                              

                for (var i = 0; i < inputParam.length; i++) {
                    var val = inputParam[i].DomainId;
                    var text = inputParam[i].DomainCode + "-" + inputParam[i].DomainName;
                    $("#drpDomain").append($("<option>").val(val).html(text));
                }

            }
          
            

        }

    });

}

function LoadFieldsForDropDown()
{
    var param = JSON.stringify({ 'isActive': parseInt(1) });
    $.ajax({
        url: '/Field/LoadFields',
        type: 'post',
        data: param,
        contentType: 'application/json',
        success: function (inputParam) {

            $("#drpField").children().empty();
            $("#drpField").empty().append($("<option>").val(0).html(""));

            if (inputParam != null) {


                for (var i = 0; i < inputParam.length; i++) {
                    var val = inputParam[i].FieldId;
                    var text = inputParam[i].FieldCode + "-" + inputParam[i].FieldName;
                    $("#drpField").append($("<option>").val(val).html(text));
                }

            }

        }

    });
}

function LoadClientsForDropDown() {
    var param = JSON.stringify({ 'isActive': parseInt(1) });
    $.ajax({
        url: '/Client/LoadClients',
        type: 'post',
        data: param,
        contentType: 'application/json',
        success: function (inputParam) {

            $("#drpClient").children().empty();
            $("#drpClient").empty().append($("<option>").val(0).html(""));

            if (inputParam != null) {


                for (var i = 0; i < inputParam.length; i++) {
                    var val = inputParam[i].ClientId;
                    var text = inputParam[i].ClientCode + "-" + inputParam[i].ClientName;
                    $("#drpClient").append($("<option>").val(val).html(text));
                }

            }

        }

    });
}





//Global Variable to store all errors
var errorObjArr = [];

function GetAllErrors()
{
    $.ajax({
        url: '/SearchSolution/GetAllErrors',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            //Assign all the retrieved errors to this global varibale.
            //This is done to make the searching option more faster through client side without 
            //server calls
            errorObjArr = inputParam;
            BuildDynamicErrorDivs(errorObjArr);

        }

    });
}

function FilterErrorListBySearchQuery() {

     var searchQuery =$("#txtSearchError").val();

     var masterErrorArr = []; //The main array to store the arrays of errors
     
     if (searchQuery.length==0)
     {

         masterErrorArr=errorObjArr;
     }
     else
     {
         for (var k = 0; k < errorObjArr.length; k++) {
             if ((errorObjArr[k].Domain.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].Field.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].Client.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].ErrorCaption.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].ErrorDescription.toLowerCase().indexOf(searchQuery)) > 0)
             {
                 var suppObjArr = {};
                 suppObjArr = errorObjArr[k];
                 masterErrorArr.push(suppObjArr);
             }
         }
     }

     BuildDynamicErrorDivs(masterErrorArr);

 
}

function BuildDynamicErrorDivs(errorList)
{
    var divContent = "";
    $("#dvErrorList").html("");

    if (errorList != null && errorList.length > 0) {

        for (var m = 0; m < errorList.length; m++) {
            var currentFrame = "";
            currentFrame = [
                               '<div class="thumbnail" style="cursor: pointer;margin-left:2%" onclick="GetDetailsForErrorCode(\'' + (errorList[m].ErrorCode) + '\')">',
                               '<div class="row" style=margin-left:2%>',
                               '<div class="col-md-4 col-sm-4 col-xs-4">',
                               '<img  class="img-circle" style="width:25%;height:25%" src="' + errorList[m].ErrorUserImageURL + '" alt="..."/>',
                               '<p style="font-size:small;font-family:Segoe UI">' + errorList[m].ErrorUsername + '</p>',
                               '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%">' + errorList[m].ErrorFullName + '</p>',
                               '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%">' + errorList[m].ErrorLogDate + '</p>',
                               '</div>',
                               '<div class="col-md-8 col-sm-8 col-xs-8">',
                               '<p style="font-size:smaller;font-family:Segoe UI;"><b>Domain: ' + errorList[m].Domain + '</b></p><br/>',
                               '<p style="font-size:smaller;font-family:Segoe UI;margin-top:-4%"><b>Field : ' + errorList[m].Field + '</b></p><br/>',
                               '<p style="font-size:smaller;font-family:Segoe UI;margin-top:-4%"><b>Client:' + errorList[m].Client + '</b></p><br/>',
                               '<p style="font-size:small;font-family:Segoe UI"><b>' + errorList[m].ErrorCaption + '</b></p><br/>',
                               '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%">' + errorList[m].ErrorDescription + '</p>',
                               '</div>',
                               '</div>',
                               '</div>'


            ];
            divContent += GetHtml(currentFrame);
        }

        $("#dvErrorList").append(divContent);
    }
    else {
        $("#dvErrorList").html("");
    }
}

function GetDetailsForErrorCode(errorCode)
{
    var param = JSON.stringify({ 'errorCode': errorCode /*$("#txtErrorCode").val()*/ });

    $.ajax({
        url: '/ErrorMgt/GetDetailsForErrorCode',
        type: 'post',
        data:param,
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            var domainId = inputParam[0].DomainId == null ? "0" : inputParam[0].DomainId.toString();
            var fieldId = inputParam[0].FieldId == null ? "0" : inputParam[0].FieldId.toString();
            var clientId = inputParam[0].ClientId == null ? "0" : inputParam[0].ClientId.toString();

            $("#txtErrorCode").val( inputParam[0].ErrorCode);
            $("#drpDomain").select2("val", domainId);
            $("#drpField").select2("val", fieldId);
            $("#drpClient").select2("val", clientId);
            $("#txtErrorCaption").val(inputParam[0].ErrorCaption);
            $("#txtErrorDescription").val(inputParam[0].ErrorDescription);

        }

    });

    var dialog = $("#searchErrorModal").dialog();
    dialog.dialog("close");
}

function GetHtml(template)
{
    return template.join('\n');
}


/************************/


/*Region to handle saving of main error details and media files***/

function SaveMainErrorDetails() {
    var errorCode = $("#txtErrorCode").val();
    var domainID = $("#drpDomain option:selected").val();
    var fieldID = $("#drpField option:selected").val();
    var clientID = $("#drpClient option:selected").val();
    var errorCaption = $("#txtErrorCaption").val();
    var errorDescription = $("#txtErrorDescription").val();
    var solutionCode = $("#txtSolutionCode").val();
    var solutionComment = $("#txtSolution").val();
       

    var catSplit, typeSplit, catName, typeName;

    if (errorCode.length > 0) {
        HideErrorMsg();
        if (domainID!=null && (parseInt(domainID)!=0)) {
            HideErrorMsg();
            if (fieldID != null && (parseInt(fieldID) != 0)) {
                HideErrorMsg();
              
                if (errorCaption.length>0) {

                  
                    if (((isSolutionAddForError == true || isErrorWithSolution == true) && solutionComment.length > 0 && solutionCode.length > 0) || ((isErrorOnly==true)))
                    {
                        HideErrorMsg();

                        //For error object
                        var errorObj = {};
                        errorObj["ErrorCode"] = errorCode;
                        errorObj["DomainId"] = domainID;
                        errorObj["FieldId"] = fieldID;
                        errorObj["ClientId"] = clientID;
                        errorObj["ErrorCaption"] = errorCaption;
                        errorObj["ErrorDescription"] = errorDescription;

                        //For solution object 
                        var solutionObj = {};
                        solutionObj["SolutionCode"] = solutionCode;
                        solutionObj["DomainId"] = domainID;
                        solutionObj["FieldId"] = fieldID;
                        solutionObj["ClientId"] = clientID;
                        solutionObj["SolutionComment"] = solutionComment;
                                               

                        var dataObject = JSON.stringify({
                            'errorObjParam': errorObj,
                            'solObjParam': solutionObj,
                            'errorImageList': errorImageFileArr,
                            'solutionImageList': solutionImageFileArr,
                            'isSolutionAddForError': isSolutionAddForError,
                            'isErrorWithSolution': isErrorWithSolution

                        });
                                                                             

                        var isErrorImageSaved = true, isSolutionImageSaved = true;
                                              
                        if (errorImageFileArr.length > 0) {
                            isErrorImageSaved = UploadFileToS3Bucket(errorCode, errorImageFileArr);
                        }
                        if (solutionImageFileArr.length > 0) {
                            isSolutionImageSaved = UploadFileToS3Bucket(errorCode, solutionImageFileArr);
                        }
                       
                                   
                        if (isErrorImageSaved == true && isSolutionImageSaved==true) {

                            $.ajax({
                                url: '/ErrorMgt/SaveMainErrorDetails',
                                type: 'post',
                                data: dataObject,
                                contentType: 'application/json',
                                async: false,
                                success: function (inputParam) {
                                    var result = inputParam.d;
                                    if (result != "0000") {

                                        swal({
                                            title: "",
                                            text: "The details you posted have been saved successfully! ",
                                            type: "success",
                                            //timer: 3000,
                                            showConfirmButton: true
                                        });

                                        ResetControls();
                                       
                                       
                                    }
                                    else {

                                        swal({
                                            title: "",
                                            text: "Oops..Unable to save the Error details",
                                            type: "error",
                                           // timer: 3000,
                                            showConfirmButton: true
                                        });
                                     
                                        
                                    }

                                }
                            });

                        }
                        else {
                                                    
                           
                            DisplayErrorMsg('Unable to save the uploaded images..');
                            return false;
                        }




                    }
                    else {
                        DisplayErrorMsg('Solution code or solution comment is not available.');
                    }



                }
                else {
                    DisplayErrorMsg('Error Caption cannot be empty.');
                }
            }
            else {
                DisplayErrorMsg('A Field should be selected.');
            }
        }
        else {
            DisplayErrorMsg('A Domain should be selected.');
        }
    }
    else {
        DisplayErrorMsg('Error Code cannot be empty.');
    }
}

function ResetControls() {

    $(':input').val('');
    $("#drpDomain").select2("val", "0");
    $("#drpField").select2("val", "0");
    $("#drpClient").select2("val", "0");

    isSolutionAddForError=false;
    isErrorWithSolution = false;

    $("#fileUpErrorAttachment").val("");
    $("#fileUpSolutionAttachment").val("");
    HideErrorMsg();
    GetNewErrorCode();
    GetNewSolutionCode();
}



/**********************************************************/

/**Section to handle redirected mode=>Scenario where user is redirected from  'Search Solution' page to 'ErrorMgt'
to add a solution for the selected error ****/

function SetErrorForSolution() {
    //Get the value from hidden field 'hdnRedirectedErrorCode'
    //If this value is null, then it is NOT a redirected instance of the 'ErrorMgt' Page
    var redirectedErrorCode = $("#hdnRedirectedErrorCode").val();
      

    if (redirectedErrorCode != "")
    {
        isErrorOnly = false;
        isErrorWithSolution = false;
        isSolutionAddForError = true;
        ChangeErrorAdditionMode();
        GetDetailsForErrorCode(redirectedErrorCode);

    }

}

/**************************************/