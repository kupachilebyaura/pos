[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token && u.PasswordResetTokenExpires > DateTime.UtcNow);
    if (user == null)
        return BadRequest(new { message = "Token inválido o expirado." });

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.PasswordResetToken = null;
    user.PasswordResetTokenExpires = null;
    await _context.SaveChangesAsync();

    return Ok(new { message = "Contraseña restablecida correctamente." });
}