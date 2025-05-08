using Microsoft.AspNetCore.Mvc;
using api_cleany_app.src.Repositories;
using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;
using api_cleany_app.src.Services;
using Microsoft.Extensions.Caching.Memory;

namespace api_cleany_app.src.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthRepository _authRepository;
        private readonly IMemoryCache _cache;
        private JwtHelper _jwtHelper;
        private readonly ForgotPasswordService _forgotPasswordService;

        public AuthController(IConfiguration configuration, AuthRepository authRepository, IMemoryCache memoryCache, ForgotPasswordService forgotPasswordService)
        {
            _configuration = configuration;
            _authRepository = authRepository;
            _cache = memoryCache;
            _forgotPasswordService = forgotPasswordService;
        }

        [HttpPost("login")]
        public ActionResult login([FromBody] Login login)
        {
            User user = null;
            var isAuthenticated = _authRepository.Authentication(login.Email, login.Password, out user);
            if (isAuthenticated)
            {
                _jwtHelper = new JwtHelper(_configuration);

                var token = _jwtHelper.GenerateJwtToken(user);
                return Ok(new
                {
                    token = token,
                    user = new
                    {
                        user.UserId,
                        user.Username,
                        user.Email,
                        user.Role
                    }
                });
            }
            else
            {
                return Unauthorized($"Invalid credentials {_authRepository.GetError()}");
            }
        }

        [HttpPost("register")]
        public ActionResult register([FromBody] User user)
        {
            var requesterRole = User.FindFirst("Role")?.Value;
            if (ValidationHelper.validateUserData(user))
            {
                if ((user.Role == "Cleaner" || user.Role == "Admin") && requesterRole != "Admin")
                {
                    return BadRequest("You hasn't permission to create account with admin or cleaner role");
                }
                else
                {
                    var isRegistered = _authRepository.Registration(user);
                    if (isRegistered)
                    {
                        return Ok("User registered successfully");
                    }
                    else
                    {
                        return BadRequest($"Registration failed: {_authRepository.GetError()}");
                    }
                }
            }
            else
            {
                return BadRequest($"Invalid user data{_authRepository.GetError()}");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPassword user)
        {
            bool emailExists = _authRepository.IsEmailExist(user.Email);

            if (!_authRepository.IsEmailExist(user.Email))
            {
                return BadRequest($"Email not found: {_authRepository.GetError()}");
            }

            string username = _authRepository.GetUsernameByEmail(user.Email);
            string verificationCode = _forgotPasswordService.GenerateVerificationCode();
            bool isEmailSent = await _forgotPasswordService.SendVerificationEmailAsync(username, user.Email, verificationCode);

            if (isEmailSent)
            {
                return Ok("Verification code sent to your email.");
            }

            return BadRequest($"Failed to send email: {_forgotPasswordService.GetError()}");
        }


        [HttpPost("verify")]
        public ActionResult VerifyOtp([FromBody] OtpVerifyRequest request)
        {
            bool isValid = _forgotPasswordService.ValidateOtpToken(request.Email, request.Code);

            if (isValid)
            {
                _cache.Set($"otp_verified_{request.Email}", true, TimeSpan.FromMinutes(10));
                return Ok("Verification code is valid.");
            }

            return BadRequest("Invalid or expired verification code.");
        }

        [HttpPost("reset-password")]
        public ActionResult ResetPassword([FromBody] ResetPassword resetPassword)
        {
            if (!_cache.TryGetValue($"otp_verified_{resetPassword.Email}", out bool verified) || !verified)
            {
                return BadRequest("OTP verification required before resetting password.");
            }

            if (!ValidationHelper.isPasswordValid(resetPassword.NewPassword))
            {
                return BadRequest("Invalid password format. Ensure the password meets the required criteria.");
            }

            bool isPasswordReset = _authRepository.ResetPassword(resetPassword);
            if (isPasswordReset)
            {
                _cache.Remove($"otp_verified_{resetPassword.Email}");
                _cache.Remove($"otp_code_{resetPassword.Email}");
                return Ok("Password reset successfully.");
            }

            return BadRequest($"Failed to reset password: {_authRepository.GetError()}");
        }

    }
}
