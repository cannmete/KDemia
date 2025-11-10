using System.ComponentModel.DataAnnotations;

namespace KDemia.ViewModels
{
    public class CategoryModel : BaseModel
    {

        [Display(Name = "Adı")]
        [Required(ErrorMessage = "Kategori Adı Giriniz!")]
        public string categoryName { get; set; }

    }
}