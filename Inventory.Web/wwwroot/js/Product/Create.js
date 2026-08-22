$('#imgInput').on('change', function () {
    const file = this.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            $('#imgPreview').attr('src', e.target.result);
            $('#previewContainer').removeClass('d-none');
        };
        reader.readAsDataURL(file);
    } else {
        $('#previewContainer').addClass('d-none');
    }
});

$('#createProductForm').on('submit', function (e) {
    e.preventDefault();

    if (!$(this).valid()) return;

    const $form = $(this);
    const formData = new FormData(this);

    $.ajax({
        url: $form.attr('action') || '@Url.Action("Create", "Products")',
        type: 'POST',
        data: formData,
        processData: false, 
        contentType: false, 
        success: function (response) {
            Swal.fire({
                icon: response.icon || 'success',
                title: 'Success!',
                text: response.message || 'Product created successfully.',
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
                title: 'Creation Failed',
                html: errorMessage
            });
        }
    });
});
