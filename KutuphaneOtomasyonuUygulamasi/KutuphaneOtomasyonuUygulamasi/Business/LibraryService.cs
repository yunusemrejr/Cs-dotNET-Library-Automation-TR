using System;
using System.Collections.Generic;
using System.Data;
using KutuphaneOtomasyonuUygulamasi.Models;
using KutuphaneOtomasyonuUygulamasi.DataAccess;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.Business
{
    public class LibraryService
    {
        private readonly BookRepository _bookRepository;
        private readonly UserRepository _userRepository;
        private readonly MemberRepository _memberRepository;
        private readonly BorrowRepository _borrowRepository;

        public LibraryService()
        {
            _bookRepository = new BookRepository();
            _userRepository = new UserRepository();
            _memberRepository = new MemberRepository();
            _borrowRepository = new BorrowRepository();
        }

        public User Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Kullanıcı adı ve şifre boş olamaz.");
            }

            return _userRepository.AuthenticateUser(username, password);
        }

        public bool AddBook(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Kitap adı boş olamaz.");

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Yazar adı boş olamaz.");

            if (!string.IsNullOrWhiteSpace(book.ISBN) && !ValidationHelper.IsValidISBN(book.ISBN))
                throw new ArgumentException("Geçersiz ISBN formatı.");

            if (!ValidationHelper.IsValidYear(book.PublicationYear))
                throw new ArgumentException("Geçersiz yayın yılı.");

            book.Title = ValidationHelper.SanitizeInput(book.Title);
            book.Author = ValidationHelper.SanitizeInput(book.Author);

            return _bookRepository.AddBook(book);
        }

        public bool UpdateBook(Book book)
        {
            if (book.ID <= 0)
                throw new ArgumentException("Geçersiz kitap ID.");

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Kitap adı boş olamaz.");

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Yazar adı boş olamaz.");

            book.Title = ValidationHelper.SanitizeInput(book.Title);
            book.Author = ValidationHelper.SanitizeInput(book.Author);
            book.ModifiedDate = DateTime.Now;

            return _bookRepository.UpdateBook(book);
        }

        public bool DeleteBook(int bookId)
        {
            if (bookId <= 0)
                throw new ArgumentException("Geçersiz kitap ID.");

            return _bookRepository.DeleteBook(bookId);
        }

        public List<Book> GetAllBooks()
        {
            return _bookRepository.GetAllBooks();
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllBooks();

            return _bookRepository.SearchBooks(ValidationHelper.SanitizeInput(searchTerm));
        }

        public DataTable GetBooksDataTable()
        {
            return _bookRepository.GetBooksAsDataTable();
        }

        public bool BorrowBook(int bookId, int memberId, int daysToReturn = 14)
        {
            var book = _bookRepository.GetBookById(bookId);
            if (book == null)
                throw new ArgumentException("Kitap bulunamadı.");

            if (!book.IsAvailable)
                throw new InvalidOperationException("Kitap mevcut değil.");

            var member = _memberRepository.GetMemberById(memberId);
            if (member == null)
                throw new ArgumentException("Üye bulunamadı.");

            if (!member.CanBorrow)
                throw new InvalidOperationException("Üye ödünç alma limitine ulaştı veya cezası var.");

            var borrowRecord = new BorrowRecord
            {
                BookID = bookId,
                MemberID = memberId,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(daysToReturn),
                Status = BorrowStatus.Active
            };

            return _borrowRepository.BorrowBook(borrowRecord);
        }

        public bool ReturnBook(int borrowId)
        {
            var borrows = _borrowRepository.GetActiveBorrows();
            var borrow = borrows.Find(b => b.ID == borrowId);

            if (borrow == null)
                throw new ArgumentException("Ödünç kaydı bulunamadı.");

            decimal? lateFee = null;
            if (borrow.IsOverdue)
            {
                lateFee = borrow.DaysOverdue * 2; // 2 TL per day
            }

            return _borrowRepository.ReturnBook(borrowId, lateFee);
        }

        public DataTable GetBorrowsDataTable()
        {
            return _borrowRepository.GetBorrowsAsDataTable();
        }

        public bool AddMember(Member member)
        {
            if (string.IsNullOrWhiteSpace(member.FullName))
                throw new ArgumentException("Üye adı boş olamaz.");

            if (!string.IsNullOrWhiteSpace(member.Email) && !ValidationHelper.IsValidEmail(member.Email))
                throw new ArgumentException("Geçersiz e-posta adresi.");

            if (!string.IsNullOrWhiteSpace(member.Phone) && !ValidationHelper.IsValidPhone(member.Phone))
                throw new ArgumentException("Geçersiz telefon numarası.");

            member.FullName = ValidationHelper.SanitizeInput(member.FullName);
            member.MembershipNumber = GenerateMembershipNumber();

            return _memberRepository.AddMember(member);
        }

        public DataTable GetMembersDataTable()
        {
            return _memberRepository.GetMembersAsDataTable();
        }

        private string GenerateMembershipNumber()
        {
            return $"M{DateTime.Now:yyyyMMddHHmmss}";
        }

        public Dictionary<string, int> GetDashboardStatistics()
        {
            var stats = new Dictionary<string, int>();

            try
            {
                var books = _bookRepository.GetAllBooks();
                var borrows = _borrowRepository.GetActiveBorrows();
                var members = _memberRepository.GetAllMembers();

                stats["TotalBooks"] = books.Count;
                stats["AvailableBooks"] = books.FindAll(b => b.IsAvailable).Count;
                stats["BorrowedBooks"] = borrows.Count;
                stats["TotalMembers"] = members.Count;
                stats["ActiveMembers"] = members.FindAll(m => m.IsActive).Count;
                stats["OverdueBooks"] = borrows.FindAll(b => b.IsOverdue).Count;
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting dashboard statistics", ex);
            }

            return stats;
        }
    }
}
