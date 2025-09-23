public class CashMovement
{
    public int Id { get; set; }
    public int CashSessionId { get; set; }
    public CashSession Session { get; set; }
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } // "Ingreso" | "Retiro"
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int UserId { get; set; }
}