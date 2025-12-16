using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using KDemia.Models;
using KDemia.Repositories;
using KDemia.ViewModels;
using Microsoft.AspNetCore.Identity; // EKLENDİ
using System.Linq;
using System.Threading.Tasks; // Async için EKLENDİ

namespace KDemia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly GenericRepository<Course> _courseRepo;
        private readonly GenericRepository<Category> _categoryRepo;

        // Repository yerine UserManager kullanıyoruz (Daha güvenli ve Identity uyumlu)
        private readonly UserManager<User> _userManager;

        public CourseController(
            GenericRepository<Course> courseRepo,
            GenericRepository<Category> categoryRepo,
            UserManager<User> userManager) // Değiştirildi
        {
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
            _userManager = userManager;
        }

        // 1. LİSTELEME
        public IActionResult Index()
        {
            // "User" ve "Category" tablolarını Join'liyoruz (Include)
            var courses = _courseRepo.GetAll("User", "Category");
            return View(courses);
        }

        [HttpGet]
        public IActionResult Index(string search, int? categoryId)
        {
            var courses = _courseRepo.GetAll("User", "Category").AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                courses = courses.Where(x => x.Title.ToLower().Contains(search.ToLower()) ||
                                             (x.ShortDescription != null && x.ShortDescription.ToLower().Contains(search.ToLower())));
            }

            if (categoryId != null && categoryId > 0)
            {
                courses = courses.Where(x => x.CategoryId == categoryId);
            }

            ViewBag.Categories = _categoryRepo.GetAll()
                .Where(x => x.IsActive == true) // Eğer IsActive property'si yoksa burayı sil
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
                CategoryList = _categoryRepo.GetAll()
                    //.Where(x => x.IsActive == true) // Modelinde IsActive yoksa hata verir, varsa kalsın
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    })
            };

            return View(model);
        }

        // 3. CREATE POST (ASYNC OLDU)
        [HttpPost]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            try
            {
                // Validation kontrolü (Course nesnesi dolu mu?)
                // Not: model.Course null gelemez ama içindeki propertyler boş gelebilir.
                // ModelState.IsValid kontrolü yapmak daha iyidir ama senin yapına sadık kaldım.

                if (model.Course != null)
                {
                    // Identity'den o anki giriş yapmış kullanıcıyı buluyoruz
                    var user = await _userManager.GetUserAsync(User);

                    if (user != null)
                    {
                        // ID artık String olduğu için bu atama hatasız çalışır
                        model.Course.UserId = user.Id;
                    }

                    // Entity Framework ilişkileri tekrar eklemeye çalışmasın diye null yapıyoruz
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

            // Hata olursa Dropdown'ı tekrar doldur
            model.CategoryList = _categoryRepo.GetAll()
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

            CourseViewModel model = new CourseViewModel
            {
                Course = course,
                CategoryList = _categoryRepo.GetAll()
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

                // Sadece değişmesi gereken alanları güncelliyoruz
                // UserId'yi GÜNCELLEMİYORUZ (Kursun sahibi değişmemeli)
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

                model.CategoryList = _categoryRepo.GetAll()
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