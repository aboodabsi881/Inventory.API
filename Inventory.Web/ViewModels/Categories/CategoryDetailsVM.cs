using Inventory.Web.ViewModels.Products;
using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Categories
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