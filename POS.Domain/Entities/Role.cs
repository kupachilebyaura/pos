using System.Collections.Generic;

namespace POS.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }
}