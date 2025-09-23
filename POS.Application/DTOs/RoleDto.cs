using System.Collections.Generic;

namespace POS.Application.DTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<string> Permissions { get; set; } = new();
    }
}