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

function getDetailsConfig() {
    return window.CategoryDetailsConfig || {};
}

window.showLightbox = function (imgUrl, title) {
    Swal.fire(getThemeSwalConfig({
        title: title || '',
        imageUrl: imgUrl,
        imageAlt: title || 'Image',
        imageWidth: 500,
        imageHeight: 'auto',
        showConfirmButton: false,
        showCloseButton: true,
        customClass: {
            image: 'img-fluid rounded shadow-sm'
        }
    }));
};

window.updateCartQuantity = function (productId, change) {
    const config = getDetailsConfig();
    const token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val()
        || $('input[name="__RequestVerificationToken"]').val();

    const cleanProductId = parseInt(productId, 10);
    const actionType = change < 0 ? "decrement" : "increment";

    $.ajax({
        url: config.cartUrl,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: {
            productId: cleanProductId,
            actionType: actionType,
            change: change < 0 ? -1 : 1,
            __RequestVerificationToken: token
        },
        success: function (response) {
            const container = $(`#cart-control-container-${cleanProductId}`);

            let finalQty = 0;
            if (response) {
                if (typeof response.quantity === 'number') {
                    finalQty = response.quantity;
                } else if (response.item && typeof response.item.quantity === 'number') {
                    finalQty = response.item.quantity;
                }
            }

            const isRemoved = response.removed === true || finalQty <= 0;

            if (isRemoved) {
                container.html(`
                    <button type="button"
                            class="btn btn-outline-primary w-100 rounded-pill py-2 shadow-sm d-flex align-items-center justify-content-center gap-2 fw-semibold"
                            onclick="updateCartQuantity(${cleanProductId}, 1)">
                        <i class="bi bi-bag-plus fs-6"></i>
                    </button>
                `);
            } else {
                container.html(`
                    <div class="d-flex align-items-center justify-content-between bg-primary text-white rounded-pill px-2 py-1 shadow-sm" style="min-height: 38px;">
                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25 stepper-btn"
                                style="width: 28px; height: 28px;"
                                onclick="updateCartQuantity(${cleanProductId}, -1)">
                            <i class="bi bi-dash-lg"></i>
                        </button>

                        <span class="fw-bold px-1 fs-6 user-select-none text-truncate" id="card-qty-${cleanProductId}">
                            <span class="qty-num" dir="ltr">${finalQty}</span>
                        </span>

                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25 stepper-btn"
                                style="width: 28px; height: 28px;"
                                onclick="updateCartQuantity(${cleanProductId}, 1)">
                            <i class="bi bi-plus-lg"></i>
                        </button>
                    </div>
                `);
            }

            if (response && response.message) {
                showThemeToast({
                    icon: response.icon || (isRemoved ? 'info' : 'success'),
                    title: response.message
                });
            }
        },
        error: function (xhr) {
            console.error("Cart AJAX error:", xhr.status, xhr.responseText);
            const err = xhr.responseJSON;
            showThemeToast({
                icon: (err && err.icon) ? err.icon : 'error',
                title: (err && err.message) ? err.message : 'Error'
            });
        }
    });
};

window.toggleFavorite = function (productId) {
    const config = getDetailsConfig();
    const token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val()
        || $('input[name="__RequestVerificationToken"]').val();

    const cleanProductId = parseInt(productId, 10);

    $.ajax({
        url: config.favoriteUrl,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: {
            productId: cleanProductId,
            __RequestVerificationToken: token
        },
        success: function (response) {
            const icon = $(`#fav-icon-${cleanProductId}`);

            if (response.isFavorite) {
                icon.removeClass('bi-heart text-secondary').addClass('bi-heart-fill text-danger');
            } else {
                icon.removeClass('bi-heart-fill text-danger').addClass('bi-heart text-secondary');
            }

            if (response.message) {
                showThemeToast({
                    icon: response.icon || 'success',
                    title: response.message
                });
            }
        },
        error: function (xhr) {
            console.error("Favorite AJAX error:", xhr);
            const err = xhr.responseJSON;
            showThemeToast({
                icon: (err && err.icon) ? err.icon : 'error',
                title: (err && err.message) ? err.message : 'Error'
            });
        }
    });
};