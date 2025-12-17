using System.Threading.Tasks;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.AssetManagement
{
    public class AssetTypeService : IAssetTypeService
    {
        private readonly IAssetTypeRepository _repository;

        public AssetTypeService(IAssetTypeRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> CreateAssetTypeAsync(CreateAssetTypeDto dto)
            => _repository.CreateAssetTypeAsync(dto);

        public Task<CommonResponseModel> UpdateAssetTypeAsync(UpdateAssetTypeDto dto)
            => _repository.UpdateAssetTypeAsync(dto);

        public Task<CommonResponseModel> DeleteAssetTypeAsync(int id, int tenantId)
            => _repository.DeleteAssetTypeAsync(id, tenantId);

        public Task<GetAllRecord<AssetTypeResponseDto>> GetAllAssetTypesAsync(int tenantId)
            => _repository.GetAllAssetTypesAsync(tenantId);

        public Task<AssetTypeResponseDto?> GetAssetTypeByIdAsync(int id, int tenantId)
            => _repository.GetAssetTypeByIdAsync(id, tenantId);
    }
}
