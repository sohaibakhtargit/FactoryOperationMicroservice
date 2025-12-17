using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement
{
    public interface IAssetTrackingServices
    {

        Task<CommonResponseModel> CreateAssetTrackingAsync(AssetTrackingCreateDto dto);
        Task<CommonResponseModel> UpdateAssetTrackingAsync(AssetTrackingUpdateDto dto);
        Task<CommonResponseModel> DeleteAssetTrackingAsync(long trackingId, int tenantId);
        Task<GetAllRecord<AssetTrackingDto>> GetAllAssetTrackingsAsync(int tenantId);
        Task<GetSpecificRecord<AssetTrackingDto>> GetAssetTrackingByIdAsync(long trackingId, int tenantId);
        Task<GetAllRecord<AssetTrackingDto>> GetLatestAssetTrackingsAsync(int tenantId, int count = 3);

    }
}
