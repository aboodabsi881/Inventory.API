$(function () {
    $('#togglePassword').on('click', function () {
        const $input = $('#passwordInput');
        const $icon = $('#toggleIcon');
        const isPassword = $input.attr('type') === 'password';

        if (isPassword) {
            $input.attr('type', 'text');
            $icon.removeClass('bi-eye-slash').addClass('bi-eye text-primary');
        } else {
            $input.attr('type', 'password');
            $icon.removeClass('bi-eye text-primary').addClass('bi-eye-slash');
        }
    });

    function setButtonError($btn, $text, $spinner, message) {
        $spinner.addClass('d-none');
        $btn.prop('disabled', false)
            .removeClass('btn-primary')
            .addClass('btn-danger');
        $text.text(message);

        setTimeout(() => {
            $btn.removeClass('btn-danger').addClass('btn-primary');
            $text.text('@Localizer["Sign In"]');
        }, 3000);
    }

    $('#loginForm').on('submit', function (e) {
        e.preventDefault();
        const $form = $(this);
        const $btn = $('#submitBtn');
        const $spinner = $('#submitSpinner');
        const $text = $('#submitText');

        if (!$form.valid()) {
            setButtonError($btn, $text, $spinner, '@Localizer["Please fill all required fields"]');
            return;
        }

        $btn.prop('disabled', true);
        $spinner.removeClass('d-none');
        $text.text('@Localizer["Signing in..."]');

        $.ajax({
            url: $form.attr('action') || '/Accounts/Login',
            type: 'POST',
            data: $form.serialize(),
            success: function (response) {
                Swal.fire({
                    icon: response.icon || 'success',
                    title: '@Localizer["Welcome!"]',
                    text: response.message || '@Localizer["Login successful!"]',
                    timer: 1200,
                    showConfirmButton: false
                }).then(() => {
                    window.location.href = response.redirectUrl || '/Home/Index';
                });
            },
            error: function (xhr) {
                const err = xhr.responseJSON;
                let errorMessage = '@Localizer["Invalid credentials"]';

                if (err && err.message) {
                    errorMessage = err.message;
                } else if (err && err.errors) {
                    errorMessage = Object.values(err.errors).flat()[0] || errorMessage;
                }

                setButtonError($btn, $text, $spinner, errorMessage);

                Swal.fire({
                    icon: 'error',
                    title: '@Localizer["Login Failed"]',
                    text: response.message || errorMessage,
                });
            }
        });
    });
});
