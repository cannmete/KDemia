using KDemia.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KDemia.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Identity için bu satır şart!

            // --- YORUM TABLOSU AYARLARI ---

            // 1. Kurs silinirse -> Yorumlar SİLİNSİN (Cascade)
            // Çünkü kurs yoksa yorumun bir anlamı kalmaz.
            builder.Entity<CourseReview>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Kullanıcı silinirse -> Yorumlar SİLİNMESİN (Restrict)
            // Hata almamak için zinciri buradan kırıyoruz.
            builder.Entity<CourseReview>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1. Kullanıcı silinirse -> Satın alma geçmişi SİLİNMESİN (Restrict)
            // Bu sayede kullanıcıyı silmeye çalışırsan "Önce kayıtları temizle" hatası alırsın, veri kaybı önlenir.
            builder.Entity<CourseEnrollment>()
                .HasOne(e => e.User)
                .WithMany() // User modelinde 'Enrollments' koleksiyonu varsa buraya yazabilirsin, yoksa boş bırak.
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Kurs silinirse -> Satın alma kaydı SİLİNMESİN (Restrict)
            // Bir kursu sildiğinde, onu satın alan öğrencilerin geçmişini bozmamak için bunu da kısıtlıyoruz.
            builder.Entity<CourseEnrollment>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Wishlist>()
                .HasOne(w => w.Course)
                .WithMany()
                .HasForeignKey(w => w.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}