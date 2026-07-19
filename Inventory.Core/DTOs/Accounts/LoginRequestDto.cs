using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.DTOs
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email or Username is required.")]
        public string LoginIdentifier { get; set; } = string.Empty; // Allows logging in via Username or Email

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}