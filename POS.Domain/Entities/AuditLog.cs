public class AuditLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Action { get; set; } // e.g. "login", "logout", "create-product", "delete-product", "register-sale"
    public string Details { get; set; } // JSON or text with relevant details
}