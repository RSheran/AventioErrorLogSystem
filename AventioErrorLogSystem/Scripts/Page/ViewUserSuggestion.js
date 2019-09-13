$(document).ready(function () {


    LoadAllLoggedInSessions();
});

//Load session log details
function LoadAllLoggedInSessions() {
  
    var param = JSON.stringify({ 'userId': 0 });

    $.ajax({
        url: '/SystemUserDirectory/GetAllLoggedInSessions',
        type: 'post',
        data: param,
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {


            BindSessionLogsToDataTable(inputParam);

        }

    });

}

function BindSessionLogsToDataTable(inputParam) {


    if (inputParam != null && inputParam.length > 0) {
        $("#dvAlert").hide();
        $("#dvUserSessionTable").show();
        $("#dvBtnControlGroup").show();

        //First destroy the datatable before reinitialising
        $('#tblUserSessionLog').dataTable().fnDestroy();

        var tableSessionLog = $('#tblUserSessionLog').DataTable({
            "data": inputParam,
            select: "single",

            //"columnDefs": [{
            //    "targets": [0,1],
            //    "visible": false
            //}],

            "columns": [
                //{ "data": "UserSessionLogId", "width": "5%" },
                //{ "data": "UserId", "width": "5%" },
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
                }

                //{ "data": "Username", "width": "10%" },
                //{ "data": "UserGroupName", "width": "10%" },
                //{ "data": "UserFullName", "width": "10%" },
                //{ "data": "UserCallingName", "width": "10%" },               
                //{ "data": "LoggedInTimestamp", "width": "30%" },
                //{ "data": "LoggedOffTimestamp", "width": "20%" },


            ],
            //  "order": [[1, 'asc']]
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
        $("#dvUserSessionTable").hide();
    }
}