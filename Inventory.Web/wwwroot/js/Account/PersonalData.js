$(function () {
    // 1. Helper function to get theme-aware SweetAlert configuration
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

    // 2. Avatar Instant Preview
    const imgInput = document.getElementById('img-input');
    if (imgInput) {
        imgInput.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (file) {
                // Optional: validate size (e.g., max 2MB)
                if (file.size > 2 * 1024 * 1024) {
                    Swal.fire(getThemeSwalConfig({
                        icon: 'warning',
                        title: 'File Too Large',
                        text: 'Please select an image smaller than 2 MB.'
                    }));
                    this.value = '';
                    return;
                }

                const reader = new FileReader();
                reader.onload = function (event) {
                    const preview = document.getElementById('avatar-preview');
                    if (preview) {
                        preview.src = event.target.result;
                    }
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // 3. Form Submission with Theme-Aware Alerts
    $('#personalDataForm').on('submit', function (e) {
        e.preventDefault();

        const $form = $(this);
        const $submitBtn = $form.find('button[type="submit"]');

        if ($.validator && !$form.valid()) return;

        $submitBtn.prop('disabled', true);
        const formData = new FormData(this);

        $.ajax({
            url: $form.attr('action') || '/Accounts/PersonalData',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                $submitBtn.prop('disabled', false);

                Swal.fire(getThemeSwalConfig({
                    icon: response.icon || 'success',
                    title: response.title || 'Updated!',
                    text: response.message || 'Profile updated successfully!',
                    timer: 1500,
                    showConfirmButton: false
                })).then(() => {
                    window.location.href = response.redirectUrl || '/Accounts';
                });
            },
            error: function (xhr) {
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
                    : 'Failed to update profile. Please check your inputs.';

                Swal.fire(getThemeSwalConfig({
                    icon: 'error',
                    title: 'Update Failed',
                    html: displayHtml
                }));
            }
        });
    });
});