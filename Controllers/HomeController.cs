using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectListItem için
using KDemia.Models;
using KDemia.Repositories;
using System.Diagnostics;
using System.Linq;

namespace KDemia.Controllers
{
    // Bu Controller Herkese Açýktýr
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly GenericRepository<Course> _courseRepo;
        private readonly GenericRepository<Category> _categoryRepo;

        public HomeController(GenericRepository<Course> courseRepo, GenericRepository<Category> categoryRepo)
        {
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
        }

        // 1. VÝTRÝN SAYFASI (Katalog)
        public IActionResult Index(string search, int? categoryId)
        {
            // Sadece YAYINDA (IsPublished == true) olanlarý getir
            var courses = _courseRepo.GetAll("Category", "User")
                                     .Where(x => x.IsPublished == true)
                                     .AsQueryable();

            // Arama Filtresi
            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(x => x.Title.ToLower().Contains(search.ToLower()) ||
                                             x.ShortDescription.Contains(search));
            }

            // Kategori Filtresi
            if (categoryId != null && categoryId > 0)
            {
                courses = courses.Where(x => x.CategoryId == categoryId);
            }

            // Kategorileri Dropdown için hazýrla (Sidebar veya üst menü için)
            ViewBag.Categories = _categoryRepo.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = x.Id == categoryId
            }).ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(courses.ToList());
        }

        // 2. KURS DETAY SAYFASI
        [Authorize]
        public IActionResult Details(int id)
        {
            var course = _courseRepo.Get(x => x.Id == id);

            // Kurs yoksa veya yayýnda deðilse gösterme.
            if (course == null || !course.IsPublished)
            {
                return NotFound();
            }

            // Kategori bilgisini de yükleyelim (Repo Get metodun include destekliyorsa oradan, yoksa manuel)
            // Eðer Get metodun include parametresi almýyorsa, lazy loading veya ayrý çaðrý gerekebilir.
            // Þimdilik basitçe kategoriyi de çekelim:
            course.Category = _categoryRepo.Get(x => x.Id == course.CategoryId);

            return View(course);
        }
    }
}