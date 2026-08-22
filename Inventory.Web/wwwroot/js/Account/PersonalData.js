document.getElementById('img-input').addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (event) {
            document.getElementById('avatar-preview').src = event.target.result;
        };
        reader.readAsDataURL(file);
    }
});

$('#personalDataForm').on('submit', function (e) {
    e.preventDefault();

    if (!$(this).valid()) return;

    const $form = $(this);
    const formData = new FormData(this);

    $.ajax({
        url: $form.attr('action') || '/Accounts/PersonalData',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            Swal.fire({
                icon: response.icon || 'success',
                title: 'Updated!',
                text: response.message || 'Profile updated successfully!',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.href = response.redirectUrl || '/Accounts';
            });
        },
        error: function (xhr) {
            const err = xhr.responseJSON;
            let errorMessage = 'Failed to update profile.';

            if (err && err.errors) {
                errorMessage = Object.values(err.errors).flat().join('<br/>');
            } else if (err && err.message) {
                errorMessage = err.message;
            }

            Swal.fire({
                icon: 'error',
                title: 'Update Failed',
                html: errorMessage
            });
        }
    });
});