using api_cleany_app.src.Services;
using api_cleany_app.src.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace api_cleany_app.src.Controllers
{
    [Route("/api/task")]
    //[Authorize]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _service;
        private readonly UserService _userService;
        private readonly TaskAssignmentService _assignmentService;

        public TaskController(TaskService service, UserService userService, TaskAssignmentService assignmentService)
        {
            _service = service;
            _userService = userService;
            _assignmentService = assignmentService;
        }

        [HttpGet]
        public ActionResult<List<Tasks>> GetAllTask()
        {
            var tasks = _service.getTasks();
            if (tasks != null)
            {
                return Ok(new ApiResponse<List<Tasks>>
                {
                    Success = true,
                    Message = "Fetch tasks successfull",
                    Data = tasks,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Tasks>>
                {
                    Success = false,
                    Message = "Fetch tasks failed",
                    Data = null,
                    Error = _service.getError()
                    
                });
            }
        }

        [HttpGet("{taskId}")]
        public ActionResult<Tasks> GetTask(int taskId)
        {
            var task = _service.getTaskById(taskId);
            if (task != null)
            {
                return Ok(new ApiResponse<Tasks>
                {
                    Success = true,
                    Message = "Fetch tasks successfull",
                    Data = task,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<Tasks>
                {
                    Success = false,
                    Message = "Fetch tasks failed",
                    Data = null,
                    Error = _service.getError()

                });
            }
        }

        [HttpGet("routine")]
        public ActionResult<List<Tasks>> GetRoutineTask()
        {
            var tasks = _service.getTasksByType("routine");
            if (tasks != null)
            {
                return Ok(new ApiResponse<List<Tasks>>
                {
                    Success = true,
                    Message = "Fetch routine tasks successfull",
                    Data = tasks,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Tasks>>
                {
                    Success = false,
                    Message = "Fetch routine tasks failed",
                    Data = null,
                    Error = _service.getError()

                });
            }
        }

        [HttpGet("report")]
        public ActionResult<List<Tasks>> GetReportTask()
        {
            var tasks = _service.getTasksByType("report");
            if (tasks != null)
            {
                return Ok(new ApiResponse<List<Tasks>>
                {
                    Success = true,
                    Message = "Fetch report tasks successfull",
                    Data = tasks,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Tasks>>
                {
                    Success = false,
                    Message = "Fetch report tasks failed",
                    Data = null,
                    Error = _service.getError()

                });
            }
        }

        [HttpPost("report/add")]
        public ActionResult AddReportTask([FromBody] ReportTaskDto task)
        {
            var username = User.Identity?.Name;
            int userId = _userService.getUserByUsername(username);

            int taskId = _service.addReportTask(task, userId);
            if (taskId != 0)
            {
                bool result_assignment = _assignmentService.AddAssignmentTask(taskId,2);
                if (result_assignment)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Add new report task and assignment successfull",
                        Data = null,
                        Error = null,
                    });
                }
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Add assignment failed",
                    Data= null,
                    Error = _service.getError()
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Add task report failed",
                    Data = null,
                    Error = _service.getError()
                });
            }

        }
        [HttpPost("routine/add")]
        public ActionResult AddRoutineTask([FromBody] RoutineTaskDto task)
        {
            var username = User.Identity?.Name;
            int userId = _userService.getUserByUsername(username);

            int taskId = _service.addRoutineTask(task, userId, out DateOnly startDate);
            if (taskId != 0)
            {
                if (startDate == DateOnly.FromDateTime(DateTime.Today))
                {
                    bool result = _assignmentService.AddAssignmentTask(taskId, 1);
                    if (result)
                    {
                        return Ok(new ApiResponse<object>
                        {
                            Success = true,
                            Message = "Add routine task and assignment successfull",
                            Data = null,
                            Error = null,
                        });
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Add routine task assignment failed",
                            Data = null,
                            Error = _service.getError()
                        });
                    }
                }
                else
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Add routine task successfull",
                        Data = null,
                        Error = null,
                    });
                }
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Add routine task failed",
                    Data = null,
                    Error = _service.getError()
                });
            }

        }

        [HttpPut("routine/update/{taskId}")]
        public ActionResult updateRoutineTask([FromBody] RoutineTaskDto task, int taskId)
        {
            bool isUpdated = _service.updateRoutineTask(task, taskId);
            if (isUpdated)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Task with ID {taskId} Updated Successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed Updated Task with ID {taskId}",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpPut("report/update/{taskId}")]
        public ActionResult updateReportTask([FromBody] ReportTaskDto task, int taskId)
        {
            bool isUpdated = _service.updateReportTask(task, taskId);
            if (isUpdated)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Task with ID {taskId} Updated Successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed Updated Task with ID {taskId}",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }

        [HttpDelete("delete/{taskId}")]
        public ActionResult DeleteTask(int taskId)
        {
            bool isDeleted = _service.deleteTask(taskId);
            if (isDeleted)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Task with ID {taskId} Deleted Successfull",
                    Data = null,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Failed Delete User with ID {taskId}",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }
    }
}