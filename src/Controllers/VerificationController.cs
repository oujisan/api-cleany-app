using api_cleany_app.src.Services;
using api_cleany_app.src.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace api_cleany_app.src.Controllers
{
    [Route("api/verification")]
    //[Authorize]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private VerificationService _service;
        private UserService _userService;
        public VerificationController(VerificationService verificationService , UserService userservice)
        {
            _service = verificationService;
            _userService = userservice;
        }
        [HttpGet]
        public ActionResult<List<Verification>> GetAllVerifications()
        {
            var verifications = _service.getAllVerifications();
            if (verifications != null)
            {
                return Ok(new ApiResponse<List<Verification>>
                {
                    Success = true,
                    Message = "Fetch Verifications Successfull",
                    Data = verifications,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Verification>>
                {
                    Success = false,
                    Message = "Failed fetch verification data",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpGet("{verificationId}")]
        public ActionResult<Verification> GetVerificationById(int verificationId)
        {
            var verification = _service.getAllVerificationById(verificationId);
            if (verification != null)
            {
                return Ok(new ApiResponse<Verification>
                {
                    Success = true,
                    Message = $"Fetch Verification with ID {verificationId} Successfull",
                    Data = verification,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<Verification>
                {
                    Success = false,
                    Message = $"Failed fetch verification with ID {verificationId}",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpGet("task/{taskId}")]
        public ActionResult<List<Verification>> getVerificationByTask(int taskId)
        {
            var verification = _service.getVerificationByTask(taskId);
            if (verification != null)
            {
                return Ok(new ApiResponse<List<Verification>>
                {
                    Success = true,
                    Message = $"Fetch verification data by task successfull",
                    Data = verification,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Verification>>
                {
                    Success = false,
                    Message = $"Failed fetch verification data by task with ID {taskId}",
                    Data = null,
                    Error = _service.getError(),
                });
            }
        }

        [HttpGet("assignment/{assignmentId}")]
        public ActionResult<List<Verification>> GetAllVerificationByAssignment(int assignmentId)
        {
            var verifications = _service.getAllVerificationByAssignmentId(assignmentId);
            if (verifications != null)
            {
                return Ok(new ApiResponse<List<Verification>>
                {
                    Success = true,
                    Message = $"Fetch verifications assignment with ID {assignmentId} Successfull",
                    Data = verifications,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Verification>>
                {
                    Success = false,
                    Message = $"Failed fetch verification assignment with ID {assignmentId}",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpPut("assigment/update/{assignmentId}")]
        public ActionResult UpdateVerification(int assignmentId, [FromBody] VerificationDto verificationData)
        {
            var username = User.Identity?.Name;
            int userId = _userService.getUserByUsername(username);
            var verification = _service.updateVerification(verificationData, assignmentId, userId);
            if (verification != null)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Update Verification with ID {assignmentId} Successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed update verification with ID {assignmentId}",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpDelete("delete/{verificationId}")]
        public ActionResult DeleteVerification(int verificationId)
        {
            var verifications = _service.deleteVerification(verificationId);
            if (verifications)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Fetch verifications assignment with ID {verificationId} Successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed fetch verification assignment with ID {verificationId}",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }
    }
}
