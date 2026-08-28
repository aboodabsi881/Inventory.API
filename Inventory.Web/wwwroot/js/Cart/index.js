// 1. Toast Notification Setup
const Toast = Swal.mixin({
    toast: true,
    position: 'top',
    showConfirmButton: false,
    timer: 1500,
    timerProgressBar: true
});

function getCartConfig() {
    return window.CartConfig || {};
}

// 2. Update Quantity (+ / -)
function updateQuantity(productId, change) {
    const config = getCartConfig();
    const token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val()
        || $('input[name="__RequestVerificationToken"]').val();

    const cleanProductId = parseInt(productId, 10);
    const actionType = change < 0 ? "decrement" : "increment";

    $.ajax({
        url: config.addOrUpdateUrl,
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
        success: function (res) {
            if (res && res.removed) {
                location.reload();
                return;
            }

            if (res) {
                const itemObj = res.item || res.Item;
                const qty = res.quantity !== undefined
                    ? res.quantity
                    : (itemObj ? (itemObj.quantity ?? itemObj.Quantity ?? 0) : 0);

                const unitPrice = parseFloat($(`#price-${cleanProductId}`).attr('data-price')) || 0;

                let itemTotal = itemObj ? (itemObj.totalPrice ?? itemObj.TotalPrice ?? 0) : 0;
                if (!itemTotal || itemTotal === 0) {
                    itemTotal = qty * unitPrice;
                }

                const grandTotal = res.grandTotal ?? res.GrandTotal ?? 0;

                $(`#qty-${cleanProductId}`).text(qty);
                $(`#total-${cleanProductId}`).text('$' + Number(itemTotal).toFixed(2));

                $('#summary-subtotal').text('$' + Number(grandTotal).toFixed(2));
                $('#summary-grandtotal').text('$' + Number(grandTotal).toFixed(2));

                // Controller-driven message & icon
                if (res.message) {
                    Toast.fire({
                        icon: res.icon || 'success',
                        title: res.message
                    });
                }
            }
        },
        error: function (xhr) {
            console.error("Cart Update AJAX Error:", xhr);
            const err = xhr.responseJSON;
            Toast.fire({
                icon: (err && err.icon) ? err.icon : 'error',
                title: (err && err.message) ? err.message : 'Error'
            });
        }
    });
}

// 3. Remove Item
function removeItem(cartId, name) {
    const config = getCartConfig();
    const token = $('#antiForgeryForm input[name="__RequestVerificationToken"]').val()
        || $('input[name="__RequestVerificationToken"]').val();

    Swal.fire({
        text: `"${name}"`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: config.removeUrl,
                type: 'POST',
                headers: {
                    "RequestVerificationToken": token
                },
                data: {
                    id: cartId,
                    __RequestVerificationToken: token
                },
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

                    // Controller-driven message & icon
                    if (res && res.message) {
                        Toast.fire({
                            icon: res.icon || 'info',
                            title: res.message
                        });
                    }
                },
                error: function (xhr) {
                    console.error("Cart Remove AJAX Error:", xhr);
                    const err = xhr.responseJSON;
                    Toast.fire({
                        icon: (err && err.icon) ? err.icon : 'error',
                        title: (err && err.message) ? err.message : 'Error'
                    });
                }
            });
        }
    });
}

// 4. Image Preview Modal
function showFullImage(imgUrl, productName) {
    Swal.fire({
        title: productName || '',
        imageUrl: imgUrl,
        imageAlt: productName || '',
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