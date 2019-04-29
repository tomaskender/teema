/*$(document).on('DOMSubtreeModified', function () {
    if ($(this).hasClass('format')) {
        alert('format')
        format($(this));
    } else if ($(this).hasClass('reformat')) {
        alert('reformat')
        reformat($(this));
    }
});*/

$('.format').each(function () {
    format($(this));
});

$('.reformat').each(function () {
    reformat($(this));
});

function format(target) {
    var re = /\W(@|#)\w+/g;
    var s = $(target).text();
    var m;

    do {
        m = re.exec(s);
        if (m) {
            var caughtText = m[0].substring(1);
            var formattedHtml = $(target).html();
            var tag;
            if (caughtText[0] == '@') {
                tag = 'u';
            } else if (caughtText[0] == '#') {
                tag = 't';
            } else return;

            var name = caughtText.substring(1);
            formattedHtml = formattedHtml.replace(caughtText,
                '<a href="/' + tag + '/' + name + '">' + name + '</a>');

            $(target).html(formattedHtml);
        }
    } while (m);

    re = /\W\$(http(s)?:)?(www.)?(\w+.)+\w{2,}\S*/g;
    var userLink;

    do {
        m = re.exec(s);
        if (m) {
            var caughtText = m[0].substring(1);
            var formattedHtml = $(target).html();
            var tag;
            var userLink;
            if (caughtText[0] == '$') {
                var link = caughtText.substring(1);
                //link = link.replace(/\W\$(http(s)?\:\/\/)?(www\.)?/, '');
                link = link.replace(/(http(s)?:\/\/)?(www.)?/, '');
                formattedHtml = formattedHtml.replace(caughtText, '<a href="http://' + link + '">' + link + '</a>');
                if (!userLink) {
                    userLink = link;
                }
            }
            $(target).html(formattedHtml);
        }
    } while (m);
    if (userLink) {
        var userLinkObj = $(target).parent().children('.user-link');
        userLinkObj.children('.title').html('<a href="http://' + userLink
            + '" style="color:inherit; text-decoration: none;">' + userLink.replace(/\/.*/, '') + '</a>');
        userLinkObj.children('.link').html('<a href="http://'
            + userLink + '" style="color:inherit;">' + userLink + '</a>');
        $(userLinkObj).show();
    }
}

function reformat(target) {
    target.children('a').each(function () {
        var char;

        if ($(this).attr('href')[0] != '/') {
            char = '$';
        } else {
            var tag = $(this).attr('href')[1];
            if (tag == 'u' || tag == 't') {
                switch (tag) {
                    case 'u': char = '@'; break;
                    case 't': char = '#'; break;
                    default: break;
                }

            }
        }
        $(this).replaceWith(char + $(this).text());
    });
}