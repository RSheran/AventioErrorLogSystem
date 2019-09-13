$(document).ready(function () {

    $("#btnChkConnection").click(IsDestinationURLValid);
    $("#btnMigrate").click(PushDataToTargetDatabase);
    $("#btnReset").click(ResetControls);
    $("#errorStrip").hide();
    $("#successStrip").hide();
    $("#dvCollectionList").hide();
});

var isConnectionSucceeded = false;

var collectionArr=[],loadedCollectioArr=[];

function GetAllCollections()
{
    $.ajax({
        url: '/DataMigration/GetAllCollections',
        type: 'post',
        contentType: 'application/json',
        async: false,
        success: function (inputParam) {

            BindAllCollections(inputParam);


        }

    });
}

function BindAllCollections(collectionList)
{
    var divContent = "";
    var collectionChkBoxId;

    $("#dvCollectionList").html("");

    if (collectionList != null && collectionList.length > 0)
    {
       

        $("#dvCollectionList").append('<label style="margin-left:2%">Select the tables/collections to migrate</label><hr  style="margin-left:2%"/>');
      
         
        //Append the 'All' option
        currentFrame = "";
        collectionChkBoxId = "chkCollection_AllCollections";

        currentFrame = [

          '<div class="row" style="margin-left:2%">',
          '<input id="' + collectionChkBoxId + '" class="col-sm-1 col-md-1 col-xs-1" name="chkIsCorrect" type="checkbox" onclick="AddCollectionToArray(\'' + collectionChkBoxId + '\')" />',
          '<label class="col-sm-8 col-md-8 col-xs-8 control-label" style="text-align: left;font-family:Segoe UI"><i>All Collections</i></label>',
          '</div>'

        ];
        divContent += GetHtml(currentFrame);

        for (var j = 0; j < collectionList.length; j++)
        {
            loadedCollectioArr.push(collectionList[j]);

            currentFrame = "";
            collectionChkBoxId = "chkCollection_" + collectionList[j];

            currentFrame = [
        
              '<div class="row" style="margin-left:2%">',
              '<input id="' + collectionChkBoxId + '" class="col-sm-1 col-md-1 col-xs-1" name="chkIsCorrect" type="checkbox" onclick="AddCollectionToArray(\'' + collectionChkBoxId + '\')"  />',
              '<label class="col-sm-8 col-md-8 col-xs-8 control-label" style="text-align: left;font-family:Segoe UI">' + collectionList[j] + '</label>',              
              '</div>'
              
            ];

            divContent += GetHtml(currentFrame);
        }

        $("#dvCollectionList").append(divContent);
      
      
      
       
    }
}



function AddCollectionToArray(clickedCheckBoxId)
{
    var isChecked = $('#' + clickedCheckBoxId).is(":checked");
    var splittedVal = clickedCheckBoxId.split('_');

    if (clickedCheckBoxId == 'chkCollection_AllCollections')
    {
        if (isChecked == true) {
            SelectOtherCollectionChkBoxes();
            AddAllCollectionsToArray();
        }
        else
        {
            UnselectOtherCollectionChkBoxes();
            collectionArr = [];
          
        }


    }
    else {

        if (isChecked == true) {
            RemoveElement(splittedVal[1]);
            collectionArr.push(splittedVal[1]);
           
        }
        else {
            RemoveElement(splittedVal[1]);
          
        }
    }
   
}

function RemoveElement(collectionName)
{
    for (var i = 0; i < collectionArr.length; i++)
    {
        if (collectionArr[i] == collectionName) {
            collectionArr.splice(i, 1);
            break;
        }
        else {
            continue;
        }
    }
}

function SelectOtherCollectionChkBoxes()
{
    for (var i = 0; i < loadedCollectioArr.length; i++)
    {
        $('#chkCollection_' + loadedCollectioArr[i]).prop("disabled", true);
        $('#chkCollection_' + loadedCollectioArr[i]).prop("checked", true);
    }
}

function UnselectOtherCollectionChkBoxes() {
    for (var i = 0; i < loadedCollectioArr.length; i++) {
        $('#chkCollection_' + loadedCollectioArr[i]).prop("disabled", false);
        $('#chkCollection_' + loadedCollectioArr[i]).prop("checked", false);
    }
}

function AddAllCollectionsToArray()
{
    collectionArr = [];
    for (var i = 0; i < loadedCollectioArr.length; i++)
    {
        collectionArr.push(loadedCollectioArr[i]);
    }
    
}


function GetHtml(template) {
    return template.join('\n');
}

function IsDestinationURLValid()
{
    var targetMongoDbURL = $("#txtTargetDbURL").val();//"mongodb://rajindra:rajindra123@aventiocluster-shard-00-00-eb7p0.mongodb.net:27017,aventiocluster-shard-00-01-eb7p0.mongodb.net:27017,aventiocluster-shard-00-02-eb7p0.mongodb.net:27017/admin?replicaSet=AventioCluster-shard-0&ssl=true";

    if (targetMongoDbURL.length > 0) {
        var param = JSON.stringify({ 'targetMongoDbURL': targetMongoDbURL });

        $.ajax({
            url: '/DataMigration/IsDestinationURLValid',
            type: 'post',
            data: param,
            contentType: 'application/json',
            success: function (inputParam) {

                if (inputParam == false) {
                    $("#errorStrip").show();
                    $("#successStrip").hide();
                    $("#userAlertErr").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp; Oops!..The URL failed in connection..');
                    $("#userAlertErr").show().delay(3000).fadeOut();

                    $("#btnMigrate").attr("disabled", true);
                }
                else {
                    $("#errorStrip").hide();
                    $("#successStrip").show();
                    $("#userAlertSuccess").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp;The URL succeeded in connection');
                    $("#userAlertSuccess").show().delay(3000).fadeOut();


                    //Show the collection name div
                    $("#dvCollectionList").show();
                    GetAllCollections();


                    $("#btnMigrate").attr("disabled", false);
                }

                isConnectionSucceeded = inputParam;

            }

        });
    }
    else
    {
        $("#errorStrip").show();
        $("#successStrip").hide();
        $("#userAlertErr").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp; You must enter a URL..');
        $("#userAlertErr").show().delay(3000).fadeOut();
    }
}

function PushDataToTargetDatabase()
{
    var targetMongoDbURL = $("#txtTargetDbURL").val();//"mongodb://rajindra:rajindra123@aventiocluster-shard-00-00-eb7p0.mongodb.net:27017,aventiocluster-shard-00-01-eb7p0.mongodb.net:27017,aventiocluster-shard-00-02-eb7p0.mongodb.net:27017/admin?replicaSet=AventioCluster-shard-0&ssl=true";
    var param = JSON.stringify({ 'targetMongoDbURL': targetMongoDbURL, 'collectionArr': collectionArr });

    if (targetMongoDbURL.length > 0) {
        if (isConnectionSucceeded == true) {
            if (collectionArr.length > 0)
            {
                        swal({
                    title: "Are you sure to migrate data to the provided target database ?",
                    //text: "You will not be able to recover this imaginary file!",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#01DF74",
                    confirmButtonText: "Yes,migrate..",
                    showLoaderOnConfirm: true,
                    closeOnConfirm: false,
                    height: "100px",
                    width: "100px"
                },
        function () {

            $.ajax({
                url: '/DataMigration/PushDataToTargetDatabase',
                type: 'post',
                data: param,
                contentType: 'application/json',
                success: function (data) {
                    if (data) {
                        swal({
                            title: "",
                            text: "The data has been successfully migrated! ",
                            type: "success",
                            timer: 3000,
                            showConfirmButton: false
                        });

                        ResetControls();
                    }
                    else {
                        swal({
                            title: "",
                            text: "Oops!..Unable to perform the migration.",
                            type: "error",
                            timer: 3000,
                            showConfirmButton: false
                        });

                    }
                },


            });

        });
            }
            else {
                swal({
                    title: "",
                    text: "You must select at least 1 collection to migrate.",
                    type: "error",
                    timer: 3000,
                    showConfirmButton: false
                });
            }
        

        }
        else {

            swal({
                title: "",
                text: "Oops!..The above URL failed in connection.",
                type: "error",
                timer: 3000,
                showConfirmButton: false
            });
        }
    }
    else {
        $("#errorStrip").show();
        $("#successStrip").hide();
        $("#userAlertErr").html('<span class="glyphicon glyphicon-exclamation-sign" aria-hidden="true"></span> &nbsp;&nbsp; You must enter a URL..');
        $("#userAlertErr").show().delay(3000).fadeOut();
    }

}

function ResetControls()
{
    $("#txtTargetDbURL").val('');
    collectionArr = [];
    loadedCollectioArr = [];
}