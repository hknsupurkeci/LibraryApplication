using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryApplication.ViewModel
{
    public class BorrowBookViewModel
    {
        public int book_id { get; set; }
        public string? user_name { get; set; }
        public DateTime delivery_date { get; set; }
    }
}