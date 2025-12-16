using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using KDemia.Models;
using Microsoft.AspNetCore.SignalR;
using KDemia.Hubs;
using KDemia.Repositories;
using System.Security.Claims;
using System.Linq;

namespace KDemia.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly GenericRepository<Course> _courseRepo;
        private readonly GenericRepository<Category> _categoryRepo;
        private readonly GenericRepository<CourseReview> _reviewRepo;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly GenericRepository<User> _userRepo;

        public HomeController(
            GenericRepository<Course> courseRepo,
            GenericRepository<Category> categoryRepo,
            GenericRepository<User> userRepo,
            GenericRepository<CourseReview> reviewRepo, IHubContext<NotificationHub> hubContext)

        {
            _courseRepo = courseRepo;
            _categoryRepo = categoryRepo;
            _userRepo = userRepo;
            _reviewRepo = reviewRepo;
            _hubContext = hubContext;
        }

        // 1. Home/Index
        public IActionResult Index(string search, int? categoryId)
        {
            // "User" include ederek eðitmen adýný ana sayfada gösterebilirsin
            var courses = _courseRepo.GetAll("Category", "User", "Reviews")
                                     .Where(x => x.IsPublished == true)
                                     .AsQueryable();

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

        // 2. KURS DETAY SAYFASI
        public IActionResult Details(int id)
        {
            // 1. Kursu bul (Sadece kurs bilgileri gelir)
            var course = _courseRepo.Get(x => x.Id == id);

            if (course == null || !course.IsPublished)
            {
                return NotFound();
            }

            // 2. Kategori bilgisini doldur
            course.Category = _categoryRepo.Get(x => x.Id == course.CategoryId);

            // 3. Eðitmen bilgisini doldur (UserId artýk String olduðu için sorun yok)
            course.User = _userRepo.Get(x => x.Id == course.UserId);

            // 4. --- YENÝ EKLENEN KISIM: Yorumlarý Getir ---
            // Yorumlarý çekerken, yorumu yapan kiþiyi ("User") da getirmemiz lazým.
            // _reviewRepo.GetAll("User") diyerek User tablosunu dahil ediyoruz.
            // Ardýndan Where ile sadece BU kursa ait olanlarý süzüyoruz.

            course.Reviews = _reviewRepo.GetAll("User")
                                        .Where(x => x.CourseId == id)
                                        .OrderByDescending(x => x.CreatedDate) // En yeni yorum en üstte görünsün
                                        .ToList();

            return View(course);

        }
        // 3. YORUM KAYDETME (POST)
        [HttpPost]
        [Authorize] // Sadece giriþ yapmýþ kullanýcýlar yorum yapabilir
        public async Task<IActionResult> AddReview(int courseId, int rating, string comment)
        {
            // O anki kullanýcýnýn ID'sini Identity'den buluyoruz
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) return RedirectToAction("Login", "Auth");

            // Basit bir kontrol: Boþ yorumu engelle
            if (string.IsNullOrEmpty(comment) || rating < 1 || rating > 5)
            {
                return RedirectToAction("Details", new { id = courseId });
            }

            // Yorum nesnesini oluþtur
            var newReview = new CourseReview
            {
                CourseId = courseId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                CreatedDate = DateTime.Now
            };

            // Veritabanýna kaydet
            _reviewRepo.Add(newReview);

            string message = $"{User.Identity.Name} kullanýcýsý bir kursa yorum yaptý!";
             await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);

            // Detay sayfasýna geri dön (Yorumun göründüðü yer)
            return RedirectToAction("Details", new { id = courseId });
        }

        [HttpPost]
        [Authorize] // Giriþ yapmýþ olmak þart
        public async Task<IActionResult> DeleteReview(int id)
        {
            // 1. Yorumu bul
            var review = _reviewRepo.Get(x => x.Id == id);
            if (review == null) return NotFound();

            // 2. Güvenlik Kontrolü: Sadece 'Admin' silebilir.
            // (Ýstersen || review.UserId == currentUserId ekleyerek kiþinin kendi yorumunu silmesini de saðlayabilirsin)
            if (User.IsInRole("Admin"))
            {
                // 3. Silme iþlemi
                _reviewRepo.Delete(review);
                string message = "Bir yorum Admin tarafýndan silindi.";
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            }

            // 4. Kurs detayýna geri dön
            return RedirectToAction("Details", new { id = review.CourseId });
        }
    }
}
