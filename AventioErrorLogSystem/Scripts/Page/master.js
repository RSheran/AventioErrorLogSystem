
$(document).ready(function () {
    $(".txt_length_1").attr('maxlength', '1');
    $(".txt_length_2").attr('maxlength', '2');
    $(".txt_length_3").attr('maxlength', '3');
    $(".txt_length_4").attr('maxlength', '4');
    $(".txt_length_5").attr('maxlength', '5');
    $(".txt_length_6").attr('maxlength', '6');
    $(".txt_length_7").attr('maxlength', '7');
    $(".txt_length_8").attr('maxlength', '8');
    $(".txt_length_9").attr('maxlength', '9');
    $(".txt_length_10").attr('maxlength', '10');
    $(".txt_length_15").attr('maxlength', '15');
    $(".txt_length_20").attr('maxlength', '20');
    $(".txt_length_30").attr('maxlength', '30');
    $(".txt_length_50").attr('maxlength', '50');
    $(".txt_length_100").attr('maxlength', '100');
    $(".txt_length_200").attr('maxlength', '200');
    GetLoggedInUserDetails();
    ///**Change password***/
    //$("#btn-change-pwd").click(showChangePasswordWindow);
    //$("#btn-change-pwd-2").click(showChangePasswordWindow);

    //$("#txt-old-password").blur(validateOldPassword);
    //$("#txt-new-pwd").blur(validateNewPassword);
    //$("#txt-confirm-pwd").blur(validateConfirmPassword);
    //$("#btn-change").click(updatePassword);
    /***********************/
});


//Retrieve date for javascript
function getdateonly(timestamp) {
    if (timestamp != null) {

        var datevalue = new Date(parseInt(timestamp.substr(6)));
        
        var ret = ('0' + datevalue.getDate()).slice(-2) + "/" + ('0' + (datevalue.getMonth() + 1)).slice(-2) + "/" + datevalue.getFullYear();
        return ret;
    }
    else {
        return '';
    }
}

function getTimeOnly(timestamp) {
    if (timestamp != null) {

        var datevalue = new Date(parseInt(timestamp.substr(6)));
        var ret = datevalue.getHours() + ":" + datevalue.getMinutes() + ":" + datevalue.getSeconds();
        return ret;
    }
    else {
        return '';
    }
}

//Load Current date
function date_time(id) {
    date = new Date;
    year = date.getFullYear();
    month = date.getMonth();
    months = new Array('January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December');
    d = date.getDate();
    day = date.getDay();
    days = new Array('Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday');
    h = date.getHours();
    if (h < 10) {
        h = "0" + h;
    }
    m = date.getMinutes();
    if (m < 10) {
        m = "0" + m;
    }
    s = date.getSeconds();
    if (s < 10) {
        s = "0" + s;
    }
    result = days[day] + ' ' + months[month] + ' ' + d + ' ' + year + ' ' + h + ':' + m + ':' + s;
    document.getElementById(id).innerHTML = result;
    setTimeout('date_time("' + id + '");', '1000');
    return true;
}

function date_time_landing_page(id) {
    date = new Date;
    year = date.getFullYear();
    month = date.getMonth();
    months = new Array('January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December');
    d = date.getDate();
    day = date.getDay();
    days = new Array('Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday');
    h = date.getHours();
    if (h < 10) {
        h = "0" + h;
    }
    m = date.getMinutes();
    if (m < 10) {
        m = "0" + m;
    }
    s = date.getSeconds();
    if (s < 10) {
        s = "0" + s;
    }
    result = '<font style="color:white">' + days[day] + ' ' + months[month] + ' ' + d + ' ' + year + ' ' + h + ':' + m + ':' + s + '</font>';
    document.getElementById(id).innerHTML = result;
    setTimeout('date_time_landing_page("' + id + '");', '1000');
    return true;
}

/************Logged in user details**********************/
function GetLoggedInUserDetails() {

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



//Random number generation for image retrieval
function S4() {
    return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
}

/***************************************/

$(document).on('keydown', ".numericTextbox ", function (eef) {
    // Allow: backspace, delete, tab, escape, enter and .
    if ($.inArray(eef.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
        // Allow: Ctrl+A
        (eef.keyCode == 65 && eef.ctrlKey === true) ||
        // Allow: home, end, left, right
        (eef.keyCode >= 35 && eef.keyCode <= 39)) {
        // let it happen, don't do anything
        return;
    }
    // Ensure that it is a number and stop the key press
    if ((eef.shiftKey || (eef.keyCode < 48 || eef.keyCode > 57)) && (eef.keyCode < 96 || eef.keyCode > 105)) {
        eef.preventDefault();
    }
});



