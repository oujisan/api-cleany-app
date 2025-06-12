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
    }
}
