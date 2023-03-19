using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace SocialNetwork
{
    public class UserLoginDto
    {       
        [Required]
        [NotNull]
        public string Password { get; set; } = string.Empty;

        [Required]
        [NotNull]
        public string Email { get; set; } = string.Empty;
    }
}
