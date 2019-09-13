function CallServerAsync(inputMethod, Param, CallBackFunc) {
    $.ajax({
        url: '/UserMessage/' + inputMethod,
        type: 'post',
        contentType: 'application/json',
        data: Param,
        async: true,
        success: function (inputParam) {
            CallBackFunc(inputParam);
        }

    });
}

function CallServer(inputMethod, Param, CallBackFunc) {
    $.ajax({
        url: '/UserMessage/' + inputMethod,
        type: 'post',
        contentType: 'application/json',
        data: Param,
        async: false,
        success: function (inputParam) {
            CallBackFunc(inputParam);
        }

    });
}

$(document).ready(function () {

    GetNotReadMessageCount();
    //$("#tabViewMsg").addClass("active");
    //$("#tabViewMsg").removeClass("active");
    //$("#tabreplyToMsg").addClass("active");
    //$("#tabreplyToMsg").removeClass("active");


    // --------------------Send message tab------------------------------
    $(function () {
        $("#drpSMGUserList").chosen();
    });

    SMGGetUserList();

    $("a[href='#tabSendMsg']").on('show.bs.tab', function (e) {
        SMGGetUserList();
        GetNotReadMessageCount();
    });

    



    $("#btnSMGSend").click(SMGSendMessage);
    $("#btnSMGCancel").click(SMGClearContent);
    //---------------------End of send message tab-------------------------


    //---------------------View Sent Messages ------------------------------
    $("a[href='#tabviewSentMsg']").on('show.bs.tab', function (e) {
        VSMGLoadSentMessages();
        GetNotReadMessageCount();
    });

    $("#btnVSMGDelete").click(VSMGDeleteMessges);
    $("#btnVSMGCancel").click(VSMGCancel);
    $("#btnVSMGDeleteAll").click(VSMGDeleteAll);



    //---------------------End of send message tab--------------------------

    //---------------------Reply To Message---------------------------------
    $("a[href='#tabreplyToMsg']").on('show.bs.tab', function (e) {
        RMSGLoadMessageList();
        GetNotReadMessageCount();
    });
    $(document).on("click", ".myclass", function (event) {
        RMSGMessagesLoad($(this).attr('id'));
    });
    
    $(document).on("click", ".myCancelbtn", function (event) {
        RMSGLoadMessageList();        
    });

    $(document).on("click", ".mySendbtn", function (event) {
        RMSGSendMessage($(this).attr('id'));
    });

    //---------------------End Of Reply To Message -------------------------
});

function GetNotReadMessageCount() {
    CallServerAsync("GetMessageCount", '', BindMessageCount);
}

function BindMessageCount(count) {
    if (count != null && count >= 0) {
        $("#spnUnreadCount").text(count);
    }
}


// --------------------Send message tab------------------------------
function SMGSendMessage() {
    var userList = $("#drpSMGUserList").chosen().val();
    var subject = $("#txtSMGSubject").val();
    var message = $("#txtSMGMessage").val();

    var Param = JSON.stringify({ 'UserList': userList, 'Subject': subject, 'Message': message });
    if (userList != null && userList.length != 0 && subject.trim() != '' && message.trim() != '') {
        swal({
            title: "Are you sure to send this message?",
            // text: "You want to send this message..",
            allowOutsideClick: false,
            showConfirmButton: true,
            showCancelButton: true,
            closeOnConfirm: true,
            closeOnCancel: true,
            confirmButtonText: 'Yes',
            confirmButtonColor: '#8CD4F5',
            cancelButtonText: 'No',
            closeOnConfirm: false,
            closeOnCancel: false
        },
 function (isConfirm) {

     if (isConfirm) {
         CallServer('SMGSendMessage', Param, SMGSendMessageCallBack);

     } else {
         swal("Cancelled", "Oops!..Unable to send the message(s).", "error");
     }
 });
    }
    else {
        swal({
            title: "Check fields to complete",
            text: "Error in sending message..",
            type: "error",
            timer: 2500,
            showConfirmButton: false
        });
    }
}

function SMGSendMessageCallBack(input) {
    if (input == 0) {
        swal({
            title: "",
            text: "Messages Sent..",
            type: "success",
            timer: 3000,
            showConfirmButton: false
        });
        SMGClearContent();
    }
    else {
        swal({
            title: "",
            text: "Error in sending message..",
            type: "error",
            timer: 3000,
            showConfirmButton: false
        });
    }
}

function SMGGetUserList() {
    CallServerAsync('SMGGetUserList', '', SMGBindUsers);
}

function SMGBindUsers(input) {
    var list = null;
    if (input.length > 0) {
        $('#drpSMGUserList').empty();
        $.each(input, function (i, data) {
            $('#drpSMGUserList').append("<option " + " value=" + data.UserId + ">" + data.FullName + "</option>").trigger('chosen:updated');;
        });
    }
}

function SMGClearContent() {
    $("#drpSMGUserList").val('');
    $("#drpSMGUserList").trigger('chosen:updated');
    $("#txtSMGSubject").val('');
    $("#txtSMGMessage").val('');
}

//---------------------End of send message tab----------------------------------------------


//--------------------View Sent Message tab-------------------------------------------------

function VSMGLoadSentMessages() {

    swal({
        title: "Loading",
        text: 'Messages are loading please wait...',
        showCancelButton: false,
        showConfirmButton: false
    });


    $(".swal2-modal").css('background-color', '#000');//Optional changes the color of the sweetalert 
    $(".swal2-container.in").css('background-color', 'rgba(43, 165, 137, 0.45)');
    CallServerAsync('VSMGLoadSentMessages', '', VSMGBindSendMessageData);
}


var MessageList;
var DeleteList = [];
function VSMGBindSendMessageData(ResultList) {
    $('#tblVSMGSendMessages').dataTable().fnDestroy();

    var table = $('#tblVSMGSendMessages').DataTable({
        "data": ResultList,
        select: "single",
        "columns": [

            { "data": "UserMessageId", "width": "0%", 'visible': false },
            {
                "data": "UserDetails",
                "width": "20%",
                render: function (data, display) {
                    if (data.ProfilePicture != null) {
                        return "<span style='border-radius: 2px; border:2px black solid;font-size: 11px; display:inline-block;text-align: center'>" +
                            "<img style='height: 40px; width: 40px;' class='img-circle' src='" + data.ProfilePicture + "'></br>&nbsp " + data.FullName + "&nbsp</span>";
                    }
                    else {
                        return "<span style='border-radius: 2px; border:2px black solid;font-size: 11px; display:inline-block;text-align: center'>" +
                            "&nbsp " + data.FullName + "&nbsp</span>";
                    }
                },
                className: "dt-body-center"
            },
            { "data": "MessageSentDate", "width": "10%" },
            {
                "data": "MessageDeliveryDate",
                "width": "10%",
                render: function (data, type, raw) {
                    if (data == null) {
                        return 'Not Read'
                    }
                    else return data;
                }
            },
            { "data": "MessageSubject", "width": "10%" },
            { "data": "MessageBody", "width": "40%" },
            {
                "data": "IsDelete",
                "width": "10%",
                render: function () {
                    return '<input type="checkbox">';
                },
                className: "dt-body-center"
            },
        ],
        "order": [[1, 'asc']]
    });
    swal.close();
    MessageList = $('#tblVSMGSendMessages').DataTable();


    $('#tblVSMGSendMessages').on('click', 'tr', function () {

        var currentIndex = $(this).index();

        //To prevent mouse click on invalid spots
        if (typeof (currentIndex) == 'undefined') {
            return;
        }

        var data = MessageList.row(this).data();

        if (typeof (data) === 'undefined') {
            return;
        }

        var ErrorLoggerId = data.UserMessageId;

        var $chkObj = $(this).find(':checkbox');
        if ($chkObj.prop('checked')) {

            //Remove from the Approval List if same TempConversionHeaderID row is already existing 
            for (var i = 0; i < DeleteList.length; i++) {
                if (DeleteList[i] == ErrorLoggerId) {
                    DeleteList.splice(i, 1);
                    break;
                }
                else {
                    continue;
                }

            }
            //Add selected row details to the list 
            DeleteList.push(parseInt(ErrorLoggerId));
        }
        else if (!$chkObj.prop('checked')) {
            for (var i = 0; i < DeleteList.length; i++) {
                if (DeleteList[i] == ErrorLoggerId) {
                    DeleteList.splice(i, 1);
                    break;
                }
                else {
                    continue;
                }
            }
        }
    });
    MessageList = $('#tblVSMGSendMessages').DataTable();
}


function VSMGDeleteMessges() {

    if (DeleteList.length == 0) { return; }
    else {
        swal({
            title: "Deleting",
            text: 'Messages are deleting please wait...',
            showCancelButton: false,
            showConfirmButton: false
        });
        var Param = JSON.stringify({ 'DeleteList': DeleteList });
        CallServerAsync('VSMGDeleteMessages', Param, VSMGBindDeleteResult);
    }

}

function VSMGBindDeleteResult(Result) {
    if (Result == null || Result == 1) {
        swal({
            title: "Error",
            text: 'Please Contact Administrator...',
            showCancelButton: false,
            showConfirmButton: false,
            timer: 1000
        });
    }
    else if (Result == 0) {
        DeleteList = [];
        VSMGBindSendMessageData(null);
    }
    else {
        DeleteList = [];
        VSMGBindSendMessageData(Result);
    }
}

function VSMGCancel() {
    $(':checkbox').prop('checked', false);
}

function VSMGDeleteAll() {
    swal({
        title: "Are you sure to delete all the Messages?",
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
      CallServerAsync('VSMGDeleteAllMessages', '', VSMGBindSendMessageData);
  });
}


//-------------------End of View sent Message tab------------------------------------------

//-------------------Start OF Reply To Messages---------------------------------------------

function RMSGLoadMessageList() {
    CallServer('RMSGLoadMessageList', '', RMSGBindMessageList)

}

function RMSGBindMessageList(MessageList) {
    if (MessageList != null) {
        $("#tabreplyToMsg").empty();
        
        $.each(MessageList, function (i, value) {
            var SentBackground = (value.IsSent == false ? "ReciveBackground" : "SentBackground");
            var IsRead = value.IsMessageRead == false ? "IsNotRead" : "";
            var IsSent = (value.IsSent == false ? "IsNotSent-Time" : "IsSent-Time");
            var IsSentCss = (value.IsSent == false ? "IsNotSent" : "IsSent");
                $("#tabreplyToMsg").append(
                "<div class='row'>"+
                "<span class='col-sm-2' style='border-radius: 2px; border:2px black solid;font-size: 11px; display:inline-block; text-align:center'>" +
        "<img style='height: 40px; width: 40px;' class='img-circle' src='" + value.UserDetails.ProfilePicture + "'></br>&nbsp " + value.UserDetails.FullName + "&nbsp</span>" +
                "</div>" +
                "<div class='row'>" +
                "<div id ='" + value.UserMessageId + "' class='myclass container1 " + IsRead + " col-sm-10' contenteditable='false'>" +
                "<h5><strong> " + value.MessageSubject + "</strong></h5>" +
                "<hr>"+
                "<p>" + value.MessageBody + "</p>" +
                "<span class='" + IsSent + "'>" + value.MessageSentDate + "</span>" +
                "</div>" +
                "</div>");            
        });
    }
}

function RMSGMessagesLoad(MessageID) {
    if (parseInt(MessageID) > 0) {
        var Param = JSON.stringify({ 'MessageID': MessageID });
        CallServerAsync("RMSGLoadPreviousMessages", Param,RSMGViewOldMessages)
    }
}

function RSMGViewOldMessages(MessageList) {
    if (MessageList.length > 0) {
        var lastMessageID = 0;
        $("#tabreplyToMsg").empty();
        var htmlPage = "<div class='row'>" +
                "<span class='col-sm-2' style='border-radius: 1px; border:1px black solid;font-size: 11px; display:inline-block; text-align:center;'>" +
        "<img style='height: 30px; width: 30px;' class='img-circle' src='" + MessageList[0].UserDetails.ProfilePicture + "'></br>&nbsp " + MessageList[0].UserDetails.FullName + "&nbsp</span>" +
                "</div>" +
            "<div id='scrollBox' class='myClass2 col-sm-10' style='overflow:auto; height:300px; border: thin solid black;'>";
        $.each(MessageList, function (i, value) {

            var SentBackground = (value.IsSent == false ? "ReciveBackground" : "SentBackground");
            var IsRead = value.IsMessageRead == false ? "IsNotRead" : "";
            var IsSent = (value.IsSent == false ? "IsNotSent-Time" : "IsSent-Time");
            var IsSentCss = (value.IsSent == false ? "IsNotSent" : "IsSent");
                lastMessageID = value.UserMessageId;
                htmlPage +=
                        "<div id ='" + value.UserMessageId + "' class='myClass2 container1 " + SentBackground +" "+ IsRead + " " + IsSentCss + "col-sm-10'' contenteditable='false'>" +
                        "<h5 class='" + IsSentCss + "'><strong> " + value.MessageSubject + "</strong></h5>  </br>" +
                        "<hr>" +
                        "<p class='" + IsSentCss + "'>" + value.MessageBody + "</p> </br>" +
                        "<p class='" + IsSent + "'>" + value.MessageSentDate + "</p>" +
                        "</div>";
            
        });
        htmlPage += "<div class='container1' contenteditable='false'>" +                            
                            "<div class='form-group  col-sm-6'>" +
                                    "<label>Subject</label>" +
                                    "<input type='text' class='form-control' id='txtRMSGSubject'>" +
                            "</div>" +
                            "<div class='form-group  col-sm-8'>" +
                                    "<label for='txtRMSGMessage'>Message</label>" +
                                    "<textarea class='form-control' rows='3' id='txtRMSGMessage'></textarea>" +
                            "</div>" +
                            "<div class='form-group  col-sm-8'>" +
                                "<button type='button' id ='" + lastMessageID + "' class='mySendbtn btn btn-info'>Send</button>" +
                                "<button type='button' id='btnRMSGCancel' class='myCancelbtn btn'>Cancel</button>";
        "</div>" +
        "</div>";
        htmlPage += "</div>";
        $("#tabreplyToMsg").append(htmlPage);
    }

    console.log(MessageList);
}

function RMSGSendMessage(MessageID) {
    if (parseInt(MessageID) > 0) {
        var subject = $("#txtRMSGSubject").val();
        var message = $("#txtRMSGMessage").val();

        var Param = JSON.stringify({ 'MessageID' : MessageID, 'Subject': subject, 'Message': message });
        if (subject.trim() != '' && message.trim() != '') {
            swal({
                title: "Are you sure to send this message?",
                // text: "You want to send this message..",
                allowOutsideClick: false,
                showConfirmButton: true,
                showCancelButton: true,
                closeOnConfirm: true,
                closeOnCancel: true,
                confirmButtonText: 'Yes',
                confirmButtonColor: '#8CD4F5',
                cancelButtonText: 'No',
                closeOnConfirm: false,
                closeOnCancel: false
            },
     function (isConfirm) {

         if (isConfirm) {
             CallServer('RMSGSendMessage', Param, RMSGSendMessageCallBack);;

         } else {
             swal("Cancelled", "Oops!..Unable to send the message(s).", "error");
         }
     });
        }
        else {
            swal({
                title: "Check fields to complete",
                text: "Error in sending message..",
                type: "error",
                timer: 2500,
                showConfirmButton: false
            });
        }
    }
}

function RMSGSendMessageCallBack(input) {
    if (input == 0) {
        swal({
            title: "",
            text: "Messages Sent..",
            type: "success",
            timer: 3000,
            showConfirmButton: false
        });
        RMSGBindMessageList(input);
    }
    else {
        swal({
            title: "",
            text: "Error in sending message..",
            type: "error",
            timer: 3000,
            showConfirmButton: false
        });
    }
}























//-----------------------End Of Reply To Message----------------------------------------------