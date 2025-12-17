using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class FactoryRoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public FactoryRoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<CommonResponseModel> AddRole(AddRoleDto dto)
            => await _roleRepository.AddRole(dto);

        public async Task<CommonResponseModel> UpdateRole(AddRoleDto dto)
            => await _roleRepository.UpdateRole(dto);

        public async Task<CommonResponseModel> DeleteRole(int id, int TenantId)
            => await _roleRepository.DeleteRole(id, TenantId);

        public GetAllRecord<GetRolePermissionDto> GetAllRoles(int TenantId)
            => _roleRepository.GetAllRoles(TenantId);
    }
}
