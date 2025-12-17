using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TenantManagement
{
    public interface ITenantRepository
    {
        public Task<CommonResponseModel> AddTenant(AddTenantDto tenant);
        public Task<GetAllRecord<GetAllTenantsDto>> GetAllTenants();
        public Task<GetAllRecord<ModulelistDto>> GetAllModuleList();
        public Task<CommonResponseModel> UpdateTenants(AddTenantDto tenant);
        Task<CommonResponseModel> UpdateTenantModulesAsync(UpdateTenantModulesDto dto);
        public Task<CommonResponseModel> DeleteTenants(int Id);

        public Task<CommonResponseModel> ChangeTenants(int Id);

        public Task<CommonResponseModel> ForceLogout(int Id);
        public Task<CommonResponseModel> Suspend(int Id);
        public Task<GetAllRecord<ModulelistDto>> GetAllModuleAsync(int TenantId);
    }
}
