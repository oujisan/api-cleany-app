using Microsoft.AspNetCore.Mvc;
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
        private readonly AuthService _authService;
        private readonly IMemoryCache _cache;
        private JwtHelper _jwtHelper;

        public AuthController(IConfiguration configuration, AuthService authService, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _authService = authService;
            _cache = memoryCache;
        }

        [HttpPost("login")]
        public ActionResult login([FromBody] Login login)
        {
            User user = null;
            var isAuthenticated = _authService.Authentication(login.Email, login.Password, out user);
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
                return Unauthorized($"Invalid credentials {_authService.GetError()}");
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
                    var isRegistered = _authService.Registration(user);
                    if (isRegistered)
                    {
                        return Ok("User registered successfully");
                    }
                    else
                    {
                        return BadRequest($"Registration failed: {_authService.GetError()}");
                    }
                }
            }
            else
            {
                return BadRequest($"Invalid user data{_authService.GetError()}");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPassword user)
        {
            bool emailExists = _authService.IsEmailExist(user.Email);

            if (!_authService.IsEmailExist(user.Email))
            {
                return BadRequest($"Email not found: {_authService.GetError()}");
            }

            string username = _authService.GetUsernameByEmail(user.Email);
            string verificationCode = _authService.GenerateVerificationCode();
            bool isEmailSent = await _authService.SendVerificationEmailAsync(username, user.Email, verificationCode);

            if (isEmailSent)
            {
                return Ok("Verification code sent to your email.");
            }

            return BadRequest($"Failed to send email: {_authService.GetError()}");
        }


        [HttpPost("verify")]
        public ActionResult VerifyOtp([FromBody] OtpVerifyRequest request)
        {
            bool isValid = _authService.ValidateOtpToken(request.Email, request.Code);

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

            bool isPasswordReset = _authService.ResetPassword(resetPassword);
            if (isPasswordReset)
            {
                _cache.Remove($"otp_verified_{resetPassword.Email}");
                _cache.Remove($"otp_code_{resetPassword.Email}");
                return Ok("Password reset successfully.");
            }

            return BadRequest($"Failed to reset password: {_authService.GetError()}");
        }

    }
}
