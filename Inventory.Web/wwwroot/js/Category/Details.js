const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 1500,
    timerProgressBar: true
});

function getDetailsConfig() {
    return window.CategoryDetailsConfig || { texts: {} };
}

document.addEventListener('DOMContentLoaded', () => {
    // Category Banner Lightbox
    $(document).on('click', '.category-banner-preview', function () {
        const imgUrl = $(this).data('img');
        const name = $(this).data('name') || 'Category Image';
        showLightbox(imgUrl, name);
    });

    // Product Card Lightbox
    $(document).on('click', '.product-image-preview', function (e) {
        e.preventDefault();
        const imgUrl = $(this).data('img');
        const name = $(this).data('name') || 'Product Image';
        showLightbox(imgUrl, name);
    });
});

function showLightbox(imgUrl, title) {
    Swal.fire({
        title: title,
        imageUrl: imgUrl,
        imageAlt: title,
        imageWidth: 500,
        imageHeight: 'auto',
        showConfirmButton: false,
        showCloseButton: true,
        background: '#fff',
        customClass: {
            image: 'img-fluid rounded shadow-sm'
        }
    });
}

function updateCartQuantity(productId, change) {
    const config = getDetailsConfig();
    const texts = config.texts || {};
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: `${config.cartUrl}?productId=${productId}&change=${change}`,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: {
            productId: productId,
            change: change,
            __RequestVerificationToken: token
        },
        success: function (response) {
            const container = $(`#cart-control-container-${productId}`);

            if (response.removed) {
                container.html(`
                    <button type="button"
                            class="btn btn-outline-primary w-100 rounded-pill py-2 shadow-sm d-flex align-items-center justify-content-center gap-2 fw-semibold"
                            onclick="updateCartQuantity(${productId}, 1)">
                        <i class="bi bi-bag-plus fs-6"></i>
                        <span>${texts.addToCart || 'Add to Cart'}</span>
                    </button>
                `);
                Toast.fire({ icon: 'info', title: texts.itemRemoved || 'Item removed from cart' });
            } else if (response.item) {
                const newQty = response.item.quantity;

                container.html(`
                    <div class="d-flex align-items-center justify-content-between bg-primary text-white rounded-pill px-2 py-1 shadow-sm" style="min-height: 38px;">
                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25 stepper-btn"
                                style="width: 28px; height: 28px;"
                                onclick="updateCartQuantity(${productId}, -1)">
                            <i class="bi bi-dash-lg"></i>
                        </button>

                        <span class="fw-bold px-1 fs-6 user-select-none text-truncate" id="card-qty-${productId}">
                            ${newQty} <small class="fw-normal text-white-50 ms-1 small">${texts.inCart || 'in cart'}</small>
                        </span>

                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25 stepper-btn"
                                style="width: 28px; height: 28px;"
                                onclick="updateCartQuantity(${productId}, 1)">
                            <i class="bi bi-plus-lg"></i>
                        </button>
                    </div>
                `);

                Toast.fire({ icon: 'success', title: texts.cartUpdated || 'Cart updated' });
            }
        },
        error: function (xhr) {
            console.error("Cart AJAX error:", xhr);
            const texts = getDetailsConfig().texts || {};
            Toast.fire({ icon: 'error', title: texts.cartError || 'Failed to update cart.' });
        }
    });
}

function toggleFavorite(productId) {
    const config = getDetailsConfig();
    const texts = config.texts || {};
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: config.favoriteUrl,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: {
            productId: productId,
            __RequestVerificationToken: token
        },
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
            console.error("Favorite AJAX error:", xhr);
            const texts = getDetailsConfig().texts || {};
            Toast.fire({ icon: 'error', title: texts.favError || 'Failed to update favorite status.' });
        }
    });
}