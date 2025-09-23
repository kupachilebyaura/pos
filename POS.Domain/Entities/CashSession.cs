public class CashSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal InitialAmount { get; set; }
    public decimal? ClosingAmount { get; set; }
    public decimal? Difference { get; set; }
    public ICollection<CashMovement> Movements { get; set; } = new List<CashMovement>();
}