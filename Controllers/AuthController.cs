using AutoMapper;
using KDemia.Extensions;
using KDemia.Models;
using KDemia.ViewModels;
using KDemia.Repositories; // Repository namespace'i eklendi
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KDemia.Controllers
{
    public class AuthController : Controller
    {
        // ARTIK Context YOK, Repository VAR
        private readonly GenericRepository<User> _userRepo;
        private readonly IMapper _mapper;

        // Constructor'da Repository istiyoruz
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

                string userSalt = Guid.NewGuid().ToString();
                string rawPassword = model.Password + userSalt;
                string hashedPassword = rawPassword.ToSHA256();

                newUser.Salt = userSalt;
                newUser.PasswordHash = hashedPassword;
                newUser.CreatedDate = DateTime.Now;

                _userRepo.Add(newUser);

                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
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
                    string rawTry = model.Password + user.Salt;
                    string tryHash = rawTry.ToSHA256();

                    if (tryHash == user.PasswordHash)
                    {   
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Email),
                            new Claim("FullName", user.FullName ?? ""),
                            new Claim(ClaimTypes.Role, user.Role)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties();

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        return RedirectToAction("Index", "Admin");
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
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}