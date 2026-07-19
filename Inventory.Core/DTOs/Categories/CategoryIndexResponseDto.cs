using System.Collections.Generic;

namespace Inventory.Core.DTOs
{
    public class CategoryIndexResponseDto
    {
        public IReadOnlyList<CategoryResponseDto> Categories { get; set; } = new List<CategoryResponseDto>();
        public IReadOnlyList<FavoriteResponseDto> Favorites { get; set; } = new List<FavoriteResponseDto>();
        public IReadOnlyList<CartResponseDto> CartItems { get; set; } = new List<CartResponseDto>();
    }
}