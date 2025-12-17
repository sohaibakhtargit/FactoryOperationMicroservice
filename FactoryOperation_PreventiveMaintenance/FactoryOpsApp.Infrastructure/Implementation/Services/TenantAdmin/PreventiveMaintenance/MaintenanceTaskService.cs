using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.PreventiveMaintenance
{
    public class MaintenanceTaskService : IMaintenanceTaskService
    {
        private readonly IMaintenanceTaskRepository _repository;

        public MaintenanceTaskService(IMaintenanceTaskRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddMaintenanceTaskAsync(MaintenanceTaskDto dto)
        {
            return _repository.AddMaintenanceTaskAsync(dto);
        }

        public Task<CommonResponseModel> UpdateMaintenanceTaskAsync(MaintenanceTaskDto dto)
        {
            return _repository.UpdateMaintenanceTaskAsync(dto);
        }

        public Task<CommonResponseModel> DeleteMaintenanceTaskAsync(int taskId, int tenantId)
        {
            return _repository.DeleteMaintenanceTaskAsync(taskId, tenantId);
        }

        public Task<CommonResponseModel> UpdateTaskStatusAsync(TaskStatusUpdateDto dto)
        {
            return _repository.UpdateTaskStatusAsync(dto);
        }

        public Task<CommonResponseModel> VerifyTaskAsync(TaskVerificationDto dto)
        {
            return _repository.VerifyTaskAsync(dto);
        }

        public Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksByWorkOrderAsync(int workOrderId, int tenantId)
        {
            return _repository.GetTasksByWorkOrderAsync(workOrderId, tenantId);
        }

        public Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksByStatusAsync(int tenantId)
        {
            return _repository.GetTasksByStatusAsync(tenantId);
        }

        public Task<GetSpecificRecord<GetMaintenanceTaskDto>> GetMaintenanceTaskByIdAsync(int taskId, int tenantId)
        {
            return _repository.GetMaintenanceTaskByIdAsync(taskId, tenantId);
        }

        public Task<GetAllRecord<GetMaintenanceTaskDto>> GetTasksRequiringVerificationAsync(int tenantId)
        {
            return _repository.GetTasksRequiringVerificationAsync(tenantId);
        }
    }
}