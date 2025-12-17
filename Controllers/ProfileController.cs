using KDemia.Models;
using KDemia.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KDemia.Controllers
{
    [Authorize] 
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 1. PROFİL SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // 2. BİLGİLERİ GÜNCELLE (POST)
        [HttpPost]
        public async Task<IActionResult> UpdateInfo(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Hata varsa sayfaya geri dön (View'da sekme mantığı olduğu için Index'e dönüyoruz)
                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            // Değişiklikleri uygula
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            // Email değişirse güvenlik süreçleri (onay vs.) gerekir, şimdilik basit tutalım:
            if (user.Email != model.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email; // UserName'i de Email ile aynı tutuyoruz
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                // Oturumu tazele (Email değiştiyse cookie bozulmasın diye)
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Index", model);
        }

        // 3. ŞİFRE DEĞİŞTİRME (POST)
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Validation hatası varsa ana sayfaya dönüyoruz ama aslında 
                // ayrı bir viewmodel yollamamız lazım.
                // Basitlik adına TempData ile hata yollayıp redirect edeceğiz.
                TempData["ErrorMessage"] = "Lütfen alanları doğru doldurunuz.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // Çok Önemli: Şifre değişince SecurityStamp değişir, oturum düşmesin diye tazeliyoruz.
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Index");
            }

            // Hata varsa (Örn: Eski şifre yanlış)
            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] = error.Description; // "Incorrect password." gibi
            }

            return RedirectToAction("Index");
        }
    }
}