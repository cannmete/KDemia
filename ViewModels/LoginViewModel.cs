using System.ComponentModel.DataAnnotations;

namespace KDemia.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool KeepMe { get; set; }
    }
}
