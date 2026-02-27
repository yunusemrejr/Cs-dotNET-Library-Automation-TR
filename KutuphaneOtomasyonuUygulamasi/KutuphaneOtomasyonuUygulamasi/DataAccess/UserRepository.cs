using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using KutuphaneOtomasyonuUygulamasi.Models;
using KutuphaneOtomasyonuUygulamasi.Utilities;

namespace KutuphaneOtomasyonuUygulamasi.DataAccess
{
    public class UserRepository
    {
        public User AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ID, username, password_hash, full_name, email, 
                                    phone, role, is_active, created_date, last_login_date 
                                    FROM login WHERE username = @Username AND is_active = 1";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString("password_hash");
                                
                                if (PasswordHelper.VerifyPassword(password, storedHash))
                                {
                                    var user = MapReaderToUser(reader);
                                    UpdateLastLogin(user.ID);
                                    Logger.LogInfo($"User authenticated: {username}");
                                    return user;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error authenticating user: {username}", ex);
            }

            return null;
        }

        public bool AddUser(User user)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"INSERT INTO login 
                                    (username, password_hash, full_name, email, phone, role, 
                                    is_active, created_date) 
                                    VALUES (@Username, @PasswordHash, @FullName, @Email, @Phone, 
                                    @Role, @IsActive, @CreatedDate)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        command.Parameters.AddWithValue("@FullName", user.FullName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Role", (int)user.Role);
                        command.Parameters.AddWithValue("@IsActive", user.IsActive);
                        command.Parameters.AddWithValue("@CreatedDate", user.CreatedDate);

                        int result = command.ExecuteNonQuery();
                        Logger.LogInfo($"User added: {user.Username}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error adding user: {user.Username}", ex);
                throw;
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ID, username, password_hash, full_name, email, 
                                    phone, role, is_active, created_date, last_login_date 
                                    FROM login ORDER BY ID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(MapReaderToUser(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting all users", ex);
                throw;
            }

            return users;
        }

        private void UpdateLastLogin(int userId)
        {
            try
            {
                using (var connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = "UPDATE login SET last_login_date = @LastLogin WHERE ID = @ID";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@LastLogin", DateTime.Now);
                        command.Parameters.AddWithValue("@ID", userId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error updating last login for user ID: {userId}", ex);
            }
        }

        private User MapReaderToUser(MySqlDataReader reader)
        {
            return new User
            {
                ID = reader.GetInt32("ID"),
                Username = reader.GetString("username"),
                PasswordHash = reader.GetString("password_hash"),
                FullName = reader.IsDBNull(reader.GetOrdinal("full_name")) ? null : reader.GetString("full_name"),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString("email"),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                Role = (UserRole)reader.GetInt32("role"),
                IsActive = reader.GetBoolean("is_active"),
                CreatedDate = reader.GetDateTime("created_date"),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("last_login_date")) ? (DateTime?)null : reader.GetDateTime("last_login_date")
            };
        }
    }
}
