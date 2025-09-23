using POS.Domain.Entities;
using POS.Infrastructure;
using System.Security.Claims;

public class AuditService
{
    private readonly ApplicationDbContext _context;
    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int userId, string userName, string action, string details)
    {
        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            UserName = userName,
            Action = action,
            Details = details
        };
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}