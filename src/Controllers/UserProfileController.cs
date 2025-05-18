using api_cleany_app.src.Models;
using api_cleany_app.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_cleany_app.src.Controllers
{
    [Route("api/userProfile")]
    [Authorize]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly UserProfileService _service;
        private readonly IConfiguration _configuration;

        public UserProfileController(UserProfileService userProfileService, IConfiguration configuration)
        {
            _service = userProfileService;
            _configuration = configuration;
        }

        [HttpGet("{userId}")]
        public ActionResult<UserProfile> GetUser(int userId)
        {
            var user = _service.getUserProfile(userId);

            if (user != null)
            {
                return Ok(new ApiResponse<UserProfile>
                {
                    Success = true,
                    Message = $"Get user profile successfull.",
                    Data = user,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<UserProfile>
                {
                    Success = false,
                    Message = $"Failed to fetch user data with id {userId}",
                    Data = null,
                    Error = _service.getError(),
                });
            }
        }

        [HttpPut("update/{userId}")]
        public ActionResult updateUser([FromBody] UserProfile user, int userId)
        {
            bool dataUser = _service.updateUserProfile(user, userId);
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
                    Error = _service.getError()
                });
            }
        }

        [HttpPut("softdelete/{userId}")]
        public ActionResult SoftDeleteUser(int userId)
        {
            bool dataUser = _service.softDeleteUser(userId);
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
                    Error = _service.getError()
                });
            }
        }
    }
}