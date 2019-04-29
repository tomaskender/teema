//$('#hide-chat-btn').hide();
//$('.chat-sidebar').hide();
//$('.notifications-bar').hide();
var sidebarTemplate = $('#sidebar-name-template');
var windowTemplate = $('#popup-box-template');
var notificationMessageTemplate = $('#notifications-message-template')

$('.notifications-opener').click(function () {
    $('.notifications-bar').toggle();
});

$('.sidebar-btn').click(function () {
    $('#hide-chat-btn').toggle();
    $('#show-chat-btn').toggle();
    $('.chat-sidebar').toggle();
});

function setupNotifications() {
    chat.server.getNotificationList().done(function (result) {
        var notifications = JSON.parse(result);
        for (var i = 0; i < notifications.length; i++) {
            var clone = notificationMessageTemplate.clone();
            clone.removeAttr('id');
            clone.attr('data-notificationId', notifications[i].Id);
            clone.attr('data-link', notifications[i].Link);
            clone.text(notifications[i].Username + ' ' + notifications[i].Action);
            if (notifications[i].Seen == 0) {
                clone.css('background-color', "#ffd943");
                $('.notifications-asterisk').show();
            }
            $('.notifications-bar').prepend(clone);
            clone.show();
        }

        $('.notifications-message').click(function (e) {
            var messageObj = $(this);
            $.ajax({
                url: '/Notification/ReadNotification',
                type: 'post',
                data: 'notificationId=' + $(this).attr('data-notificationid')
            }).done(function () {
                window.location.href = $(messageObj).attr('data-link');
            });
        });
    });
}

function setupChat() {
    chat.server.getFriendList().done(function (result) {
        var contacts = JSON.parse(result);
        for (var i = 0; i < contacts.length; i++) {
            var clone = sidebarTemplate.clone();
            clone.removeAttr('id');
            clone.children('a').attr('href', 'javascript:registerPopup("' + contacts[i] + '");');
            clone.children('a').children('img').attr('src', '/Account/GetAvatar?username=' + contacts[i]);
            clone.children('a').children('span').text(contacts[i]);
            clone.show();
            $('.chat-sidebar').append(clone);
        }
    });
}

$(".chat-windows").on('keyup', function (event) {
    if (event.keyCode === 13) {
        var receiver = $(event.target).parent().find('.popup-head-left').text();
        var message = $(event.target).val();
        chat.server.sendMessage(receiver, message)
        sendMessage(message, receiver)
        $(event.target).val('');

        var msg = new messageInfo(true, message);
        //alert(msg.isLocalAuthor + ' ' + msg.message);

        var oldMessages = Cookies.getJSON(receiver);
        if (oldMessages == null) {
            Cookies.set(receiver, JSON.stringify([msg]));
        } else {
            oldMessages.push(msg);
            if (oldMessages.length > 5)
                oldMessages.shift();
            Cookies.set(receiver, JSON.stringify(oldMessages));
        }
    }
});

//this function can remove a array element.
Array.remove = function (array, from, to) {
    var rest = array.slice((to || from) + 1 || array.length);
    array.length = from < 0 ? array.length + from : from;
    return array.push.apply(array, rest);
};

//this variable represents the total number of popups can be displayed according to the viewport width
var total_popups = 0;

//arrays of popups ids
var popups = [];

//this is used to close a popup
function closePopup(name) {
    for (var iii = 0; iii < popups.length; iii++) {
        if (name == popups[iii]) {
            Array.remove(popups, iii);
            $('.popup-box:contains("' + name + '")').remove();

            calculatePopups();

            return;
        }
    }
}

//displays the popups. Displays based on the maximum number of popups that can be displayed on the current viewport width
function displayPopups() {
    var right = 220;
    var iii = 0;
    for (iii; iii < total_popups; iii++) {
        if (popups[iii] != undefined) {
            var element = $('.popup-box:contains("' + popups[iii] + '")');
            $(element).css('right', right + "px");
            right = right + 320;
            element.show();
        }
    }

    for (var jjj = iii; jjj < popups.length; jjj++) {
        var element = $('.popup-box:contains("' + popups[jjj] + '")')
        element.hide()
    }
}

//creates markup for a new popup. Adds the id to popups array.
function registerPopup(name) {

    for (var iii = 0; iii < popups.length; iii++) {
        //already registered. Bring it to front.
        if (name == popups[iii]) {
            Array.remove(popups, iii);
            popups.unshift(name);

            calculatePopups();

            return;
        }
    }
    var windowClone = windowTemplate.clone();
    windowClone.removeAttr('id');
    windowClone.find('.popup-messages').empty();
    windowClone.find('.popup-head-left').text(name);
    windowClone.find('.popup-head-right').children('a').attr('href', 'javascript:closePopup("' + name + '");');
    windowClone.show();
    $('.chat-windows').append(windowClone);

    popups.unshift(name);

    messages = Cookies.getJSON(name);
    if (messages != null) {
        for (var i = 0; i < messages.length; i++)
            if (messages[i].isLocalAuthor) {
                sendMessage(messages[i].message, name);
            } else {
                receiveMessage(messages[i].message, name);
            }
    }

    calculatePopups();
}

//calculate the total number of popups suitable and then populate the toatal_popups variable.
function calculatePopups() {
    var width = window.innerWidth;
    if (width < 540) {
        total_popups = 0;
    }
    else {
        width = width - 200;
        //320 is width of a single popup box
        total_popups = parseInt(width / 320);
    }

    displayPopups();

}

function receiveMessage(message, sender) {
    var template = windowTemplate.find('#message-receiver-template').clone();
    template.text(message);
    var parent = $('.popup-box:contains("' + sender + '")').children('.popup-messages');
    parent.append(template);
    parent.scrollTop($(template).position().top);
}

function sendMessage(message, receiver) {
    var template = windowTemplate.find('#message-sender-template').clone();
    template.text(message);
    var parent = $('.popup-box:contains("' + receiver + '")').children('.popup-messages');
    parent.append(template);
    parent.scrollTop($(template).position().top);
}

//recalculate when window is loaded and also when window is resized.
window.addEventListener("resize", calculatePopups);
window.addEventListener("load", calculatePopups);

var chat = $.connection.chatHub;

$.connection.hub.start().done(function () {
    setupChat();
    setupNotifications();
});

chat.client.addMessageToBrowser = function (message, sender) {
    var oldMessages = Cookies.getJSON(sender);
    if (oldMessages == null) {
        Cookies.set(sender, JSON.stringify([new messageInfo(false, message)]));
    } else {
        oldMessages.push(new messageInfo(false, message));
        if (oldMessages.length > 5)
            oldMessages.shift();
        Cookies.set(sender, JSON.stringify(oldMessages));
    }
    receiveMessage(message, sender)
    registerPopup(sender);
};

chat.client.disconnectFromHub = function () {
}

var messageInfo = function (author, messageContent) {
    //isLocalAuthor => was the message written by the user on this computer?
    this.isLocalAuthor = author;
    this.message = messageContent;
}