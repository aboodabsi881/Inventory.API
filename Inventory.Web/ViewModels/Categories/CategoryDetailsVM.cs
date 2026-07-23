using Inventory.ViewModel.Products;
using System.Text.Json.Serialization;

namespace Inventory.ViewModel.Categories
{
    public class CategoryDetailsVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Img { get; set; }
        [JsonPropertyName("products")]
        public List<ProductVM> ProductsVM { get; set; } = new();
    }
}