function deleteUser(id, userName) {
    Swal.fire({
        title: 'Delete User?',
        text: `Do you really want to delete user "${userName}"?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `@Url.Action("Delete", "Accounts")/${id}`,
                type: 'POST',
                headers: { "RequestVerificationToken": token },
                success: function (response) {
                    Swal.fire({
                        icon: response.icon || 'success',
                        title: response.title || 'Deleted!',
                        text: response.message || 'User deleted successfully.',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        $(`#user-row-${id}`).fadeOut(300, function () {
                            $(this).remove();

                            // Render empty table state dynamically if no rows remain
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
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: xhr.responseJSON?.message || 'An error occurred while attempting to delete the user.'
                    });
                }
            });
        }
    });
}

function showFullImage(imgUrl, userName) {
    Swal.fire({
        title: userName || 'User Avatar',
        imageUrl: imgUrl,
        imageWidth: 280,
        imageHeight: 280,
        showConfirmButton: false,
        showCloseButton: true,
        customClass: { image: 'rounded-circle object-fit-cover shadow' }
    });
}