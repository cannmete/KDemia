using Microsoft.AspNetCore.Mvc;
using KDemia.Models;

namespace KDemia.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            // Simülasyon için statik kurs listesi
            var courses = new List<Course>
            {
                new Course { Id = 1, Name = "ASP.NET Core", Description = "Web API + MVC" },
                new Course { Id = 2, Name = "C# Programming", Description = "Temel ve ileri C#" },
            };
            return View(courses);
        }
    }
}
