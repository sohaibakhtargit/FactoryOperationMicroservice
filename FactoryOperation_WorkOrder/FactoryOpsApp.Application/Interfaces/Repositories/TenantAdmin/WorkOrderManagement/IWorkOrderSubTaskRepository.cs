using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement
{
    public interface IWorkOrderSubTaskRepository
    {
        //Task<GetAllRecord<WorkOrderSubTaskDto>> GetAllWorkOrderSubTaskAsync(int tenantId);
        Task<GetAllRecord<WorkOrderWithSubTasksDto>> GetAllWorkOrderSubTaskAsync(int tenantId);

        Task<GetSpecificRecord<WorkOrderSubTaskDto>> GetWorkOrderSubTaskByIdAsync(int tenantId, int subTaskId);
        Task<CommonResponseModel> AddWorkOrderSubTaskAsync(CreateWorkOrderSubTaskDto dto);
        Task<CommonResponseModel> UpdateWorkOrderSubTaskAsync(UpdateWorkOrderSubTaskDto dto);
        Task<CommonResponseModel> DeleteWorkOrderSubTaskAsync(int tenantId, int subTaskId);
    }
}
