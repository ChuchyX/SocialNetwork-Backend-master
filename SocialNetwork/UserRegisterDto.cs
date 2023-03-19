using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SocialNetwork
{
    public class UserRegisterDto
    {
        [Required]
        [NotNull]
        public string Username { get; set; } = string.Empty;

        [Required]
        [NotNull]
        public string Password { get; set; } = string.Empty;

        [Required]
        [NotNull]
        public string Email { get; set; } = string.Empty;

        [Required]
        [NotNull]
        public int Edad { get; set; }

        [Required]
        [NotNull]
        public string Sexo { get; set; } = string.Empty;
    }
}
