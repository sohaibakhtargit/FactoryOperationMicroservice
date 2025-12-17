using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement
{
    public interface IInventoryCostIntegrationRepository
    {
        Task<InventoryCostSummaryResponse> GetInventoryCostsAsync(int tenantId);
        Task<GetSpecificRecord<InventoryCostIntegrationDto>> GetInventoryCostByIdAsync(int id, int tenantId);

        Task<GetAllRecord<InventoryItemInfoDto>> GetInventoryItemInfo(int tenantId);
        Task<GetAllRecord<WorkOrderCostIntegrationDto>> GetWorkOrderIntegration(int tenantId);
        Task<CommonResponseModel> AddInventoryCostAsync(CreateInventoryCostIntegrationDto dto);
        Task<CommonResponseModel> UpdateInventoryCostAsync(CreateInventoryCostIntegrationDto dto);
        Task<CommonResponseModel> DeleteInventoryCostAsync(int id, int tenantId);
    }
}
