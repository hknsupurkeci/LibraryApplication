using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApplication.Models
{
    public class Book
    {
        public int id { get; set; }
        [Required]
        public string? name { get; set; }
        [Required]
        public string? author { get; set; }
        [Required]
        public string? img { get; set; }
        public bool isAvaible { get; set; }
        public bool status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        // BorrowedBook kaydına sahip olabileceğini gösterir. Yani, bir kitap (Book) birden fazla kez ödünç alınabilir ve her ödünç alınma olayı BorrowedBook sınıfıyla kaydedilir.
        // "Lazy Loading", ilişkili verilerin (bu durumda BorrowedBooks) sadece gerektiğinde yüklenmesini sağlar.
        public virtual ICollection<BorrowedBook>? BorrowedBooks { get; set; }
    }
}
