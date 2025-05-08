using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace api_cleany_app.src.Services
{
    public class ForgotPasswordService
    {
        public string _errorMessage;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public ForgotPasswordService(IConfiguration configuration, IMemoryCache cache)
        {
            _config = configuration;
            _cache = cache;
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
