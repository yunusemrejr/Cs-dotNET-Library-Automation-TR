using System;

namespace KutuphaneOtomasyonuUygulamasi.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public User()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
            Role = UserRole.Librarian;
        }
    }

    public enum UserRole
    {
        Admin = 1,
        Librarian = 2,
        Member = 3
    }
}
