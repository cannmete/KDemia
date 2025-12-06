using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;

namespace KDemia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // 1. DEĞİŞKENLER
        private readonly GenericRepository<User> _userRepo;
        private readonly GenericRepository<Category> _categoryRepo;

        // 2. CONSTRUCTOR 
        public AdminController(GenericRepository<User> userRepo, GenericRepository<Category> categoryRepo)
        {
            // Eğer buradaki eşleştirmeleri yapmazsan hata alırsın!
            _userRepo = userRepo;           // <-- Kullanıcılar için atama
            _categoryRepo = categoryRepo;  // <-- Kategoriler için atama
        }

        // 3. DASHBOARD
        public IActionResult Index()
        {
            return View();
        }

        // 4. KULLANICI LİSTESİ
        [HttpGet]
        public IActionResult UserList()
        {
            
            var users = _userRepo.GetAll();
            return View(users);
        }

        // 5. KATEGORİ SİLME (AJAX)
        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var category = _categoryRepo.Get(x => x.Id == id);
            if (category == null)
                return Json(new { success = false, message = "Kategori bulunamadı." });

            _categoryRepo.Delete(category);
            return Json(new { success = true, message = "Silindi." });
        }
    }
}
