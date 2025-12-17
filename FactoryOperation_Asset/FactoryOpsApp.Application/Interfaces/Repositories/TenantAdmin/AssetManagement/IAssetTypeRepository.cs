using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement
{
    public interface IAssetTypeRepository
    {
        Task<CommonResponseModel> CreateAssetTypeAsync(CreateAssetTypeDto dto);
        Task<CommonResponseModel> UpdateAssetTypeAsync(UpdateAssetTypeDto dto);
        Task<CommonResponseModel> DeleteAssetTypeAsync(int id, int tenantId);
        Task<GetAllRecord<AssetTypeResponseDto>> GetAllAssetTypesAsync(int tenantId);
        Task<AssetTypeResponseDto?> GetAssetTypeByIdAsync(int id, int tenantId);
    }
}
