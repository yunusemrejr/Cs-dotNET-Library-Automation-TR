using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using KutuphaneOtomasyonuUygulamasi.Models;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.DataAccess
{
    public class BorrowRepository
    {
        public List<BorrowRecord> GetActiveBorrows()
        {
            var borrows = new List<BorrowRecord>();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT b.*, m.full_name as member_name, k.ad as book_title 
                                    FROM borrow_records b
                                    INNER JOIN members m ON b.member_id = m.ID
                                    INNER JOIN kitaplar k ON b.book_id = k.ID
                                    WHERE b.status = 1 OR b.status = 3
                                    ORDER BY b.borrow_date DESC";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            borrows.Add(MapReaderToBorrowRecord(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting active borrows", ex);
                throw;
            }

            return borrows;
        }

        public bool BorrowBook(BorrowRecord record)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string insertQuery = @"INSERT INTO borrow_records 
                                                (book_id, member_id, borrow_date, due_date, status, notes) 
                                                VALUES (@BookID, @MemberID, @BorrowDate, @DueDate, @Status, @Notes)";

                            using (var command = new MySqlCommand(insertQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@BookID", record.BookID);
                                command.Parameters.AddWithValue("@MemberID", record.MemberID);
                                command.Parameters.AddWithValue("@BorrowDate", record.BorrowDate);
                                command.Parameters.AddWithValue("@DueDate", record.DueDate);
                                command.Parameters.AddWithValue("@Status", (int)record.Status);
                                command.Parameters.AddWithValue("@Notes", record.Notes ?? (object)DBNull.Value);

                                command.ExecuteNonQuery();
                            }

                            string updateBookQuery = "UPDATE kitaplar SET available_copies = available_copies - 1 WHERE ID = @BookID";
                            using (var command = new MySqlCommand(updateBookQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@BookID", record.BookID);
                                command.ExecuteNonQuery();
                            }

                            string updateMemberQuery = "UPDATE members SET borrowed_books_count = borrowed_books_count + 1 WHERE ID = @MemberID";
                            using (var command = new MySqlCommand(updateMemberQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@MemberID", record.MemberID);
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            Logger.LogInfo($"Book borrowed: BookID {record.BookID}, MemberID {record.MemberID}");
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error borrowing book: BookID {record.BookID}", ex);
                throw;
            }
        }

        public bool ReturnBook(int borrowId, decimal? lateFee = null)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            int bookId = 0;
                            int memberId = 0;

                            string selectQuery = "SELECT book_id, member_id FROM borrow_records WHERE ID = @ID";
                            using (var command = new MySqlCommand(selectQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@ID", borrowId);
                                using (var reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        bookId = reader.GetInt32("book_id");
                                        memberId = reader.GetInt32("member_id");
                                    }
                                }
                            }

                            string updateQuery = @"UPDATE borrow_records SET 
                                                return_date = @ReturnDate, 
                                                late_fee = @LateFee, 
                                                status = @Status 
                                                WHERE ID = @ID";

                            using (var command = new MySqlCommand(updateQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@ReturnDate", DateTime.Now);
                                command.Parameters.AddWithValue("@LateFee", lateFee ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@Status", (int)BorrowStatus.Returned);
                                command.Parameters.AddWithValue("@ID", borrowId);

                                command.ExecuteNonQuery();
                            }

                            string updateBookQuery = "UPDATE kitaplar SET available_copies = available_copies + 1 WHERE ID = @BookID";
                            using (var command = new MySqlCommand(updateBookQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@BookID", bookId);
                                command.ExecuteNonQuery();
                            }

                            string updateMemberQuery = @"UPDATE members SET 
                                                        borrowed_books_count = borrowed_books_count - 1,
                                                        total_fines = total_fines + @LateFee
                                                        WHERE ID = @MemberID";
                            using (var command = new MySqlCommand(updateMemberQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@MemberID", memberId);
                                command.Parameters.AddWithValue("@LateFee", lateFee ?? 0);
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            Logger.LogInfo($"Book returned: BorrowID {borrowId}");
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error returning book: BorrowID {borrowId}", ex);
                throw;
            }
        }

        public DataTable GetBorrowsAsDataTable()
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT b.ID, m.full_name as 'Üye Adı', k.ad as 'Kitap Adı', 
                                    b.borrow_date as 'Ödünç Tarihi', b.due_date as 'İade Tarihi',
                                    b.return_date as 'Teslim Tarihi', b.late_fee as 'Gecikme Ücreti',
                                    CASE b.status 
                                        WHEN 1 THEN 'Aktif'
                                        WHEN 2 THEN 'İade Edildi'
                                        WHEN 3 THEN 'Gecikmiş'
                                        WHEN 4 THEN 'Kayıp'
                                    END as 'Durum'
                                    FROM borrow_records b
                                    INNER JOIN members m ON b.member_id = m.ID
                                    INNER JOIN kitaplar k ON b.book_id = k.ID
                                    ORDER BY b.borrow_date DESC";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting borrows as DataTable", ex);
                throw;
            }

            return dataTable;
        }

        private BorrowRecord MapReaderToBorrowRecord(MySqlDataReader reader)
        {
            return new BorrowRecord
            {
                ID = reader.GetInt32("ID"),
                BookID = reader.GetInt32("book_id"),
                MemberID = reader.GetInt32("member_id"),
                MemberName = reader.GetString("member_name"),
                BookTitle = reader.GetString("book_title"),
                BorrowDate = reader.GetDateTime("borrow_date"),
                DueDate = reader.GetDateTime("due_date"),
                ReturnDate = reader.IsDBNull(reader.GetOrdinal("return_date")) ? (DateTime?)null : reader.GetDateTime("return_date"),
                LateFee = reader.IsDBNull(reader.GetOrdinal("late_fee")) ? (decimal?)null : reader.GetDecimal("late_fee"),
                Status = (BorrowStatus)reader.GetInt32("status"),
                Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes")
            };
        }
    }
}
