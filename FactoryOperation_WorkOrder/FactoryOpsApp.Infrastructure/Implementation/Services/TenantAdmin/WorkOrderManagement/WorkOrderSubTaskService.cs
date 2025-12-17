using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement
{
    public class WorkOrderSubTaskService : IWorkOrderSubTaskService
    {
        private readonly IWorkOrderSubTaskRepository _workOrderSubTaskRepository;

        public WorkOrderSubTaskService(IWorkOrderSubTaskRepository workOrderSubTaskRepository)
        {
            _workOrderSubTaskRepository = workOrderSubTaskRepository;
        }
        // public Task<GetAllRecord<WorkOrderSubTaskDto>> GetAllWorkOrderSubTaskAsync(int tenantId)
        public Task<GetAllRecord<WorkOrderWithSubTasksDto>> GetAllWorkOrderSubTaskAsync(int tenantId)
            => _workOrderSubTaskRepository.GetAllWorkOrderSubTaskAsync(tenantId);
        public Task<GetSpecificRecord<WorkOrderSubTaskDto>> GetWorkOrderSubTaskByIdAsync(int tenantId, int subTaskId)
            => _workOrderSubTaskRepository.GetWorkOrderSubTaskByIdAsync(tenantId, subTaskId);
        public Task<CommonResponseModel> AddWorkOrderSubTaskAsync(CreateWorkOrderSubTaskDto dto)
            => _workOrderSubTaskRepository.AddWorkOrderSubTaskAsync(dto);
        public Task<CommonResponseModel> UpdateWorkOrderSubTaskAsync(UpdateWorkOrderSubTaskDto dto)
            => _workOrderSubTaskRepository.UpdateWorkOrderSubTaskAsync(dto);
        public Task<CommonResponseModel> DeleteWorkOrderSubTaskAsync(int tenantId, int subTaskId)
            => _workOrderSubTaskRepository.DeleteWorkOrderSubTaskAsync(tenantId, subTaskId);
    }
}
