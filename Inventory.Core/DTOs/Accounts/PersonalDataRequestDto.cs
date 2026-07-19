using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.DTOs
{
    public class PersonalDataRequestDto
    {
        [Required(ErrorMessage = "Arabic Name is required.")]
        public string NameAr { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Name is required.")]
        public string NameEn { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Img { get; set; }
    }
}