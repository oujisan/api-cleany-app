using Microsoft.AspNetCore.Mvc;

namespace api_cleany_app.src.Controllers
{
    public class ShiftController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
