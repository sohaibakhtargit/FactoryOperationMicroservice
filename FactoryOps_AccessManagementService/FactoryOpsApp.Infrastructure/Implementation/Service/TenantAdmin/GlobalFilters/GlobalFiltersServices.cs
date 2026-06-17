using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.GlobalFilters;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.GlobalFilters;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.GlobalFilters
{
    public class GlobalFiltersServices : IGlobalFiltersServices
    {
        private readonly IGlobalFiltersRepositories _globalFiltersRepository;

        public GlobalFiltersServices(IGlobalFiltersRepositories globalFiltersRepository)
        {
            _globalFiltersRepository = globalFiltersRepository;
        }

        public Task<CommonResponseModel> CreateAsync(CreateFilterConfigurationDto dto)
        {
            return _globalFiltersRepository.CreateAsync(dto);
        }

        public Task<GetAllRecord<GetFilterConfigurationDto>> GetAllAsync(int? roleId, int? tenantId, string? module, string? submodule)
        {
            return _globalFiltersRepository.GetAllAsync(roleId, tenantId, module, submodule);
        }

        public Task<CommonResponseModel> DeleteAsync(int pId, int tenantId)
        {
            return _globalFiltersRepository.DeleteAsync(pId, tenantId);
        }
    }
}
