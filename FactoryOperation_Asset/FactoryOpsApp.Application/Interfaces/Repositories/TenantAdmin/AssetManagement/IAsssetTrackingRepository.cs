using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement
{
    public interface IAsssetTrackingRepository
    {
        Task<CommonResponseModel> CreateAssetTrackingAsync(AssetTrackingCreateDto dto);
        Task<CommonResponseModel> UpdateAssetTrackingAsync(AssetTrackingUpdateDto dto);
        Task<CommonResponseModel> DeleteAssetTrackingAsync(long trackingId, int tenantId);
        Task<GetAllRecord<AssetTrackingDto>> GetAllAssetTrackingsAsync(int tenantId);
        Task<GetSpecificRecord<AssetTrackingDto>> GetAssetTrackingByIdAsync(long trackingId, int tenantId);
        Task<GetAllRecord<AssetTrackingDto>> GetLatestAssetTrackingsAsync(int tenantId, int count = 3);

    }
}
