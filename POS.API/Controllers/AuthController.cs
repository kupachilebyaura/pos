using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POS.Application.DTOs;
using POS.Infrastructure.Services;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // Aquí deberías inyectar el contexto y la lógica real
            // var user = await _context.Users.FirstOrDefaultAsync(...);
            // ...
            return Ok(new { message = "Contraseña restablecida correctamente." });
        }
    }
}