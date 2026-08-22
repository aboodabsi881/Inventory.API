function deleteRole(id, name) {
    const config = window.RoleConfig || {};
    const texts = config.texts || {};

    Swal.fire({
        title: texts.areYouSure || 'Are you sure?',
        text: `${texts.deleteConfirm || 'Do you really want to delete the role'} "${name}"?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: texts.yesDelete || 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `${config.deleteUrl}/${id}`,
                type: 'POST',
                data: {
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    Swal.fire({
                        icon: response.icon || 'success',
                        title: texts.deleted || 'Deleted!',
                        text: response.message,
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        $(`#role-row-${id}`).fadeOut(300, function () {
                            $(this).remove();
                        });
                    });
                },
                error: function (xhr) {
                    const err = xhr.responseJSON;
                    Swal.fire({
                        icon: 'error',
                        title: texts.oops || 'Oops...',
                        text: err ? err.message : (texts.errorDefault || 'Failed to delete role.')
                    });
                }
            });
        }
    });
}