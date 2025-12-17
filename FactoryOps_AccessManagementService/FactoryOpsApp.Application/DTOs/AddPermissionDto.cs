using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class AddPermissionDto
    {
        public int? PermissionId { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AddSubPermissionDto
    {
        public int? SubPermissionId { get; set; }
        public int ParentPermissionId { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PermissionWithSubDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }

        public List<SubPermissionResponseDto> SubPermissions { get; set; }
    }

    public class SubPermissionResponseDto
    {
        public int SubPermissionId { get; set; }
        public string SubPermissionName { get; set; }
    }


}
