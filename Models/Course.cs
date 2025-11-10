using System.ComponentModel.DataAnnotations;

namespace KDemia.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Kurs Adı" )]
        [Required(ErrorMessage = "Kurs Adı Girilmelidir !")]

        public string courseName { get; set; }
        
       [Display(Name = "Kurs Açıklaması")]
       [Required(ErrorMessage = "Kurs Açıklaması Girilmelidir !")]

        public string Description { get; set; }

        [Display(Name = "Aktif/Pasif ")]
        public bool IsActive { get; set; }
    }
}
