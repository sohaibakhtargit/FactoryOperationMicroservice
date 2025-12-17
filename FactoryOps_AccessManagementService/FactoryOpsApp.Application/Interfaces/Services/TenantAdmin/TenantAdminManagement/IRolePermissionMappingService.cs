using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement
{
    public interface IRolePermissionMappingService
    {
        Task<CommonResponseModel> AssignPermissionsToRole(RolePermissionDto dto);
        Task<CommonResponseModel> RemovePermissionsFromRole(int roleId, List<int> permissionIds, List<int> subPermissionIds, int tenantId);
        GetAllRecord<PermissionDto> GetPermissionsByRoleId(int roleId, int TenantId);
    }
}
