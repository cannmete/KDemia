using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KDemia.Models
{
    public class CourseReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Comment { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } 

        public DateTime CreatedDate { get; set; } = DateTime.Now;



        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}