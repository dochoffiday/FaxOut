/* METHODS */

function getParameterByName(name, url) {
    if (!url) {
        url = window.location.href;
    }
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

/* APP */

var loaded = false;

$(document).on('change', '#contact', function () {
    updateForm();
});

function updateForm() {
    var $input = $('#contact option:selected');
    var slug = '#' + $input.data('slug');

    if (loaded || !getParameterByName('to')) {
        $("#to").val($input.data('name'));
    }
    if (loaded || !getParameterByName('number')) {
        $("#number").val($input.data('number'));
    }

    if (loaded) {
        if (history.pushState) {
            history.pushState(null, null, slug);
        }
        else {
            location.hash = slug;
        }
    }

    loaded = true;
}

$(document).on('blur', ".has-error", function () {
    $(this).closest('.form-group').removeClass('has-error');
});

$(document).on('click', '#btn-preview', function () {
    $spinner = $("#spinner");
    $btn = $(this);

    $("#errors").hide().empty();
    $spinner.show();
    $btn.prop('disabled', true);

    var data = {
        to: $("#to").val(),
        number: $("#number").val(),
        subject: $("#subject").val(),
        message: $("#message").val()
    };

    $.post('home/create', data, function (result) {
        if (result.Errors.length > 0) {
            var errors = [];

            $.each(result.Errors, function (key, value) {
                errors.push('<strong>' + value.Value + '</strong>');
                $("#" + value.Key).closest('.form-group').addClass('has-error');
            });

            $("#errors").show().html(errors.join('<br/>'));
            $('html, body').animate({
                scrollTop: $('#errors').offset().top - 20
            }, 'slow');
            $spinner.hide();
            $btn.prop('disabled', false);
        } else {
            $("#iframe").attr('src', 'html/' + result.Id).data('id', result.Id);
            $('#preview').show();
            $('html, body').animate({
                scrollTop: $('#preview').offset().top - 20
            }, 'slow');
            $spinner.hide();
            $btn.prop('disabled', false);
        }
    }).fail(function (response) {
        alert('error: ' + response.responseText);
        $spinner.hide();
        $btn.prop('disabled', false);
    });

    return false;
});

$(document).on('click', '#btn-send', function () {
    $(this).prop('disabled', true);

    var url = 'send/' + $("#iframe").data('id');

    var win = window.open(url, '_blank');
    win.focus();

    $(window).scrollTop(0);
    window.location.reload();

    return false;
});