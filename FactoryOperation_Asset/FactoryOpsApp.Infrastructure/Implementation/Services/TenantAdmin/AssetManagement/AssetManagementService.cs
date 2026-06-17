using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.Repository.TenantAdmin.AssetManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.AssetManagement
{
    public class AssetManagementService : IAssetManagementService
    {
        private readonly IAssetManagementRepository _repository;

        public AssetManagementService(IAssetManagementRepository assetManagementRepository)
        {
            _repository = assetManagementRepository;
        }
        public Task<CommonResponseModel> AddAsset(AssetRegistryDto dto) 
            => _repository.AddAsset(dto);
        public Task<BulkAssetImportResult> ImportBulkAssetAsync(BulkAssetImportRequest request) 
            => _repository.ImportBulkAssetAsync(request);
        public Task<CommonResponseModel> UpdateAsset(AssetRegistryDto dto) 
            => _repository.UpdateAsset(dto);

        public GetAllRecord<GetAssetRegistryDto> GetAllAssets(int TenantId) 
            => _repository.GetAllAssets(TenantId);

        public Task<CommonResponseModel> DeleteAsset(int id, int TenantId) => _repository.DeleteAsset(id, TenantId);
        public Task<CommonResponseModel> BulkAssetDeleteAsync(int tenantId, List<int> AssetId)
            => _repository.BulkAssetDeleteAsync(tenantId, AssetId);
        public async Task<(byte[] fileBytes, string fileName)> DownloadAssetBillingPdf(int tenantId, int assetId)
            =>  await _repository.GenerateAssetBillingPdf(tenantId, assetId);
       
    }
}
