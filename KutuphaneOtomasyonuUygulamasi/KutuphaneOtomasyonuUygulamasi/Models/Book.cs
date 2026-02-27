using System;

namespace KutuphaneOtomasyonuUygulamasi.Models
{
    public class Book
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public int? PublicationYear { get; set; }
        public string Category { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string ShelfLocation { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public Book()
        {
            CreatedDate = DateTime.Now;
            TotalCopies = 1;
            AvailableCopies = 1;
        }

        public bool IsAvailable => AvailableCopies > 0;
    }
}
