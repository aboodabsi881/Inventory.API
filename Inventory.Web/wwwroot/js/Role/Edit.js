
$('#editRoleForm').on('submit', function (e) {
    e.preventDefault();

    if (!$(this).valid()) return;

    const form = $(this);

    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: form.serialize(),
        success: function (response) {
            Swal.fire({
                icon: response.icon || 'success',
                title: 'Updated!',
                text: response.message,
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.href = response.redirectUrl;
            });
        },
        error: function (xhr) {
            const err = xhr.responseJSON;
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: err ? err.message : 'Failed to update role.'
            });
        }
    });
});
