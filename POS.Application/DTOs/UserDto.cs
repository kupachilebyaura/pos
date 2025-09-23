namespace POS.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}