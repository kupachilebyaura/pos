// Ejemplo: Registrar en el servicio de auditoría las acciones clave en los endpoints de producto
[Authorize]
[HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] ProductDto dto, [FromServices] AuditService audit)
{
    // ... lógica de creación ...
    await audit.LogAsync(userId, userName, "create-product", $"Producto: {dto.Name}");
    // ...
}

[Authorize]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteProduct(int id, [FromServices] AuditService audit)
{
    // ... lógica de borrado ...
    await audit.LogAsync(userId, userName, "delete-product", $"Id: {id}");
    // ...
}