
// 💡 Live Image Preview on File Selection
$('#imgInput').on('change', function () {
    const file = this.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            $('#imgPreview').attr('src', e.target.result);
        };
        reader.readAsDataURL(file);
    }
});

// 💡 AJAX Form Submission
$('#editCategoryForm').on('submit', function (e) {
    e.preventDefault();

    // 1️⃣ التحقق من صحة المدخلات بجهة العميل
    if (!$(this).valid()) return;

    // 2️⃣ حزم البيانات مع الملف
    const $form = $(this);
    const formData = new FormData(this);

    $.ajax({
        url: $form.attr('action') || `@Url.Action("Edit", "Categories")/${$('#Id').val()}`,
        type: 'POST',
        data: formData,
        processData: false, // 👈 ضروري لرفع الملفات والصور
        contentType: false, // 👈 يمنع jQuery من إفساد نوع المحتوى
        success: function (response) {
            Swal.fire({
                icon: response.icon || 'success',
                title: 'Updated!',
                text: response.message || 'Category updated successfully.',
                timer: 1500,
                showConfirmButton: false
            }).then(() => {
                window.location.href = response.redirectUrl || '@Url.Action("Index", "Categories")';
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
