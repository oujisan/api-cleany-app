using Microsoft.AspNetCore.Mvc;
using api_cleany_app.src.Repositories;
using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;

namespace api_cleany_app.src.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthRepository _authRepository;
        private JwtHelper _jwtHelper;

        public AuthController(IConfiguration configuration, AuthRepository authRepository)
        {
            _configuration = configuration;
            _authRepository = authRepository;
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
    }
}