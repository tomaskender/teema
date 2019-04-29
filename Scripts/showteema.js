$('#add-roles-button').click(function () {
    $('#modal').load('/Teema/AddRole?teema=' + $('#name').text(), function () {
        $('#modal').modal('show');
    });
});

$('#edit-roles-button').click(function () {
    $('#modal').load('/Teema/EditRoles?teema=' + $('#name').text(), function () {
        $('#modal').modal('show');
    });
});

$('#modify-settings-button').click(function () {
    $('#modal').load('/Teema/Settings?teema=' + $('#name').text(), function () {
        $('#modal').modal('show');
    });
});


$('#create-thread-button').click(function () {
    $('#modal').load('/Thread/Create?teema=' + $('#name').text(), function () {
        $('#modal').modal('show');
    });
});

$('#subscription-button').click(function () {
    $('#modal').load('/Teema/ConfirmSubscription?teema=' + $('#name').text(), function () {
        $('#modal').modal('show');
    });
});

$("#modal").on("submit", function (e) {
    e.preventDefault();  // prevent standard form submission
    var form = $('#modal').find('form');
    $.ajax({
        url: form.attr("action"),
        method: form.attr("method"),  // post
        data: form.serialize(),
        success: function (partialResult) {
            $("#modal").html(partialResult);
        }
    });
});
