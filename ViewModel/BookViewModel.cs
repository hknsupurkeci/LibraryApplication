using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApplication.ViewModel
{
    public class BookViewModel
    {
        [Required]
        public string? name { get; set; }
        [Required]
        public string? author { get; set; }
        /// <summary>
        /// IFormFile arayüzü, ASP.NET Core'da form üzerinden yüklenen dosyaları temsil eder. 
        /// Bu arayüz, kullanıcıların bir web formu aracılığıyla yükledikleri dosyalarla çalışmanızı sağlar.
        /// Özellikle, dosyaları sunucuya yüklemek ve işlemek için kullanılır.
        /// </summary>
        /// <value></value>
        [Required]
        public IFormFile? img { get; set; }
    }
}