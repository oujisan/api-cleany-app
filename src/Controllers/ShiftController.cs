using Microsoft.AspNetCore.Mvc;
using api_cleany_app.src.Services;
using Microsoft.AspNetCore.Authorization;
using api_cleany_app.src.Models;

namespace api_cleany_app.src.Controllers
{
    [Route("/api/shift")]
    [Authorize(Roles = "admin")]
    [ApiController]

    public class ShiftController : ControllerBase
    {
        private readonly ShiftService _shiftService;
        private readonly IConfiguration _configuration;

        public ShiftController(ShiftService shiftService, IConfiguration configuration)
        {
            _shiftService = shiftService;
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult<List<Shift>> GetShift() {
            var shifts = _shiftService.getAllShift();
            if (shifts != null)
            {
                return Ok(new ApiResponse<List<Shift>>
                {
                    Success = true,
                    Message = "Shifts fetched successfully",
                    Data = shifts,
                    Error = null,
                });
            }
            else
            {
                return BadRequest(new ApiResponse<List<Shift>>
                {
                    Success = false,
                    Message = "Failed to fetch shifts.",
                    Data = null,
                    Error = _shiftService.GetError(),
                });
            }
        }

        [HttpGet("id/{shiftId}")]
        public ActionResult GetShiftById(int shiftId) {

            var shift = _shiftService.getShiftById(shiftId);
            if (shift != null)
            {
                return Ok(new ApiResponse<Shift>
                {
                    Success = true,
                    Message = "Shift fetched successfully",
                    Data = shift,
                    Error = null,
                });
            }
            else
            {
                return NotFound(new ApiResponse<Shift>
                {
                    Success = false,
                    Message = "Shift not found.",
                    Data = null,
                    Error = _shiftService.GetError(),
                });
            }
        }

        [HttpGet("name/{shiftName}")]
        public ActionResult<int?> GetShiftIdByName(string shiftName)
        {
            var shiftId = _shiftService.getShiftIdByName(shiftName);
            if (shiftId.HasValue)
            {
                return Ok(new ApiResponse<int?>
                {
                    Success = true,
                    Message = "Shift ID fetched successfully",
                    Data = shiftId,
                    Error = null,
                });
            }
            else
            {
                return NotFound(new ApiResponse<int?>
                {
                    Success = false,
                    Message = "Shift not found.",
                    Data = null,
                    Error = _shiftService.GetError(),
                });
            }
        }

        [HttpPost("add")]
        public ActionResult AddShift([FromBody] ShiftDto shift)
        {
            var success = _shiftService.addShift(shift);
            if (success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Shift created successfully",
                    Data = shift, 
                    Error = null
                });
            }
            else
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create shift.",
                    Data = null,
                    Error = _shiftService.GetError()
                });
            }
        }


        [HttpPut("update/{shiftId}")]
        public ActionResult UpdateShift(int shiftId, [FromBody] ShiftDto shift)
        {
           bool _isUpdated = _shiftService.updateShift(shift);
            if (_isUpdated)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Shift updated successfully",
                    Data = shift,
                    Error = null
                });
            }
            else
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to update shift.",
                    Data = null,
                    Error = _shiftService.GetError(),
                });
            }
        }


        [HttpDelete("{shiftId}")]
        public ActionResult DeleteShift(int shiftId)
        {
            var success = _shiftService.deleteShift(shiftId);

            if (success)
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Shift deleted successfully",
                    Data = null,
                    Error = null,
                });
            }
            else
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to delete shift.",
                    Data = null,
                    Error = _shiftService.GetError(),
                });
            }
        }

    }

}