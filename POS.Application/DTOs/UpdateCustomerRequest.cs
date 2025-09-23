namespace POS.Application.DTOs
{
    public class UpdateCustomerRequest
    {
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}