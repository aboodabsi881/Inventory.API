// 1. Theme-aware SweetAlert configuration helper
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

// 2. Dynamic Theme-Aware Toast Function
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

function getProductConfig() {
    return window.ProductConfig || {};
}

// 3. Cart Stepper / Add to Cart
function updateCartQuantity(productId, change) {
    const config = getProductConfig();

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

                        <span class="fw-bold px-2 fs-6 user-select-none" id="card-qty-${cleanProductId}">
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

            // Message and icon delivered directly from controller
            if (response && response.message) {
                showThemeToast({
                    icon: response.icon || (isRemoved ? 'info' : 'success'),
                    title: response.message
                });
            }
        },
        error: function (xhr) {
            console.error("Cart AJAX error:", xhr);
            const err = xhr.responseJSON;
            showThemeToast({
                icon: (err && err.icon) ? err.icon : 'error',
                title: (err && err.message) ? err.message : 'Error'
            });
        }
    });
}

// 4. Toggle Favorites
function toggleFavorite(productId) {
    const config = getProductConfig();
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

            // Message and icon delivered directly from controller
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
}

// 5. Delete Product
function deleteProduct(id, name) {
    const config = getProductConfig();
    const token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val()
        || $('input[name="__RequestVerificationToken"]').val();

    Swal.fire(getThemeSwalConfig({
        text: `"${name}"`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#64748b'
    })).then((result) => {
        if (result.isConfirmed) {
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
                        title: response.title || '',
                        text: response.message,
                        timer: 1500,
                        showConfirmButton: false
                    })).then(() => {
                        $(`#product-card-${id}`).fadeOut(300, function () {
                            $(this).remove();
                            if ($('#products-container .product-item-col').length === 0) {
                                $('#noResultsMsg').removeClass('d-none');
                            }
                        });
                    });
                },
                error: function (xhr) {
                    const err = xhr.responseJSON;
                    Swal.fire(getThemeSwalConfig({
                        icon: (err && err.icon) ? err.icon : 'error',
                        text: (err && err.message) ? err.message : 'Failed to delete product.'
                    }));
                }
            });
        }
    });
}

// 6. Full Image Lightbox
function showFullImage(imgUrl, productName) {
    Swal.fire(getThemeSwalConfig({
        title: productName || '',
        imageUrl: imgUrl,
        imageAlt: productName || '',
        imageWidth: 500,
        imageHeight: 'auto',
        showConfirmButton: false,
        showCloseButton: true,
        customClass: {
            image: 'img-fluid rounded shadow-sm'
        }
    }));
}