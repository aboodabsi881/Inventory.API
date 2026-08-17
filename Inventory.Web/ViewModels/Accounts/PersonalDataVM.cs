using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Accounts
{
    public class PersonalDataVM
    {
        // 💡 Changed from string to int
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "English Name is required")]
        [Display(Name = "English Name")]
        public string NameEn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arabic Name is required")]
        [Display(Name = "Arabic Name")]
        public string NameAr { get; set; } = string.Empty;

        [ValidateNever]
        [Display(Name = "Role")]
        public string? RoleName { get; set; }

        [ValidateNever]
        public SelectList? RoleLookup { get; set; }

        [ValidateNever]
        [JsonPropertyName("img")]
        public string? Img { get; set; }

        [ValidateNever]
        [Display(Name = "Profile Picture")]
        public IFormFile? ImgFile { get; set; }
    }
}