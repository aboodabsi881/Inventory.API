const Toast = Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 1500,
    timerProgressBar: true
});

function updateCartQuantity(productId, change) {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: `${window.ProductConfig.cartUrl}?productId=${productId}&change=${change}`,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        success: function (response) {
            const container = $(`#cart-control-container-${productId}`);

            if (response.removed) {
                container.html(`
                    <button type="button"
                            class="btn btn-primary bg-primary-subtle text-primary border-0 w-100 rounded-pill py-2 shadow-sm d-flex align-items-center justify-content-center gap-2 fw-semibold"
                            onclick="updateCartQuantity(${productId}, 1)">
                        <i class="bi bi-bag-plus-fill fs-6"></i>
                        <span>Add to Cart</span>
                    </button>
                `);
                Toast.fire({ icon: 'info', title: 'Item removed from cart' });
            } else if (response.item) {
                const newQty = response.item.quantity;

                container.html(`
                    <div class="d-flex align-items-center justify-content-between bg-primary text-white rounded-pill px-2 py-1 shadow-sm" style="min-height: 38px;">
                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25"
                                style="width: 28px; height: 28px;"
                                onclick="updateCartQuantity(${productId}, -1)">
                            <i class="bi bi-dash-lg"></i>
                        </button>

                        <span class="fw-bold px-3 fs-6 user-select-none" id="card-qty-${productId}">
                            ${newQty} <small class="fw-normal text-white-50 ms-1 small">in cart</small>
                        </span>

                        <button type="button"
                                class="btn btn-sm text-white rounded-circle d-flex align-items-center justify-content-center p-0 border-0 bg-white bg-opacity-25"
                                style="width: 28px; height: 28px;"
                                onclick="updateCartQuantity(${productId}, 1)">
                            <i class="bi bi-plus-lg"></i>
                        </button>
                    </div>
                `);

                Toast.fire({ icon: 'success', title: 'Cart updated' });
            }
        },
        error: function (xhr) {
            console.error("AJAX Error Details:", xhr);
            Toast.fire({ icon: 'error', title: 'Failed to update cart.' });
        }
    });
}

function toggleFavorite(productId) {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: window.ProductConfig.favoriteUrl,
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
        error: function () {
            Toast.fire({ icon: 'error', title: 'Failed to update favorite status.' });
        }
    });
}

function deleteProduct(id, name) {
    Swal.fire({
        title: 'Are you sure?',
        text: `Do you really want to delete the product "${name}"?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `${window.ProductConfig.deleteUrl}/${id}`,
                type: 'POST',
                data: { __RequestVerificationToken: token },
                success: function (response) {
                    Swal.fire({
                        icon: response.icon || 'success',
                        title: 'Deleted!',
                        text: response.message,
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        $(`#product-card-${id}`).fadeOut(300, function () {
                            $(this).remove();
                        });
                    });
                }
            });
        }
    });
}

function showFullImage(imgUrl, productName) {
    Swal.fire({
        title: productName || 'Product Image',
        imageUrl: imgUrl,
        imageAlt: productName,
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