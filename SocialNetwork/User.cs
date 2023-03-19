using Microsoft.EntityFrameworkCore;

namespace SocialNetwork
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Edad { get; set; }
        public string Sexo { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
        public string ProfilePicture { get; set; } = string.Empty;
    }
}
