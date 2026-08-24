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

function deleteUser(id, userName) {
    Swal.fire(getThemeSwalConfig({
        title: 'Delete User?',
        text: `Do you really want to delete user "${userName}"?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#64748b',
        confirmButtonText: 'Yes, delete it!'
    })).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `/Accounts/Delete/${id}`,
                type: 'POST',
                data: {
                    id: id,
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    Swal.fire(getThemeSwalConfig({
                        icon: response.icon || 'success',
                        title: response.title || 'Deleted!',
                        text: response.message || 'User deleted successfully.',
                        timer: 1500,
                        showConfirmButton: false
                    })).then(() => {
                        $(`#user-row-${id}`).fadeOut(300, function () {
                            $(this).remove();

                            if ($('#usersTable tbody tr').length === 0) {
                                const emptyHtml = `
                                    <tr id="emptyRow">
                                        <td colspan="5" class="text-center py-5 text-muted">
                                            <i class="bi bi-person-x display-6 d-block mb-2 text-secondary"></i>
                                            No users registered yet. Click <strong>Add New User</strong> to get started.
                                        </td>
                                    </tr>`;
                                $('#usersTable tbody').html(emptyHtml);
                            }
                        });
                    });
                },
                error: function (xhr) {
                    let errMsg = 'An error occurred while attempting to delete the user.';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errMsg = xhr.responseJSON.message;
                    } else if (xhr.responseText) {
                        errMsg = xhr.responseText;
                    }

                    Swal.fire(getThemeSwalConfig({
                        icon: 'error',
                        title: 'Error',
                        text: errMsg
                    }));
                }
            });
        }
    });
}

function showFullImage(imgUrl, userName) {
    Swal.fire(getThemeSwalConfig({
        title: userName || 'User Avatar',
        imageUrl: imgUrl,
        imageWidth: 280,
        imageHeight: 280,
        showConfirmButton: false,
        showCloseButton: true,
        customClass: {
            image: 'rounded-circle object-fit-cover shadow-sm'
        }
    }));
}