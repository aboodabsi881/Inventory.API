$(function () {
    const $form = $('#changePasswordForm');
    const $submitBtn = $('#btnSubmitPassword');
    const $spinner = $('#btnSubmitSpinner');
    const $icon = $('#btnSubmitIcon');
    const $btnText = $('#btnSubmitText');

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

    // Generic Eye Toggle for all password fields
    $('.toggle-password-btn').on('click', function () {
        const targetSelector = $(this).data('target');
        const $input = $(targetSelector);
        const $eyeIcon = $(this).find('i');
        const isPassword = $input.attr('type') === 'password';

        if (isPassword) {
            $input.attr('type', 'text');
            $eyeIcon.removeClass('bi-eye-slash').addClass('bi-eye text-primary');
        } else {
            $input.attr('type', 'password');
            $eyeIcon.removeClass('bi-eye text-primary').addClass('bi-eye-slash');
        }
    });

    $form.on('submit', function (e) {
        e.preventDefault();

        // 1. Client-side validation check
        if ($.validator && !$form.valid()) {
            return;
        }

        // 2. Loading state
        $submitBtn.prop('disabled', true);
        $spinner.removeClass('d-none');
        $icon.addClass('d-none');

        const formData = new FormData(this);

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                $spinner.addClass('d-none');
                $icon.removeClass('d-none');
                $submitBtn.prop('disabled', false);

                Swal.fire(getThemeSwalConfig({
                    icon: response.icon || 'success',
                    title: response.title || 'Success!',
                    text: response.message || 'Password updated successfully!',
                    timer: 1500,
                    showConfirmButton: false
                })).then(() => {
                    window.location.href = response.redirectUrl || '/Accounts';
                });
            },
            error: function (xhr) {
                $spinner.addClass('d-none');
                $icon.removeClass('d-none');
                $submitBtn.prop('disabled', false);

                let errorMessages = [];
                let data = xhr.responseJSON;

                if (!data && xhr.responseText) {
                    try {
                        data = JSON.parse(xhr.responseText);
                    } catch (e) {
                        errorMessages.push(xhr.responseText);
                    }
                }

                if (data) {
                    if (data.errors && typeof data.errors === 'object') {
                        Object.keys(data.errors).forEach(function (key) {
                            const errVal = data.errors[key];
                            if (Array.isArray(errVal)) {
                                errorMessages.push(...errVal);
                            } else if (typeof errVal === 'string') {
                                errorMessages.push(errVal);
                            }
                        });
                    } else if (data.message) {
                        errorMessages.push(data.message);
                    } else if (data.title) {
                        errorMessages.push(data.title);
                    }
                }

                const displayHtml = errorMessages.length > 0
                    ? errorMessages.join('<br/>')
                    : 'Failed to update password. Please check your inputs.';

                Swal.fire(getThemeSwalConfig({
                    icon: 'error',
                    title: 'Validation Error',
                    html: displayHtml
                }));
            }
        });
    });
});