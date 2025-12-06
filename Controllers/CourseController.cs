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

            // 2. Arama Kelimesi Filtresi (Başlık veya Açıklamada arar)
            if (!string.IsNullOrEmpty(search))
            {
                // Büyük/küçük harf duyarsız arama
                courses = courses.Where(x => x.Title.ToLower().Contains(search.ToLower()) ||
                                             (x.ShortDescription != null && x.ShortDescription.ToLower().Contains(search.ToLower())));
            }

            // 3. Kategori Filtresi
            if (categoryId != null && categoryId > 0)
            {
                courses = courses.Where(x => x.CategoryId == categoryId);
            }

            // 4. Dropdown için Kategorileri Hazırla
            // (Kullanıcının seçtiği kategori sayfada "Seçili" kalsın diye logic ekledik)
            ViewBag.Categories = _categoryRepo.GetAll().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = x.Id == categoryId
            }).ToList();

            // Filtreleri View'a geri gönder ki inputların içinde yazılı kalsın
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
                CategoryList = _categoryRepo.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            return View(model);
        }

        // 3. CREATE POST (SADE VE GÜVENLİ)
        [HttpPost]
        public IActionResult Create(CourseViewModel model)
        {
            try
            {
                if (model.Course != null)
                {
                    // a. Kullanıcıyı Bul ve Ata.
                    var userEmail = User.Identity.Name;
                    var user = _userRepo.Get(x => x.Email == userEmail);

                    if (user != null)
                    {
                        model.Course.UserId = user.Id;
                    }

                    // b. İlişki hatalarını önlemek için nesneleri boşalt.
                    model.Course.Category = null;
                    model.Course.User = null;

                    // c. Kaydet
                    _courseRepo.Add(model.Course);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hata: " + ex.Message);
            }

            // Hata varsa listeyi tekrar doldur.
            model.CategoryList = _categoryRepo.GetAll().Select(x => new SelectListItem
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

            CourseViewModel model = new CourseViewModel
            {
                Course = course,
                CategoryList = _categoryRepo.GetAll().Select(x => new SelectListItem
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
                // 1. Orijinal kaydı çek
                var existingCourse = _courseRepo.Get(x => x.Id == model.Course.Id);

                if (existingCourse == null) return NotFound();

                // 2. Alanları Manuel Güncelle (Resim YOK)
                existingCourse.Title = model.Course.Title;
                existingCourse.Price = model.Course.Price;
                existingCourse.ShortDescription = model.Course.ShortDescription;
                existingCourse.DetailContent = model.Course.DetailContent;
                existingCourse.IsPublished = model.Course.IsPublished;
                existingCourse.CategoryId = model.Course.CategoryId;

                // Video Linkleri
                existingCourse.VideoUrl = model.Course.VideoUrl;
                existingCourse.VideoSource = model.Course.VideoSource;

                // 3. Veritabanına Yansıt.
                _courseRepo.Update(existingCourse);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Hata: " + ex.Message);

                // Hata durumunda listeyi tekrar doldur.
                model.CategoryList = _categoryRepo.GetAll().Select(x => new SelectListItem
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