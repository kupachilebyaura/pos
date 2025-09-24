using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POS.Application.DTOs;
using POS.Infrastructure.Services;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        // Ejemplo: Registrar en el servicio de auditoría las acciones clave en los endpoints de producto
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto dto, [FromServices] AuditService audit)
        {
            // ... lógica de creación ...
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
            var userId = int.Parse(userIdStr);
            var userName = User.Identity?.Name ?? "unknown";
            await audit.LogAsync(userId, userName, "create-product", $"Producto: {dto.Name}");
            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id, [FromServices] AuditService audit)
        {
            // ... lógica de borrado ...
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
            var userId = int.Parse(userIdStr);
            var userName = User.Identity?.Name ?? "unknown";
            await audit.LogAsync(userId, userName, "delete-product", $"Id: {id}");
            return Ok();
        }
    }
}