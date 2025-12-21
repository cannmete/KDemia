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
        // İlişkili tabloların repository'leri
        private readonly GenericRepository<CourseEnrollment> _enrollmentRepo;
        private readonly GenericRepository<CourseReview> _reviewRepo;
        private readonly GenericRepository<Wishlist> _wishlistRepo;

        // 2. CONSTRUCTOR 
        public AdminController(
            GenericRepository<User> userRepo,
            GenericRepository<Category> categoryRepo,
            GenericRepository<Course> courseRepo,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            GenericRepository<CourseEnrollment> enrollmentRepo,
            GenericRepository<CourseReview> reviewRepo,
            GenericRepository<Wishlist> wishlistRepo)
        {
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
            _courseRepo = courseRepo;
            _userManager = userManager;
            _roleManager = roleManager;
            _enrollmentRepo = enrollmentRepo;
            _reviewRepo = reviewRepo;
            _wishlistRepo = wishlistRepo;
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


                return View(courses);
            }
            catch
            {
                ViewBag.TotalUsers = 0;
                ViewBag.TotalCourses = 0;
                return View();
            }
        }

        // 4. KULLANICI LİSTESİ
        [HttpGet]
        public IActionResult UserList()
        {
            var users = _userRepo.GetAll();
            return View(users);
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
        public async Task<IActionResult> ChangeRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

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


        //  8. KURS SİLME
        [HttpPost]
        public IActionResult DeleteCourse(int id)
        {
            var course = _courseRepo.Get(x => x.Id == id);

            if (course == null) return NotFound();

            // A. YORUMLARI SİL
            var reviews = _reviewRepo.GetAll().Where(x => x.CourseId == id).ToList();
            foreach (var review in reviews)
            {
                _reviewRepo.Delete(review);
            }

            // B. SATIN ALMALARI SİL
            var enrollments = _enrollmentRepo.GetAll().Where(x => x.CourseId == id).ToList();
            foreach (var enrollment in enrollments)
            {
                _enrollmentRepo.Delete(enrollment);
            }

            // C. İSTEK LİSTELERİNİ SİL
            var wishlists = _wishlistRepo.GetAll().Where(x => x.CourseId == id).ToList();
            foreach (var wish in wishlists)
            {
                _wishlistRepo.Delete(wish);
            }

            // D. KURSU SİL
            _courseRepo.Delete(course);

            return RedirectToAction("Index");
        }

        // --- USER EDIT ---
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
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
                existingUser.UserName = user.Email;

                await _userManager.UpdateAsync(existingUser);
                return RedirectToAction("ControlPanel");
            }
            return View(user);
        }

        // --- USER DELETE ---
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("ControlPanel");
        }
    }
}