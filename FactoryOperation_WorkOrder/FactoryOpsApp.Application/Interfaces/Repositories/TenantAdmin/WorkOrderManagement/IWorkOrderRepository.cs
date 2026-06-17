using FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using System;
using static FactoryOperation_WorkOrder.FactoryOpsApp.Application.DTOs.TechnicianLoadDto;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement
{
    public interface IWorkOrderRepository
    {
        Task<GetAllRecord<WorkOrderDto>> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum? workOrderType);
        Task<CommonWorkOrderResponseModel> CreateWorkOrderAsync(WorkOrderCreateDto dto);
        Task<CommonResponseModel> UploadWorkOrderMediaAsync(WorkOrderProgressMediaDto dto);
        Task<BulkWorkOrderImportResult> ImportBulkWorkOrdersAsync(BulkWorkOrderImportRequest request);
        Task<GetSpecificRecord<WorkOrderDto>> GetWorkOrderByIdAsync(int WorkOrderId, int tenantId);
        Task<CommonResponseModel> UpdateWorkOrderAsync(WorkOrderUpdateDto dto);
        Task<CommonResponseModel> UpdateWorkOrderProgressAsync(WorkOrderProgresssUpdateDto dto);
        Task<CommonResponseModel> ApproveRejectWorkOrderAsync(WorkOrderApprovalDto dto);
        Task<GetAllRecord<GetWorkOrderProgresssUpdateDto>> GetWorkOrderProgressAsync(int tenantId, int? userId);
        Task<CommonResponseModel> DeleteWorkOrderAsync(WorkOrderDeleteDto dto);
        Task<CommonResponseModel> BulkWorkOrderDelete(WorkOrderBulkDeleteDto dto);
        Task<GetSpecificRecord<LaborAnalyticsDto>> GetLaborAnalyticsAsync(int tenantId);
        Task<GetSpecificRecord<ResourceUsageAnalyticsDto>> GetResourceUsageAnalyticsAsync(int tenantId);
        Task<GetSpecificRecord<LaborResourceAnalyticsDto>> GetLaborResourceAnalyticsAsync(int tenantId);
        Task<GetAllRecord<InventoryItemInfoDto>> GetInventoryItemInfo(int tenantId);
        Task<GetAllRecord<WorkOrderCostIntegrationDto>> GetWorkOrderIntegration(int tenantId);
        Task<CostReportDto> GetCostReportAsync(int tenantId);
        Task <GetAllRecord<WorkOrderPartUsageDto>> GetWorkOrderPartUsageAsync(int tenantId);
        Task<GetSpecificRecord<WorkOrderDashboardDto>> GetWorkOrderDashboardAsync(int tenantId);
        Task<GetAllRecord<WorkOrderTimelineDto>> GetWorkOrderTimelineAsync(int tenantId, int? userId = null);
        Task<GetAllRecord<RecentWorkOrderUpdateDto>> GetRecentWorkOrderUpdatesAsync(int tenantId, int? userId, int? workorderId);
        Task<CommonResponseModel> UpdateWorkOrderCalendarAsync(WorkOrderCalendarUpdateDto dto);
        

        // Calendar API - aggregate scheduled, unscheduled and technician loads.
        //ask<GetSpecificRecord<CalendarDataDto>> GetCalendarAsync(int tenantId, DateTime? from = null, DateTime? to = null);
    }
}
