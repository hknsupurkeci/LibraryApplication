function setBookId(bookId, buttonElement) {
    console.log("setBookId called with bookId: " + bookId); // Kontrol logu
    $('#book_id').val(bookId);
    // Butonun referansını sakla
    $(buttonElement).data('bookCardSelector', '#book-card-' + bookId);
}

$(document).ready(function() {
    $('#borrowBookForm').on('submit', function(e) {
        e.preventDefault();

        // Formdan bookId'yi alıp doğru seçiciyi oluştur
        var bookId = $('#book_id').val();
        var bookCardSelector = '#book-card-' + bookId;
        console.log("Form submitted for: " + bookCardSelector); // Kontrol logu

        var formData = {
            book_id: bookId,
            user_name: $('#user_name').val(),
            delivery_date: $('#delivery_date').val()
        };

        $.ajax({
            url: '/Book/BorrowBook',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                console.log("AJAX success, response: ", response); // Konsol logu ekle
                if(response.success) {
                    $(bookCardSelector).find('.card-used-username').append($('#user_name').val());
                    $(bookCardSelector).find('.card-used-deliverydate').append($('#delivery_date').val());
                    $(bookCardSelector).find('button').remove(); // Butonu kaldır
                } else {
                    alert(response.message);
                }
                $('#exampleModal').modal('hide');
            },
            error: function(error) {
                console.log("AJAX error: ", error); // Konsol logu ekle
                alert('Bir hata oluştu!');
            }
        });
    });
});
