using Microsoft.AspNetCore.Http;
using POS.Infrastructure;
using System.Security.Claims;
using System.Text.Json;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        // Only audit on certain endpoints (example for login/logout)
        if (context.Request.Path.StartsWithSegments("/api/auth/login") && context.Request.Method == "POST")
        {
            // Let the login happen so userId is available
            await _next(context);

            // If login was successful
            if (context.Response.StatusCode == 200 && context.User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var userName = context.User.Identity.Name!;
                dbContext.AuditLogs.Add(new POS.Domain.Entities.AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    UserId = userId,
                    UserName = userName,
                    Action = "login",
                    Details = "{}"
                });
                await dbContext.SaveChangesAsync();
            }
            return;
        }
        // For logout, etc. (implement as needed)
        await _next(context);
    }
}