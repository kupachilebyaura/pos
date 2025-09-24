using POS.Domain.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace POS.Infrastructure.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            // TODO: Validar el token
            // TODO: Implementar el hash de la contraseña
            // user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}