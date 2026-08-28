using System.ComponentModel.DataAnnotations;

namespace Inventory.Core.DTOs
{
    public class FavoriteRequestDto
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }
        public bool IsFavorite { get; set; } = true;
    }
}