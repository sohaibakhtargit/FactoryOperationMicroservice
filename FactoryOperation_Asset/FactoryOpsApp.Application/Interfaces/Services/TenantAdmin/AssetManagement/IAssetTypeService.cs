using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement
{
    public interface IAssetTypeService
    {
        Task<CommonResponseModel> CreateAssetTypeAsync(CreateAssetTypeDto dto);
        Task<CommonResponseModel> UpdateAssetTypeAsync(UpdateAssetTypeDto dto);
        Task<CommonResponseModel> DeleteAssetTypeAsync(int id, int tenantId);
        Task<GetAllRecord<AssetTypeResponseDto>> GetAllAssetTypesAsync(int tenantId);
        Task<AssetTypeResponseDto?> GetAssetTypeByIdAsync(int id, int tenantId);
    }
}
