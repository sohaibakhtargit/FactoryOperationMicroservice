namespace FactoryOpsApp.Application.DTOs
{
    public class RolePermissionDto
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
        public int TenantId { get; set; }
    }

    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public int TenantId { get; set; }
    }
}
