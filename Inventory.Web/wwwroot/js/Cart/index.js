const Toast = Swal.mixin({
    toast: true,
    position: 'top',
    showConfirmButton: false,
    timer: 1500,
    timerProgressBar: true
});

function getCartConfig() {
    return window.CartConfig || { texts: {} };
}

function updateQuantity(productId, change) {
    const config = getCartConfig();
    const texts = config.texts || {};
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: `${config.addOrUpdateUrl}?productId=${productId}&change=${change}`,
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        success: function (res) {
            if (res.removed) {
                location.reload();
            } else if (res) {
                const itemObj = res.item || res.Item;
                const qty = itemObj ? (itemObj.quantity ?? itemObj.Quantity ?? 0) : 0;

                const unitPrice = parseFloat($(`#price-${productId}`).attr('data-price')) || 0;

                let itemTotal = itemObj ? (itemObj.totalPrice ?? itemObj.TotalPrice ?? 0) : 0;
                if (!itemTotal || itemTotal === 0) {
                    itemTotal = qty * unitPrice;
                }

                const grandTotal = res.grandTotal ?? res.GrandTotal ?? 0;

                $(`#qty-${productId}`).text(qty);
                $(`#total-${productId}`).text('$' + Number(itemTotal).toFixed(2));

                $('#summary-subtotal').text('$' + Number(grandTotal).toFixed(2));
                $('#summary-grandtotal').text('$' + Number(grandTotal).toFixed(2));

                Toast.fire({ icon: 'success', title: texts.cartUpdated || 'Cart updated' });
            }
        },
        error: function (xhr) {
            console.error("AJAX Error Details:", xhr);
            let msg = texts.cartError || 'Failed to update cart.';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                msg = xhr.responseJSON.message;
            }
            Toast.fire({ icon: 'error', title: msg });
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

function removeItem(cartId, name) {
    const config = getCartConfig();
    const texts = config.texts || {};

    Swal.fire({
        title: texts.removeTitle || 'Remove Item?',
        text: `Remove "${name}" from your cart?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: texts.yesRemove || 'Yes, remove'
    }).then((result) => {
        if (result.isConfirmed) {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: `${config.removeUrl}/${cartId}`,
                type: 'POST',
                headers: { "RequestVerificationToken": token },
                success: function (res) {
                    $(`#cart-row-${cartId}`).fadeOut(300, function () {
                        $(this).remove();

                        const grandTotal = res ? (res.grandTotal ?? res.GrandTotal ?? 0) : 0;
                        $('#summary-subtotal').text('$' + Number(grandTotal).toFixed(2));
                        $('#summary-grandtotal').text('$' + Number(grandTotal).toFixed(2));

                        if ($('#cartTable tbody tr').length === 0) {
                            location.reload();
                        }
                    });
                },
                error: function () {
                    Toast.fire({ icon: 'error', title: texts.removeError || 'Failed to remove item' });
                }
            });
        }
    });
}