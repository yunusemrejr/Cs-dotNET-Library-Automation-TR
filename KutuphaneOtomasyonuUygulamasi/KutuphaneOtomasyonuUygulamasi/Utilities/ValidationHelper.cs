using System;
using System.Text.RegularExpressions;

namespace KutuphaneOtomasyonuUygulamasi.Utilities
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            var regex = new Regex(@"^[\d\s\-\+\(\)]+$");
            return regex.IsMatch(phone) && phone.Length >= 10;
        }

        public static bool IsValidISBN(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            isbn = isbn.Replace("-", "").Replace(" ", "");
            return isbn.Length == 10 || isbn.Length == 13;
        }

        public static bool IsValidYear(int? year)
        {
            if (!year.HasValue)
                return true;

            return year.Value >= 1000 && year.Value <= DateTime.Now.Year + 1;
        }

        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Trim();
        }
    }
}
