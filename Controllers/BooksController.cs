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
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BooksController> _logger;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<BooksController> logger)
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
                if(fileName == null)
                {
                    return View(bookViewModel);
                }
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
                    string fileExtension = Path.GetExtension(bookViewModel.img.FileName).ToLower(); // Dosya uzantısı tipini küçük harf olarak al
                    if(fileExtension != ".png" && fileExtension != ".jpg")
                    {
                        _logger.LogError("istenmeyen dosya tipi");
                        ModelState.AddModelError("", "Dosya tipi sadece png ve jpg kabul edilebilir.");
                        return null;
                    }
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images"); // WebRootPath'e images alt yolunu ekliyor
                    string fileName = Guid.NewGuid().ToString() + "-" + bookViewModel.img.FileName; // Daha sonra ilgili görsele uygun bir spesifik isim oluşturuluyor. FileName: Yüklenen dosyanın adını içerir.
                    string filePath = Path.Combine(uploadDir, fileName); // Path ile isim birleştiriliyor.
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
        /// Belirtilen Name'e sahip kitabın ödünç durumunu temizler ve kitabı tekrar müsait hale getirir.
        /// Kitap bulunamazsa hata sayfasına yönlendirir.
        /// </summary>
        /// <param name="name">Durumu temizlenecek kitabın name'i.</param>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearBookBorrowById(string name)
        {
            try
            {
                var book = await _context.books
                    .Include(b => b.BorrowedBooks)
                    .FirstOrDefaultAsync(b => b.name == name);

                if (book == null)
                {
                    return NotFound();
                }

                if (book.BorrowedBooks != null)
                {
                    foreach (var borrowedBook in book.BorrowedBooks)
                    {
                        borrowedBook.status = false;
                        // break koyulabilir, zaten bir tane olacak ama olası durumlara karşı kalsın.
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
