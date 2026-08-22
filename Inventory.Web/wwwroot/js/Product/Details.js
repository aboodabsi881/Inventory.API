const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 1500,
    timerProgressBar: true
});

function checkAuthOrRedirect() {
    const config = window.AppConfig || {};
    const texts = config.texts || {};

    if (!config.isAuthenticated) {
        Swal.fire({
            icon: 'info',
            title: texts.signInRequired || 'Sign In Required',
            text: texts.signInPrompt || 'Please log in to manage your cart and favorites.',
            showCancelButton: true,
            confirmButtonText: texts.signInBtn || 'Sign In',
            confirmButtonColor: '#4f46e5'
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = config.loginUrl || '/Accounts/Login';
            }
        });
        return false;
    }
    return true;
}

function updateCartQuantity(productId, change) {
    if (!checkAuthOrRedirect()) return;

    const config = window.AppConfig || {};
    const texts = config.texts || {};
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: `${config.endpoints.cartAddOrUpdate}?productId=${productId}&change=${change}`,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        success: function (response) {
            const container = $(`#cart-control-container-${productId}`);

            if (response.removed) {
                container.html(`
                    <button type="button"
                            class="btn btn-primary bg-primary-subtle text-primary border-0 w-100 rounded-pill py-2.5 shadow-sm d-flex align-items-center justify-content-center gap-2 fw-semibold"
                            onclick="updateCartQuantity(${productId}, 1)">
                        <i class="bi bi-bag-plus-fill fs-5"></i>
                        <span>${texts.addToCart || 'Add to Cart'}</span>
                    </button>
                `);
                Toast.fire({ icon: 'info', title: texts.cartRemoved || 'Item removed from cart' });
            } else if (response.item) {
                const newQty = response.item.quantity;

                container.html(`
                    <div class="d-flex align-items-center justify-content-between bg-primary text-white rounded-pill px-3 py-2 shadow-sm" style="min-height: 45px;">
                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25"
                                style="width: 32px; height: 32px;"
                                onclick="updateCartQuantity(${productId}, -1)">
                            <i class="bi bi-dash-lg fs-6"></i>
                        </button>

                        <span class="fw-bold px-3 fs-6 user-select-none" id="card-qty-${productId}">
                            ${newQty} <small class="fw-normal text-white-50 ms-1 small">${texts.inCart || 'in cart'}</small>
                        </span>

                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25"
                                style="width: 32px; height: 32px;"
                                onclick="updateCartQuantity(${productId}, 1)">
                            <i class="bi bi-plus-lg fs-6"></i>
                        </button>
                    </div>
                `);

                Toast.fire({ icon: 'success', title: texts.cartUpdated || 'Cart updated' });
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                checkAuthOrRedirect();
            } else {
                Toast.fire({ icon: 'error', title: texts.cartError || 'Failed to update cart.' });
            }
        }
    });
}

function toggleFavorite(productId) {
    if (!checkAuthOrRedirect()) return;

    const config = window.AppConfig || {};
    const texts = config.texts || {};
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: config.endpoints.favoriteAdd,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: { productId: productId },
        success: function (response) {
            const icon = $(`#fav-icon-${productId}`);

            if (response.isFavorite) {
                icon.removeClass('bi-heart text-secondary').addClass('bi-heart-fill text-danger');
            } else {
                icon.removeClass('bi-heart-fill text-danger').addClass('bi-heart text-secondary');
            }

            Toast.fire({
                icon: response.icon || 'success',
                title: response.message
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                checkAuthOrRedirect();
            } else {
                Toast.fire({ icon: 'error', title: texts.favError || 'Failed to update favorite status.' });
            }
        }
    });
}