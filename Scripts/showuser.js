
//loads the profile when the page is loaded
$('#profile-button').click();

//nahradene Html.Action-om vo Viewe, je to ovela rychlejsie ako cakanie na nacitanie stranky a nasledne volanie javascriptu
/*$('#subscriptions-button').click(function () {
    $('#subscriptions-tab').load('/Account/ShowSubscriptions?username=' + $('#username').text());
});

$('#follows-button').click(function () {
    $('#follows-tab').load('/Account/ShowFollows?username=' + $('#username').text());
});

$('#options-button').click(function () {
    $('#options-tab').load('/Account/ShowOptions');
});*/

$("#options-tab").on("DOMSubtreeModified", function () {
    $("form").on("submit", function (e) {
        e.preventDefault();  // prevent standard form submission
        var form = $(this);

        $.ajax({
            url: form.attr("action"),
            method: form.attr("method"),  // post
            data: new FormData(this),
            cache: false,
            contentType: false,
            processData: false,
            success: function (partialResult) {
                $("#options-tab").html(partialResult);
            }
        });
    });
});

$("#follow-button").click(function () {
    $('#modal').load('/Account/ConfirmUserFollow?username=' + $('#username').text(), function () {
        $('#modal').modal('show');
    });
});