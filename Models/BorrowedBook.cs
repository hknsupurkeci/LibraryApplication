using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApplication.Models
{
    public class BorrowedBook
    {
        public int id { get; set; }
        public int book_id { get; set; }
        public string? user_name { get; set; }
        public bool status { get; set; }
        public DateTime delivery_date { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        [ForeignKey("book_id")]
        public virtual Book? Book { get; set; }

    }
}