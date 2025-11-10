using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;

namespace KDemia.Controllers
{
    public class CoursesController : Controller
    {

        private readonly CourseRepository _courseRepository;

        public CoursesController(CourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        private static List<Course> _courses = new List<Course>
        {
            new Course { Id = 1, courseName = "ASP.NET Core", Description = "Web API + MVC" },
            new Course { Id = 2, courseName = "C# Programming", Description = "Temel ve ileri C#" },
        };

        public IActionResult Index()
        {
            return View(_courses);
        }


        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]

        public IActionResult Add(Course model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _courseRepository.Add(model);
            return RedirectToAction("Index");
        }
    }
}
