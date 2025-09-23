using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Application.DTOs;
using POS.Domain.Entities;
using POS.Infrastructure;
using System.Text.RegularExpressions;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protege todos los endpoints con JWT
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET paginado y búsqueda
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            try
            {
                var query = _context.Customers.AsQueryable();
                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(c => c.Name.Contains(search));

                var total = await query.CountAsync();
                var customers = await query
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Phone = c.Phone
                    })
                    .ToListAsync();

                return Ok(new { total, customers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al consultar los clientes.", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound(new { message = "Cliente no encontrado." });

                return Ok(new CustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al obtener el cliente.", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerRequest request)
        {
            // Validación avanzada
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "El nombre es obligatorio." });

            if (!string.IsNullOrWhiteSpace(request.Email) && !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { message = "El email no es válido." });

            if (!string.IsNullOrWhiteSpace(request.Phone) && !Regex.IsMatch(request.Phone, @"^[\d\s\+\-]+$"))
                return BadRequest(new { message = "El teléfono solo puede contener números, espacios, + y -." });

            try
            {
                var customer = new Customer
                {
                    Name = request.Name.Trim(),
                    Email = request.Email?.Trim(),
                    Phone = request.Phone?.Trim()
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = customer.Id }, new CustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al crear el cliente.", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "El nombre es obligatorio." });

            if (!string.IsNullOrWhiteSpace(request.Email) && !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { message = "El email no es válido." });

            if (!string.IsNullOrWhiteSpace(request.Phone) && !Regex.IsMatch(request.Phone, @"^[\d\s\+\-]+$"))
                return BadRequest(new { message = "El teléfono solo puede contener números, espacios, + y -." });

            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound(new { message = "Cliente no encontrado." });

                customer.Name = request.Name.Trim();
                customer.Email = request.Email?.Trim();
                customer.Phone = request.Phone?.Trim();

                await _context.SaveChangesAsync();

                return Ok(new CustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al actualizar el cliente.", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer == null) return NotFound(new { message = "Cliente no encontrado." });

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al eliminar el cliente.", details = ex.Message });
            }
        }
    }
}