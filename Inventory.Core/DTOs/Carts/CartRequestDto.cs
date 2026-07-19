using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.DTOs
{
    public class CartRequestDto
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}