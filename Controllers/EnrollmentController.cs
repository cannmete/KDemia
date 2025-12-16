using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KDemia.Models;
using KDemia.Repositories;
using System.Security.Claims;
using System.Threading.Tasks; // Task.Delay için

namespace KDemia.Controllers
{
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly GenericRepository<CourseEnrollment> _enrollmentRepo;
        private readonly GenericRepository<Course> _courseRepo;

        public EnrollmentController(
            GenericRepository<CourseEnrollment> enrollmentRepo,
            GenericRepository<Course> courseRepo)
        {
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
        }

        // 1. ÖDEME EKRANI (GET)
        [HttpGet]
        public IActionResult Checkout(int courseId)
        {
            // Kursun detaylarını çekelim ki ödeme ekranında "Şunu alıyorsunuz: 100 TL" diyebilelim
            var course = _courseRepo.Get(x => x.Id == courseId);

            if (course == null) return NotFound();

            // Kullanıcı bu kursu zaten almış mı kontrolü
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var existing = _enrollmentRepo.GetAll().FirstOrDefault(x => x.UserId == userId && x.CourseId == courseId);

            if (existing != null)
            {
                return RedirectToAction("MyCourses");
            }

            return View(course);
        }

        // 2. SATIN ALMAYI TAMAMLA (POST)
        [HttpPost]
        public async Task<IActionResult> CompletePurchase(int courseId)
        {
            // Simülasyon: Sanki bankaya bağlanıyormuşuz gibi 2 saniye bekletelim
            await Task.Delay(2000);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Tekrar kontrol (Güvenlik)
            var existing = _enrollmentRepo.GetAll().FirstOrDefault(x => x.UserId == userId && x.CourseId == courseId);
            if (existing != null) return RedirectToAction("MyCourses");

            var enrollment = new CourseEnrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.Now
            };

            _enrollmentRepo.Add(enrollment);

            // Başarılı ödeme sonrası Kurslarım'a yönlendir
            return RedirectToAction("MyCourses");
        }

        // 3. KURSLARIM SAYFASI
        public IActionResult MyCourses()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kurs bilgileriyle beraber Enrollment kayıtlarını çekiyoruz
            // Not: View tarafında Enrollment ID'si lazım olacağı için direkt Course listesi yerine
            // Enrollment listesi gönderiyoruz.
            var enrollments = _enrollmentRepo.GetAll("Course")
                                             .Where(x => x.UserId == userId)
                                             .OrderByDescending(x => x.EnrollmentDate)
                                             .ToList();

            return View(enrollments);
        }

        // 4. KURSU İADE ET / SİL (POST)
        [HttpPost]
        public IActionResult RemoveCourse(int enrollmentId)
        {
            var enrollment = _enrollmentRepo.Get(x => x.Id == enrollmentId);

            if (enrollment != null)
            {
                // Sadece kendi kursunu silebilir (Güvenlik)
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (enrollment.UserId == currentUserId)
                {
                    _enrollmentRepo.Delete(enrollment);
                }
            }

            return RedirectToAction("MyCourses");
        }
    }
}