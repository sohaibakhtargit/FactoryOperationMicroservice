// Application/Interfaces/Repositories/IAssetDocumentRepository.cs
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement
{
    public interface IAssetDocumentRepository
    {
        GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocuments(int tenantId);
        GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsByAssetId(int tenantId, int assetId);
        GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsUrl(int tenantId);
        GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocumentsCompliance(int tenantId);
        Task<CommonResponseModel> AddAssetDocumentAsync(CreateAssetDocumentDto dto);
        Task<CommonResponseModel> UpdateAssetDocument(UpdateAssetDocumentDto dto);
        Task<CommonResponseModel> DeleteAssetDocument(int id, int tenantId);
    }

}