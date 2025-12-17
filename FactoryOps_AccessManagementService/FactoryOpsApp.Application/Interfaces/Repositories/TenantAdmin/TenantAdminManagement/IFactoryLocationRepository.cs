using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement
{
    public interface IFactoryLocationRepository
    {
        Task<CommonResponseModel> AddLocationAsync(LocationDto dto);
        Task<CommonResponseModel> UpdateLocationAsync(LocationDto dto);
        Task<CommonResponseModel> DeleteLocationAsync(int TenantId, int LocationId);
        Task<GetSpecificRecord<LocationDto>> GetLocationByIdAsync(int tenantId, int locationId);
        Task<GetAllRecord<LocationDto>> GetAllLocationsAsync(int tenantId);
        Task<LocationResponse> GetLocationWithChildrenAsync(int tenantId, int selectedLocationId);
    }
}
