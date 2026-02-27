using System;

namespace KutuphaneOtomasyonuUygulamasi.Models
{
    public class Member
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string MembershipNumber { get; set; }
        public DateTime MembershipDate { get; set; }
        public DateTime? MembershipExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int BorrowedBooksCount { get; set; }
        public decimal TotalFines { get; set; }

        public Member()
        {
            MembershipDate = DateTime.Now;
            MembershipExpiryDate = DateTime.Now.AddYears(1);
            IsActive = true;
            BorrowedBooksCount = 0;
            TotalFines = 0;
        }

        public bool CanBorrow => IsActive && BorrowedBooksCount < 5 && TotalFines < 100;
    }
}
