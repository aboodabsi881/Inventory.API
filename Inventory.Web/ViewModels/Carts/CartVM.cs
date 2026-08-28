namespace Inventory.Web.ViewModels.Carts
{
    public class CartVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductImg { get; set; }

        public int AvailableStock { get; set; }

        public bool IsOutOfStock => AvailableStock <= 0;
        public bool IsMaxStockReached => AvailableStock > 0 && Quantity >= AvailableStock;
        public bool IsLowStock => AvailableStock > 0 && AvailableStock <= 5;
    }
}