using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddRoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int TenantId { get; set; }
        public List<int> PermissionIds { get; set; } = new();
    }

    public class GetRolePermissionDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int TenantId { get; set; }
        public List<RolePermissionMapDto> Permissions{ get; set; } = new();

    }

    public class RolePermissionMapDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
    }


}
