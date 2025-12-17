using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.Threading.Tasks;

namespace FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance
{
    public interface IMaintenanceTaskService
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