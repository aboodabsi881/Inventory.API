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

function removeFavorite(favoriteId, name) {
    const config = window.FavoriteConfig || {};
    const texts = config.texts || {};

    const confirmMessage = `${texts.removeConfirm || 'Are you sure you want to remove'} "${name}" ${texts.fromFavorites || 'from your favorites?'}`;

    Swal.fire(getThemeSwalConfig({
        title: texts.removeTitle || 'Remove from Favorites?',
        text: confirmMessage,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#64748b',
        confirmButtonText: texts.yesRemove || 'Yes, remove it!'
    })).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `${config.deleteUrl}/${favoriteId}`,
                type: 'POST',
                headers: {
                    "RequestVerificationToken": token
                },
                data: {
                    id: favoriteId,
                    __RequestVerificationToken: token
                },
                success: function (response) {
                    Swal.fire(getThemeSwalConfig({
                        icon: response.icon || 'info',
                        title: texts.removed || 'Removed!',
                        text: response.message || 'Item removed from favorites.',
                        timer: 1500,
                        showConfirmButton: false
                    })).then(() => {
                        $(`#favorite-card-${favoriteId}`).fadeOut(300, function () {
                            $(this).remove();

                            if ($('#favorites-container .col, #favorites-container [class*="col-"]').length === 0) {
                                location.reload();
                            }
                        });
                    });
                },
                error: function (xhr) {
                    const err = xhr.responseJSON;
                    Swal.fire(getThemeSwalConfig({
                        icon: 'error',
                        title: texts.oops || 'Oops...',
                        text: err ? err.message : (texts.errorDefault || 'Failed to remove favorite.')
                    }));
                }
            });
        }
    });
}