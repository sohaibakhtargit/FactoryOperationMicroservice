using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement
{
    public interface IInventoryRepository
    {
        Task<CommonResponseModel> CreateAsync(InventoryDto dto);
        Task<GetAllRecord<InventoryDto>> GetAllAsync(int tenantId);
        Task<GetSpecificRecord<StockTrackingSummaryDto>> GetStockTrackingAsync(int tenantId);
        Task<GetSpecificRecord<SerialBatchSummaryDto>> GetSerialBatchTrackingAsync(int tenantId);
        Task<GetSpecificRecord<StockReservationSummaryDto>> GetStockReservationsAsync(int tenantId);
        Task<GetSpecificRecord<StockTrackingResponseDto>> GetStockTrackingSummaryAsync(int tenantId, StockTrackingType type);
        Task<GetSpecificRecord<InventoryDto>> GetByIdAsync(int id, int tenantId);
        Task<CommonResponseModel> UpdateAsync(InventoryDto dto);
        Task<CommonResponseModel> DeleteAsync(int id, int tenantId);
    }
}
