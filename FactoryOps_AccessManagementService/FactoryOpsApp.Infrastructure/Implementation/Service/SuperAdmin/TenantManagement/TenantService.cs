using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TenantManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.TenantManagement
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _ITenantRepo;
        public TenantService(ITenantRepository ITenantRepo)
        {
            _ITenantRepo = ITenantRepo;

        }
        public async Task<CommonResponseModel> AddTenant(AddTenantDto AddTenant)
        {
            return await _ITenantRepo.AddTenant(AddTenant);
        }

        public async Task<GetAllRecord<GetAllTenantsDto>> GetAllTenants()
        {
            return await _ITenantRepo.GetAllTenants();
        }
        public async Task<GetAllRecord<ModulelistDto>> GetAllModuleList()
        {
            return await _ITenantRepo.GetAllModuleList();
        }
        public Task<CommonResponseModel> DeleteTenants(int Id)
        {
            return _ITenantRepo.DeleteTenants(Id);
        }
        public async Task<CommonResponseModel> UpdateTenants(AddTenantDto UpdateTenants)
        {
            return await _ITenantRepo.UpdateTenants(UpdateTenants);
        }
        public async Task<CommonResponseModel> UpdateTenantModulesAsync(UpdateTenantModulesDto UpdateTenants)
        {
            return await _ITenantRepo.UpdateTenantModulesAsync(UpdateTenants);
        }
        public async Task<CommonResponseModel> ChangeTenants(int Id)
        {
            return await _ITenantRepo.ChangeTenants(Id);
        }

        public Task<CommonResponseModel> ForceLogout(int Id)
        {
            return _ITenantRepo.ForceLogout(Id);
        }
        public Task<CommonResponseModel> Suspend(int Id)
        {
            return _ITenantRepo.Suspend(Id);
        }
        public Task<GetAllRecord<ModulelistDto>> GetAllModuleAsync(int TenantId)
        {
            return _ITenantRepo.GetAllModuleAsync(TenantId);
        }

        public Task<GetSpecificRecord<TenantDashboardDto>> GetTenantDashboard(int TenantId)
        {
            return _ITenantRepo.GetTenantDashboard(TenantId);
        }

    }
}
