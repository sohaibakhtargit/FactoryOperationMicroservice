using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;

namespace FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.WorkOrderManagement
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IWorkOrderRepository _repository;
        public WorkOrderService(IWorkOrderRepository repository)
        {
            _repository = repository;
        }
        public Task<GetAllRecord<WorkOrderDto>> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum workOrderType)
            => _repository.GetWorkOrderAllAsync(tenantId, workOrderType);
        public Task<CommonResponseModel> CreateWorkOrderAsync(WorkOrderCreateDto dto)
                => _repository.CreateWorkOrderAsync(dto);

        public Task<GetSpecificRecord<WorkOrderDto>> GetWorkOrderByIdAsync(int WorkOrderId, int tenantId)
           => _repository.GetWorkOrderByIdAsync(WorkOrderId, tenantId);
        public Task<CommonResponseModel> UpdateWorkOrderAsync(WorkOrderUpdateDto dto)
             => _repository.UpdateWorkOrderAsync(dto);
        public Task<CommonResponseModel> UpdateWorkOrderProgressAsync(WorkOrderProgresssUpdateDto dto)
            => _repository.UpdateWorkOrderProgressAsync(dto);
       public Task<GetAllRecord<GetWorkOrderProgresssUpdateDto>> GetWorkOrderProgressAsync(int tenantId)
            => _repository.GetWorkOrderProgressAsync(tenantId);
        public Task<CommonResponseModel> DeleteWorkOrderAsync(int WorkOrderId, int tenantId)
               => _repository.DeleteWorkOrderAsync(WorkOrderId, tenantId);
        public Task<GetSpecificRecord<LaborAnalyticsDto>> GetLaborAnalyticsAsync(int tenantId) 
            => _repository.GetLaborAnalyticsAsync(tenantId);
        public Task<GetSpecificRecord<ResourceUsageAnalyticsDto>> GetResourceUsageAnalyticsAsync(int tenantId)
            => _repository.GetResourceUsageAnalyticsAsync(tenantId);
        public Task<GetSpecificRecord<LaborResourceAnalyticsDto>> GetLaborResourceAnalyticsAsync(int tenantId)
            => _repository.GetLaborResourceAnalyticsAsync(tenantId);
    }
}
