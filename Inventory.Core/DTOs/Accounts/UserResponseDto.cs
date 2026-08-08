namespace Inventory.Core.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Img { get; set; }
        public string Token { get; set; } = string.Empty; // Bearer token for API authentication
        public List<string> Roles { get; set; } = new();
        public string RoleName { get; internal set; }
    }
}