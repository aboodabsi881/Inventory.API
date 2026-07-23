using System.ComponentModel.DataAnnotations;
using Inventory.Core.Utilities.Enums;

namespace Inventory.Core.DTOs
{
    public class ProductRequestDto
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public PowerType? PowerType { get; set; }

        [Required(ErrorMessage = "Product image path is required.")]
        public string? Img { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }
    }
}