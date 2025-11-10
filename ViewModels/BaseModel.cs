using System.ComponentModel.DataAnnotations;

namespace KDemia.ViewModels
{
    public class BaseModel
    {
        public int Id { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}