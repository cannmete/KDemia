using Microsoft.AspNetCore.Mvc;

namespace KDemia.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
