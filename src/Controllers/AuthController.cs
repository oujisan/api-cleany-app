using Microsoft.AspNetCore.Mvc;
using api_cleany_app.src.Repositories;
using api_cleany_app.src.Helpers;
using api_cleany_app.src.Models;

namespace api_cleany_app.src.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly AuthRepository _authRepository;
        private JwtHelper _jwtHelper;

        public AuthController(IConfiguration configuration, AuthRepository authRepository)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("LocalConnection");
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
    }
}