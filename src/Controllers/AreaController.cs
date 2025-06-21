using Microsoft.AspNetCore.Mvc;
using api_cleany_app.src.Services;
using Microsoft.AspNetCore.Authorization;
using api_cleany_app.src.Models;

namespace api_cleany_app.src.Controllers
{
    [Route("api/area")]
    [Authorize]
    [ApiController]
    public class AreaController : ControllerBase
    {
        private readonly AreaService _service;
        private readonly IConfiguration _configuration;

        public AreaController(AreaService areaService)
        {
            _service = areaService;
        }

        [HttpGet]
        public ActionResult<List<Area>> GetAllArea()
        {
            var users = _service.getAllArea();

            if (users != null)
            {
                return Ok(new ApiResponse<List<Area>>
                {
                    Success = true,
                    Message = "Users fetched successfully",
                    Data = users,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Area>>
                {
                    Success = false,
                    Message = $"Failed fetch users.",
                    Data = null,
                    Error = _service.getError(),
                });
            }
        }

        [HttpGet("id/{areaId}")]
        public ActionResult GetAreaById(int areaId)
        {
            var area = _service.getAreaById(areaId);
            if (area != null)
            {
                return Ok(new ApiResponse<Area>
                {
                    Success = true,
                    Message = "Area fetched successfully",
                    Data = area,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<Area>
                {
                    Success = false,
                    Message = $"Failed to fetch area.",
                    Data = null,
                    Error = _service.getError(),
                });
            }
        }

        [HttpPost("add")]
        public ActionResult addArea([FromBody] AreaDto area)
        {
            if (area == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid area data.",
                    Data = null,
                    Error = "Area data cannot be null.",
                });
            }
            var result = _service.addArea(area);
            if (result)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Area added successfully",
                    Data = area,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to add area.",
                    Data = null,
                    Error = _service.getError(),
                });
            }
        }
        [HttpPut("update/{areaId}")]
        public ActionResult updateArea(int areaId, [FromBody] AreaDto area)
        {
            bool _isUpdated = _service.updateArea(area);
            if (_isUpdated)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Area updated successfully",
                    Data = area,
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update area.",
                    Data = null,
                    Error = _service.getError()
                });
            }
        }
        [HttpDelete("delete/{areaId}")]
        public ActionResult deleteArea(int areaId)
        {
            var success = _service.deleteArea(areaId);
            if (success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Area deleted successfully",
                    Data = null,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to delete area.",
                    Data = null,
                    Error = _service.getError(),
                });
            }
        }
    }
}
