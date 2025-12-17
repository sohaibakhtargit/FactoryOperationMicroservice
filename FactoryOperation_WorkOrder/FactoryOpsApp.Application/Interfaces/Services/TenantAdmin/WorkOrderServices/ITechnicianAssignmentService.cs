using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices
{
    public interface ITechnicianAssignmentService
    {
        Task<GetAllRecord<TechnicianAssignment_DispatchDto>> GetTechnicianDashboardSummaryAsync(int tenantId);
        Task<GetAllRecord<WorkOrdersRequiringAssignmentDto>> GetTechnicianWorkOrdersAsync(int tenantId);
        Task<GetAllRecord<TechnicianDto>> GetTechniciansAsync(int tenantId);
        Task<GetAllRecord<AssignmentHistoryDto>> GetLatestAssignmentHistoryAsync(int tenantId);
        Task<CommonResponseModel> AssignTechnicianAsync(AssignTechnicianUpdateWorkOrder dto);
    }
}
