// Dynamic Theme-Aware Toast
function showThemeToast(options) {
    const isDark = $('html').attr('data-bs-theme') === 'dark' ||
        $('body').attr('data-bs-theme') === 'dark' ||
        localStorage.getItem('theme') === 'dark';

    const Toast = Swal.mixin({
        toast: true,
        position: 'top',
        showConfirmButton: false,
        timer: 1500,
        timerProgressBar: true,
        background: isDark ? '#1e293b' : '#ffffff',
        color: isDark ? '#f8fafc' : '#0f172a',
        customClass: {
            popup: 'shadow-sm border border-secondary border-opacity-25'
        }
    });

    return Toast.fire(options);
}

function getFavoriteConfig() {
    return window.FavoriteConfig || {};
}

// Instant Toggle Action (No Confirm Dialog)
function toggleFavoriteRemove(favoriteId) {
    const config = getFavoriteConfig();
    const token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val()
        || $('input[name="__RequestVerificationToken"]').val();

    const cleanId = parseInt(favoriteId, 10);
    const $icon = $(`#fav-icon-${cleanId}`);
    const $card = $(`#favorite-card-${cleanId}`);

    // Instant visual toggle
    $icon.removeClass('bi-heart-fill text-danger').addClass('bi-heart text-secondary');

    $.ajax({
        url: `${config.deleteUrl}/${cleanId}`,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: {
            id: cleanId,
            __RequestVerificationToken: token
        },
        success: function (response) {
            // Show localized controller message via Toast
            if (response && response.message) {
                showThemeToast({
                    icon: response.icon || 'info',
                    title: response.message
                });
            }

            // Smooth fade out and remove
            $card.fadeOut(300, function () {
                $(this).remove();

                // If no cards left, reload to show the empty state design
                if ($('#favorites-container .col').length === 0) {
                    location.reload();
                }
            });
        },
        error: function (xhr) {
            console.error("Favorite Delete AJAX Error:", xhr);

            // Revert heart state on error
            $icon.removeClass('bi-heart text-secondary').addClass('bi-heart-fill text-danger');

            const err = xhr.responseJSON;
            showThemeToast({
                icon: (err && err.icon) ? err.icon : 'error',
                title: (err && err.message) ? err.message : 'Error'
            });
        }
    });
}