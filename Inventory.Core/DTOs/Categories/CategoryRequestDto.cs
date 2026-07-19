using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.DTOs
{
    public class CategoryRequestDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category image path is required.")]
        public string Img { get; set; } = string.Empty;
    }
}