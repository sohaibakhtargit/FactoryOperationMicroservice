using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement
{
    public interface IAssetManagementRepository
    {
        Task<CommonResponseModel> AddAsset(AssetRegistryDto dto);
        Task<CommonResponseModel> UpdateAsset(AssetRegistryDto dto);
        GetAllRecord<GetAssetRegistryDto> GetAllAssets(int tenantId);
        Task<CommonResponseModel> DeleteAsset(int id, int tenantId);
    }
}
