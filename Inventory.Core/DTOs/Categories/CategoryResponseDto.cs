namespace Inventory.Core.DTOs
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Img { get; set; } = string.Empty;
        public List<ProductResponseDto> Products { get; set; } = new();
        public int ProductsCount { get; set; }
    }
}