namespace FactoryOpsApp.Application.DTOs
{
    public class RolePermissionDto
    {
        public int RoleId { get; set; }

        // Parent Permission IDs
        public List<int> PermissionIds { get; set; } = new();

        // SubPermission IDs
        public List<int> SubPermissionIds { get; set; } = new();

        public int TenantId { get; set; }
    }
    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public int? TenantId { get; set; }

        // SubPermissions mapped under each permission
        public List<SubPermissionListResponseDto> SubPermissions { get; set; } = new();
    }
    public class SubPermissionListResponseDto
    {
        public int SubPermissionId { get; set; }
        public string SubPermissionName { get; set; } = string.Empty;
    }
}

