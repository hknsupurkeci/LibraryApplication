function setBookId(bookId, buttonElement) {
    // Her tıklandığında modal temizleniyor.
    clearModalFields();
    $('#book_id').val(bookId);
    // Butonun referansını sakla
    $(buttonElement).data('bookCardSelector', '#book-card-' + bookId);
}
// Modal kapatıldığında veya yeni bir kitaba tıkladığınızda çalışacak fonksiyon
function clearModalFields() {
    // Modal içindeki input alanlarını temizle
    $('#user_name').val('');
    $('#delivery_date').val('');
}

$(document).ready(function () {
    $('#borrowBookForm').on('submit', function (e) {
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
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: '/Borrows/BorrowBook',
            type: 'POST',
            contentType: 'application/json',
            headers: { "RequestVerificationToken": token },
            data: JSON.stringify(formData),
            success: function (response) {
                console.log("AJAX success, response: ", response); // Konsol logu ekle
                if (response.success) {
                    $(bookCardSelector).find('.card-used-username').append($('#user_name').val());
                    $(bookCardSelector).find('.card-used-deliverydate').append($('#delivery_date').val());
                    $(bookCardSelector).find('button').remove(); // Butonu kaldır
                }
                $('#exampleModal').modal('hide');
                // Modal tamamen kapandıktan sonra alert mesajını göster
                $('#exampleModal').on('hidden.bs.modal', function (e) {
                    alert(response.message);
                    // Birden fazla AJAX çağrısında her seferinde bu olayın tetiklenmemesi için olay dinleyicisini kaldır
                    $('#exampleModal').off('hidden.bs.modal');
                });
                clearModalFields();
            },
            error: function (error) {
                console.log("AJAX error: ", error); // Konsol logu ekle
                alert('Bir hata oluştu!');
            }
        });
    });
});
