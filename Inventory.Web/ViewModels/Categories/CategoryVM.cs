using Inventory.ViewModel.Products;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventory.ViewModel.Categories
{
    public class CategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Img { get; set; }
        public IFormFile? ImgFile { get; set; }
        [JsonPropertyName("products")]
        public List<ProductVM> Products { get; set; } = new();

    }
}