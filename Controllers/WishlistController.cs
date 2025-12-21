using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;
using System.Security.Claims;

namespace KDemia.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly GenericRepository<Wishlist> _wishlistRepo;

        public WishlistController(GenericRepository<Wishlist> wishlistRepo)
        {
            _wishlistRepo = wishlistRepo;
        }

        // 1. İSTEK LİSTESİ SAYFASI
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var items = _wishlistRepo.GetAll("Course")
                                     .Where(x => x.UserId == userId)
                                     .OrderByDescending(x => x.CreatedDate)
                                     .ToList();

            return View(items);
        }

        // 2. EKLE / ÇIKAR 
        [HttpPost]
        public IActionResult Toggle(int courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var existingItem = _wishlistRepo.GetAll()
                                            .FirstOrDefault(x => x.UserId == userId && x.CourseId == courseId);

            if (existingItem != null)
            {

                _wishlistRepo.Delete(existingItem);
                TempData["WishlistMessage"] = "Kurs istek listesinden çıkarıldı.";
            }
            else
            {

                var newItem = new Wishlist
                {
                    UserId = userId,
                    CourseId = courseId
                };
                _wishlistRepo.Add(newItem);
                TempData["WishlistMessage"] = "Kurs istek listesine eklendi!";
            }

            return RedirectToAction("Details", "Home", new { id = courseId });
        }

        // 3. LİSTEDEN SİL 
        [HttpPost]
        public IActionResult Remove(int id)
        {
            var item = _wishlistRepo.Get(x => x.Id == id);
            if (item != null)
            {
                _wishlistRepo.Delete(item);
            }
            return RedirectToAction("Index");
        }
    }
}