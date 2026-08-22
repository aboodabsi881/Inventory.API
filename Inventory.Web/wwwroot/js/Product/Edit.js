
$('#editProductForm').on('submit', function (e) {
    e.preventDefault();

    // 1️⃣ Client-side validation check
    if (!$(this).valid()) return;

    // 2️⃣ Collect form data into FormData object for multipart/form-data upload
    const $form = $(this);
    const formData = new FormData(this);

    $.ajax({
        url: $form.attr('action') || `@Url.Action("Edit", "Products")/${$('#Id').val()}`,
        type: 'POST',
        data: formData,
        processData: false, // 👈 Required for file uploads
        contentType: false, // 👈 Prevents jQuery from setting incorrect header
        success: function (response) {
            Swal.fire({
                icon: response.icon || 'success',
                title: 'Updated!',
                text: response.message || 'Product updated successfully.',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.href = response.redirectUrl || '@Url.Action("Index", "Products")';
            });
        },
        error: function (xhr) {
            const err = xhr.responseJSON;
            let errorMessage = 'Validation failed or server error.';

            if (err && err.errors) {
                errorMessage = Object.values(err.errors).flat().join('<br/>');
            } else if (err && err.message) {
                errorMessage = err.message;
            }

            Swal.fire({
                icon: 'error',
                title: 'Update Failed',
                html: errorMessage
            });
        }
    });
});
