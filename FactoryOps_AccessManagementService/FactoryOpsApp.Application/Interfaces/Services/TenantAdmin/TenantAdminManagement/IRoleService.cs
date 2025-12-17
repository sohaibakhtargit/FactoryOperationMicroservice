using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement
{

    public interface IRoleService
    {
        Task<CommonResponseModel> AddRole(AddRoleDto dto);
        Task<CommonResponseModel> UpdateRole(AddRoleDto dto);
        Task<CommonResponseModel> DeleteRole(int id, int TenantId);
        GetAllRecord<GetRolePermissionDto> GetAllRoles(int TenantId);
    }
}
