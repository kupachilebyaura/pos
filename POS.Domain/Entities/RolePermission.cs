namespace POS.Domain.Entities
{
    public class RolePermission
    {
        public int Id { get; set; }
        public string Permission { get; set; } = null!;
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }
}