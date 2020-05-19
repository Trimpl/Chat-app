"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable send button until connection is established
//document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (data) {
    $('#messageInput').val('');
    var json = JSON.parse(data);
    var img = '';
    if ($('#toUser').val() == json.User2Id) {
        img = $('#User1Img').val();
    } else {
        img = $('#User2Img').val();
    }
    // Время ------
    var LastTime = $('input[id = "LastTime"]:last').val();
    var Time = $.format.date(json.DateCreate, "HH:mm");
    var DateMessage = $.format.date(json.DateCreate, "dd.MM.yyyy");
    var DateLastMessage = $.format.date(LastTime, "dd.MM.yyyy");
    if (DateMessage != DateLastMessage) {
        $('#messagesList').append('<input type="hidden" id="LastTime" value="' + DateMessage + '" /><div class="text-center"><span>' + DateMessage + '</span></div>');
    }
    // Конец времени ------
    var idA = $('div[name = "message"]:last').attr('id');
    var LastAuthor = $('input[id = "LastAuthor"]:last').val();

    var divNew = '<div id="'+ (idA+1) + '" class="break-word mt-3 row">' +
        '<div id="cartinka" class="col-2">' +
        '<img class="rounded-circle" style="width: 36px;height: 36px;object-fit: cover;object-position: 0 0;" src="' + img + '" />' +
        '</div>' +
        '<input type="hidden" id="LastAuthor" value="' + json.User1Id + '" />'+
        '<div class="col-6">' +
        '<strong>' + json.User1Id + '</strong> <small class="text-muted">' + Time + '</small><br />' + json.Contect +
        '</div>' +
        '</div>';
    var divOld = '<div id="' + (idA + 1) + '" class="break-word row">' +
        '<div class="col-2"></div>' +
        '<div class="col-10">' +
        json.Contect +
        '</div>' +
        '</div>';

    if (LastAuthor != json.User1Id) {
        $('#messagesList').append(divNew);
    } else {
        $('#messagesList').append(divOld);
    }
    var block = document.getElementById("blockScroll");
    block.scrollTop = block.scrollHeight;
});


connection.on("Notify", function (message) {
    var li = document.createElement("div");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("Others", function (message) {
    var message = JSON.parse(message);
    var div = '<a href="Home/ShowChat?toUser=' + message.User1Id + '" style="display: block;"><div id="' + message.Id + '" aria-live="polite" aria-atomic="true" style="position: relative; top:0; right:0; min-height: 100%;">' +
        '<div role="alert" aria-live="assertive" aria-atomic="true" class="toast" data-autohide="false"' +
        ' style="position: static; bottom: 0; right: 0;margin-bottom:10px; min-width:350px;">' +
        '<div class="toast-header">' +
        '<strong class="mr-auto">' + message.User1Id + '</strong>' +
        '<small>' + $.format.date(message.DateCreate, "HH:mm") + '</small>' +
        '<button onclick="deleteNotice(\'' + message.Id + '\')" type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">' +
        '<span aria-hidden="true">&times;</span>' +
        '</button>' +
        '</div>' +
        '<div class="toast-body text-truncate text-dark">' +
        message.Contect +
        '</div>' +
        '</div>' +
        '</div></a>';
    console.log(document.location.pathname + document.location.search);
    if (!(document.location.pathname + document.location.search).includes('ShowChat?toUser='+message.User1Id)) {
        $('#Others').append(div);
        $('.toast').toast('show');
        setTimeout(deleteNotice, 10000, message.Id);
    };
});


connection.start().then(function () {
    var block = document.getElementById("blockScroll");
    block.scrollTop = block.scrollHeight;
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    var to = document.getElementById("toUser").value;
    connection.invoke("SendMessage", user, message, to).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

function deleteNotice(id) {
    $('#' + id).detach();
};

