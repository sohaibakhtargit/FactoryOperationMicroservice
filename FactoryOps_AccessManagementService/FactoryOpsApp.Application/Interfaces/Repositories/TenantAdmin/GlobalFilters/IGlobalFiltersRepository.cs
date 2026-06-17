using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.GlobalFilters
{
    public interface IGlobalFiltersRepositories
    {
        Task<CommonResponseModel> CreateAsync(CreateFilterConfigurationDto dto);
        Task<GetAllRecord<GetFilterConfigurationDto>> GetAllAsync(int? roleId, int? tenantId, string? module, string? submodule);
        Task<CommonResponseModel> DeleteAsync(int pId, int tenantId);
    }
}
