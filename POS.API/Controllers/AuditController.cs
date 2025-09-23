using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetAuditHistory(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int? userId,
            [FromQuery] string? action,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.Timestamp >= from.Value);
            if (to.HasValue)
                query = query.Where(l => l.Timestamp <= to.Value);
            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);
            if (!string.IsNullOrEmpty(action))
                query = query.Where(l => l.Action == action);

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.Timestamp,
                    l.UserId,
                    l.UserName,
                    l.Action,
                    l.Details
                })
                .ToListAsync();

            return Ok(new { data = logs, total, page, pageSize });
        }
    }
}