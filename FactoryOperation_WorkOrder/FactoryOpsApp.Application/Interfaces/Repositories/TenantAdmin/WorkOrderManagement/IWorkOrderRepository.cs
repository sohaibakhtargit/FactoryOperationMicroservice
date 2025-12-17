using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using static FactoryOpsApp.Application.DTOs.WorkOrderCreateDto;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement
{
    public interface IWorkOrderRepository
    {
        Task<GetAllRecord<WorkOrderDto>> GetWorkOrderAllAsync(int tenantId, WorkOrderTypeEnum workOrderType);
        Task<CommonResponseModel> CreateWorkOrderAsync(WorkOrderCreateDto dto);
        Task<BulkWorkOrderImportResult> ImportBulkWorkOrdersAsync(BulkWorkOrderImportRequest request);

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
