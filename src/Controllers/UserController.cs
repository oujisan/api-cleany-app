using Microsoft.AspNetCore.Mvc;
using api_cleany_app.src.Services;
using Microsoft.AspNetCore.Authorization;
using api_cleany_app.src.Models;

namespace api_cleany_app.src.Controllers
{
    [Route("api/user")]
    [Authorize(Roles = "admin")]
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
                return Ok(new ApiResponse<List<User>>
                {
                    Success = true,
                    Message = "Users fetched successfully",
                    Data = users,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<User>>
                {
                    Success = false,
                    Message = $"Failed fetch users.",
                    Data = null,
                    Error = _service.GetError(),
                });
            }
        }

        [HttpGet("{userId}")]
        public ActionResult<User> GetUser(int userId)
        {
            var user = _service.getUserById(userId);

            if (user != null)
            {
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Message = $"Get user with ID {user.UserId} successfull.",
                    Data = user,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<User>
                {
                    Success = false,
                    Message = $"Failed to fetch user data with id {userId}",
                    Data = null,
                    Error = _service.GetError(),
                });
            }
        }

        [HttpPost("create")]
        public ActionResult addUser([FromBody]User user)
        {
            bool dataUser = _service.addUser(user);

            if (dataUser)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User added successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Failed added user",
                    Data = null,
                    Error = _service.GetError()
                });
            }
        }

        [HttpPut("update/{userId}")]
        public ActionResult updateUser([FromBody] User user, int userId)
        {
            bool dataUser = _service.updateUser(user, userId);
            if (dataUser)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"User with ID {userId} Updated Successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed Updated User with ID {userId}",
                    Data = null,
                    Error = _service.GetError()
                });
            }
        }

        [HttpDelete("delete/{userId}")]
        public ActionResult DeleteUser(int userId)
        {
            bool dataUser = _service.deleteUser(userId);
            if (dataUser)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"User with ID {userId} Deleted Successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed Delete User with ID {userId}",
                    Data = null,
                    Error = _service.GetError()
                });
            }
        }
    }
}
