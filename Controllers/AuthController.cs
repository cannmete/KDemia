using AutoMapper;
using KDemia.Models;
using KDemia.ViewModels;
using KDemia.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KDemia.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly GenericRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<User> _passwordHasher;

        // EKLENDİ: Rolleri sorgulamak için UserManager şart
        private readonly UserManager<User> _userManager;

        public AuthController(
            GenericRepository<User> userRepo,
            IMapper mapper,
            IPasswordHasher<User> passwordHasher,
            UserManager<User> userManager) // Constructor'a eklendi
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _userManager = userManager; // Eşlendi
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Email kontrolü (Repo üzerinden)
            var existingUser = _userRepo.Get(x => x.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Bu e-posta adresi zaten kullanılıyor.");
                return View(model);
            }

            // 2. Kullanıcı nesnesini oluşturma
            var newUser = new User
            {
                UserName = model.Email,
                NormalizedUserName = model.Email.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                FullName = model.FullName,
                CreatedDate = DateTime.Now,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            // 3. GÜVENLİ ŞİFRELEME
            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

            // 4. Kayıt
            _userRepo.Add(newUser);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Kullanıcıyı bul
            var user = _userRepo.Get(x => x.Email == model.Email);

            if (user != null)
            {
                // 2. Şifre Doğrulama
                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

                if (result == PasswordVerificationResult.Success)
                {
                    // --- BURASI GÜNCELLENDİ ---

                    // 3. Kullanıcının GERÇEK rollerini veritabanından çekiyoruz
                    var userRoles = await _userManager.GetRolesAsync(user);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim("FullName", user.FullName ?? "")
                    };

                    // Her bir rolü listeye ekliyoruz (Admin, User, Editor vb.)
                    foreach (var role in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    // Eğer kullanıcının hiç rolü yoksa, güvenlik için en azından 'User' verelim
                    if (!userRoles.Any())
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "User"));
                    }

                    // --- GÜNCELLEME BİTTİ ---

                    var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.KeepMe,
                        ExpiresUtc = model.KeepMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(60)
                    };

                    await HttpContext.SignInAsync(
                        IdentityConstants.ApplicationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "E-posta veya şifre hatalı!");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}