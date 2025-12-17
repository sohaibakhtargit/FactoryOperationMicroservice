using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement
{
    public class TechnicianAssignmentService : ITechnicianAssignmentService
    {
        private readonly ITechnicianAssignmentRepository _repository;
        public TechnicianAssignmentService(ITechnicianAssignmentRepository repository)
        {
            _repository = repository;
        }
        public Task<GetAllRecord<TechnicianAssignment_DispatchDto>> GetTechnicianDashboardSummaryAsync(int tenantId)

            => _repository.GetTechnicianDashboardSummaryAsync(tenantId);
        public Task<GetAllRecord<WorkOrdersRequiringAssignmentDto>> GetTechnicianWorkOrdersAsync(int tenantId)
            => _repository.GetTechnicianWorkOrdersAsync(tenantId);
        public Task<GetAllRecord<TechnicianDto>> GetTechniciansAsync(int tenantId)
            => _repository.GetTechniciansAsync(tenantId);
        public Task<GetAllRecord<AssignmentHistoryDto>> GetLatestAssignmentHistoryAsync(int tenantId)
            => _repository.GetLatestAssignmentHistoryAsync(tenantId);
        public Task<CommonResponseModel> AssignTechnicianAsync(AssignTechnicianUpdateWorkOrder dto)
            => _repository.AssignTechnicianAsync(dto);


    }
}
