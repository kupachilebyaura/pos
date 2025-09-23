// Ejemplo: Registrar venta
[Authorize]
[HttpPost]
public async Task<IActionResult> RegisterSale([FromBody] SaleDto dto, [FromServices] AuditService audit)
{
    // ... lógica de registro ...
    await audit.LogAsync(userId, userName, "register-sale", $"Venta total: {dto.Total}, Cliente: {dto.ClientId}");
    // ...
}