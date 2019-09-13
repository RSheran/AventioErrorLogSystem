$(document).ready(function () {

    $("#drpDomain").select2();
    $("#drpField").select2();
    $("#drpClient").select2();

    LoadDomainsForDropDown();

    $("#drpDomain").change(GetExistingErrorListForParam);
    $("#drpField").change(GetExistingErrorListForParam);
    $("#drpClient").change(GetExistingErrorListForParam);

    // $("#btnSearchError").click(ShowSearchErrorPopUp);
    //$("#txtSearchError").keyup(GetAllErrorsForSearchQuery);/*(FilterErrorListBySearchQuery);*/
    $("#btnSearchError").click(function () { GetAllErrorsForSearchQuery(null); });
    $("#btnReloadAll").click(GetAllErrors);

    HideErrorMsg();
    GetAllErrors();
    $("#dvSearchedCaption").hide();

    GetErrorForRedirectedErrorCode();
});


function LoadDomainsForDropDown() {

    var param = JSON.stringify({
        'isActive': 1
    });

    $.ajax({
        url: '/Domain/LoadDomains',
        type: 'post',
        contentType: 'application/json',
        data: param,
        async: false,
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
var errorCode; //Global variable to capture selected error from popup

function GetExistingErrorListForParam() {

    var domainID = $("#drpDomain option:selected").val();
    var fieldID = $("#drpField option:selected").val();
    var clientID = $("#drpClient option:selected").val();

    var domainIdParam = typeof (domainID) === 'undefined' ? 0 : parseInt(domainID);
    var fieldIdParam = typeof (fieldID) === 'undefined' ? 0 : parseInt(fieldID);
    var clientIdParam = typeof (clientID) === 'undefined' ? 0 : parseInt(clientID);

    var paramObj = JSON.stringify({
        'domainId': domainIdParam,
        'fieldId': fieldIdParam,
        'clientId': clientIdParam
    });

    $.ajax({
        url: '/SearchSolution/GetExistingErrorListForParam',
        type: 'post',
        data: paramObj,
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            BindDataToDatatable(inputParam);

        }

    });
}

function BindDataToDatatable(dtList) {

    //First destroy the datatable before reinitialising
    $('#tblSearchError').dataTable().fnDestroy();

    var table = $('#tblSearchError').DataTable({
        "data": dtList,
        select: "single",

        "columnDefs": [{
            "targets": [6, 7, 8, 9, 10, 11],
            "visible": false
        }],

        "columns": [
            //{
            //    "className": 'details-control',
            //    "orderable": false,
            //    "data": null,
            //    "defaultContent": '',
            //    "render": function () {
            //        return '<span class="fa fa-plus-circle" aria-hidden="true" id="txt1" style="font-size:25px"></span>';
            //    },
            //    width: "15px"
            //},
            {
                "data": "ErrorCode",
                "width": "10%"
            },
            {
                "data": "Domain",
                "width": "20%"
            },
            {
                "data": "Field",
                "width": "20%"
            },
            {
                "data": "Client",
                "width": "10%"
            },
            {
                "data": "ErrorCaption",
                "width": "20%"
            },
            {
                "data": "ErrorDescription",
                "width": "20%"
            },
            {
                "data": "ErrorUsername"
            },
            {
                "data": "ErrorFullName"
            },
            {
                "data": "ErrorUserImageURL"
            },
            {
                "data": "ErrorAttachments"
            },
            {
                "data": "SolutionMasters"
            },
            {
                "data": "ErrorLogDate"
            },



        ],
        "order": [
            [1, 'asc']
        ]
    });

    dtErrorDetails = $('#tblSearchError').DataTable();



    $('#tblSearchError').on('click', 'tr', function () {

        var currentIndex = $(this).index();

        //To prevent mouse click on invalid spots
        if (typeof (currentIndex) == 'undefined') {
            return;
        }

        var data = dtErrorDetails.row(this).data();

        if (typeof (data) === 'undefined') {
            return;
        }

        //Get the currently selected error code 
        errorCode = data.ErrorCode;

        var domain = data.Domain;
        var field = data.Field;
        var client = data.Client;
        var errorCaption = data.ErrorCaption;
        var errorDetails = data.ErrorDescription;

        //<Binding the Error details>
        $("#dvSearchedCaption").show();
        $("#spnSearchedErrorCaption").html(errorCaption);
        $("#spnSearchedErrorDetails").html(errorDetails);

        //Binding the User Details who posted the error
        var divContent = "";

        var currentFrame = "";
        currentFrame = [
            '<div class="thumbnail">',
            '<p style="font-size:medium">This error was posted by:</p>',
            '<div class="row" style=margin-left:2%>',
            '<div class="col-md-4 col-sm-4 col-xs-4">',
            '<img  class="img-circle" style="width:25%;height:25%" src="' + data.ErrorUserImageURL + '" alt="..."/>',
            '<h6><b>' + data.ErrorUsername + '</b></h6>',
            '<h5><b>' + data.ErrorFullName + '</b></h6>',
            '<h5><b>' + data.ErrorLogDate + '</b></h6>',
            '</div>',
            '</div>',
            '</div>',

        ];
        divContent += GetHtml(currentFrame);

        $("#dvErrorUserSection").append(divContent);

        //<End>

        var errorAttachmentList = data.ErrorAttachments;
        var solutionList = data.SolutionMasters;


        if (solutionList != null && solutionList.length > 0) {

            $("#dvSolutionSegment").show();
            $("#spnSolutionCount").html('Found ' + solutionList.length + ' solution(s) for this error');
            BuildDynamicSolutionDivs(solutionList, domain, field, client);
        } else {
            $("#dvSolutionSegment").hide();
            $("#spnSolutionCount").html('Oops..! Found no solution(s) for this error');
        }

        swal({
            title: "",
            text: "Error Code selected successfully! ",
            type: "success",
            timer: 1000,
            showConfirmButton: false
        });

        //GetDetailsForErrorCode();
        HideErrorMsg();
        var dialog = $("#searchErrorModal").dialog();
        dialog.dialog("close");




    });

}


function GetExistingErrorListForErrorCode() {

    var paramObj = JSON.stringify({
        'errorCode': errorCode
    });

    $.ajax({
        url: '/SearchSolution/GetExistingErrorListForErrorCode',
        type: 'post',
        data: paramObj,
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            BuildDynamicSolutionDivs(inputParam[0].SolutionMasters, inputParam[0].Domain, inputParam[0].Field, inputParam[0].Client);

        }

    });
}

//Global Variable to store all errors
var errorObjArr = [];

function GetAllErrors() {
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

function GetAllErrorsForSearchQuery(searchParam) {

    var searchQuery = searchParam == null ? $("#txtSearchError").val() : searchParam;

    if (searchQuery.length > 0) {

        $("#showError").hide();

        var param = JSON.stringify({
            'searchQuery': searchQuery
        });

        $.ajax({
            url: '/SearchSolution/GetAllErrorsForSearchQuery',
            type: 'post',
            data: param,
            contentType: 'application/json',
            async: false,
            success: function (inputParam) {

                BuildDynamicErrorDivs(inputParam);

            }

        });
    } else {
        $("#errorStrip").show();
        $("#errorAlert").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp; Search query is empty.');
        $("#errorAlert").show().delay(3000).fadeOut();
        // $("#showError").hide();

    }
}

function FilterErrorListBySearchQuery() {

    var searchQuery = $("#txtSearchError").val();

    var masterErrorArr = []; //The main array to store the arrays of errors

    if (searchQuery.length == 0) {

        masterErrorArr = errorObjArr;
    } else {
        for (var k = 0; k < errorObjArr.length; k++) {
            if ((errorObjArr[k].Domain.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].Field.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].Client.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].ErrorCaption.toLowerCase().indexOf(searchQuery)) >= 0 ||
                (errorObjArr[k].ErrorDescription.toLowerCase().indexOf(searchQuery)) > 0) {
                var suppObjArr = {};
                suppObjArr = errorObjArr[k];
                masterErrorArr.push(suppObjArr);
            }
        }
    }

    BuildDynamicErrorDivs(masterErrorArr);


}



function GetExistingErrorListForErrorCodeAsParam(dvThumbnailId, errorCodeParam) {

    //First,update the error frequency(How many times the error was searched)
    UpdateSearchFrequencyForError(errorCodeParam);

    var elements = document.getElementsByClassName('clsError');
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.backgroundColor = "white";
    }


    $("#" + dvThumbnailId).css("background-color", "lightblue");

    //Get the currently selected error code 
    errorCode = errorCodeParam;

    var paramObj = JSON.stringify({
        'errorCode': errorCodeParam
    });

    $.ajax({
        url: '/SearchSolution/GetExistingErrorListForErrorCode',
        type: 'post',
        data: paramObj,
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            $("#spnSearchedErrorDetails").html('');
            $("#spnSearchedErrorCaption").html(inputParam[0].ErrorCaption);
            $("#spnSearchedErrorDetails").html(inputParam[0].ErrorDescription);

            var divContent = "";
            
                     
            if (inputParam[0].SolutionMasters != null && inputParam[0].SolutionMasters.length > 0) {

                $("#dvSolutionSegment").show();
                $("#spnSolutionCount").html('Found ' + inputParam[0].SolutionMasters.length + ' solution(s) for this error');

            } else {
                $("#dvSolutionSegment").hide();
                $("#spnSolutionCount").html('Oops..! Found no solution(s) for this error');
            }

         

            //Get error attachments
            var errorAttachments = inputParam[0].ErrorAttachments;

          
            if (errorAttachments != null && errorAttachments.length > 0) {
                //divContent += "<h6 style='font-family:Segoe UI'> " + inputParam[0].ErrorDescription + "</h6><br/><br/>";

                //First, bind the images
                for (var k = 0; k < errorAttachments.length; k++) {
                    if (errorAttachments[k].MediaType.toLowerCase() == 'image') {
                        var imageId = "img_" + errorAttachments[k].AttachmentId;
                       
                        var currentFrame = "";
                        currentFrame = [
                            '<div class="thumbnail">',
                            '<img style="cursor:pointer" id="' + imageId + '"+ style="width:75%;height:75%;" src="' + errorAttachments[k].MediaURL + '" alt="..." />',
                            '</div>'

                        ];
                       
                        divContent += GetHtml(currentFrame);
                    }
                }


                //Then bind the other files as downloadable/viewable elemets

                //Open a expandable 'accordion' to bind the other files
                divContent += "<div class='panel panel-default'>" +
                    "<div class='panel-body'>" +
                    "<div class='panel-group' id='errorAccordion'>" +
                    " <div class='panel panel-default' id='errorOtherFiles'>" +
                    "<div class='panel-heading'>" +
                    "<h4 class='panel-title'>" +
                    "<a data-toggle='collapse' data-parent='#accordion' href='#collapseOne'><i class='glyphicon glyphicon-file'></i>&nbsp;Other Attachments" +
                    "</a>" +
                    "</h4>" +
                    "</div>" +
                    "</div>" +
                    "<div id='collapseOne' class='panel-collapse collapse'>" +
                    "<div class='panel-body'>";

                //Start binding other files if available only
                for (var l = 0; l < errorAttachments.length; l++) {
                    if (errorAttachments[l].MediaType.toLowerCase() == 'other') {
                        var currentFrame = "";
                        currentFrame = [
                            '<div class="row">',
                            '<div class="col-md-7 col-sm-7 col-xs-7">',
                            '<a target="_blank" class="btn btn-default form-control" href="' + errorAttachments[l].MediaURL + '" style="cursor: pointer;margin-top:2%">',
                            '<p style="font-size:small;font-family:Segoe UI;text-align:left"><i class="glyphicon glyphicon-download"></i>&nbsp;' + errorAttachments[l].MediaName + '</p>',
                            '</a>',
                            '</div>',
                            '</div>'

                        ];
                        divContent += GetHtml(currentFrame);
                    }

                }

                //Close the accordion section
                divContent += "</div></div></div></div></div>"

            }

            //Before binding the solution drtails,bind the 'Add Solution' button
            //Add button with redirection link to add solution(Link to 'Index' Action of 'ErrorMgt' Page)
            var currentFrameBtn = "";
            var redirectLink = "/ErrorMgt/Index?redirectedErrorCode=" + inputParam[0].ErrorCode;
            currentFrameBtn = [
                   '<div class="clearfix"></div>',
                   '<a type="button" target="_blank" href="' + redirectLink + '" id="btnUserDirectory" class="btn btn-md btn-info">',
                   '<i class="glyphicon glyphicon-plus"></i> &nbsp;Add Solution',
                    '</a>',
                    '<div class="clearfix"></div>'
            ];

            divContent += GetHtml(currentFrameBtn);

            $("#spnSearchedErrorDetails").append(divContent);
            $("#dvSearchedCaption").show();

            

            //Bind the solution details
            BuildDynamicSolutionDivs(inputParam[0].SolutionMasters, inputParam[0].Domain, inputParam[0].Field, inputParam[0].Client);

        }

    });
}

function GetSolutionsForRedirectedError(errorCodeParam) {

    //First,update the error frequency(How many times the error was serached)
    UpdateSearchFrequencyForError(errorCodeParam);
       
    //Get the currently selected error code 
    errorCode = errorCodeParam;

    var paramObj = JSON.stringify({
        'errorCode': errorCodeParam
    });

    $.ajax({
        url: '/SearchSolution/GetExistingErrorListForErrorCode',
        type: 'post',
        data: paramObj,
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            $("#spnSearchedErrorCaption").html(inputParam[0].ErrorCaption);
            $("#spnSearchedErrorDetails").html(inputParam[0].ErrorDescription);

            $("#spnSearchedErrorDetails").html('');
            if (inputParam[0].SolutionMasters != null && inputParam[0].SolutionMasters.length > 0) {

                $("#dvSolutionSegment").show();
                $("#spnSolutionCount").html('Found ' + inputParam[0].SolutionMasters.length + ' solution(s) for this error');

            } else {
                $("#dvSolutionSegment").hide();
                $("#spnSolutionCount").html('Oops..! Found no solution(s) for this error');
            }

            //Get error attachments
            var errorAttachments = inputParam[0].ErrorAttachments;

            divContent = "";
            if (errorAttachments != null && errorAttachments.length > 0) {
                divContent += "<h6 style='font-family:Segoe UI'> " + inputParam[0].ErrorDescription + "</h6><br/><br/>";

                //First, bind the images
                for (var k = 0; k < errorAttachments.length; k++) {
                    if (errorAttachments[k].MediaType.toLowerCase() == 'image') {
                        var imageId = "img_" + errorAttachments[k].AttachmentId;

                        var currentFrame = "";
                        currentFrame = [
                            '<div class="thumbnail">',
                            '<img style="cursor:pointer" id="' + imageId + '"+ style="width:75%;height:75%;" src="' + errorAttachments[k].MediaURL + '" alt="..." />',
                            '</div>'

                        ];

                        divContent += GetHtml(currentFrame);
                    }
                }


                //Then bind the other files as downloadable/viewable elemets

                //Open a expandable 'accordion' to bind the other files
                divContent += "<div class='panel panel-default'>" +
                    "<div class='panel-body'>" +
                    "<div class='panel-group' id='errorAccordion'>" +
                    " <div class='panel panel-default' id='errorOtherFiles'>" +
                    "<div class='panel-heading'>" +
                    "<h4 class='panel-title'>" +
                    "<a data-toggle='collapse' data-parent='#accordion' href='#collapseOne'><i class='glyphicon glyphicon-file'></i>&nbsp;Other Attachments" +
                    "</a>" +
                    "</h4>" +
                    "</div>" +
                    "</div>" +
                    "<div id='collapseOne' class='panel-collapse collapse'>" +
                    "<div class='panel-body'>";

                //Start binding other files if available only
                for (var l = 0; l < errorAttachments.length; l++) {
                    if (errorAttachments[l].MediaType.toLowerCase() == 'other') {
                        var currentFrame = "";
                        currentFrame = [
                            '<div class="row">',
                            '<div class="col-md-7 col-sm-7 col-xs-7">',
                            '<a target="_blank" class="btn btn-default form-control" href="' + errorAttachments[l].MediaURL + '" style="cursor: pointer;margin-top:2%">',
                            '<p style="font-size:small;font-family:Segoe UI;text-align:left"><i class="glyphicon glyphicon-download"></i>&nbsp;' + errorAttachments[l].MediaName + '</p>',
                            '</a>',
                            '</div>',
                            '</div>'

                        ];
                        divContent += GetHtml(currentFrame);
                    }

                }

                //Close the accordion section
                divContent += "</div></div></div></div></div>"

            }

            $("#spnSearchedErrorDetails").append(divContent);
            $("#dvSearchedCaption").show();
            BuildDynamicSolutionDivs(inputParam[0].SolutionMasters, inputParam[0].Domain, inputParam[0].Field, inputParam[0].Client);

        }

    });
}



function BuildDynamicSolutionDivs(solutionList, domain, field, client) {


    var divContent = "";
    if (solutionList != null && solutionList.length > 0) {

        $("#dvSolutionList").html("");



        for (var j = 0; j < solutionList.length; j++) {


            var solutionAttachmentList = solutionList[j].ErrorAttachments;
            var solutionFeedBackList = solutionList[j].SolutionFeedbackList;

            divContent += "<div class='jumbotron'>";

            //Bind a Verified Tick Mark for the first solution if its verification count is > 0 only
            if (solutionList[0].VerifiedCount > 0 && j == 0) {
                divContent += "<h1 style='font-family:Segoe UI;margin-top:-4%'><i class='glyphicon glyphicon-ok text-success' style='font-size:50px'></i></h1>";
            }

            divContent += "<div class='row'>" +
                "<div class='col-md-4 col-sm-4 col-xs-4'>";
            var currentFrame = "";
            currentFrame = [
                '<img  class="img-circle" style="width:25%;height:25%" src="' + solutionList[j].SolutionUserImageURL + '" alt="..."/>',
                '<h6 style="font-family:Segoe UI"><b>' + solutionList[j].SolutionUsername + '</b></h6>',
                '<h5 style="font-family:Segoe UI"><b>' + solutionList[j].SolutionUserFullName + '</b></h5>',
                '<h6 style="font-family:Segoe UI"><b>' + solutionList[j].SolutionLogDate + '</b></h6>'
            ];
            divContent += GetHtml(currentFrame);
            divContent += "</div>" +
                "<div class='col-md-8 col-sm-8 col-xs-8'>" +
                "<p style='font-size:medium;font-family:Segoe UI'><b>Domain :</b>" + domain + "</p>" +
                "<p style='font-size:medium;font-family:Segoe UI'><b>Field :</b>" + field + "</p>" +
                "<p style='font-size:medium;font-family:Segoe UI'><b>Client :</b>" + client + "</p>" +
                "<br/>" +
                "<p style='font-size:medium;font-family:Segoe UI'>" + DetectURLAndReplaceAsHyperlink(solutionList[j].SolutionComment) + "</p>";


            if (solutionAttachmentList != null && solutionAttachmentList.length > 0) {

                //First, bind the images
                for (var k = 0; k < solutionAttachmentList.length; k++) {
                    if (solutionAttachmentList[k].MediaType.toLowerCase() == 'image') {
                        var currentFrame = "";
                        currentFrame = [
                            '<div class="thumbnail">',
                            '<img style="width:100%;height:100%;" src="' + solutionAttachmentList[k].MediaURL + '" alt="..."/>',
                            '</div>'

                        ];
                        divContent += GetHtml(currentFrame);
                    }
                }


                //Then bind the other files as downloadable/viewable elemets

                //Open a expandable 'accordion' to bind the other files
                var solAccordionGroupId = "dvSolAccordionGrp_" + solutionList[j].SolutionCode;
                var solAccordionMainId = "dvSolAccordionMain_" + solutionList[j].SolutionCode;
                var solAccordionHrefId = "dvSolAccordionHref_" + solutionList[j].SolutionCode;
                var solAccordionHref = "#dvSolAccordionHref_" + solutionList[j].SolutionCode;

                divContent += "<div class='panel panel-default'>" +
                    "<div class='panel-body'>";
                     var currentFrame = "";
                     currentFrame = [
                         '<div class="panel-group" id="' + solAccordionGroupId + '">' ,
                         '<div class="panel panel-default" id="' + solAccordionMainId + '">',
                         '<div class="panel-heading">',
                         '<h4 class="panel-title">' ,
                         '<a data-toggle="collapse" data-parent="#accordion" href="' + solAccordionHref + '"><i class="glyphicon glyphicon-file"></i>&nbsp;Other Attachments',
                         '</a>',
                         '</h4>',
                         '</div>',
                         '</div>',
                         '<div id="' + solAccordionHrefId + '" class="panel-collapse collapse">'+
                         '<div class="panel-body">'
                     ];
                     divContent += GetHtml(currentFrame);

                //Start binding other files if available only
                for (var l = 0; l < solutionAttachmentList.length; l++) {
                    if (solutionAttachmentList[l].MediaType.toLowerCase() == 'other')
                    {
                        var currentFrame = "";
                        currentFrame = [
                            '<div class="row">',
                            '<div class="col-md-10 col-sm-10 col-xs-10">',
                            '<a target="_blank" class="btn btn-default form-control" href="' + solutionAttachmentList[l].MediaURL + '" style="cursor: pointer;margin-top:2%">',
                            '<p style="font-size:small;font-family:Segoe UI;text-align:left"><i class="glyphicon glyphicon-download"></i>&nbsp;' + solutionAttachmentList[l].MediaName + '</p>',
                            '</a>',
                            '</div>',
                            '</div>'

                        ];
                        divContent += GetHtml(currentFrame);
                    }
                   
                }
                //Close the accordion section
                divContent += "</div></div></div></div></div>";
               
              
            }



            divContent += "<hr/>"

            //Section to bind available user feedback for the solution
            if (solutionFeedBackList != null && solutionFeedBackList.length > 0) {
                for (var m = 0; m < solutionFeedBackList.length; m++) {
                    var currentFrame = "";
                    currentFrame = [
                        '<div class="thumbnail">',
                        '<div class="row" style=margin-left:2%>',
                        '<div class="col-md-4 col-sm-4 col-xs-4">',
                        '<img  class="img-circle" style="width:25%;height:25%" src="' + solutionFeedBackList[m].FeedbackUserImageURL + '" alt="..."/>',
                        '<h6 style="font-family:Segoe UI"><b>' + solutionFeedBackList[m].FeedbackUsername + '</b></h6>',
                        '<h5 style="font-family:Segoe UI"><b>' + solutionFeedBackList[m].FeedbackUserFullName + '</b></h5>',
                        '<p style="font-family:Segoe UI;font-size:smaller"><b>' + solutionFeedBackList[m].FeedBackDate + '</b></p>',
                        '</div>',
                        '<div class="col-md-8 col-sm-8 col-xs-8">',
                        '<h6 style="font-family:Segoe UI"><b>' + DetectURLAndReplaceAsHyperlink(solutionFeedBackList[m].FeedBackComment) + '</b></h6>',
                        '</div>',
                        '</div>',
                        '</div>',

                    ];
                    divContent += GetHtml(currentFrame);
                }
            }


            divContent += "<hr/>"

            //Section for user feedback for the solution
            currentFrame = "";
            var textBoxId = "txt_" + solutionList[j].SolutionCode;
            var IsSolCorrectRadioButtonId = "rbSolutionCorrect_" + solutionList[j].SolutionCode;
            var IsSolInCorrectRadioButtonId = "rbSolutionWrong_" + solutionList[j].SolutionCode;


            currentFrame = [
                //'<div class="row">',
                //'<label class="col-sm-8 col-md-8 col-xs-8 control-label" style="text-align: left; margin-left: -2%">Did this solution work for you?</label>',
                //'</div>',
                //'<div class="row">',
                //'<div class="col-sm-1 col-md-1 col-xs-1" style="margin-left: -5%;">',
                //'<label class="col-sm-8 col-md-8 col-xs-8 control-label" style="text-align: left; margin-left: -2%">Did this solution work for you?</label>',
                //'</div>',
                //'<input type="checkbox" id="chkIsSolutionCorrect_"' + solutionList[j].SolutionCode + '" onclick="UpdateSolutionAsCorrect(\'' + ("chkIsSolutionCorrect_" + solutionList[j].SolutionCode) + "','" + solutionList[j].SolutionCode + '\')"/>',
                // '</div>',
                // '</div>',
                '<div class="row">',
                '<label  class="col-sm-8 col-md-8 col-xs-8 control-label" style="text-align: left; margin-left:4%;font-family:Segoe UI">Did this solution work for you?</label>',
                '</div>',
                '<div class="row">',
                '<label class="col-sm-2 col-md-2 col-xs-2 control-label" style="text-align: left; margin-left: 4%;font-family:Segoe UI">Yes</label>',
                '<input id="' + IsSolCorrectRadioButtonId + '" class="col-sm-1 col-md-1 col-xs-1" style="margin-left: -10%" name="chkIsCorrect" type="radio" />',
                '<label class="col-sm-2 col-md-2 col-xs-2 control-label" style="text-align: left; margin-left: 4%">No</label>',
                '<input id="' + IsSolInCorrectRadioButtonId + '" class="col-sm-1 col-md-1 col-xs-1"  style="margin-left: -10%" name="chkIsCorrect" type="radio"/>',
                '</div>',
                '<div class="row">',
                '<div class="col-md-10 col-sm-10 col-xs-10">',
                '<textarea id="' + textBoxId + '" class="form-control" style="width: 100%; margin-left: 4%;font-family:Segoe UI" placeholder="Your Feedback"></textarea>',
                '</div>',
                ' </div>',
                '<a href="#" class="btn btn-info" style="margin-left: 2%;margin-top:2%" role="button" onclick="SaveUserFeedback(\'' + ('txt_' + solutionList[j].SolutionCode) + '\')">',
                '<i class="glyphicon glyphicon-ok">',
                "</i>&nbsp;",
                'Save Feedback Comment',
                '</a>'
            ];

            divContent += GetHtml(currentFrame);

            divContent += "</div>" +

                "</div>" + "</div>";




        }

        //$('body').find('#dvShiftEmpCount').append("</div>");
        // divContent += "</div>";

        $("#dvSolutionList").append(divContent);
    }

}

function BuildDynamicErrorDivs(errorList) {
    var divContent = "";
    $("#dvErrorList").html("");

    if (errorList != null && errorList.length > 0) {

        for (var m = 0; m < errorList.length; m++) {

            var dvTHumbnailId = "dv_" + errorList[m].ErrorCode;
            var currentFrame = "";
            currentFrame = [
                '<div id="' + dvTHumbnailId + '" class="thumbnail clsError" style="cursor: pointer;margin-left:2%;" onclick="GetExistingErrorListForErrorCodeAsParam(\'' + dvTHumbnailId + '\',\'' + (errorList[m].ErrorCode) + '\')">',
                '<div class="row" style=margin-left:2%>',
                '<div class="col-md-4 col-sm-4 col-xs-4">',
                '<img  class="img-circle" style="width:25%;height:25%" src="' + errorList[m].ErrorUserImageURL + '" alt="..."/>',
                '<p style="font-size:small;font-family:Segoe UI">' + errorList[m].ErrorUsername + '</p>',
                '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%">' + errorList[m].ErrorFullName + '</p>',
                '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%">' + errorList[m].ErrorLogDate + '</p>',
                '</div>',
                '<div class="col-md-8 col-sm-8 col-xs-8">',
                '<p style="font-size:small;font-family:Segoe UI;"><b>Domain: ' + errorList[m].Domain + '</b></p><br/>',
                '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%"><b>Field : ' + errorList[m].Field + '</b></p><br/>',
                '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%"><b>Client:' + errorList[m].Client + '</b></p><br/>',
                '<p style="font-size:small;font-family:Segoe UI"><b>' + errorList[m].ErrorCaption + '</b></p><br/>',
                '<p style="font-size:small;font-family:Segoe UI;margin-top:-4%">' + DetectURLAndReplaceAsHyperlink(errorList[m].ErrorDescription) + '</p>',
                '</div>',
                '</div>',
                '</div>'


            ];
            divContent += GetHtml(currentFrame);
        }

        $("#dvErrorList").append(divContent);
    } else {
        $("#dvErrorList").html("");
    }
}

//Function to detect URL text within a comment and display it as a link
function DetectURLAndReplaceAsHyperlink(text) {
    var urlRegex = /(https?:\/\/[^\s]+)/g;
    return text.replace(urlRegex, function (url) {
        return '<p style="font-size:small;font-family:Segoe UI;"><a target="_blank" href="' + url + '">' + url + '</a></p>';
    })
    // or alternatively
    // return text.replace(urlRegex, '<a href="$1">$1</a>')
}

function GetHtml(template) {
    return template.join('\n');
}



/********************************************/

/**Show PopUp******/
function ShowSearchErrorPopUp() {

    LoadDomainsForDropDown();
    GetExistingErrorListForParam();

    dialog = $("#searchErrorModal").dialog({
        autoOpen: false,
        title: "Search Error",
        width: 1000,
        modal: true,
        closeOnEscape: false,
        open: function (event, ui) {
            $(".ui-dialog-titlebar-close", ui.dialog || ui).hide();
        },
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
        close: function () { }
    });

    dialog.dialog("open");
}


/****************************/

/**Section to save User Feedback and update solution as correct****/

function SaveUserFeedback(feedbackTextBoxId) {

    var isSolutionCorrect = $("#rbSolutionCorrect_" + feedbackTextBoxId.split("_")[1]).is(':checked');
    var isSolutionInCorrect = $("#rbSolutionWrong_" + feedbackTextBoxId.split("_")[1]).is(':checked');
    var solutionCode = feedbackTextBoxId.split("_")[1];

    if (isSolutionCorrect == false && isSolutionInCorrect == false) {
        swal({
            title: "",
            text: "For better feedback reference,you must select whether the solution is correct or not by selecting 'Yes' or 'No'",
            type: "error",
            timer: 6000,
            showConfirmButton: true
        });
    } else if ($("#" + feedbackTextBoxId).val().length == 0) {
        swal({
            title: "",
            text: "You must enter a comment for clear feedback.",
            type: "error",
            timer: 6000,
            showConfirmButton: true
        });
    } else {

        //var isSuccess = false;

        swal({
            title: "Are you sure to add this feedback?",
            //text: "You will not be able to recover this imaginary file!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#01DF74",
            confirmButtonText: "Yes, add this feedback..",
            showLoaderOnConfirm: true,
            closeOnConfirm: false,
            height: "100px",
            width: "100px"
        },
            function () {
                var solFeedbackObj = {};
                solFeedbackObj["IsSolutionCorrect"] = isSolutionCorrect;
                solFeedbackObj["FeedBackComment"] = $("#" + feedbackTextBoxId).val();

                var param = JSON.stringify({
                    'solFeedbackObj': solFeedbackObj,
                    'solutionCode': solutionCode
                });

                $.ajax({
                    url: '/SearchSolution/SaveSolutionFeedback',
                    type: 'post',
                    contentType: 'application/json',
                    data: param,
                    async: false,
                    success: function (inputParam) {

                        isSuccess = inputParam;

                    }

                });

                if (isSuccess == true) {

                    swal({
                        title: "",
                        text: "Your feedback was added to the solution successfully! ",
                        type: "success",
                        timer: 3000,
                        showConfirmButton: false
                    });

                    GetExistingErrorListForErrorCode();
                } else {
                    swal({

                        title: "",
                        text: "Oops!..Unable to add your feedback.",
                        type: "error",
                        timer: 3000,
                        showConfirmButton: false
                    });
                }




            });



    }
}


//Function to update how many times a particular error was searched
//(This will be usd later for Top 10 Most Searched Error List)
function UpdateSearchFrequencyForError(errorCode)
{
    var param = JSON.stringify({'errorCode': errorCode});

    $.ajax({
        url: '/ErrorFrequency/UpdateSearchFrequencyForError',
        type: 'post',
        contentType: 'application/json',
        data: param,
        async: false,
        success: function (inputParam) {

            var isSuccess = inputParam;

        }

    });
    
}





/*********************************/


function HideErrorMsg() {
    $("#errorAlert").text('');
    $("#errorStrip").css('display', 'none');
}

/**Section to handle redirected Request for errorCode****/

function GetErrorForRedirectedErrorCode()
{
    //Get the value from hidden field 'hdnRedirectedErrorCode'
    //If this value is null, then it is NOT a redirected instance of the 'SearchSolution' Page
    var redirectedErrorCode = $("#hdnRedirectedErrorCode").val();


    if (redirectedErrorCode != "")
    {
        GetAllErrorsForSearchQuery(redirectedErrorCode);
        GetSolutionsForRedirectedError(redirectedErrorCode);
    }
   
}

/********************************************************/