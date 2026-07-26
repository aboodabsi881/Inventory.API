using Inventory.Core.Utilities.Enums;
using Inventory.ViewModel.Categories;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventory.ViewModel.Products
{
    public class ProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PowerType? PowerType { get; set; }
        public string? Img { get; set; }
        public IFormFile? ImgFile { get; set; }
        public decimal Price { get; set; }
        public bool IsFavorite { get; set; }
        public int QuantityInCart { get; set; }
        public CategoryDetailsVM? Category { get; set; }
    }
}