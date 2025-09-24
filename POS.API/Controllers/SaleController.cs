using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POS.Application.DTOs;
using POS.Infrastructure.Services;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleController : ControllerBase
    {
        // Ejemplo: Registrar venta
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegisterSale([FromBody] SaleDto dto, [FromServices] AuditService audit)
        {
            // ... lógica de registro ...
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
            var userId = int.Parse(userIdStr);
            var userName = User.Identity?.Name ?? "unknown";
            await audit.LogAsync(userId, userName, "register-sale", $"Venta total: {dto.Total}, Cliente: {dto.CustomerId}");
            return Ok();
        }
    }
}