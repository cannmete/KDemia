using KDemia.Models;
using System.ComponentModel.DataAnnotations;

namespace KDemia.Models
{
    public class Category
    {
        // Birincil Anahtar (Primary Key)
        [Key]
        public int Id { get; set; }

        // Kategori Adı
        [MaxLength(50)]

        public string Name { get; set; }

        // Kategori Açıklaması (Opsiyonel)
        [MaxLength(150)]

        public string? Description { get; set; }

        // Kategori Oluşturulma Tarihi
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Kategori Durumu (Aktif/Pasif)
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}