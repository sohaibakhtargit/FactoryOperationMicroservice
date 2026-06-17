using FactoryOperation_Asset.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;

namespace FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement
{
    public interface IAssetBOMService
    {
        Task<CommonResponseModel> AddBOMPart(AssetBOMDto dto);
        Task<CommonResponseModel> UpdateBOMPart(UpdateAssetBomDto dto);
        Task<CommonResponseModel> DeleteBOMPart(DeleteAssetBomDto dto);
        Task<GetAllRecord<GetAssetBOMDto>> GetBOMPart(int tenantId);
        Task<GetSpecificRecord<GetAssetBOMDto>> GetBOMById(int bomPartId, int tenantId);
    }
}
