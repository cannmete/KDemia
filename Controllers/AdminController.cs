using Microsoft.AspNetCore.Mvc;

namespace KDemia.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
