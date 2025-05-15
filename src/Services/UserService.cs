using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using Npgsql;

namespace api_cleany_app.src.Services
{
    public class UserService
    {
        private string _connectionString;
        private string _errorMessage = string.Empty;
        private RoleService _roleService;
        private ShiftService _shiftService;

        public UserService(RoleService roleService, ShiftService shiftService)
        {
            _connectionString = DbConfig.ConnectionString;
            _roleService = roleService;
            _shiftService = shiftService;
        }

        public List<User> getAllUser()
        {
            List<User> users = new List<User>();
            string query = @"SELECT user_id, username, first_name, last_name, email, password, r.name AS role_name, s.name AS shift_name
              FROM users u
              JOIN roles r ON u.role_id = r.role_id
              LEFT JOIN shifts s ON u.shift_id = s.shift_id
              ORDER BY user_id ASC";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            try
            {
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User()
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            LastName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Email = reader.GetString(4),
                            ImageUrl = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Password = reader.GetString(6),
                            Role = reader.GetString(7),
                            Shift = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
            }
            return users;
        }

        public User getUserById(int userId)
        {
            User user = null;
            string query = @"SELECT user_id, username, first_name, last_name, email, password, r.name role_name, s.name AS shift_name
                FROM users u
                JOIN roles r ON u.role_id = r.role_id
                LEFT JOIN shifts s ON u.shift_id = s.shift_id
                WHERE user_id = @user_id";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            try
            {
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User()
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            LastName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            Email = reader.GetString(4),
                            ImageUrl = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            Password = reader.GetString(6),
                            Role = reader.GetString(7),
                            Shift = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                        };
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return user;
        }

        public bool addUser(User user)
        {
            string query = @"
                INSERT INTO users 
                    (first_name, last_name, username, email, password, image_url, role_id, shift_id) 
                VALUES
                    (@firstName, @lastName, @username, @email, crypt(@password, gen_salt('bf')), @imageUrl, @roleId, @shiftId);";

            int? roleId = _roleService.getRoleIdByName(user.Role);
            if (roleId == null)
            {
                _errorMessage = $"Role '{user.Role}' was not found in the database.";
                return false;
            }

            int? shiftId = _shiftService.getShiftIdByName(user.Shift);

            try
            {
                using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("@firstName", user.FirstName);
                    command.Parameters.AddWithValue("@lastName", (object?)user.LastName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@imageUrl", (object?)user.ImageUrl ?? DBNull.Value);
                    command.Parameters.AddWithValue("@roleId", roleId.Value);
                    command.Parameters.AddWithValue("@shiftId", (object?)shiftId ?? DBNull.Value);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        return true;

                    _errorMessage = "Add User failed.";
                    return false;
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
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return false;
            }
        }

        public bool updateUser(User user, int userId)
        {
            string query = @"UPDATE users SET
                first_name = @firstName,
                last_name = @lastName,
                username = @username,
                email = @email,
                role_id = @roleId,
                shift_id = @shiftId,
                updated_at = NOW()
                WHERE user_id = @userId";

            int? roleId = _roleService.getRoleIdByName(user.Role);
            if (roleId == null)
            {
                _errorMessage = $"Role '{user.Role}' was not found in the database.";
                return false;
            }

            int? shiftId = _shiftService.getShiftIdByName(user.Shift);

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
            try
            {
                using (NpgsqlCommand command = sqlDbHelper.NpgsqlCommand(query))
                {
                    command.Parameters.AddWithValue("userId", userId);
                    command.Parameters.AddWithValue("firstName", user.FirstName);
                    command.Parameters.AddWithValue("lastName", (object?)user.LastName ?? DBNull.Value);
                    command.Parameters.AddWithValue("username", user.Username);
                    command.Parameters.AddWithValue("email", user.Email);
                    command.Parameters.AddWithValue("roleId", roleId);
                    command.Parameters.AddWithValue("shiftId", (object?)shiftId ?? DBNull.Value);

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

        public bool deleteUser(int userId)
        {
            string query = "DELETE FROM users WHERE user_id = @UserId";

            using (SqlDbHelper sqlDbHelper = new SqlDbHelper(_connectionString))
                try
                {
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

        public string GetError() => _errorMessage;
    }
}