using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Infrastructure;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> Get()
        {
            var roles = await _context.Roles
                .Include(r => r.Permissions)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Permissions = r.Permissions.Select(p => p.Permission).ToList()
                }).ToListAsync();

            return Ok(roles);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<RoleDto>> Create([FromBody] RoleDto dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                Permissions = dto.Permissions.Select(p => new RolePermission { Permission = p }).ToList()
            };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleDto dto)
        {
            var role = await _context.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return NotFound();

            role.Name = dto.Name;
            role.Permissions.Clear();
            foreach (var perm in dto.Permissions)
                role.Permissions.Add(new RolePermission { Permission = perm });

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}