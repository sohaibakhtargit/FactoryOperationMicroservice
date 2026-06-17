using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IServiceRequestRepository _repository;

        public ServiceRequestService(IServiceRequestRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonServiceRequestResponseModel> CreateServiceRequestAsync(ServiceRequestDto dto)
        {
            return _repository.CreateServiceRequestAsync(dto);
        }

        public Task<CommonResponseModel> UpdateServiceRequestAsync(ServiceRequestDto dto)
        {
            return _repository.UpdateServiceRequestAsync(dto);
        }

        public Task<CommonResponseModel> DeleteServiceRequestAsync(int serviceRequestId, int tenantId)
        {
            return _repository.DeleteServiceRequestAsync(serviceRequestId, tenantId);
        }

        public Task<CommonResponseModel> UpdateServiceRequestStatusAsync(ServiceRequestStatusUpdateDto dto)
        {
            return _repository.UpdateServiceRequestStatusAsync(dto);
        }

        public Task<GetAllRecord<GetServiceRequestDto>> GetAllServiceRequestsAsync(int tenantId)
        {
            return _repository.GetAllServiceRequestsAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetServiceRequestDto>> GetServiceRequestByIdAsync(int serviceRequestId, int tenantId)
        {
            return _repository.GetServiceRequestByIdAsync(serviceRequestId, tenantId);
        }

        public Task<GetAllRecord<GetServiceRequestDto>> GetServiceRequestsByStatusAsync(int tenantId, ServiceRequestStatus status)
        {
            return _repository.GetServiceRequestsByStatusAsync(tenantId, status);
        }

        public Task<GetAllRecord<GetServiceRequestDto>> GetOverdueServiceRequestsAsync(int tenantId)
        {
            return _repository.GetOverdueServiceRequestsAsync(tenantId);
        }

        public Task<CommonResponseModel> ApproveServiceRequestAsync(ApproveServiceRequestDto dto)
        {
            return _repository.ApproveServiceRequestAsync(dto);
        }
        public Task<CommonResponseModel> RejectServiceRequestAsync(RejectServiceRequestDto dto)
        {
            return _repository.RejectServiceRequestAsync(dto);
        }

        public Task<CommonResponseModel> AssignServiceRequestAsync(AssignServiceRequestDto dto)
        {
            return _repository.AssignServiceRequestAsync(dto);
        }
        public Task<CommonResponseModel> ReopenServiceRequestAsync(ServiceRequestDto dto)
        {
            return _repository.ReopenServiceRequestAsync(dto);
        }

        public Task<CommonResponseModel> UploadServiceRequestMediaAsync(ServiceRequestMediaDto dto)
        {
           return _repository.UploadServiceRequestMediaAsync(dto);
        }

    }
}
