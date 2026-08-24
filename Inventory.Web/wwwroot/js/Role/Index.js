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

function deleteRole(id, name) {
    const config = window.RoleConfig || {};
    const texts = config.texts || {};

    Swal.fire(getThemeSwalConfig({
        title: texts.areYouSure || 'Are you sure?',
        text: `${texts.deleteConfirm || 'Do you really want to delete the role'} "${name}"?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#64748b',
        confirmButtonText: texts.yesDelete || 'Yes, delete it!'
    })).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `${config.deleteUrl}/${id}`,
                type: 'POST',
                headers: {
                    "RequestVerificationToken": token
                },
                data: {
                    id: id,
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    Swal.fire(getThemeSwalConfig({
                        icon: response.icon || 'success',
                        title: texts.deleted || 'Deleted!',
                        text: response.message || 'Role deleted successfully.',
                        timer: 1500,
                        showConfirmButton: false
                    })).then(() => {
                        $(`#role-row-${id}`).fadeOut(300, function () {
                            $(this).remove();
                        });
                    });
                },
                error: function (xhr) {
                    const err = xhr.responseJSON;
                    Swal.fire(getThemeSwalConfig({
                        icon: 'error',
                        title: texts.oops || 'Oops...',
                        text: err ? err.message : (texts.errorDefault || 'Failed to delete role.')
                    }));
                }
            });
        }
    });
}