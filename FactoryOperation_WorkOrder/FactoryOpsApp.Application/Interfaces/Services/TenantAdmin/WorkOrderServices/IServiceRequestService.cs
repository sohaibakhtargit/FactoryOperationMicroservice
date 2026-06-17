using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices
{
    public interface IServiceRequestService
    {
        Task<CommonServiceRequestResponseModel> CreateServiceRequestAsync(ServiceRequestDto dto);
        Task<CommonResponseModel> UpdateServiceRequestAsync(ServiceRequestDto dto);
        Task<CommonResponseModel> DeleteServiceRequestAsync(int serviceRequestId, int tenantId);
        Task<CommonResponseModel> UpdateServiceRequestStatusAsync(ServiceRequestStatusUpdateDto dto);
        Task<GetAllRecord<GetServiceRequestDto>> GetAllServiceRequestsAsync(int tenantId);
        Task<GetSpecificRecord<GetServiceRequestDto>> GetServiceRequestByIdAsync(int serviceRequestId, int tenantId);
        Task<GetAllRecord<GetServiceRequestDto>> GetServiceRequestsByStatusAsync(int tenantId, ServiceRequestStatus status);
        Task<GetAllRecord<GetServiceRequestDto>> GetOverdueServiceRequestsAsync(int tenantId);

        Task<CommonResponseModel> ApproveServiceRequestAsync(ApproveServiceRequestDto dto);
        Task<CommonResponseModel> RejectServiceRequestAsync(RejectServiceRequestDto dto);
        Task<CommonResponseModel> AssignServiceRequestAsync(AssignServiceRequestDto dto);
        Task<CommonResponseModel> ReopenServiceRequestAsync(ServiceRequestDto dto);
        Task<CommonResponseModel> UploadServiceRequestMediaAsync(ServiceRequestMediaDto dto);

    }
}
