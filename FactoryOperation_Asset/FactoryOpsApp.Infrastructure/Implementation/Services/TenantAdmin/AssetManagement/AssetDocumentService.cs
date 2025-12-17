// Application/Services/AssetDocumentService.cs
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.AssetManagement
{
    public class AssetDocumentService : IAssetDocumentService
    {
        private readonly IAssetDocumentRepository _assetDocumentRepository;
        private readonly IFileStorageService _fileStorageService;

        public AssetDocumentService(
            IAssetDocumentRepository assetDocumentRepository,
            IFileStorageService fileStorageService)
        {
            _assetDocumentRepository = assetDocumentRepository;
            _fileStorageService = fileStorageService;
        }
        
        public Task<CommonResponseModel> AddAssetDocumentAsync(CreateAssetDocumentDto dto)
        => _assetDocumentRepository.AddAssetDocumentAsync(dto);

        public GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocuments(int tenantId)
        => _assetDocumentRepository.GetAllAssetDocuments(tenantId);

        public GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsByAssetId(int tenantId, int assetId)
   => _assetDocumentRepository.GetAssetDocumentsByAssetId(tenantId, assetId);

        public GetAllRecord<AssetDocumentResponseDto> GetAssetDocumentsUrl(int tenantId)
       => _assetDocumentRepository.GetAssetDocumentsUrl(tenantId);

        public GetAllRecord<AssetDocumentResponseDto> GetAllAssetDocumentsCompliance(int tenantId)
    => _assetDocumentRepository.GetAllAssetDocumentsCompliance(tenantId);

        public Task<CommonResponseModel> UpdateAssetDocument(UpdateAssetDocumentDto dto) =>
            _assetDocumentRepository.UpdateAssetDocument(dto);


        public Task<CommonResponseModel> DeleteAssetDocument(int id, int TenantId) =>
            _assetDocumentRepository.DeleteAssetDocument(id, TenantId);
    }
}