namespace POS.Application.DTOs
{
    public class UpdateUserRequest
    {
        public string? Password { get; set; }
        public string Role { get; set; } = "Seller";
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}