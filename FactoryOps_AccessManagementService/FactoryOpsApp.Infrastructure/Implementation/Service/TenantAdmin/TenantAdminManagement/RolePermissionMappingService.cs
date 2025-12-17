using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class RolePermissionMappingService : IRolePermissionMappingService
    {
        private readonly IRolePermissionMappingRepository _repository;

        public RolePermissionMappingService(IRolePermissionMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<CommonResponseModel> AssignPermissionsToRole(RolePermissionDto dto)
        {
            return await _repository.AssignPermissionsToRole(dto);
        }

        public async Task<CommonResponseModel> RemovePermissionsFromRole(int roleId, List<int> permissionIds, List<int> subPermissionIds, int tenantId)
        {
            return await _repository.RemovePermissionsFromRole(roleId, permissionIds, subPermissionIds, tenantId);
        }

        public GetAllRecord<PermissionDto> GetPermissionsByRoleId(int roleId, int TenantId)
        {
            return _repository.GetPermissionsByRoleId(roleId, TenantId);
        }
    }
}

