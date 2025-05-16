using api_cleany_app.src.Services;
using api_cleany_app.src.Models;
using Microsoft.AspNetCore.Mvc;

namespace api_cleany_app.src.Controllers
{
    [Route("/api/task")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _service;

        public TaskController(TaskService service)
        {
            _service = service;
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
                return BadRequest(new ApiResponse<List<TaskDto>>
                {
                    Success = false,
                    Message = "Fetch tasks failed",
                    Data = null,
                    Error = _service.getError()
                    
                });
            }
        }

        [HttpGet("routine/assignment")]
        public ActionResult<List<TaskAssignment>> GetAssignmentRoutineTask()
        {
            var routineTask = _service.getTaskAssignmnetRoutine();
            if (routineTask != null)
            {
                return Ok(new ApiResponse<List<TaskAssignment>>
                {
                    Success = true,
                    Message = "Fetch routine task assignment successfull",
                    Data = routineTask,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<TaskAssignment>>
                {
                    Success = false,
                    Message = "Failed fetch routine task assignment",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }
        [HttpGet("report/assignment")]
        public ActionResult<List<TaskAssignment>> GetAssignmentReportTask()
        {
            var reportTask = _service.getTaskAssignmentReport();
            if (reportTask != null)
            {
                return Ok(new ApiResponse<List<TaskAssignment>>
                {
                    Success = true,
                    Message = "Fetch report task assignment successfull",
                    Data = reportTask,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<TaskAssignment>>
                {
                    Success = false,
                    Message = "Failed fetch report task assignment",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }
    }
}