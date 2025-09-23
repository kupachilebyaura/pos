namespace POS.Application.DTOs
{
    public class CreateUserRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "Seller";
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}