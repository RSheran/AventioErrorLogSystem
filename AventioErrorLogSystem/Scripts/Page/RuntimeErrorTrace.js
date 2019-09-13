$(document).ready(function () {
        
    LoadAllRuntimeErrors();
    $("#btnRefresh").click(LoadAllRuntimeErrors);
    $("#btnReset").click(ResetControls);
    $("#btnDeleteSelected").click(DeleteSelectedErrors);
    $("#btnDeleteAll").click(DeleteAllErrors);


});

//Global Data Array

var removeErrorList = []; //To store all errors to be deleted

var dtErrorTraceDetails;//To capture the initiated datatable with its data

/**Data Loading section***/

function LoadAllRuntimeErrors() {

   
    $.ajax({
        url: '/RuntimeErrorTrace/GetAllRuntimeErrors',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {
                   

            BindErrorTracesToDataTable(inputParam);

        }

    });

}


function BindErrorTracesToDataTable(inputParam)
{
   

    if (inputParam != null && inputParam.length > 0)
    {
        $("#dvAlert").hide();
        $("#dvErrorTraceTable").show();
        $("#dvBtnControlGroup").show();

        //First destroy the datatable before reinitialising
         $('#tblRuntimeErrorTrace').dataTable().fnDestroy();

        var table = $('#tblRuntimeErrorTrace').DataTable({
            "data": inputParam,
            select: "single",
          
            //"columnDefs": [{
            //    "targets": [7, 8, 9, 10, 11, 12],
            //    "visible": false
            //}],

            "columns": [
              
                { "data": "TimeStamp", "width": "30%"},
                { "data": "Schema", "width": "30%" },
                { "data": "Username", "width": "10%" },
                { "data": "Exception_Error", "width": "30%" },
                {
                    "data": "ErrorLoggerId",
                    "width": "10%",
                    render: function (data, type, row) {
                       
                            if (type === 'display') {
                                return '<input type="checkbox">';
                            }
                            return data;
                        

                    },
                    className: "dt-body-center"
                },
           

            ],
           "order": [[1, 'asc']]
        });

        dtErrorTraceDetails = $('#tblRuntimeErrorTrace').DataTable();



        $('#tblRuntimeErrorTrace').on('click', 'tr', function () {

            var currentIndex = $(this).index();

            //To prevent mouse click on invalid spots
            if (typeof (currentIndex) == 'undefined') {
                return;
            }

            var data = dtErrorTraceDetails.row(this).data();

            if (typeof (data) === 'undefined') {
                return;
            }

            var ErrorLoggerId = data.ErrorLoggerId;
            

            var $chkObj = $(this).find(':checkbox');


            if ($chkObj.prop('checked')) {

                var suppArr = {};
                suppArr["ErrorLoggerId"] = ErrorLoggerId;
              



                //Remove from the Approval List if same TempConversionHeaderID row is already existing 
                for (var i = 0; i < removeErrorList.length; i++) {
                    if (removeErrorList[i].ErrorLoggerId == ErrorLoggerId) {
                        removeErrorList.splice(i, 1);
                        break;
                    }
                    else {
                        continue;
                    }

                }

                //Add selected row details to the list 
                removeErrorList.push(suppArr);

               

              
            }
            else if (!$chkObj.prop('checked')) {

                //Remove from the Approval List 
                for (var i = 0; i < removeErrorList.length; i++) {
                    if (removeErrorList[i].ErrorLoggerId == ErrorLoggerId) {
                        removeErrorList.splice(i, 1);
                        break;
                    }
                    else {
                        continue;
                    }

                }

                




            }



        });

    }
    else
    {
        $("#dvBtnControlGroup").hide();
        $("#dvAlert").show();
        $("#dvErrorTraceTable").hide();
    }
}

/************************/

/***Section to delete all/selected errors***/

function DeleteSelectedErrors() {

    if (removeErrorList.length>0)
    {
        var dataObject = JSON.stringify({ 'errorTraceArr': removeErrorList });

        swal({
            title: "Are you sure to delete  the selected runtime errors?",
            //text: "You will not be able to recover this imaginary file!",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#01DF74",
            confirmButtonText: "Yes, delete..",
            showLoaderOnConfirm: true,
            closeOnConfirm: false,
            height: "100px",
            width: "100px"
        },
function () {

    $.ajax({
        url: '/RuntimeErrorTrace/DeleteSelectedErrorTraces',
        type: 'post',
        data: dataObject,
        contentType: 'application/json',
        success: function (data) {
            if (data) {
                swal({
                    title: "",
                    text: "The selected Error Traces have been deleted successfully! ",
                    type: "success",
                    timer: 3000,
                    showConfirmButton: false
                });

                LoadAllRuntimeErrors();
            }
            else {
                swal({
                    title: "",
                    text: "Oops!..Unable to perform the deletion.",
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
            text: "You must select at least one error to delete.",
            type: "error",
            timer: 3000,
            showConfirmButton: false
        });
    }

  
}

function DeleteAllErrors()
{
    swal({
        title: "Are you sure to delete all the runtime errors?",
        //text: "You will not be able to recover this imaginary file!",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#01DF74",
        confirmButtonText: "Yes, delete all..",
        showLoaderOnConfirm: true,
        closeOnConfirm: false,
        height: "100px",
        width: "100px"
    },
  function () {
    
      $.ajax({
          url: '/RuntimeErrorTrace/DeleteAllErrorTraces',
          type: 'post',
          contentType: 'application/json',
          success: function (data) {
              if (data) {
                  swal({
                      title: "",
                      text: "All Error Traces deleted successfully! ",
                      type: "success",
                      timer: 3000,
                      showConfirmButton: false
                  });

                  ResetControls();
              }
              else {
                  swal({
                      title: "",
                      text: "Oops!..Unable to perform the deletion.",
                      type: "error",
                      timer: 3000,
                      showConfirmButton: false
                  });

              }
          },


      });

  });
}

function ResetControls()
{
    LoadAllRuntimeErrors();
    removeErrorList = null;
    removeErrorList = [];
}


/****************************************/