using Inventory.Core.Utilities.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Products
{
    public class CreateUpdateProductVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Display(Name = "Power Type")]
        public PowerType? PowerType { get; set; }

        [ValidateNever]
        public string? Img { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImgFile { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 1000000.00, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, 10000, ErrorMessage = "Quantity must be 0 or greater")]
        [Display(Name = "Stock Quantity")]
        public int Quantity { get; set; } = 0;

        [Required(ErrorMessage = "Please select a category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public SelectList? CategoryLookup { get; set; }
    }
}