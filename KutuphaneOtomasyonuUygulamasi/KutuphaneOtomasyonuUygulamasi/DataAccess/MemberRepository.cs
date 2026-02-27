using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using KutuphaneOtomasyonuUygulamasi.Models;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.DataAccess
{
    public class MemberRepository
    {
        public List<Member> GetAllMembers()
        {
            var members = new List<Member>();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT * FROM members ORDER BY ID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(MapReaderToMember(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting all members", ex);
                throw;
            }

            return members;
        }

        public Member GetMemberById(int id)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT * FROM members WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapReaderToMember(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error getting member with ID {id}", ex);
                throw;
            }

            return null;
        }

        public bool AddMember(Member member)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"INSERT INTO members 
                                    (full_name, email, phone, address, membership_number, 
                                    membership_date, membership_expiry_date, is_active) 
                                    VALUES (@FullName, @Email, @Phone, @Address, @MembershipNumber, 
                                    @MembershipDate, @MembershipExpiryDate, @IsActive)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FullName", member.FullName);
                        command.Parameters.AddWithValue("@Email", member.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Phone", member.Phone ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Address", member.Address ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MembershipNumber", member.MembershipNumber);
                        command.Parameters.AddWithValue("@MembershipDate", member.MembershipDate);
                        command.Parameters.AddWithValue("@MembershipExpiryDate", member.MembershipExpiryDate ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IsActive", member.IsActive);

                        int result = command.ExecuteNonQuery();
                        Logger.LogInfo($"Member added: {member.FullName}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error adding member: {member.FullName}", ex);
                throw;
            }
        }

        public bool UpdateMember(Member member)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"UPDATE members SET 
                                    full_name = @FullName, email = @Email, phone = @Phone, 
                                    address = @Address, membership_expiry_date = @MembershipExpiryDate, 
                                    is_active = @IsActive, borrowed_books_count = @BorrowedBooksCount,
                                    total_fines = @TotalFines
                                    WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", member.ID);
                        command.Parameters.AddWithValue("@FullName", member.FullName);
                        command.Parameters.AddWithValue("@Email", member.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Phone", member.Phone ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Address", member.Address ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@MembershipExpiryDate", member.MembershipExpiryDate ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IsActive", member.IsActive);
                        command.Parameters.AddWithValue("@BorrowedBooksCount", member.BorrowedBooksCount);
                        command.Parameters.AddWithValue("@TotalFines", member.TotalFines);

                        int result = command.ExecuteNonQuery();
                        Logger.LogInfo($"Member updated: ID {member.ID}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error updating member ID: {member.ID}", ex);
                throw;
            }
        }

        public DataTable GetMembersAsDataTable()
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ID, full_name as 'Ad Soyad', email as 'E-posta', 
                                    phone as 'Telefon', membership_number as 'Üyelik No', 
                                    membership_date as 'Üyelik Tarihi', 
                                    borrowed_books_count as 'Ödünç Kitap',
                                    total_fines as 'Toplam Ceza',
                                    is_active as 'Aktif'
                                    FROM members ORDER BY ID DESC";

                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting members as DataTable", ex);
                throw;
            }

            return dataTable;
        }

        private Member MapReaderToMember(MySqlDataReader reader)
        {
            return new Member
            {
                ID = reader.GetInt32("ID"),
                FullName = reader.GetString("full_name"),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                Address = reader.IsDBNull(reader.GetOrdinal("address")) ? null : reader.GetString("address"),
                MembershipNumber = reader.GetString("membership_number"),
                MembershipDate = reader.GetDateTime("membership_date"),
                MembershipExpiryDate = reader.IsDBNull(reader.GetOrdinal("membership_expiry_date")) ? (DateTime?)null : reader.GetDateTime("membership_expiry_date"),
                IsActive = reader.GetBoolean("is_active"),
                BorrowedBooksCount = reader.GetInt32("borrowed_books_count"),
                TotalFines = reader.GetDecimal("total_fines")
            };
        }
    }
}
