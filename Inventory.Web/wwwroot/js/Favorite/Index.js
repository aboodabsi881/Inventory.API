function removeFavorite(favoriteId, name) {
    const config = window.FavoriteConfig || {};
    const texts = config.texts || {};

    const confirmMessage = `${texts.removeConfirm || 'Are you sure you want to remove'} "${name}" ${texts.fromFavorites || 'from your favorites?'}`;

    Swal.fire({
        title: texts.removeTitle || 'Remove from Favorites?',
        text: confirmMessage,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: texts.yesRemove || 'Yes, remove it!'
    }).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `${config.deleteUrl}/${favoriteId}`,
                type: 'POST',
                headers: {
                    "RequestVerificationToken": token
                },
                success: function (response) {
                    Swal.fire({
                        icon: response.icon || 'info',
                        title: texts.removed || 'Removed!',
                        text: response.message,
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        $(`#favorite-card-${favoriteId}`).fadeOut(300, function () {
                            $(this).remove();

                            if ($('#favorites-container .col').length === 0) {
                                location.reload();
                            }
                        });
                    });
                },
                error: function (xhr) {
                    const err = xhr.responseJSON;
                    Swal.fire({
                        icon: 'error',
                        title: texts.oops || 'Oops...',
                        text: err ? err.message : (texts.errorDefault || 'Failed to remove favorite.')
                    });
                }
            });
        }
    });
}