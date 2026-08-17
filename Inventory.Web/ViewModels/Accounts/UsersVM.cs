using System.Text.Json.Serialization;

namespace Inventory.Web.ViewModels.Accounts
{
    public class UsersVM
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("img")]
        public string? Img { get; set; }
        public List<string> Roles { get; set; } = new();
        private string? _roleName;
        public string RoleName
        {
            get => !string.IsNullOrEmpty(_roleName) ? _roleName : (Roles.FirstOrDefault() ?? "User");
            set => _roleName = value;
        }
    }
}