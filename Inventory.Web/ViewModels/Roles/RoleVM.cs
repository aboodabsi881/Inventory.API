using System.ComponentModel.DataAnnotations;

namespace Inventory.ViewModel.Roles
{
    public class RoleVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(256, ErrorMessage = "Role name cannot exceed 256 characters")]
        [Display(Name = "Role Name")]
        public string Name { get; set; } = string.Empty;
    }
}