using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;
using Microsoft.AspNetCore.Identity;

namespace KDemia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // 1. DEĞİŞKENLER
        private readonly GenericRepository<User> _userRepo;
        private readonly GenericRepository<Course> _courseRepo;
        private readonly GenericRepository<Category> _categoryRepo;

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // 2. CONSTRUCTOR 
        public AdminController(
            GenericRepository<User> userRepo,
            GenericRepository<Category> categoryRepo,
            GenericRepository<Course> courseRepo,
            UserManager<User> userManager,         
            RoleManager<IdentityRole> roleManager) 
        {
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
            _courseRepo = courseRepo;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 3. DASHBOARD
        public IActionResult Index()
        {
            try
            {
                var users = _userRepo.GetAll();
                var courses = _courseRepo.GetAll();

                ViewBag.TotalUsers = users != null ? users.Count() : 0;
                ViewBag.TotalCourses = courses != null ? courses.Count() : 0;
            }
            catch
            {
                ViewBag.TotalUsers = 0;
                ViewBag.TotalCourses = 0;
            }

            return View();
        }

        // 4. KULLANICI LİSTESİ
        [HttpGet]
        public IActionResult UserList()
        {
            var users = _userRepo.GetAll();
            return View(users);
        }

        // 5. CONTROL PANEL (Kullanıcı Yönetimi)
        [HttpGet]
        public IActionResult ControlPanel()
        {
            var users = _userRepo.GetAll();
            return View(users);
        }

        // 6. ROL DEĞİŞTİRME (EN ÖNEMLİ KISIM)
        // ID artık int değil string!
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            // 1. Kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                // 2. Mevcut rollerini al ve sil (Temizle)
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // 3. Rol veritabanında var mı kontrol et, yoksa oluştur (Otomatik)
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                // 4. Yeni rolü ekle
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction("ControlPanel");
        }

        // 7. KATEGORİ SİLME (AJAX)
        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var category = _categoryRepo.Get(x => x.Id == id);
            if (category == null)
                return Json(new { success = false, message = "Kategori bulunamadı." });

            _categoryRepo.Delete(category);
            return Json(new { success = true, message = "Silindi." });
        }

        // --- USER EDIT ---

        [HttpGet]
        public async Task<IActionResult> Edit(string id) // ID -> String oldu
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            var existingUser = await _userManager.FindByIdAsync(user.Id);

            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.UserName = user.Email; // UserName genelde Email ile aynı tutulur

                // Identity update işlemi
                await _userManager.UpdateAsync(existingUser);

                return RedirectToAction("ControlPanel");
            }

            return View(user);
        }

        // --- USER DELETE ---

        [HttpGet]
        public async Task<IActionResult> Delete(string id) // ID -> String oldu
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id) // ID -> String oldu
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                // Repo yerine UserManager ile silmek daha güvenlidir (ilişkili verileri temizler)
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("ControlPanel");
        }
    }
}