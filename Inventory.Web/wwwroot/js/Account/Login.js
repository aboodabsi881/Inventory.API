$(function () {
    // Helper function to get theme-aware SweetAlert configuration
    function getThemeSwalConfig(options) {
        const isDark = $('html').attr('data-bs-theme') === 'dark' ||
            $('body').attr('data-bs-theme') === 'dark' ||
            localStorage.getItem('theme') === 'dark';

        const themeDefaults = {
            background: isDark ? '#1e293b' : '#ffffff',
            color: isDark ? '#f8fafc' : '#0f172a',
            confirmButtonColor: '#4f46e5',
            cancelButtonColor: isDark ? '#334155' : '#64748b'
        };

        return Object.assign({}, themeDefaults, options);
    }

    // Toggle Password Visibility
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
            $text.text($btn.data('default-text') || 'Sign In');
        }, 3000);
    }

    $('#loginForm').on('submit', function (e) {
        e.preventDefault();
        const $form = $(this);
        const $btn = $('#submitBtn');
        const $spinner = $('#submitSpinner');
        const $text = $('#submitText');

        if (!$form.valid()) {
            setButtonError($btn, $text, $spinner, 'Please fill all required fields');
            return;
        }

        // Store original button text if not already cached
        if (!$btn.data('default-text')) {
            $btn.data('default-text', $text.text());
        }

        $btn.prop('disabled', true);
        $spinner.removeClass('d-none');
        $text.text('Signing in...');

        $.ajax({
            url: $form.attr('action') || '/Accounts/Login',
            type: 'POST',
            data: $form.serialize(),
            success: function (response) {
                $spinner.addClass('d-none');
                $btn.prop('disabled', false);

                Swal.fire(getThemeSwalConfig({
                    icon: response.icon || 'success',
                    title: 'Welcome!',
                    text: response.message || 'Login successful!',
                    timer: 1200,
                    showConfirmButton: false
                })).then(() => {
                    window.location.href = response.redirectUrl || '/Home/Index';
                });
            },
            error: function (xhr) {
                const err = xhr.responseJSON;
                let errorMessage = 'Invalid credentials';

                if (err && err.message) {
                    errorMessage = err.message;
                } else if (err && err.errors) {
                    errorMessage = Object.values(err.errors).flat()[0] || errorMessage;
                }

                setButtonError($btn, $text, $spinner, errorMessage);

                Swal.fire(getThemeSwalConfig({
                    icon: 'error',
                    title: 'Login Failed',
                    text: errorMessage
                }));
            }
        });
    });
});