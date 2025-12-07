using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using KDemia.Models;
using KDemia.Repositories;
using KDemia.ViewModels;
using System.Linq;

namespace KDemia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly GenericRepository<Course> _courseRepo;
        private readonly GenericRepository<Category> _categoryRepo;
        private readonly GenericRepository<User> _userRepo;

        public CourseController(GenericRepository<Course> courseRepo, GenericRepository<Category> categoryRepo, GenericRepository<User> userRepo)
        {
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
            _userRepo = userRepo;
        }

        // 1. LİSTELEME
        public IActionResult Index()
        {
            var courses = _courseRepo.GetAll("User", "Category");
            return View(courses);
        }

        [HttpGet]
        public IActionResult Index(string search, int? categoryId)
        {
            // 1. Tüm kursları ilişkileriyle birlikte çek.
            var courses = _courseRepo.GetAll("User", "Category").AsQueryable();

            // 2. Arama Kelimesi Filtresi
            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(x => x.Title.ToLower().Contains(search.ToLower()) ||
                                             (x.ShortDescription != null && x.ShortDescription.ToLower().Contains(search.ToLower())));
            }

            // 3. Kategori Filtresi
            if (categoryId != null && categoryId > 0)
            {
                courses = courses.Where(x => x.CategoryId == categoryId);
            }

            // 4.Kategorileri Hazırla
            ViewBag.Categories = _categoryRepo.GetAll()
                .Where(x => x.IsActive == true) // Sadece aktif kategoriler filtrede görünsün
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = x.Id == categoryId
                }).ToList();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;

            return View(courses.ToList());
        }

        // 2. CREATE GET 
        [HttpGet]
        public IActionResult Create()
        {
            CourseViewModel model = new CourseViewModel
            {
                Course = new Course(),
                // DEĞİŞİKLİK BURADA: Sadece Aktif Kategorileri Listele
                CategoryList = _categoryRepo.GetAll()
                    .Where(x => x.IsActive == true)
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    })
            };

            return View(model);
        }

        // 3. CREATE POST
        [HttpPost]
        public IActionResult Create(CourseViewModel model)
        {
            try
            {
                if (model.Course != null)
                {
                    var userEmail = User.Identity.Name;
                    var user = _userRepo.Get(x => x.Email == userEmail);

                    if (user != null)
                    {
                        model.Course.UserId = user.Id;
                    }

                    model.Course.Category = null;
                    model.Course.User = null;

                    _courseRepo.Add(model.Course);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hata: " + ex.Message);
            }

            // Hata durumunda listeyi tekrar doldur (SADECE AKTİF OLANLAR)
            model.CategoryList = _categoryRepo.GetAll()
                .Where(x => x.IsActive == true)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });

            return View(model);
        }

        // 4. DELETE (AJAX)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var course = _courseRepo.Get(x => x.Id == id);
            if (course == null)
                return Json(new { success = false, message = "Eğitim bulunamadı." });

            _courseRepo.Delete(course);
            return Json(new { success = true });
        }

        // 5. EDIT GET
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var course = _courseRepo.Get(x => x.Id == id);
            if (course == null) return NotFound();

            // DEĞİŞİKLİK BURADA:
            // Düzenleme sayfasında da sadece aktif kategoriler gelmeli.
            // NOT: Eğer düzenlediğin kursun kategorisi şu an pasifse, dropdown'da seçili gelmeyebilir
            // ve kaydettiğinde yeni bir aktif kategori seçmek zorunda kalırsın (ki istediğin de bu).
            CourseViewModel model = new CourseViewModel
            {
                Course = course,
                CategoryList = _categoryRepo.GetAll()
                    .Where(x => x.IsActive == true) // Filtre
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                        Selected = x.Id == course.CategoryId
                    })
            };

            return View(model);
        }

        // 6. EDIT POST
        [HttpPost]
        public IActionResult Edit(CourseViewModel model)
        {
            try
            {
                var existingCourse = _courseRepo.Get(x => x.Id == model.Course.Id);

                if (existingCourse == null) return NotFound();

                existingCourse.Title = model.Course.Title;
                existingCourse.Price = model.Course.Price;
                existingCourse.ShortDescription = model.Course.ShortDescription;
                existingCourse.DetailContent = model.Course.DetailContent;
                existingCourse.IsPublished = model.Course.IsPublished;
                existingCourse.CategoryId = model.Course.CategoryId;
                existingCourse.VideoUrl = model.Course.VideoUrl;
                existingCourse.VideoSource = model.Course.VideoSource;

                _courseRepo.Update(existingCourse);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hata: " + ex.Message);

                // Hata durumunda listeyi tekrar doldur (SADECE AKTİF OLANLAR)
                model.CategoryList = _categoryRepo.GetAll()
                    .Where(x => x.IsActive == true)
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString(),
                        Selected = x.Id == model.Course.CategoryId
                    });

                return View(model);
            }
        }
    }
}