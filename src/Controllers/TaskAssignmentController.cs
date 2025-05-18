using api_cleany_app.src.Models;
using api_cleany_app.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_cleany_app.src.Controllers
{
    [Route("api/task/assignment")]
    [Authorize]
    [ApiController]
    public class TaskAssignmentController : ControllerBase
    {
        private TaskAssignmentService _service;
        private UserService _userService;

        public TaskAssignmentController(TaskAssignmentService service, UserService userService)
        {
            _service = service;
            _userService = userService;
        }

        [HttpGet("report")]
        public ActionResult<List<Models.TaskAssignment>> GetAssignmentReportTask()
        {
            var reportTask = _service.getTaskAssignmentReport();
            if (reportTask != null)
            {
                return base.Ok(new ApiResponse<List<Models.TaskAssignment>>
                {
                    Success = true,
                    Message = "Fetch report task assignment successfull",
                    Data = reportTask,
                    Error = null
                });
            }
            else
            {
                return base.BadRequest(new ApiResponse<List<Models.TaskAssignment>>
                {
                    Success = false,
                    Message = "Failed fetch report task assignment",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpGet("routine")]
        public ActionResult<List<Models.TaskAssignment>> GetAssignmentRoutine()
        {
            var routineTask = _service.getTaskAssignmnetRoutine();
            if (routineTask != null)
            {
                return base.Ok(new ApiResponse<List<Models.TaskAssignment>>
                {
                    Success = true,
                    Message = "Fetch routine task assignment successfull",
                    Data = routineTask,
                    Error = null
                });
            }
            else
            {
                return base.BadRequest(new ApiResponse<List<Models.TaskAssignment>>
                {
                    Success = false,
                    Message = "Failed fetch routine task assignment",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpGet("report/user/{userId}")]
        public ActionResult<List<TaskAssignment>> GetTaskAssignmentReportByUserId(int userId)
        {
            var reportTask = _service.getTaskAssignmentReportByUserId(userId);
            if (reportTask != null)
            {
                return base.Ok(new ApiResponse<List<TaskAssignment>>
                {
                    Success = true,
                    Message = $"Fetch report task assignment by user ID {userId} successfull",
                    Data = reportTask,
                    Error = null
                });
            }
            else
            {
                return base.BadRequest(new ApiResponse<Models.TaskAssignment>
                {
                    Success = false,
                    Message = $"Failed fetch report task by user ID {userId} assignment",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpPut("status_update/{assignmentId}")]
        public ActionResult updateStatusTask([FromBody] string status, int assignmentId)
        {
            var username = User.Identity?.Name;
            int userId = _userService.getUserByUsername(username);
            bool updatedStatus = _service.updateStatusAssignmentTask(status, assignmentId, userId);

            if (updatedStatus)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Update status task successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed update status task",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }
    }
}