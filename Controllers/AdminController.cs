using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;

namespace KDemia.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        // 1. DEĞİŞKENLER
        private readonly GenericRepository<User> _userRepo;
        private readonly GenericRepository<Course> _courseRepo;
        private readonly GenericRepository<Category> _categoryRepo;

        // 2. CONSTRUCTOR 
        public AdminController(GenericRepository<User> userRepo, GenericRepository<Category> categoryRepo, GenericRepository<Course> courseRepo)
        {
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
            _courseRepo = courseRepo;
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
            return View(users); // Views/Admin/UserList.cshtml'e gider.
        }



        // 5. CONTROL PANEL
        [HttpGet]
        public IActionResult ControlPanel()
        {
            var users = _userRepo.GetAll();
            return View(users);
        }

        // 6. ROL DEĞİŞTİRME
        [HttpPost]
        public IActionResult ChangeRole(int id, string role)
        {
            var user = _userRepo.Get(x => x.Id == id);

            if (user != null)
            {
                user.Role = role;
                _userRepo.Update(user);
            }
            return RedirectToAction("ControlPanel");
        }


        // 7. KATEGORİ SİLME (AJAX )
        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var category = _categoryRepo.Get(x => x.Id == id);
            if (category == null)
                return Json(new { success = false, message = "Kategori bulunamadı." });

            _categoryRepo.Delete(category);
            return Json(new { success = true, message = "Silindi." });
        }

        // EDIT TARAFI

        // 1. DÜZENLEME SAYFASINI GETİR (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _userRepo.Get(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // 2. DÜZENLEME İŞLEMİNİ KAYDET (POST)
        [HttpPost]
        public IActionResult Edit(User user)
        {
            // Önce veritabanındaki "gerçek" kullanıcıyı bulalım
            var existingUser = _userRepo.Get(x => x.Id == user.Id);

            if (existingUser != null)
            {
                // Sadece formdan gelen alanları güncelliyoruz
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;

                // DİKKAT: Şifreye dokunmuyoruz, eski şifresi kalsın.

                _userRepo.Update(existingUser);

                // Başarılı olursa listeye dön
                return RedirectToAction("ControlPanel");
            }

            return View(user);
        }
        // DELETE TARAFI

        // 1. SİLME ONAY SAYFASI (GET)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _userRepo.Get(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // 2.  SİL (POST)
        // View'daki form "Delete" action'ına POST atıyor.
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _userRepo.Get(x => x.Id == id);

            if (user != null)
            {
                _userRepo.Delete(user);
            }
            return RedirectToAction("ControlPanel");
        }
    }
}