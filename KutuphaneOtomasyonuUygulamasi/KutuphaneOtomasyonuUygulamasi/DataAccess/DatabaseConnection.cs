using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.DataAccess
{
    public class DatabaseConnection
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = ConfigurationManager.ConnectionStrings["LibraryDB"]?.ConnectionString;
                    
                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        // Fallback to default connection string
                        _connectionString = "server=localhost;port=3306;database=kutuphane;user=root;password=;";
                    }
                }
                return _connectionString;
            }
        }

        public static MySqlConnection GetConnection()
        {
            try
            {
                var connection = new MySqlConnection(ConnectionString);
                return connection;
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to create database connection", ex);
                throw new Exception("Veritabanı bağlantısı oluşturulamadı. Lütfen bağlantı ayarlarını kontrol edin.", ex);
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Database connection test failed", ex);
                return false;
            }
        }
    }
}
