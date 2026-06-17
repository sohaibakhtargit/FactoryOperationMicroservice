using FactoryOperation_Asset.FactoryOpsApp.Application.DTOs;
using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Common;

namespace FactoryOperation_Asset.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.AssetManagement
{
    public class AssetBOMService: IAssetBOMService
    {
        private readonly IAssetBOMRepository _repository;

        public AssetBOMService(IAssetBOMRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddBOMPart(AssetBOMDto dto)
            => _repository.AddBOMPart(dto);

        public Task<CommonResponseModel> UpdateBOMPart(UpdateAssetBomDto dto)
            => _repository.UpdateBOMPart(dto);

        public Task<CommonResponseModel> DeleteBOMPart(DeleteAssetBomDto dto)
            => _repository.DeleteBOMPart(dto);

        public Task<GetAllRecord<GetAssetBOMDto>> GetBOMPart(int tenantId)
            => _repository.GetBOMPart(tenantId);

        public Task<GetSpecificRecord<GetAssetBOMDto>> GetBOMById(int bomPartId, int tenantId)
            => _repository.GetBOMById(bomPartId, tenantId);

    }
}
