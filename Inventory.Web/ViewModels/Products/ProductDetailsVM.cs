using Inventory.Core.Utilities.Enums;
using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Products
{
    public class ProductDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PowerType? PowerType { get; set; }

        public string? Img { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // 💡 Store interaction properties needed by the view:
        public int QuantityInCart { get; set; } = 0;
        public bool IsFavorite { get; set; } = false;
    }
}