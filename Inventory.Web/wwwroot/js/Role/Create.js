
$('#createRoleForm').on('submit', function (e) {
    e.preventDefault();

    if (!$(this).valid()) return;

    const form = $(this);

    $.ajax({
        url: form.attr('action') || '/Roles/Create',
        type: 'POST',
        data: form.serialize(),
        success: function (response) {
            Swal.fire({
                icon: response.icon || 'success',
                title: 'Success!',
                text: response.message,
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.href = response.redirectUrl || '/Roles';
            });
        },
        error: function (xhr) {
            const err = xhr.responseJSON;
            let errorMessage = 'Failed to create role.';

            if (err && err.message) {
                errorMessage = err.message;
            }

            Swal.fire({
                icon: 'error',
                title: 'Creation Failed',
                html: errorMessage
            });
        }
    });
});
