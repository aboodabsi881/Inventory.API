// 1. Theme-Aware SweetAlert Configuration Helper
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

function getCategoryConfig() {
    return window.CategoryConfig || {};
}

// 2. Lightbox Preview
function showCategoryImage(imgUrl, categoryName) {
    Swal.fire(getThemeSwalConfig({
        title: categoryName || '',
        imageUrl: imgUrl,
        imageAlt: categoryName || 'Category Image',
        imageWidth: 500,
        imageHeight: 'auto',
        showConfirmButton: false,
        showCloseButton: true,
        customClass: {
            image: 'img-fluid rounded shadow-sm'
        }
    }));
}

// 3. Category Delete Action (Controller Driven)
function deleteCategory(id, name) {
    const config = getCategoryConfig();
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
                        $(`#category-card-${id}`).fadeOut(300, function () {
                            $(this).remove();
                            if ($('#categories-container .col').length === 0) {
                                location.reload();
                            }
                        });
                    });
                },
                error: function (xhr) {
                    console.error("Category Delete AJAX Error:", xhr);
                    const err = xhr.responseJSON;
                    Swal.fire(getThemeSwalConfig({
                        icon: (err && err.icon) ? err.icon : 'error',
                        title: (err && err.title) ? err.title : 'Error',
                        text: (err && err.message) ? err.message : 'Failed to delete category.'
                    }));
                }
            });
        }
    });
}