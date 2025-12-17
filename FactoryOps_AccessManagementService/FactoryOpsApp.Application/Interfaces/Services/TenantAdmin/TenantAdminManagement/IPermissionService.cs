using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement
{
    public interface IPermissionService
    {
        // MODULE
        Task<CommonResponseModel> AddPermission(AddPermissionDto dto);
        Task<CommonResponseModel> UpdatePermission(AddPermissionDto dto);
        Task<CommonResponseModel> DeletePermission(int id, int TenantId);
        GetAllRecord<AddPermissionDto> GetAllPermissions(int TenantId);

        // SUB MODULE
        Task<CommonResponseModel> AddSubPermission(AddSubPermissionDto dto);
        Task<CommonResponseModel> UpdateSubPermission(AddSubPermissionDto dto);
        Task<CommonResponseModel> DeleteSubPermission(int SubPermissionId, int TenantId);
        Task<List<PermissionWithSubDto>> GetPermissionWithSubList(int TenantId);
    }
}
