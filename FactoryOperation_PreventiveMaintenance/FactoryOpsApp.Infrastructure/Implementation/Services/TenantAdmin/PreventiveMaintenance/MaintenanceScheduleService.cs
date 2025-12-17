using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.Threading.Tasks;

namespace FactoryOpsApp.Infrastructure.Service.TenantAdmin.PreventiveMaintenance
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly IMaintenanceScheduleRepository _repository;

        public MaintenanceScheduleService(IMaintenanceScheduleRepository repository)
        {
            _repository = repository;
        }

        public Task<CommonResponseModel> AddMaintenanceScheduleAsync(MaintenanceScheduleDto dto)
        {
            return _repository.AddMaintenanceScheduleAsync(dto);
        }

        public Task<CommonResponseModel> UpdateMaintenanceScheduleAsync(MaintenanceScheduleDto dto)
        {
            return _repository.UpdateMaintenanceScheduleAsync(dto);
        }

        public Task<CommonResponseModel> DeleteMaintenanceScheduleAsync(int scheduleId, int tenantId)
        {
            return _repository.DeleteMaintenanceScheduleAsync(scheduleId, tenantId);
        }

        public Task<CommonResponseModel> ApproveScheduleAsync(ScheduleApprovalDto dto)
        {
            return _repository.ApproveScheduleAsync(dto);
        }

        public Task<GetAllRecord<GetMaintenanceScheduleDto>> GetAllMaintenanceSchedulesAsync(int tenantId, WorkOrderStatus? statusFilter = null)
        {
            return _repository.GetAllMaintenanceSchedulesAsync(tenantId, statusFilter);
        }

        public Task<GetSpecificRecord<GetMaintenanceScheduleDto>> GetMaintenanceScheduleByIdAsync(int scheduleId, int tenantId)
        {
            return _repository.GetMaintenanceScheduleByIdAsync(scheduleId, tenantId);
        }

        public Task<CommonResponseModel> CalculateNextDueDateAsync(int scheduleId, int tenantId)
        {
            return _repository.CalculateNextDueDateAsync(scheduleId, tenantId);
        }
        public Task<GetAllRecord<MaintenanceScheduleOccurrence>> GetOccurrencesByScheduleIdAsync(int scheduleId, int tenantId)
            => _repository.GetOccurrencesByScheduleIdAsync(scheduleId, tenantId);
        public Task<GetAllRecord<MaintenanceScheduleOccurrence>> GetUpcomingOccurrencesAsync(int tenantId, int daysAhead)
            => _repository.GetUpcomingOccurrencesAsync(tenantId, daysAhead);
        public Task<CommonResponseModel> RegenerateOccurrencesAsync(int scheduleId, int tenantId)
            => _repository.RegenerateOccurrencesAsync(scheduleId, tenantId);
    }
}