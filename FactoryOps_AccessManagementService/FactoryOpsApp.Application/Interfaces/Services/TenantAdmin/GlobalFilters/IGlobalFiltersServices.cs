using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.GlobalFilters
{
    public interface IGlobalFiltersServices
    {
        Task<CommonResponseModel> CreateAsync(CreateFilterConfigurationDto dto);
        Task<GetAllRecord<GetFilterConfigurationDto>> GetAllAsync(int? roleId, int? tenantId, string? module, string? submodule);
        Task<CommonResponseModel> DeleteAsync(int pId, int tenantId);
    }
}
