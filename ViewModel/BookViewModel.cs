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
        [Required]
        public IFormFile? img { get; set; }
    }
}