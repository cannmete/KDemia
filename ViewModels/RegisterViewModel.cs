namespace KDemia.ViewModels
{
    public class RegisterViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Saf şifre
        public string ConfirmPassword { get; set; }
    }
}
