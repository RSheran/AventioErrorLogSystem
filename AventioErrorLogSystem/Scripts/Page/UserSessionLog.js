$(document).ready(function () {

    LoadAllSystemUsersForDropDown();
    LoadAllLoggedInSessions();
    //$("#drpUser").change(LoadAllLoggedInSessions);
    $("#btnClearSesssion").click(ClearSessionDetails);
    $("#btnRefresh").click(LoadAllLoggedInSessions);
});


/**Data Loading Section*****/

//Load users for dropdown
function LoadAllSystemUsersForDropDown()
{
    
    $.ajax({
        url: '/SystemUserDirectory/GetAllUsers',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {


            BindUsersForDropDown(inputParam);

        }

    });

}

function BindUsersForDropDown(inputParam)
{
    var userMasterArr=[];
    $("#drpUser").children().empty();
   // $("#drpUser").empty().append($("<option>").val(0).html("All"));

    if (inputParam != null) {

        //First bind the 'All' option
        var suppArr = {};
        suppArr["text"] ="All-All";
        suppArr["value"] = 0;
        suppArr["description"] = "All Users";
        suppArr["imageSrc"] =  "../../Images/BlankProfilePic.png";
        userMasterArr.push(suppArr);
           
        for (var i = 0; i < inputParam.length; i++) {

            var suppArr = {};
            suppArr["text"]=inputParam[i].Username + "-" + inputParam[i].FullName;
            suppArr["value"] = inputParam[i].UserId;
            suppArr["description"] = inputParam[i].FullName;
            suppArr["imageSrc"] = inputParam[i].ProfilePicURL == null ? "../../Images/BlankProfilePic.png" : inputParam[i].ProfilePicURL;
            userMasterArr.push(suppArr);
            //var val = inputParam[i].UserId;
            //var text = inputParam[i].Username + "-" + inputParam[i].FullName;
            //$("#drpUser").append($("<option>").val(val).html(text));
        }

        $('#drpUser').ddslick({
            data: userMasterArr,
            width: 400,        
            imagePosition: "left",
            selectText: "Select user",
            defaultSelectedIndex:0,
            onSelected: function (data) {
                LoadAllLoggedInSessions();
            }
        });

       
    }
}


//Load session log details
function LoadAllLoggedInSessions()
{
   // var selectedUserId = $("#drpUser option:selected").val();
    var selectedUserParam = $('#drpUser').data('ddslick');
    var selectedUserId = selectedUserParam.selectedData.value;
    var param = JSON.stringify({ 'userId': selectedUserId });

    $.ajax({
        url: '/UserSessionLog/GetAllLoggedInSessions',
        type: 'post',
        data:param,
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {


            BindSessionLogsToDataTable(inputParam);

        }

    });

}

function BindSessionLogsToDataTable(inputParam) {


    if (inputParam != null && inputParam.length > 0)
    {
        $("#dvAlert").html('');
        $("#dvAlert").hide();
        $("#btnClearSesssion").attr("disabled",false);

        $("#dvUserSessionTable").show();
        $("#dvBtnControlGroup").show();

        //First destroy the datatable before reinitialising
        $('#tblUserSessionLog').dataTable().fnDestroy();

        var tableSessionLog = $('#tblUserSessionLog').DataTable({
            "data": inputParam,
            select: "single",

            "columnDefs": [{
                "targets": [0,1],
                "visible": false
            }],

            "columns": [
                { "data": "UserSessionLogId", "width": "5%" },
                { "data": "UserId", "width": "5%" },
                {
                    "data": "ProfilePicURL",
                    "width": "5%",
                    render: function (data, type, row) {

                        if (type === 'display') {
                            if (data == null) {
                                return '<img  class="img-rounded" style="height:50px;width:50px" src="../../Images/BlankProfilePic.png">';

                            }
                            else {
                                return '<img  class="img-rounded" style="height:50px;width:50px" src="' + data + '">';
                            }

                        }
                        return data;


                    },
                    className: "dt-body-center"
                },               
                { "data": "Username", "width": "10%" },
                { "data": "UserGroupName", "width": "10%" },
                { "data": "UserFullName", "width": "10%" },
                { "data": "UserCallingName", "width": "10%" },
                { "data": "IPAddress", "width": "10%" },
                {
                     "data": "CountryCode",
                     "width": "5%",
                     render: function (data, type, row) {

                         if (type === 'display') {
                             if (data != null) {
                                 var imagePath='../../Images/flags/' + data + '.png';
                                 return '<img  class="img-thumbnail" style="height:50px;width:100px" src="' + imagePath + '">';

                             }
                             else {
                                 return '<img  class="img-thumbnail" style="height:50px;width:50px">';
                             }

                         }
                         return data;


                     },
                     className: "dt-body-center"
                 },
                { "data": "Country", "width": "10%" },
                { "data": "City", "width": "10%" },
                { "data": "Region", "width": "10%" },
                { "data": "LoggedInTimestamp", "width": "10%" },
                { "data": "LoggedOffTimestamp", "width": "10%" },


            ],
          //  "order": [[0, 'desc']]
        });

        //dtErrorTraceDetails = $('#tblRuntimeErrorTrace').DataTable();



        //$('#tblRuntimeErrorTrace').on('click', 'tr', function () {

        //    var currentIndex = $(this).index();

        //    //To prevent mouse click on invalid spots
        //    if (typeof (currentIndex) == 'undefined') {
        //        return;
        //    }

        //    var data = dtErrorTraceDetails.row(this).data();

        //    if (typeof (data) === 'undefined') {
        //        return;
        //    }

        //    var ErrorLoggerId = data.ErrorLoggerId;


        //    var $chkObj = $(this).find(':checkbox');


        //    if ($chkObj.prop('checked')) {

        //        var suppArr = {};
        //        suppArr["ErrorLoggerId"] = ErrorLoggerId;




        //        //Remove from the Approval List if same TempConversionHeaderID row is already existing 
        //        for (var i = 0; i < removeErrorList.length; i++) {
        //            if (removeErrorList[i].ErrorLoggerId == ErrorLoggerId) {
        //                removeErrorList.splice(i, 1);
        //                break;
        //            }
        //            else {
        //                continue;
        //            }

        //        }

        //        //Add selected row details to the list 
        //        removeErrorList.push(suppArr);




        //    }
        //    else if (!$chkObj.prop('checked')) {

        //        //Remove from the Approval List 
        //        for (var i = 0; i < removeErrorList.length; i++) {
        //            if (removeErrorList[i].ErrorLoggerId == ErrorLoggerId) {
        //                removeErrorList.splice(i, 1);
        //                break;
        //            }
        //            else {
        //                continue;
        //            }

        //        }






        //    }



        //});

    }
    else {
       
        $("#dvAlert").show();
        $("#dvAlert").html('<i class="glyphicon glyphicon-info-sign"></i>&nbsp;' + 'No Log details available for this user.');
        $("#dvUserSessionTable").hide();
        $("#btnClearSesssion").attr("disabled", true);
    }
}


/***************************/

/***Section to clear session details****/

//Clear all session details except the last session details
//Session details must not be allowed to be cleared for all users at once.
//Session details can be cleared for 1 user at a time only
function ClearSessionDetails()
{
    var selectedUserParam = $('#drpUser').data('ddslick');
    var selectedUserId = selectedUserParam.selectedData.value;
    var selectedIndex = selectedUserParam.selectedIndex;
    var selectedUsername=selectedUserParam.selectedData.text.split("-");

    if (selectedIndex > 0)
    {
        var param = JSON.stringify({ 'userId': selectedUserId });


            swal({
                title: "Are you sure to clear the session details for " + selectedUsername[0] + "?",
                text: "All the session details except the latest details will be cleared.",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#01DF74",
                confirmButtonText: "Yes, clear session details..",
                showLoaderOnConfirm: true,
                closeOnConfirm: false,
                height: "100px",
                width: "100px"
            },
     function () {

         $.ajax({
             url: '/UserSessionLog/ClearSessionDetails',
             data:param,
             type: 'post',
             contentType: 'application/json',
             success: function (data) {
                 if (data) {
                     swal({
                         title: "",
                         text: "Session details cleared successfully for " + selectedUsername[0] + "!",
                         type: "success",
                         timer: 3000,
                         showConfirmButton: false
                     });

                     LoadAllLoggedInSessions();
                 }
                 else {
                     swal({
                         title: "",
                         text: "Oops!..Unable to clear the session details.",
                         type: "error",
                         timer: 3000,
                         showConfirmButton: false
                     });

                 }
             },


         });

     });
    }
    else
    {
        swal({
            title: "",
            text: "Clearing session details for 'All Users' option is not permitted.",
            type: "error",
            timer: 5000,
            showConfirmButton: false
        });
    }
   

}


/****************************************/