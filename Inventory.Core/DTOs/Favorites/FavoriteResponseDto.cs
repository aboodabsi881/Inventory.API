namespace Inventory.Core.DTOs
{
    public class FavoriteResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string ProductImg { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}