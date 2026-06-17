using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement
{
    public interface IAssetManagementRepository
    {
        Task<CommonResponseModel> AddAsset(AssetRegistryDto dto);
        Task<BulkAssetImportResult> ImportBulkAssetAsync(BulkAssetImportRequest request);
        Task<CommonResponseModel> UpdateAsset(AssetRegistryDto dto);
        GetAllRecord<GetAssetRegistryDto> GetAllAssets(int tenantId);
        Task<CommonResponseModel> DeleteAsset(int id, int tenantId);
        Task<CommonResponseModel> BulkAssetDeleteAsync(int tenantId, List<int> AssetId);
        Task<(byte[] fileBytes, string fileName)> GenerateAssetBillingPdf(int tenantId, int assetId);
    }
}
