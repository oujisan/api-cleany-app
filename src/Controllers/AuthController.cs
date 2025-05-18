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
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Login Successfull",
                    Data = new
                    {
                        token = token,
                        user = new
                        {
                            user.UserId,
                            user.Username,
                            user.Role
                        }
                    },
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Login Failed",
                    Data = null,
                    Error = _authService.GetError()
                });
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
                        return Ok(new ApiResponse<object>
                        {
                            Success = true,
                            Message = "User registered successfully",
                            Data = null,
                            Error = null,
                        });
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Registration failed",
                            Data = null,
                            Error = _authService.GetError()
                        });
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
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Email not found.",
                    Data = null,
                    Error = _authService.GetError()
                });
            }

            string username = _authService.GetUsernameByEmail(user.Email);
            string verificationCode = _authService.GenerateVerificationCode();
            bool isEmailSent = await _authService.SendVerificationEmailAsync(username, user.Email, verificationCode);

            if (isEmailSent)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Verification code sent to your email.",
                    Data = null,
                    Error = null
                });
            }

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Failed send verification code to your email",
                Data = null,
                Error = _authService.GetError()
            });
        }


        [HttpPost("verify")]
        public ActionResult VerifyOtp([FromBody] OtpVerifyRequest request)
        {
            bool isValid = _authService.ValidateOtpToken(request.Email, request.Code);

            if (isValid)
            {
                _cache.Set($"otp_verified_{request.Email}", true, TimeSpan.FromMinutes(10));
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Verification code is valid.",
                    Data= null,
                    Error = null
                });
            }

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid or expired verification code.",
                Data = null,
                Error = _authService.GetError()
            });
        }

        [HttpPost("reset-password")]
        public ActionResult ResetPassword([FromBody] ResetPassword resetPassword)
        {
            if (!_cache.TryGetValue($"otp_verified_{resetPassword.Email}", out bool verified) || !verified)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "OTP verification required before resetting password.",
                    Data = null,
                    Error = _authService.GetError()
                });
            }

            if (!ValidationHelper.isPasswordValid(resetPassword.NewPassword))
            {
                return BadRequest(new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "Invalid password format. Ensure the password meets the required criteria.", 
                    Data = null, 
                    Error = _authService.GetError() 
                });
            }

            bool isPasswordReset = _authService.ResetPassword(resetPassword);
            if (isPasswordReset)
            {
                _cache.Remove($"otp_verified_{resetPassword.Email}");
                _cache.Remove($"otp_code_{resetPassword.Email}");
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password reset successfully.",
                    Data = null,
                    Error = null
                });
            }

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Failed to reset password",
                Data = null,
                Error = _authService.GetError()
            });
        }

    }
}
