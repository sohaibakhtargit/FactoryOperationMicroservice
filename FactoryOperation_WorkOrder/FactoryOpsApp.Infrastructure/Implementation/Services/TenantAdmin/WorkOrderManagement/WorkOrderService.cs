using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs.TechnicianLoadDto;
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
        public Task<GetAllRecord<WorkOrderDto>> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum? workOrderType)
            => _repository.GetWorkOrderAllAsync(tenantId, workOrderType);
        public Task<CommonWorkOrderResponseModel> CreateWorkOrderAsync(WorkOrderCreateDto dto)
                => _repository.CreateWorkOrderAsync(dto);
        public Task<CommonResponseModel> UploadWorkOrderMediaAsync(WorkOrderProgressMediaDto dto)
                => _repository.UploadWorkOrderMediaAsync(dto);
        public Task<BulkWorkOrderImportResult> ImportBulkWorkOrdersAsync(BulkWorkOrderImportRequest request)
                => _repository.ImportBulkWorkOrdersAsync(request);

        public Task<GetSpecificRecord<WorkOrderDto>> GetWorkOrderByIdAsync(int WorkOrderId, int tenantId)
           => _repository.GetWorkOrderByIdAsync(WorkOrderId, tenantId);
        public Task<CommonResponseModel> UpdateWorkOrderAsync(WorkOrderUpdateDto dto)
             => _repository.UpdateWorkOrderAsync(dto);
        public Task<CommonResponseModel> UpdateWorkOrderProgressAsync(WorkOrderProgresssUpdateDto dto)
            => _repository.UpdateWorkOrderProgressAsync(dto);
        public Task<CommonResponseModel> ApproveRejectWorkOrderAsync(WorkOrderApprovalDto dto)
            => _repository.ApproveRejectWorkOrderAsync(dto);
        public Task<GetAllRecord<GetWorkOrderProgresssUpdateDto>> GetWorkOrderProgressAsync(int tenantId, int? userId)
            => _repository.GetWorkOrderProgressAsync(tenantId, userId);
        public Task<CommonResponseModel> DeleteWorkOrderAsync(WorkOrderDeleteDto dto)
               => _repository.DeleteWorkOrderAsync(dto);
        public Task<CommonResponseModel> BulkWorkOrderDelete(WorkOrderBulkDeleteDto dto)
            => _repository.BulkWorkOrderDelete(dto);
        public Task<GetSpecificRecord<LaborAnalyticsDto>> GetLaborAnalyticsAsync(int tenantId) 
            => _repository.GetLaborAnalyticsAsync(tenantId);
        public Task<GetSpecificRecord<ResourceUsageAnalyticsDto>> GetResourceUsageAnalyticsAsync(int tenantId)
            => _repository.GetResourceUsageAnalyticsAsync(tenantId);
        public Task<GetSpecificRecord<LaborResourceAnalyticsDto>> GetLaborResourceAnalyticsAsync(int tenantId)
            => _repository.GetLaborResourceAnalyticsAsync(tenantId);
        public Task<GetAllRecord<InventoryItemInfoDto>> GetInventoryItemInfo(int tenantId)
           => _repository.GetInventoryItemInfo(tenantId);
        public Task<GetAllRecord<WorkOrderCostIntegrationDto>> GetWorkOrderIntegration(int tenantId)
            => _repository.GetWorkOrderIntegration(tenantId);

        public async Task<CostReportDto> GetCostReportAsync(int tenantId)
        {
            return await _repository.GetCostReportAsync(tenantId);
        }
        public Task<GetAllRecord<WorkOrderPartUsageDto>> GetWorkOrderPartUsageAsync(int tenantId)
            => _repository.GetWorkOrderPartUsageAsync(tenantId);
        public Task<GetAllRecord<WorkOrderTimelineDto>> GetWorkOrderTimelineAsync(int tenantId, int? userId = null)
            => _repository.GetWorkOrderTimelineAsync(tenantId, userId);
        public Task<GetAllRecord<RecentWorkOrderUpdateDto>> GetRecentWorkOrderUpdatesAsync(int tenantId, int? userId, int? workorderId)
            => _repository.GetRecentWorkOrderUpdatesAsync(tenantId, userId, workorderId);


        public Task<GetSpecificRecord<WorkOrderDashboardDto>> GetWorkOrderDashboardAsync(int tenantId)
         => _repository.GetWorkOrderDashboardAsync(tenantId);

        public async Task<CommonResponseModel> UpdateWorkOrderCalendarAsync(WorkOrderCalendarUpdateDto dto)
        {
            return await _repository.UpdateWorkOrderCalendarAsync(dto);
        }

    }
}
