using System;

namespace KutuphaneOtomasyonuUygulamasi.Models
{
    public class BorrowRecord
    {
        public int ID { get; set; }
        public int BookID { get; set; }
        public int MemberID { get; set; }
        public string MemberName { get; set; }
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal? LateFee { get; set; }
        public BorrowStatus Status { get; set; }
        public string Notes { get; set; }

        public BorrowRecord()
        {
            BorrowDate = DateTime.Now;
            DueDate = DateTime.Now.AddDays(14); // 2 weeks default
            Status = BorrowStatus.Active;
        }

        public bool IsOverdue => Status == BorrowStatus.Active && DateTime.Now > DueDate;
        public int DaysOverdue => IsOverdue ? (DateTime.Now - DueDate).Days : 0;
    }

    public enum BorrowStatus
    {
        Active = 1,
        Returned = 2,
        Overdue = 3,
        Lost = 4
    }
}
