using System.ComponentModel.DataAnnotations;

namespace KDemia.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-Posta adresi gereklidir.")]
        [EmailAddress]
        public string Email { get; set; }
    }
}