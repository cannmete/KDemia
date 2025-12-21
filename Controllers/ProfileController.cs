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

                return View("Index", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");


            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;


            if (user.Email != model.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email; 
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";

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

                TempData["ErrorMessage"] = "Lütfen alanları doğru doldurunuz.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {

                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Index");
            }

            // Hata varsa
            foreach (var error in result.Errors)
            {
                TempData["ErrorMessage"] = error.Description; 
            }

            return RedirectToAction("Index");
        }
    }
}