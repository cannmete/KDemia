namespace KDemia.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } = "User";

        // Şifreli hali
        public string PasswordHash { get; set; }

        // Şifreyi çözerken (doğrarken) kullanacağımız anahtar (Salt)
        public string Salt { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
