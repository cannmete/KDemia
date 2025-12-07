using AutoMapper;
using KDemia.Extensions;
using KDemia.Models;
using KDemia.Repositories;
using KDemia.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KDemia.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly GenericRepository<User> _userRepo;
        private readonly IMapper _mapper;

        public AuthController(GenericRepository<User> userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = _mapper.Map<User>(model);

                // Şifreleme (Hashing) İşlemleri
                string userSalt = Guid.NewGuid().ToString();
                string rawPassword = model.Password + userSalt;
                string hashedPassword = rawPassword.ToSHA256();

                newUser.Salt = userSalt;
                newUser.PasswordHash = hashedPassword;
                newUser.CreatedDate = DateTime.Now;

                // Varsayılan rolü User olarak atayalım (eğer null geliyorsa)
                if (string.IsNullOrEmpty(newUser.Role))
                {
                    newUser.Role = "User";
                }

                _userRepo.Add(newUser);

                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Eğer kullanıcı zaten giriş yapmışsa tekrar Login sayfasına girmesin
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Course");
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _userRepo.Get(x => x.Email == model.Email);

                if (user != null)
                {
                    // Şifre Kontrolü
                    string rawTry = model.Password + user.Salt;
                    string tryHash = rawTry.ToSHA256();

                    if (tryHash == user.PasswordHash)
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Email),
                            new Claim("FullName", user.FullName ?? ""),
                            new Claim(ClaimTypes.Role, user.Role ?? "User")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // --- Keep Me ---
                        var authProperties = new AuthenticationProperties
                        {
                            // LoginViewModel'den gelen KeepMe true ise tarayıcı kapansa da cookie silinmez.
                            IsPersistent = model.KeepMe,

                            // Beni Hatırla seçildiyse 30 gün, seçilmediyse 60 dakika oturum açık kalsın.
                            ExpiresUtc = model.KeepMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(60)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties); // authProperties buraya eklendi

                        // --- YÖNLENDİRME MANTIĞI ---
                        // Admin ise Yönetim Paneline, değilse Ana Sayfaya
                        if (user.Role == "Admin")
                        {
                            // Dashboarda yönlendiriyoruz.
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            // Normal kullanıcı Home/Index' e gitsin.
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError("", "Email veya şifre hatalı!");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}