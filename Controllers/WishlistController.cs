using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;
using System.Security.Claims;

namespace KDemia.Controllers
{
    [Authorize] // Sadece üyeler
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

            // Kurs detaylarıyla beraber çekiyoruz
            var items = _wishlistRepo.GetAll("Course")
                                     .Where(x => x.UserId == userId)
                                     .OrderByDescending(x => x.CreatedDate)
                                     .ToList();

            return View(items);
        }

        // 2. EKLE / ÇIKAR (Toggle Mantığı)
        [HttpPost]
        public IActionResult Toggle(int courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Bu kurs listede var mı?
            var existingItem = _wishlistRepo.GetAll()
                                            .FirstOrDefault(x => x.UserId == userId && x.CourseId == courseId);

            if (existingItem != null)
            {
                // Varsa SİL (Listeden Çıkar)
                _wishlistRepo.Delete(existingItem);
                TempData["WishlistMessage"] = "Kurs istek listesinden çıkarıldı.";
            }
            else
            {
                // Yoksa EKLE
                var newItem = new Wishlist
                {
                    UserId = userId,
                    CourseId = courseId
                };
                _wishlistRepo.Add(newItem);
                TempData["WishlistMessage"] = "Kurs istek listesine eklendi!";
            }

            // İşlem yapılan sayfaya geri dön (Detay sayfasına)
            return RedirectToAction("Details", "Home", new { id = courseId });
        }

        // 3. LİSTEDEN SİL (Index Sayfası İçin)
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