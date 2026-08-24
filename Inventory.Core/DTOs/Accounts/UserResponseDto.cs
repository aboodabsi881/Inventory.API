namespace Inventory.Core.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string? Username { get; set; } 
        public string Email { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string Token { get; set; } = string.Empty; 
        public List<string> Roles { get; set; } = new();
        public string? RoleName { get;  set; }
    }
}