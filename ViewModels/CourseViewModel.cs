using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using KDemia.Models;
using System.Collections.Generic;

namespace KDemia.ViewModels
{
    public class CourseViewModel
    {
        // Eğitim bilgileri
        public Course Course { get; set; }

        // Dropdown için Kategori Listesi
        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}