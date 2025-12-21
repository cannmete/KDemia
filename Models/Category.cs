using KDemia.Models;
using System.ComponentModel.DataAnnotations;

namespace KDemia.Models
{
    public class Category
    {

        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }


        [MaxLength(150)]

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}