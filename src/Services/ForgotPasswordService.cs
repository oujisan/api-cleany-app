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

        public async Task<bool> SendVerificationEmailAsync(string email, string verificationCode)
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
                From = new MailAddress(_config["Smtp:Email"]),
                Subject = "Verification Code Reset Password",
                Body = $"Your verification code is: {verificationCode}",
                IsBodyHtml = true,
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
