using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement
{
    public interface IPurchaseRequisitionRepository
    {
        Task<GetAllRecord<PurchaseRequisitionResponseDto>> GetAllAsync(int tenantId);
        Task<GetSpecificRecord<PurchaseRequisitionResponseDto>> GetByIdAsync(int tenantId, int id);
        Task<CommonResponseModel> CreatePurchaseRequestAsync(PurchaseRequest Dto);
        Task<CommonResponseModel> UpdatePurchaseRequestAsync(PurchaseRequest dto);

    }
}
