using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.InventoryManagement
{
    public class InventoryCostIntegrationService:IInventoryCostIntegrationService
    {
        private readonly IInventoryCostIntegrationRepository _inventoryCostIntegrationRepository;        
            public InventoryCostIntegrationService(IInventoryCostIntegrationRepository inventoryCostIntegrationRepository)
        {
            _inventoryCostIntegrationRepository = inventoryCostIntegrationRepository;
        }
        public Task<InventoryCostSummaryResponse> GetInventoryCostsAsync(int tenantId)
            => _inventoryCostIntegrationRepository.GetInventoryCostsAsync(tenantId);
        public Task<GetSpecificRecord<InventoryCostIntegrationDto>> GetInventoryCostByIdAsync(int id, int tenantId)
            => _inventoryCostIntegrationRepository.GetInventoryCostByIdAsync(id, tenantId);
        public Task<CommonResponseModel> AddInventoryCostAsync(CreateInventoryCostIntegrationDto dto)
            => _inventoryCostIntegrationRepository.AddInventoryCostAsync(dto);
        public Task<CommonResponseModel> UpdateInventoryCostAsync(CreateInventoryCostIntegrationDto dto)
            => _inventoryCostIntegrationRepository.UpdateInventoryCostAsync(dto);
        public Task<CommonResponseModel> DeleteInventoryCostAsync(int id, int tenantId)
            => _inventoryCostIntegrationRepository.DeleteInventoryCostAsync(id, tenantId);
        public Task<GetAllRecord<InventoryItemInfoDto>> GetInventoryItemInfo(int tenantId)
            => _inventoryCostIntegrationRepository.GetInventoryItemInfo(tenantId);
        public Task<GetAllRecord<WorkOrderCostIntegrationDto>> GetWorkOrderIntegration(int tenantId)
            => _inventoryCostIntegrationRepository.GetWorkOrderIntegration(tenantId);


    }
}
