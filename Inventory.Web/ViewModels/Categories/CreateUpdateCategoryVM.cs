using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.ViewModel.Categories
{
    public class CreateUpdateCategoryVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [ValidateNever]
        public string? Img { get; set; }

        [Display(Name = "Category Image")]
        public IFormFile? ImgFile { get; set; }
    }
}