using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class FactoryLocationService : IFactoryLocationService
    {
        private readonly IFactoryLocationRepository _repository;

        public FactoryLocationService(IFactoryLocationRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddLocationAsync(LocationDto dto) =>
            _repository.AddLocationAsync(dto);

        public Task<CommonResponseModel> UpdateLocationAsync(LocationDto dto) =>
            _repository.UpdateLocationAsync(dto);

        public Task<CommonResponseModel> DeleteLocationAsync(int TenantId, int LocationId) =>
            _repository.DeleteLocationAsync(TenantId, LocationId);

        public Task<GetSpecificRecord<LocationDto>> GetLocationByIdAsync(int tenantId, int locationId) =>
            _repository.GetLocationByIdAsync(tenantId, locationId);

        public Task<GetAllRecord<LocationDto>> GetAllLocationsAsync(int tenantId) =>
            _repository.GetAllLocationsAsync(tenantId);

        public Task<LocationResponse> GetLocationWithChildrenAsync(int tenantId, int selectedLocationId) =>
           _repository.GetLocationWithChildrenAsync(tenantId, selectedLocationId);

    }
}
