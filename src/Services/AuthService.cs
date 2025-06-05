using api_cleany_app.src.Models;
using api_cleany_app.src.Helpers;
using Npgsql;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using BCrypt;

namespace api_cleany_app.src.Services
{
    public class AuthService
    {
        private readonly string _connectionString;
        private string _errorMessage = string.Empty;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public AuthService(IConfiguration configuration, IMemoryCache cache)
        {
            _config = configuration;
            _cache = cache;
            _connectionString = DbConfig.ConnectionString;
        }

        public bool Authentication(string email, string password, out User user)
        {
            user = null;

            string getUserQuery = @"
                SELECT u.user_id, u.username, u.email, r.name AS role_name, u.password 
                FROM users u 
                JOIN roles r ON u.role_id = r.role_id 
                WHERE u.email = @Email";

            try
            {
                using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
                using (NpgsqlCommand command = dbHelper.NpgsqlCommand(getUserQuery))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hashedPasswordFromDb = reader.GetString(4);

                            // Verifikasi password
                            if (BCrypt.Net.BCrypt.Verify(password, hashedPasswordFromDb))
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
                _errorMessage = $"Authentication error: {ex.Message}";
                return false;
            }
        }


        public bool Registration(Registration user)
        {
            string query = "INSERT INTO users (first_name, last_name, username, email, password, image_url, role_id, shift_id) VALUES (@firstName, @lastName, @username, @email ,@password, NULL, @roleId, NULL);";

            

            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@firstName", user.FirstName);
                        command.Parameters.AddWithValue("@lastName", (object?)user.LastName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@username", user.Username);
                        command.Parameters.AddWithValue("@email", user.Email);
                        command.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(user.Password));
                        command.Parameters.AddWithValue("@roleId", 3);

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

        public string GetUsernameByEmail(string email)
        {
            string query = "SELECT username FROM users WHERE email = @Email";
            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        string username = command.ExecuteScalar()?.ToString();
                        return username ?? string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    return string.Empty;
                }
            }
        }

        public bool IsEmailExist(string email)
        {
            string query = "SELECT COUNT(*) FROM users WHERE email = @Email";

            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    return false; 
                }
            }
        }

        public bool verifyPassword(string email, string inputPassword)
        {
            string query = "SELECT password FROM users WHERE email = @Email";

            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
            {
                try
                {
                    using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        string hashedPassword = command.ExecuteScalar()?.ToString();
                        if (hashedPassword != null)
                        {
                            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
                        }
                        else
                        {
                            _errorMessage = "Email not found.";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    return false;
                }
            }
        }

        public bool ResetPassword(ResetPassword resetPassword)
        {
            string query_reset_password = "UPDATE users SET password = @NewPassword WHERE email = @Email";
            using (SqlDbHelper dbHelper = new SqlDbHelper(_connectionString))
                try
                {
                    using (NpgsqlCommand command = dbHelper.NpgsqlCommand(query_reset_password))
                    {
                        command.Parameters.AddWithValue("@NewPassword", BCrypt.Net.BCrypt.HashPassword(resetPassword.NewPassword));
                        command.Parameters.AddWithValue("@Email", resetPassword.Email);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return true;
                        }
                        else
                        {
                            _errorMessage = "Reset password failed.";
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    return false;
                }
        }

        public async Task<bool> SendVerificationEmailAsync(string username, string email, string verificationCode)
        {
            _cache.Set($"otp_code_{email}", verificationCode, TimeSpan.FromMinutes(10));
            var smtpClient = new SmtpClient(_config["Smtp:Host"])
            {
                Port = int.Parse(_config["Smtp:Port"]),
                Credentials = new NetworkCredential(_config["Smtp:Email"], _config["Smtp:AppPassword"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["Smtp:Email"], "Cleany App"),
                Subject = "Cleany App - Password Reset Verification Code",
                IsBodyHtml = true,
                Body = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <p>Hi {username},</p>

                            <p>We received a request to reset your Cleany App account password.</p>

                            <p><strong>Your verification code is:</strong></p>
                            <h3 style='color: #009688;'>{verificationCode}</h3>

                            <p>This code will expire in 10 minutes. Please do not share it with anyone.</p>

                            <p>If you did not request this, you can safely ignore this message.</p>

                            <p>Thank you,<br/>
                            The Cleany App Team</p>

                            <hr/>
                            <small>This is an automated message. Please do not reply to this email.</small>
                        </body>
                    </html>"
            };

            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                return false;
            }
        }
        public string GenerateVerificationCode()
        {
            var random = new Random();
            int code = random.Next(100000, 999999);
            return code.ToString();
        }
        public bool ValidateOtpToken(string email, string inputCode)
        {
            if (_cache.TryGetValue($"otp_code_{email}", out string savedCode))
            {
                return savedCode == inputCode;
            }
            return false;
        }

        public string GetError() => _errorMessage;
    }
}