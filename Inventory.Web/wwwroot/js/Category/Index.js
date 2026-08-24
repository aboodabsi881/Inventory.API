// 1. Helper function to get theme-aware SweetAlert configuration
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

document.addEventListener('DOMContentLoaded', () => {
    const config = window.CategoryConfig || {};
    const texts = config.texts || {};

    // 2. Category Image Lightbox Preview
    $(document).on('click', '.category-image-preview', function (e) {
        e.preventDefault();
        const imgUrl = $(this).data('img');
        const categoryName = $(this).data('name') || 'Category Image';

        Swal.fire(getThemeSwalConfig({
            title: categoryName,
            imageUrl: imgUrl,
            imageAlt: categoryName,
            imageWidth: 500,
            imageHeight: 'auto',
            showConfirmButton: false,
            showCloseButton: true,
            customClass: {
                image: 'img-fluid rounded shadow-sm'
            }
        }));
    });

    // 3. Category Delete Action
    $(document).on('click', '.btn-delete-category', function () {
        const id = $(this).data('id');
        const name = $(this).data('name');

        Swal.fire(getThemeSwalConfig({
            title: texts.areYouSure || 'Are you sure?',
            text: `${texts.deleteConfirm || 'Delete'} "${name}"?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#64748b',
            confirmButtonText: texts.yesDelete || 'Yes, delete it!'
        })).then((result) => {
            if (result.isConfirmed) {
                const token = $('input[name="__RequestVerificationToken"]').val();

                $.ajax({
                    url: `${config.deleteUrl}/${id}`,
                    type: 'POST',
                    data: {
                        id: id,
                        __RequestVerificationToken: token
                    },
                    headers: {
                        "RequestVerificationToken": token
                    },
                    success: function (response) {
                        Swal.fire(getThemeSwalConfig({
                            icon: response.icon || 'success',
                            title: texts.deleted || 'Deleted!',
                            text: response.message || 'Category deleted successfully.',
                            timer: 1500,
                            showConfirmButton: false
                        })).then(() => {
                            $(`#category-card-${id}`).fadeOut(300, function () {
                                $(this).remove();
                            });
                        });
                    },
                    error: function (xhr) {
                        const err = xhr.responseJSON;
                        Swal.fire(getThemeSwalConfig({
                            icon: 'error',
                            title: texts.oops || 'Error',
                            text: err?.message || texts.errorDefault || 'Failed to delete category.'
                        }));
                    }
                });
            }
        });
    });
});