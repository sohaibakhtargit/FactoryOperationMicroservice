using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance
{
    public interface IMaintenanceTaskRepository
    {
        Task<CommonResponseModel> AddMaintenanceTaskAsync(MaintenanceTaskDto dto);
        Task<CommonResponseModel> UpdateMaintenanceTaskAsync(MaintenanceTaskDto dto);
        Task<CommonResponseModel> DeleteMaintenanceTaskAsync(int taskId, int tenantId);
        Task<CommonResponseModel> UpdateTaskStatusAsync(TaskStatusUpdateDto dto);
        Task<CommonResponseModel> VerifyTaskAsync(TaskVerificationDto dto);
        Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksByWorkOrderAsync(int workOrderId, int tenantId);
        Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksByStatusAsync(int tenantId);
        Task<GetSpecificRecord<GetMaintenanceTaskDto>> GetMaintenanceTaskByIdAsync(int taskId, int tenantId);
        Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksRequiringVerificationAsync(int tenantId);
    }
}