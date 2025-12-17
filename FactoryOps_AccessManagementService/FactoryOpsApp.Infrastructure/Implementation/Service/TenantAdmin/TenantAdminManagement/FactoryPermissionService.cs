using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class FactoryPermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;

        public FactoryPermissionService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        #region ===== MODULE =====

        public async Task<CommonResponseModel> AddPermission(AddPermissionDto dto)
        {
            return await _permissionRepository.AddPermission(dto);
        }

        public async Task<CommonResponseModel> UpdatePermission(AddPermissionDto dto)
        {
            return await _permissionRepository.UpdatePermission(dto);
        }

        public async Task<CommonResponseModel> DeletePermission(int id, int TenantId)
        {
            return await _permissionRepository.DeletePermission(id, TenantId);
        }

        public GetAllRecord<AddPermissionDto> GetAllPermissions(int TenantId)
        {
            return _permissionRepository.GetAllPermissions(TenantId);
        }

        #endregion


        #region ===== SUB MODULE =====

        public async Task<CommonResponseModel> AddSubPermission(AddSubPermissionDto dto)
        {
            return await _permissionRepository.AddSubPermission(dto);
        }

        public async Task<CommonResponseModel> UpdateSubPermission(AddSubPermissionDto dto)
        {
            return await _permissionRepository.UpdateSubPermission(dto);
        }

        public async Task<CommonResponseModel> DeleteSubPermission(int SubPermissionId, int TenantId)
        {
            return await _permissionRepository.DeleteSubPermission(SubPermissionId, TenantId);
        }

        public async Task<List<PermissionWithSubDto>> GetPermissionWithSubList(int TenantId)
        {
            return await _permissionRepository.GetPermissionWithSubList(TenantId);
        }

        #endregion
    }
}
