using api_cleany_app.src.Models;
using api_cleany_app.src.Helpers;
using Npgsql;

namespace api_cleany_app.src.Repositories
{
    public class AuthRepository
    {
        private readonly string _connectionString;
        private string _errorMessage;

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LocalConnection");
        }

        private static readonly Dictionary<string, int> RoleNameToId = new()
        {
            { "admin", 1 },
            { "cleaner", 2 },
            { "user", 3 }
        };

        public bool Authentication (string email, string password, out User user)
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

        //public User Registration(User user)
        //{
        //    string query = "INSERT INTO users (first_name, last_name, username, email, password, image_url, role_id) VALUES (@firstName, @lastName, @username, @email ,@password, @imageUrl @roleId);";
        //}

        public bool isEmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM users WHERE email = @email";

            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
            using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
            {
                command.Parameters.AddWithValue("@email", email);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public bool isUsernameExists(string username)
        {
            string query = "SELECT COUNT(*) FROM users WHERE username = @username";
            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
            using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
            {
                command.Parameters.AddWithValue("@username", username);
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        public string GetError() => _errorMessage;
    }
}