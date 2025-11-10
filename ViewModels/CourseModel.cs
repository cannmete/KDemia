using System.ComponentModel.DataAnnotations;

namespace KDemia.ViewModels
{
    public class CourseModel : BaseModel
    {


        [Display(Name = "Ürün Adı")]
        [Required(ErrorMessage = "Ürün Adı Giriniz!")]
        public string courseName { get; set; }





        [Display(Name = "Ürün Açıklama")]
        [Required(ErrorMessage = "Ürün Açıklama Girmelisiniz.")]
        public string Description { get; set; }



        [Display(Name = "Kategori")]
        [Required(ErrorMessage = "Kategori Giriniz!")]
        public int CategoryId { get; set; }
    }
}