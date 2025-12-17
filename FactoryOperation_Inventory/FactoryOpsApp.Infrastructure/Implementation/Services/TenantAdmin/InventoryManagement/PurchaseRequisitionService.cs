using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.InventoryManagement
{
    public class PurchaseRequisitionService : IPurchaseRequisitionService
    {
        private readonly IPurchaseRequisitionRepository _purchaseRequisitionRepository;

        public PurchaseRequisitionService(IPurchaseRequisitionRepository purchaseRequisitionRepository)
        {
            _purchaseRequisitionRepository = purchaseRequisitionRepository;
        }

        public Task<GetAllRecord<PurchaseRequisitionResponseDto>> GetAllAsync(int tenantId)
            => _purchaseRequisitionRepository.GetAllAsync(tenantId);

        public Task<GetSpecificRecord<PurchaseRequisitionResponseDto>> GetByIdAsync(int tenantId, int id)
            => _purchaseRequisitionRepository.GetByIdAsync(tenantId, id);
        public Task<CommonResponseModel> CreatePurchaseRequestAsync(PurchaseRequest Dto)
          => _purchaseRequisitionRepository.CreatePurchaseRequestAsync(Dto);
        public Task<CommonResponseModel> UpdatePurchaseRequestAsync(PurchaseRequest dto)
            => _purchaseRequisitionRepository.UpdatePurchaseRequestAsync(dto);
    }
}
