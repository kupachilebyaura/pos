namespace POS.Domain.Entities
{
    public class CashMovement
    {
        public int Id { get; set; }
        public int CashSessionId { get; set; }
        public CashSession Session { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty; // "Ingreso" | "Retiro"
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}