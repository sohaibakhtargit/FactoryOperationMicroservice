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
        public List<int> PermissionIds { get; set; } = new();
        public List<int> SubPermissionIds { get; set; } = new();
        public int TenantId { get; set; }
    }


    //public class GetRolePermissionDto
    //{
    //    public int RoleId { get; set; }
    //    public string RoleName { get; set; }
    //    public int? TenantId { get; set; }
    //    public List<RolePermissionMapDto> Permissions{ get; set; } = new();

    //}
    public class GetRolePermissionDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int? TenantId { get; set; }
        public List<GetRolePermissionMappingDto> Permissions { get; set; } = new();
    }

    public class GetRolePermissionMappingDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public List<GetRoleSubPermissionMapDto> SubPermissions { get; set; } = new();
    }

    public class GetRoleSubPermissionMapDto
    {
        public int SubPermissionId { get; set; }
        public string SubPermissionName { get; set; } = string.Empty;
    }

    //public class RolePermissionMapDto
    //{
    //    public int PermissionId { get; set; }
    //    public string PermissionName { get; set; } = string.Empty;
    //}


}
