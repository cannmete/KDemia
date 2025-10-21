using Microsoft.AspNetCore.Mvc;

namespace KDemia.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
