using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;

namespace KDemia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly GenericRepository<Category> _categoryRepo;

        public CategoryController(GenericRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // 1. LİSTELEME SAYFASI
        public IActionResult Index()
        {
            var categories = _categoryRepo.GetAll();
            return View(categories);
        }


        // 2. EKLEME SAYFASI (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 2. EKLEME İŞLEMİ (POST)
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedDate = DateTime.Now; // Tarihi ata
                _categoryRepo.Add(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 3. DÜZENLEME SAYFASI (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _categoryRepo.Get(x => x.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 3. DÜZENLEME İŞLEMİ (POST)
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepo.Update(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // 4. SİLME İŞLEMİ (AJAX İÇİN JSON DÖNDÜRDÜK)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var category = _categoryRepo.Get(x => x.Id == id);
            if (category == null)
            {
                return Json(new { success = false, message = "Kategori bulunamadı." });
            }

            _categoryRepo.Delete(category);
            return Json(new { success = true, message = "Kategori başarıyla silindi." });
        }
    }
}