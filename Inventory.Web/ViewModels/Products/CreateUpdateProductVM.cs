using Inventory.Core.Utilities.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Products
{
    public class CreateUpdateProductVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Display(Name = "Power Type")]
        public PowerType? PowerType { get; set; }

        [ValidateNever]
        public string? Img { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }

        [Display(Name = "Product Image")]
        public IFormFile? ImgFile { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Category")]

        public int CategoryId { get; set; }
        [ValidateNever]
        public SelectList? CategoryLookup { get; set; }
    }
}