using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.DTOs
{
    public class RoleRequestDto
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}