using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.InventoryManagement
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> CreateAsync(InventoryDto dto) => _repository.CreateAsync(dto);
        public Task<GetAllRecord<InventoryDto>> GetAllAsync(int tenantId) => _repository.GetAllAsync(tenantId);
        public Task<GetSpecificRecord<InventoryDto>> GetByIdAsync(int id, int tenantId) => _repository.GetByIdAsync(id, tenantId);
        public Task<GetSpecificRecord<StockTrackingSummaryDto>> GetStockTrackingAsync(int tenantId) => _repository.GetStockTrackingAsync(tenantId);
        public Task<GetSpecificRecord<StockReservationSummaryDto>> GetStockReservationsAsync(int tenantId) => _repository.GetStockReservationsAsync(tenantId);
        public Task<GetSpecificRecord<SerialBatchSummaryDto>> GetSerialBatchTrackingAsync(int tenantId) => _repository.GetSerialBatchTrackingAsync(tenantId);
        public Task<GetSpecificRecord<StockTrackingResponseDto>> GetStockTrackingSummaryAsync(int tenantId, StockTrackingType type) => _repository.GetStockTrackingSummaryAsync(tenantId, type);
        public Task<CommonResponseModel> UpdateAsync(InventoryDto dto) => _repository.UpdateAsync(dto);
        public Task<CommonResponseModel> DeleteAsync(int id, int tenantId) => _repository.DeleteAsync(id, tenantId);
    }
}
