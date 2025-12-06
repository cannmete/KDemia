using KDemia.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace KDemia.Models
{
    public class Course
    {
        // P.K
        [Key]
        public int Id { get; set; }


        [MaxLength(25)]
        public string Title { get; set; }

        [MaxLength(50)]

        public string ShortDescription { get; set; }

        public string DetailContent { get; set; }

        // Kurs Fiyatı
        public decimal Price { get; set; }
        public string? VideoUrl { get; set; }      
        public string? VideoSource { get; set; } 
        public int UserId { get; set; } // İlişkili Kullanıcının ID'si
        [ValidateNever]
        public virtual User User { get; set; } // Kullanıcı detaylarına ulaşmak için

        public bool IsPublished { get; set; }

        // İlişkisel Anahtar (Foreign Key)
        // Hangi kategoriye ait olduğunu belirtir.
        public int CategoryId { get; set; }

        // İlişkisel Özellik
        // Kursun ait olduğu Category nesnesine erişim sağlar.
        [ValidateNever]
        public virtual Category Category { get; set; }
    }
}