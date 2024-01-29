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
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BookController> _logger;

        public BookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<BookController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: Book
        /// <summary>
        /// Kitap listesini alfabetik sıraya göre getirir ve her kitap için ödünç alınan kitapları da içerir.
        /// Hata oluşması durumunda hata sayfasına yönlendirir.
        /// </summary>
        /// <returns>İşlem başarılıysa kitap listesi görünümü, hata oluşursa hata sayfası.</returns>

        public async Task<IActionResult> Index()
        {
            try
            {
                var booksWithBorrowedBooks = await _context.books
                .Include(b => b.BorrowedBooks)
                .OrderBy(c => c.name) // Alfabedik olarak.
                .ToListAsync();

                return View(booksWithBorrowedBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index sayfasında hata oluştu");
                return View("Error"); // Hata sayfasına yönlendirme veya uygun bir yanıt
            }
        }

        // GET: Book/Create
        /// <summary>
        /// Yeni kitap oluşturma formunu görüntüler.
        /// </summary>
        /// <returns>Kitap oluşturma formu görünümü.</returns>

        public IActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        /// <summary>
        /// Gönderilen BookViewModel kullanarak yeni bir kitap oluşturur.
        /// Eğer kitabın ismi önceden mevcutsa hata mesajı döndürür.
        /// Başarılı kitap oluşturma işlemi sonrası kitap listesine yönlendirir.
        /// </summary>
        /// <param name="bookViewModel">Oluşturulacak kitap için veri modeli.</param>
        /// <returns>İşlem başarılıysa kitap listesi görünümüne yönlendirme, aksi halde aynı formu hata mesajıyla geri döndürür.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel bookViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(bookViewModel);
            }

            try
            {
                var existingBook = await _context.books.FirstOrDefaultAsync(x => x.name == bookViewModel.name);
                if (existingBook != null)
                {
                    TempData["Error"] = "Bu isimde kitap zaten mevcut.";
                    return View(bookViewModel);
                }

                string? fileName = UploadFile(bookViewModel);

                var book = new Book
                {
                    name = bookViewModel.name,
                    author = bookViewModel.author,
                    img = fileName,
                    isAvaible = true,
                    status = true,
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now,
                    BorrowedBooks = new List<BorrowedBook>()
                };
                _context.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create metodunda hata oluştu");
                ModelState.AddModelError("", "Kitap oluşturma işlemi sırasında bir hata meydana geldi.");
                return View(bookViewModel);
            }
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
            if(model.delivery_date <= DateTime.Now)
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

        /// <summary>
        /// BookViewModel içindeki resim dosyasını sunucuya yükler.
        /// Yükleme başarılı olursa dosya adını, aksi halde null döndürür.
        /// </summary>
        /// <param name="bookViewModel">Yüklenen dosya için veri modeli.</param>
        /// <returns>Yükleme başarılıysa dosya adı, aksi halde null.</returns>

        private string? UploadFile(BookViewModel bookViewModel)
        {
            try
            {
                if (bookViewModel.img != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string fileName = Guid.NewGuid().ToString() + "-" + bookViewModel.img.FileName;
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        bookViewModel.img.CopyTo(fileStream);
                    }

                    return fileName;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadFile metodunda hata oluştu");
                return null;
            }
        }

        /// <summary>
        /// Ödünç alınan kitabın durumunu temizleme formunu görüntüler.
        /// </summary>
        /// <returns>Kitap ödünç durumu temizleme formu görünümü.</returns>

        [HttpGet]
        public IActionResult ClearBookBorrowById()
        {
            return View();
        }

        /// <summary>
        /// Belirtilen ID'ye sahip kitabın ödünç durumunu temizler ve kitabı tekrar müsait hale getirir.
        /// Kitap bulunamazsa hata sayfasına yönlendirir.
        /// </summary>
        /// <param name="id">Durumu temizlenecek kitabın ID'si.</param>
        /// <returns>İşlem başarılıysa kit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearBookBorrowById(int id)
        {
            try
            {
                var book = await _context.books
                    .Include(b => b.BorrowedBooks)
                    .FirstOrDefaultAsync(b => b.id == id);

                if (book == null)
                {
                    return NotFound();
                }

                if (book.BorrowedBooks != null)
                {
                    foreach (var borrowedBook in book.BorrowedBooks)
                    {
                        borrowedBook.status = false;
                    }
                }

                book.isAvaible = true;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ClearBookBorrowById metodunda hata oluştu");
                return View("Error"); // Hata sayfasına yönlendirme veya uygun bir yanıt
            }
        }
    }
}
