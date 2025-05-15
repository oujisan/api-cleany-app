using Microsoft.AspNetCore.Mvc;
using api_cleany_app.src.Services;
using Microsoft.AspNetCore.Authorization;
using api_cleany_app.src.Models;

namespace api_cleany_app.src.Controllers
{
    [Route("api/user")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class UserController: ControllerBase
    {
        private readonly UserService _service;
        private readonly IConfiguration _configuration;

        public UserController(UserService userService, IConfiguration configuration)
        {
            _service = userService;
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<List<User>> GetAll()
        {
            var users = _service.getAllUser();

            if (users != null)
            {
                return Ok(users);
            }
            else
            {
                return BadRequest($"Failed fetch all user: {_service.GetError()}");
            }
        }

        [HttpGet("{userId}")]
        public ActionResult<User> GetUser(int userId)
        {
            var user = _service.getUserById(userId);

            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest($"Failed to fetch user data with id {userId}: {_service.GetError()}");
            }
        }

        [HttpPut("update/{userId}")]
        public ActionResult updateUser([FromBody] User user, int userId)
        {
            bool dataUser = _service.updateUser(user, userId);
            if (dataUser)
            {
                return Ok("User Updated Successfull");
            }
            else
            {
                return BadRequest($"Failed Add User: {_service.GetError()}");
            }
        }

        [HttpDelete("delete/{userId}")]
        public ActionResult DeleteUser(int userId)
        {
            bool dataUser = _service.deleteUser(userId);
            if (dataUser)
            {
                return Ok("User Deleted Successfull");
            }
            else
            {
                return BadRequest($"Failed Delete User: {_service.GetError()}");
            }
        }
    }
    }
}
