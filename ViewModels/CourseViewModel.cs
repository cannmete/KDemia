using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using KDemia.Models;
using System.Collections.Generic;

namespace KDemia.ViewModels
{
    public class CourseViewModel
    {

        public Course Course { get; set; }


        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}