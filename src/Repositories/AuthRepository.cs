using api_cleany_app.src.Models;
using api_cleany_app.src.Helpers;
using Npgsql;

namespace api_cleany_app.src.Repositories
{
    public class AuthRepository
    {
        private readonly string _connectionString;
        private string _errorMessage = string.Empty;

        public AuthRepository()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        private static readonly Dictionary<string, int> RoleNameToId = new()
            {
                { "admin", 1 },
                { "cleaner", 2 },
                { "user", 3 }
            };

        public bool Authentication(string email, string password, out User user)
        {
            user = null;

            string query = "SELECT user_id, username, email, roles.name FROM users JOIN roles ON users.role_id = roles.role_id WHERE email = @email AND password = crypt(@password, password)";

            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))

                try
                {
                    using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@password", password);
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    UserId = reader.GetInt32(0),
                                    Username = reader.GetString(1),
                                    Email = reader.GetString(2),
                                    Role = reader.GetString(3)
                                };
                                return true;
                            }
                            else
                            {
                                _errorMessage = "Invalid email or password.";
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
        }

        public bool Registration(User user)
        {
            string query = "INSERT INTO users (first_name, last_name, username, email, password, image_url, role_id) VALUES (@firstName, @lastName, @username, @email ,crypt(@password, gen_salt('bf')), @imageUrl, @roleId);";

            RoleNameToId.TryGetValue(user.Role.ToLower(), out int roleId);

            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@firstName", user.FirstName);
                        command.Parameters.AddWithValue("@lastName", (object?)user.LastName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@username", user.Username);
                        command.Parameters.AddWithValue("@email", user.Email);
                        command.Parameters.AddWithValue("@password", user.Password);
                        command.Parameters.AddWithValue("@imageUrl", (object?)user.ImageUrl ?? DBNull.Value);
                        command.Parameters.AddWithValue("@roleId", roleId);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return true;
                        }
                        else
                        {
                            _errorMessage = "Registration failed.";
                            return false;
                        }
                    }
                }
                catch (PostgresException ex) when (ex.SqlState == "23505")
                {
                    if (ex.ConstraintName == "users_email_key")
                        _errorMessage = "Email already used.";
                    else if (ex.ConstraintName == "users_username_key")
                        _errorMessage = "Username already used.";
                    else
                        _errorMessage = "Data already exist (Duplicate).";

                    return false;
                }
        }

        public string GetError() => _errorMessage;
    }
}