using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using KutuphaneOtomasyonuUygulamasi.Models;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.DataAccess
{
    public class BookRepository
    {
        public List<Book> GetAllBooks()
        {
            var books = new List<Book>();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ID, ad as Title, yazar as Author, ISBN, 
                                    publisher, publication_year, category, total_copies, 
                                    available_copies, shelf_location, created_date, modified_date 
                                    FROM kitaplar ORDER BY ID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(MapReaderToBook(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting all books", ex);
                throw;
            }

            return books;
        }

        public Book GetBookById(int id)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ID, ad as Title, yazar as Author, ISBN, 
                                    publisher, publication_year, category, total_copies, 
                                    available_copies, shelf_location, created_date, modified_date 
                                    FROM kitaplar WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapReaderToBook(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error getting book with ID {id}", ex);
                throw;
            }

            return null;
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            var books = new List<Book>();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ID, ad as Title, yazar as Author, ISBN, 
                                    publisher, publication_year, category, total_copies, 
                                    available_copies, shelf_location, created_date, modified_date 
                                    FROM kitaplar 
                                    WHERE ad LIKE @SearchTerm 
                                    OR yazar LIKE @SearchTerm 
                                    OR ISBN LIKE @SearchTerm
                                    OR category LIKE @SearchTerm
                                    ORDER BY ID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                books.Add(MapReaderToBook(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error searching books with term: {searchTerm}", ex);
                throw;
            }

            return books;
        }

        public bool AddBook(Book book)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"INSERT INTO kitaplar 
                                    (ad, yazar, ISBN, publisher, publication_year, category, 
                                    total_copies, available_copies, shelf_location, created_date) 
                                    VALUES (@Title, @Author, @ISBN, @Publisher, @PublicationYear, 
                                    @Category, @TotalCopies, @AvailableCopies, @ShelfLocation, @CreatedDate)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Title", book.Title);
                        command.Parameters.AddWithValue("@Author", book.Author);
                        command.Parameters.AddWithValue("@ISBN", book.ISBN);
                        command.Parameters.AddWithValue("@Publisher", book.Publisher ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PublicationYear", book.PublicationYear ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Category", book.Category ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
                        command.Parameters.AddWithValue("@AvailableCopies", book.AvailableCopies);
                        command.Parameters.AddWithValue("@ShelfLocation", book.ShelfLocation ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDate", book.CreatedDate);

                        int result = command.ExecuteNonQuery();
                        Logger.LogInfo($"Book added: {book.Title} by {book.Author}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error adding book: {book.Title}", ex);
                throw;
            }
        }

        public bool UpdateBook(Book book)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"UPDATE kitaplar SET 
                                    ad = @Title, yazar = @Author, ISBN = @ISBN, 
                                    publisher = @Publisher, publication_year = @PublicationYear, 
                                    category = @Category, total_copies = @TotalCopies, 
                                    available_copies = @AvailableCopies, shelf_location = @ShelfLocation,
                                    modified_date = @ModifiedDate 
                                    WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", book.ID);
                        command.Parameters.AddWithValue("@Title", book.Title);
                        command.Parameters.AddWithValue("@Author", book.Author);
                        command.Parameters.AddWithValue("@ISBN", book.ISBN);
                        command.Parameters.AddWithValue("@Publisher", book.Publisher ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PublicationYear", book.PublicationYear ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Category", book.Category ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
                        command.Parameters.AddWithValue("@AvailableCopies", book.AvailableCopies);
                        command.Parameters.AddWithValue("@ShelfLocation", book.ShelfLocation ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);

                        int result = command.ExecuteNonQuery();
                        Logger.LogInfo($"Book updated: ID {book.ID}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error updating book ID: {book.ID}", ex);
                throw;
            }
        }

        public bool DeleteBook(int id)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM kitaplar WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);

                        int result = command.ExecuteNonQuery();
                        Logger.LogInfo($"Book deleted: ID {id}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error deleting book ID: {id}", ex);
                throw;
            }
        }

        public DataTable GetBooksAsDataTable()
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ID, ad as 'Kitap Adı', yazar as 'Yazar', 
                                    ISBN, publisher as 'Yayınevi', publication_year as 'Yıl',
                                    category as 'Kategori', total_copies as 'Toplam', 
                                    available_copies as 'Mevcut', shelf_location as 'Raf'
                                    FROM kitaplar ORDER BY ID DESC";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting books as DataTable", ex);
                throw;
            }

            return dataTable;
        }

        private Book MapReaderToBook(MySqlDataReader reader)
        {
            return new Book
            {
                ID = reader.GetInt32("ID"),
                Title = reader.IsDBNull(reader.GetOrdinal("Title")) ? "" : reader.GetString("Title"),
                Author = reader.IsDBNull(reader.GetOrdinal("Author")) ? "" : reader.GetString("Author"),
                ISBN = reader.IsDBNull(reader.GetOrdinal("ISBN")) ? "" : reader.GetString("ISBN"),
                Publisher = reader.IsDBNull(reader.GetOrdinal("publisher")) ? null : reader.GetString("publisher"),
                PublicationYear = reader.IsDBNull(reader.GetOrdinal("publication_year")) ? (int?)null : reader.GetInt32("publication_year"),
                Category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetString("category"),
                TotalCopies = reader.IsDBNull(reader.GetOrdinal("total_copies")) ? 1 : reader.GetInt32("total_copies"),
                AvailableCopies = reader.IsDBNull(reader.GetOrdinal("available_copies")) ? 1 : reader.GetInt32("available_copies"),
                ShelfLocation = reader.IsDBNull(reader.GetOrdinal("shelf_location")) ? null : reader.GetString("shelf_location"),
                CreatedDate = reader.IsDBNull(reader.GetOrdinal("created_date")) ? DateTime.Now : reader.GetDateTime("created_date"),
                ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date")) ? (DateTime?)null : reader.GetDateTime("modified_date")
            };
        }
    }
}
