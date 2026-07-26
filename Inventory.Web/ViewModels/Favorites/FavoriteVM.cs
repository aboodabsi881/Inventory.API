namespace Inventory.Web.ViewModels.Favorites
{
    public class FavoriteVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public bool IsFavorite { get; set; }
        public string? ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductImg { get; set; }
    }
}