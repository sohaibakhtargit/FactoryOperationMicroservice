using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement
{
    public interface IServiceRequestRepository
    {
        Task<CommonResponseModel> CreateServiceRequestAsync(ServiceRequestDto dto);
        Task<CommonResponseModel> UpdateServiceRequestAsync(ServiceRequestDto dto);
        Task<CommonResponseModel> DeleteServiceRequestAsync(int serviceRequestId, int tenantId);
        Task<CommonResponseModel> UpdateServiceRequestStatusAsync(ServiceRequestStatusUpdateDto dto);
        Task<GetAllRecord<GetServiceRequestDto>> GetAllServiceRequestsAsync(int tenantId);
        Task<GetSpecificRecord<GetServiceRequestDto>> GetServiceRequestByIdAsync(int serviceRequestId, int tenantId);
        Task<GetAllRecord<GetServiceRequestDto>> GetServiceRequestsByStatusAsync(int tenantId, ServiceRequestStatus status);
        Task<GetAllRecord<GetServiceRequestDto>> GetOverdueServiceRequestsAsync(int tenantId);
    }
}
