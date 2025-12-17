// Application/Interfaces/Services/IAssetDocumentService.cs
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement
{
    public interface IAssetDocumentService
    {
        Task<CommonResponseModel> AddAssetDocumentAsync(CreateAssetDocumentDto newDocument);
        Task<CommonResponseModel> UpdateAssetDocument(UpdateAssetDocumentDto dto);
        GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocuments(int TenantId);
        GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsByAssetId(int tenantId, int assetId);

        GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsUrl(int TenantId);

        GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocumentsCompliance(int TenantId);
        Task<CommonResponseModel> DeleteAssetDocument(int id, int TenantId);

    }
}