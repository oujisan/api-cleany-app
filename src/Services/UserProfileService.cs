using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class UserProfileService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;

        public UserProfileService()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        public List<UserProfile> GetAllUserProfile()
        {
            List<UserProfile> users = new List<UserProfile>();
            string query = @"SELECT 
                        u.username, 
                        u.first_name, 
                        u.last_name, 
                        u.email, 
                        u.image_url, 
                        r.name AS role_name, 
                        s.name AS shift_name
                     FROM users u
                     JOIN roles r ON u.role_id = r.role_id
                     LEFT JOIN shifts s ON u.shift_id = s.shift_id";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new UserProfile()
                        {
                            Username = reader["username"].ToString(),
                            FirstName = reader["first_name"].ToString(),
                            LastName = reader.IsDBNull(reader.GetOrdinal("last_name")) ? string.Empty : reader["last_name"].ToString(),
                            Email = reader["email"].ToString(),
                            ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url")) ? string.Empty : reader["image_url"].ToString(),
                            Role = reader["role_name"].ToString(),
                            Shift = reader.IsDBNull(reader.GetOrdinal("shift_name")) ? null : reader["shift_name"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return new List<UserProfile>();
            }

            return users;
        }

        public UserProfile getUserProfile(int userId)
        {
            UserProfile user = null;
            string query = @"SELECT username, first_name, last_name, email, image_url, r.name AS role_name, s.name AS shift_name
              FROM users u
              JOIN roles r ON u.role_id = r.role_id
              LEFT JOIN shifts s ON u.shift_id = s.shift_id
              WHERE user_id = @userId";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("userId", userId);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {

                            if (reader.Read())
                            {
                                user = new UserProfile()
                                {
                                    Username = reader.GetString(0),
                                    FirstName = reader.GetString(1),
                                    LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Email = reader.GetString(3),
                                    ImageUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                    Role = reader.GetString(5),
                                    Shift = reader.GetString(5) != "cleaner" ? null : reader.GetString(6),
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                }
            return user;
        }

        public bool updateUserProfile(UserProfile user, int userId)
        {
            string query = @"UPDATE users SET
                first_name = @firstName,
                last_name = @lastName,
                username = @username,
                password = crypt(@password, gen_salt('bf')),
                email = @email,
                updated_at = NOW()
                WHERE user_id = @userId";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("userId", userId);
                        command.Parameters.AddWithValue("firstName", user.FirstName);
                        command.Parameters.AddWithValue("lastName", (object?)user.LastName ?? DBNull.Value);
                        command.Parameters.AddWithValue("password", user.Password);
                        command.Parameters.AddWithValue("username", user.Username);
                        command.Parameters.AddWithValue("email", user.Email);

                        var result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception e)
                {
                    _errorMessage = e.Message;
                }
            return false;
        }
        public bool softDeleteUser(int userId)
        {
            string query = "UPDATE users SET is_active = false WHERE user_id = @UserId";

            try
            {
                using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("UserId", userId);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }

            return false;
        }

        public string getError() => _errorMessage;
    }
}
