//initial settings
$(document).ready(function () {
    for (var i = 0; i < $('.post:not([data-parentid=""])').length; i++) {
        setPostParent($('.post:not([data-parentid=""])')[i]);
    }

    $('.post').each(function () {
        $(this).show();
    });

    setupModifyButtons();
});
//

function setupModifyButtons() {
    $('.edit-post').click(function () {
        editPost($(this).closest('.post').attr('data-postid'));
    });

    $('.delete-post').click(function () {
        deletePost($(this).closest('.post').attr('data-postid'));
    });
}
$('.comment-btn').click(function () {
    var postid = $(this).closest('li[data-postid]').attr('data-postid');
    writeReply(postid);
});

function writeReply(postId) {
    $('.comment').empty();
    var teema = $('#teema').val();
    var linkId = $('#linkId').val();
    $.ajax({
        method: 'GET',
        url: '/Thread/CreatePost',
        contentType: "application/json; charset=utf-8",
        data: { teema: teema, linkId: linkId, parentId: postId },
        dataType: 'html',
        success: function (result) {
            $('.post[data-postid=' + postId + ']').find('.comment').first().html(result);
            
        }
    });
}

function setPostParent(post) {
    var parentId = $(post).attr('data-parentid');
    $('.post[data-postid=' + parentId + ']').find('.comments').first().children('ul').append(post);
}

function upvote(postId) {
    $.ajax({
        method: 'POST',
        url: '/Thread/AddPostKarma',
        data: { postId: postId },
        dataType: 'text', //returned variable- new karma
        success: function (result) {
            $('.post[data-postid=' + postId + ']').find('.vote-panel').first().children('.badge').text(result);
            setVoteButtons(postId);
        }
    });
}

function downvote(postId) {
    $.ajax({
        method: 'POST',
        url: '/Thread/RemovePostKarma',
        data: { postId: postId },
        dataType: "text", //returned variable- new karma
        success: function (result) {
            $('.post[data-postid=' + postId + ']').find('.vote-panel').first().children('.badge').text(result);
            setVoteButtons(postId);
        }
    });
}

function setVoteButtons(postId) {
    $.ajax({
        method: 'POST',
        url: '/Thread/GetVoteStatus',
        data: { postId: postId },
        dataType: "text",
        success: function (result) {
            var votePanel = $('.post[data-postid=' + postId + ']').find('.vote-panel').first();
            switch (result) {
                case '1':
                    votePanel.children('.upvote').css('color', 'green');
                    votePanel.children('.upvote').show();
                    votePanel.children('.downvote').hide();
                    break;
                case '-1':
                    votePanel.children('.downvote').css('color', 'red');
                    votePanel.children('.downvote').show();
                    votePanel.children('.upvote').hide();
                    break;
                default:
                    votePanel.children('.upvote').css('color', 'inherit');
                    votePanel.children('.upvote').show();
                    votePanel.children('.downvote').css('color', 'inherit');
                    votePanel.children('.downvote').show();
                    break;
            }
            $('.post[data-postid=' + postId + ']').find('.vote-panel').first().children('.badge').text(result);
        }
    });
}

function deletePost(postId) {
    $.ajax({
        method: 'GET',
        url: '/Thread/DeletePost',
        contentType: "application/json; charset=utf-8",
        data: 'postId=' + postId,
        cache: false,
        dataType: 'html',
        success: function (result) {
            $('#modal').html(result);
            $('#modal').modal('show');
        }
    });
}

function confirmDeletePost(teema, postId) {
    $.ajax({
        method: 'POST',
        url: '/Thread/DeletePost',
        data: { teema: teema, postId: postId },
        contentType: "application/x-www-form-urlencoded; charset=utf-8",
        dataType: 'text',
        success: function (result) {
            var messageObj = $('.post[data-postid="' + postId + '"]').find('.message').first();
            messageObj.text(result);

            var userLinkObj = $(messageObj).parent().children('.user-link');
            userLinkObj.children('.title').html('');
            userLinkObj.children('.link').html('');
            $(userLinkObj).hide();

            $('#modal').modal('hide');
        }
    });
}

function editPost(postId) {
    $.ajax({
        method: 'GET',
        url: '/Thread/EditPost',
        contentType: "application/json; charset=utf-8",
        data: 'postId=' + postId,
        cache: false,
        dataType: 'html',
        success: function (result) {
            $('#modal').html(result);
            
            var msgObj = $('.post[data-postid="' + postId + '"]').find('.message').first().clone();
            reformat(msgObj);
            $('#modal').find('#edited-message').val(msgObj.html());
            $('#modal').modal('show');
        }
    });
}

function confirmEditPost(teema, postId) {
    var message = $('#edited-message').val();
    $.ajax({
        method: 'POST',
        url: '/Thread/EditPost',
        data: { teema: teema, postId: postId, message: message },
        contentType: "application/x-www-form-urlencoded; charset=utf-8",
        dataType: 'text',
        success: function (result) {
            var messageObj = $('.post[data-postid="' + postId + '"]').find('.message').first();
            messageObj.text(result);

            var userLinkObj = $(messageObj).parent().children('.user-link');
            userLinkObj.children('.title').html('');
            userLinkObj.children('.link').html('');
            $(userLinkObj).hide();

            format(messageObj);
            $('#modal').modal('hide');
        }
    });
}