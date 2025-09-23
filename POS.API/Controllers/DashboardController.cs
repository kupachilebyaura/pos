using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure;
using System;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts()
        {
            var now = DateTime.UtcNow;
            var startMonth = new DateTime(now.Year, now.Month, 1);

            var topProducts = await _context.SaleDetails
                .Include(sd => sd.Product)
                .Include(sd => sd.Sale)
                .Where(sd => sd.Sale.Date >= startMonth && sd.Sale.Date < startMonth.AddMonths(1))
                .GroupBy(sd => new { sd.ProductId, sd.Product.Name })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(x => x.Quantity),
                    TotalIncome = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            return Ok(topProducts);
        }
    }
}