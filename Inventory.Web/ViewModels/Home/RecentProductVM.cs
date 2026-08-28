using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Home
{
    public class RecentProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? CategoryName { get; set; }
        public string? Img { get; set; }

        [JsonPropertyName("quantity")]
        public int StockQuantity { get; set; }
    }
}
