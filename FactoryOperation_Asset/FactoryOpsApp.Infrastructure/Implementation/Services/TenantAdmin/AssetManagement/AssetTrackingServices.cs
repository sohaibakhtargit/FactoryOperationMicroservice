using FactoryOpsApp.Application.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.AssetManagement
{
    public class AssetTrackingServices : IAssetTrackingServices
    {
        private readonly IAsssetTrackingRepository _repository;
        public AssetTrackingServices(IAsssetTrackingRepository repository)
        {
            _repository = repository;
        }
        public Task<CommonResponseModel> CreateAssetTrackingAsync(AssetTrackingCreateDto dto)
            => _repository.CreateAssetTrackingAsync(dto);

        public Task<CommonResponseModel> UpdateAssetTrackingAsync(AssetTrackingUpdateDto dto)
            => _repository.UpdateAssetTrackingAsync(dto);

        public Task<CommonResponseModel> DeleteAssetTrackingAsync(long trackingId, int tenantId)
            => _repository.DeleteAssetTrackingAsync(trackingId, tenantId);

        public Task<GetAllRecord<AssetTrackingDto>> GetAllAssetTrackingsAsync(int tenantId)
            => _repository.GetAllAssetTrackingsAsync(tenantId);

        public Task<GetSpecificRecord<AssetTrackingDto>> GetAssetTrackingByIdAsync(long trackingId, int tenantId)
            => _repository.GetAssetTrackingByIdAsync(trackingId, tenantId);

        public Task<GetAllRecord<AssetTrackingDto>> GetLatestAssetTrackingsAsync(int tenantId, int count = 3)
            => _repository.GetLatestAssetTrackingsAsync(tenantId, count);

    }

}
