using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace KDemia.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
    }
}