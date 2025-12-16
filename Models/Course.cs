using KDemia.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KDemia.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(25)]
        [Required(ErrorMessage = "Başlık gereklidir.")]
        public string Title { get; set; }

        [MaxLength(50)]
        [Required(ErrorMessage = "Kısa açıklama gereklidir.")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "İçerik detayı gereklidir.")]
        public string DetailContent { get; set; }

        public decimal Price { get; set; }
        public string? VideoUrl { get; set; }
        public string? VideoSource { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [ValidateNever]
        public virtual User User { get; set; }

        public bool IsPublished { get; set; }

        public int CategoryId { get; set; }

        [ValidateNever]
        public virtual Category Category { get; set; }
        public virtual ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
    }
}