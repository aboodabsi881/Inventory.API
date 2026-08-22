$('#changePasswordForm').on('submit', function (e) {
    e.preventDefault();

    if (!$(this).valid()) return;

    const $form = $(this);
    const formData = new FormData(this);

    $.ajax({
        url: $form.attr('action') || '/Accounts/ChangePassword',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            Swal.fire({
                icon: response.icon || 'success',
                title: 'Success!',
                text: response.message || 'Password updated successfully!',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.href = response.redirectUrl || '/Accounts';
            });
        },
        error: function (xhr) {
            const err = xhr.responseJSON;
            let errorMessage = 'Failed to update password.';

            if (err && err.errors) {
                errorMessage = Object.values(err.errors).flat().join('<br/>');
            } else if (err && err.message) {
                errorMessage = err.message;
            }

            Swal.fire({
                icon: 'error',
                title: 'Password Change Failed',
                html: errorMessage
            });
        }
    });
});
