$(document).ready(function () {
 
    GetLatestTopErrors();
    GetMostSearchedTopErrors();
});

function GetLatestTopErrors() {
    $.ajax({
        url: '/TrendingErrors/GetLatestTopErrors',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

          
            BuildDynamicTopLatestErrors(inputParam);

        }

    });
}

function GetMostSearchedTopErrors() {
    $.ajax({
        url: '/TrendingErrors/GetMostSearchedTopErrors',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

           
            BuildDynamicTopMostSearchedErrors(inputParam);

        }

    });
}


function BuildDynamicTopLatestErrors(errorList) {
    var divContent = "";
    $("#dvTopLatestErrors").html("");

    if (errorList != null && errorList.length > 0) {

      
        for (var m = 0; m < errorList.length; m++) {

            var dvTHumbnailId = "dv_" + errorList[m].ErrorCode;
            var currentFrame = "";
          
            currentFrame = [
               
                '<div id="' + dvTHumbnailId + '" class="thumbnail clsError" style="cursor: pointer;margin-left:2%;" onclick="RedirectToSolution(\'' + (errorList[m].ErrorCode) + '\')">',
                '<div class="row" style=margin-left:2%>',
                '<div class="col-md-4 col-sm-4 col-xs-4">',
                '<h3><span class="badge" style="margin-top:-2%">' + (m + 1) + '</span></h3>',
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
      

        $("#dvTopLatestErrors").append(divContent);
    } else {
        $("#dvTopLatestErrors").html("");
    }
}


function BuildDynamicTopMostSearchedErrors(errorList) {
    var divContent = "";
    $("#dvTopSearchedErrors").html("");

    if (errorList != null && errorList.length > 0) {

        for (var m = 0; m < errorList.length; m++) {

            var dvTHumbnailId = "dv_" + errorList[m].ErrorCode;
            var currentFrame = "";
            currentFrame = [
               '<div id="' + dvTHumbnailId + '" class="thumbnail clsError" style="cursor: pointer;margin-left:2%;" onclick="RedirectToSolution(\'' + (errorList[m].ErrorCode) + '\')">',
                '<div class="row" style=margin-left:2%>',
                '<div class="col-md-4 col-sm-4 col-xs-4">',
                '<h3><span class="badge" style="margin-top:-2%">'+(m+1)+'</span></h3>',
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

        $("#dvTopSearchedErrors").append(divContent);
    } else {
        $("#dvTopSearchedErrors").html("");
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

//Function to redirect to 'SearchSolution' page when the user clicks on an error
function RedirectToSolution(errorCode) {
    var redirectURL = "/SearchSolution/Index?errorCode=" + errorCode;
    window.open(redirectURL,'_blank');
}
