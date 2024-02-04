using LibraryApplication.Models;
using LibraryApplication.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LibraryApplication.Controllers
{
    public class BorrowsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BooksController> _logger;

        public BorrowsController(ApplicationDbContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }
        /// <summary>
        /// Kullanıcının ödünç almak istediği kitabı işler ve durumunu günceller.
        /// Kitap mevcut değilse veya müsait değilse hata mesajı döndürür.
        /// </summary>
        /// <param name="model">Ödünç alınacak kitap için veri modeli.</param>
        /// <returns>İşlem başarılıysa başarılı mesajı, aksi halde hata mesajı içeren bir JSON nesnesi.</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook([FromBody] BorrowBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz veri girişi." });
            }

            // Su andan ileri bir tarih seçmeli, eğer küçük veya eşitse false dönecek
            if (model.delivery_date <= DateTime.Now)
            {
                return Json(new { success = false, message = "Seçilen tarih, şuanki tarihten küçük olamaz." });
            }

            try
            {
                var borrowBook = new BorrowedBook
                {
                    book_id = model.book_id,
                    user_name = model.user_name,
                    delivery_date = model.delivery_date,
                    status = true,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };

                var book = await MarkAsBorrowed(model.book_id);
                if (book == null)
                {
                    return Json(new { success = false, message = "Kitap bulunamadi veya şu anda müsait değil." });
                }

                book.BorrowedBooks = book.BorrowedBooks ?? new List<BorrowedBook>();
                book.BorrowedBooks.Add(borrowBook);

                _context.Add(borrowBook);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Kitap başariyla ödünç verildi!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BorrowBook metodunda hata oluştu");
                return Json(new { success = false, message = "İşlem sirasinda bir hata oluştu." });
            }
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kitabın ödünç alınabilir durumunu günceller.
        /// Kitap mevcut değilse veya zaten ödünç alınmışsa null döndürür.
        /// </summary>
        /// <param name="bookId">Ödünç alınacak kitabın ID'si.</param>
        /// <returns>İşlem başarılıysa güncellenmiş kitap nesnesi, aksi halde null.</returns>

        public async Task<Book?> MarkAsBorrowed(int bookId)
        {
            try
            {
                var book = await _context.books.FindAsync(bookId);
                // Veritabanında var mı && Alinabilir durumda mi
                if (book != null && book.isAvaible)
                {
                    book.isAvaible = false;
                    await _context.SaveChangesAsync();
                    return book;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkAsBorrowed metodunda hata oluştu");
                return null;
            }
        }
    }
}