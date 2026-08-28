namespace Inventory.Core.DTOs
{
    public class CartResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string? ProductImg { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public int UserId{ get; set; }
    }
}