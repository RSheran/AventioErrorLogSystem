$(document).ready(function () {
          
   LoadAllSystemUsers();
  
});

/***Data loading section****/

//Load users for directory
function LoadAllSystemUsers()
{

    $.ajax({
        url: '/SystemUserDirectory/GetAllUsers',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {


            BindUsersToDataTable(inputParam);

        }

    });
}

function BindUsersToDataTable(inputParam) {


    if (inputParam != null && inputParam.length > 0) {
        $("#dvAlert").hide();
        $("#dvUserTable").show();
        $("#dvBtnControlGroup").show();

        //First destroy the datatable before reinitialising
        $('#tblUser').dataTable().fnDestroy();

        var table = $('#tblUser').DataTable({
            "data": inputParam,
            select: "single",

            "columnDefs": [{
                "targets": [0,1],
                "visible": false
            }],

            "columns": [

                { "data": "UserId", "width": "10%" },
                { "data": "UserGroupId", "width": "10%" },
                {
                    "data": "ProfilePicURL",
                     "width": "5%",
                     render: function (data, type, row) {

                         if (type === 'display') {
                             if (data == null)
                             {
                                 return '<img  class="img-rounded" style="height:50px;width:50px" src="../../Images/BlankProfilePic.png">';

                             }
                             else
                             {
                                 return '<img  class="img-rounded" style="height:50px;width:50px" src="' + data + '">';
                             }
                           
                         }
                         return data;


                     },
                     className: "dt-body-center"
                },
                { "data": "Username", "width": "10%" },
                { "data": "UserGroupName", "width": "10%" },
                { "data": "FullName", "width": "10%" },
                { "data": "CallingName", "width": "10%" },
                { "data": "Email", "width": "10%" },
                {
                    "data": "IsActive",
                    "width": "5%",
                     render: function (data, type, row) {
                        return CommonStatusReturn(data);
                    },
                    className: "dt-body-center"
                },

                {
                    "data": "IsLoggedIn",
                    "width": "5%",
                    render: function (data, type, row) {
                        return CommonStatusReturn(data);
                    },
                    className: "dt-body-center"
                },
                { "data": "LastLogInTime", "width": "30%" },
                { "data": "LastLogOffTime", "width": "20%" },
                {
                    "data": "UserId",
                    "width": "5%",
                    render: function (data, type, row) {

                        var redirectURL = "/UpdateUser/Index?userId=" + data;
                      
                        if (type === 'display') {
                           
                            return '<a href="' + redirectURL + '" target="_blank"><i class="fa fa-edit"></a>';

                       }
                        return data;


                    },
                    className: "dt-body-center"
                },


            ],
            "order": [[1, 'asc']]
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
        $("#dvBtnControlGroup").hide();
        $("#dvAlert").show();
        $("#dvUserTable").hide();
    }
}

//Common function to return html 'tick' or 'cross-mark'
function CommonStatusReturn(status) {
    if (status == true) {
        return '<i class="glyphicon glyphicon-ok" style="color:green"></i>';
    }
    else {
        return '<i class="glyphicon glyphicon-remove" style="color:red"></i>';
    }
}

/******************************/



