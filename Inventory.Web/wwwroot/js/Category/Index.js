document.addEventListener('DOMContentLoaded', () => {
    const config = window.CategoryConfig || {};
    const texts = config.texts || {};

    $(document).on('click', '.category-image-preview', function (e) {
        e.preventDefault();
        const imgUrl = $(this).data('img');
        const categoryName = $(this).data('name') || 'Category Image';

        Swal.fire({
            title: categoryName,
            imageUrl: imgUrl,
            imageAlt: categoryName,
            imageWidth: 500,
            imageHeight: 'auto',
            showConfirmButton: false,
            showCloseButton: true,
            background: '#fff',
            customClass: {
                image: 'img-fluid rounded shadow-sm'
            }
        });
    });

    $(document).on('click', '.btn-delete-category', function () {
        const id = $(this).data('id');
        const name = $(this).data('name');

        Swal.fire({
            title: texts.areYouSure,
            text: `${texts.deleteConfirm} "${name}"?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: texts.yesDelete
        }).then((result) => {
            if (result.isConfirmed) {
                const token = $('input[name="__RequestVerificationToken"]').val();

                $.ajax({
                    url: `${config.deleteUrl}/${id}`,
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: token
                    },
                    success: function (response) {
                        Swal.fire({
                            icon: response.icon || 'success',
                            title: texts.deleted,
                            text: response.message,
                            timer: 1500,
                            showConfirmButton: false
                        }).then(() => {
                            $(`#category-card-${id}`).fadeOut(300, function () {
                                $(this).remove();
                            });
                        });
                    },
                    error: function (xhr) {
                        const err = xhr.responseJSON;
                        Swal.fire({
                            icon: 'error',
                            title: texts.oops,
                            text: err?.message || texts.errorDefault
                        });
                    }
                });
            }
        });
    });
});