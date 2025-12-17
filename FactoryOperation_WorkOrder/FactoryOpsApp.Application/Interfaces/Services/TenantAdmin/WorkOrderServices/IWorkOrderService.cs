using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.WorkOrderManagement;
using Microsoft.AspNetCore.Mvc;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement
{
    public interface IWorkOrderService
    {
        Task<GetAllRecord<WorkOrderDto>> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum workOrderType);
        Task<CommonResponseModel> CreateWorkOrderAsync(WorkOrderCreateDto dto);
 
        Task <BulkWorkOrderImportResult> ImportBulkWorkOrdersAsync(BulkWorkOrderImportRequest request);
        Task<GetSpecificRecord<WorkOrderDto>> GetWorkOrderByIdAsync(int WorkOrderId, int tenantId);
        Task<CommonResponseModel> UpdateWorkOrderAsync(WorkOrderUpdateDto dto);
        Task<CommonResponseModel> UpdateWorkOrderProgressAsync(WorkOrderProgresssUpdateDto dto);
        Task<GetAllRecord<GetWorkOrderProgresssUpdateDto>> GetWorkOrderProgressAsync(int tenantId);
        Task<CommonResponseModel> DeleteWorkOrderAsync(int WorkOrderId, int tenantId);
        Task<GetSpecificRecord<LaborAnalyticsDto>> GetLaborAnalyticsAsync(int tenantId);
        Task<GetSpecificRecord<ResourceUsageAnalyticsDto>> GetResourceUsageAnalyticsAsync(int tenantId);
        Task<GetSpecificRecord<LaborResourceAnalyticsDto>> GetLaborResourceAnalyticsAsync(int tenantId);
    }
}
