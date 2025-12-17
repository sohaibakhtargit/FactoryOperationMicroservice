using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System.Threading.Tasks;

namespace FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance
{
    public interface IMaintenanceScheduleService
    {
        // 🔹 CRUD + Workflow
        Task<CommonResponseModel> AddMaintenanceScheduleAsync(MaintenanceScheduleDto dto);
        Task<CommonResponseModel> UpdateMaintenanceScheduleAsync(MaintenanceScheduleDto dto);
        Task<CommonResponseModel> DeleteMaintenanceScheduleAsync(int scheduleId, int tenantId);
        Task<CommonResponseModel> ApproveScheduleAsync(ScheduleApprovalDto dto);

        // 🔹 Schedule Queries
        Task<GetAllRecord<GetMaintenanceScheduleDto>> GetAllMaintenanceSchedulesAsync(int tenantId, WorkOrderStatus? statusFilter = null);
        Task<GetSpecificRecord<GetMaintenanceScheduleDto>> GetMaintenanceScheduleByIdAsync(int scheduleId, int tenantId);

        // 🔹 Utility
        Task<CommonResponseModel> CalculateNextDueDateAsync(int scheduleId, int tenantId);

        // 🔹 Occurrence-Specific
        Task<GetAllRecord<MaintenanceScheduleOccurrence>> GetOccurrencesByScheduleIdAsync(int scheduleId, int tenantId);
        Task<GetAllRecord<MaintenanceScheduleOccurrence>> GetUpcomingOccurrencesAsync(int tenantId, int daysAhead);
        Task<CommonResponseModel> RegenerateOccurrencesAsync(int scheduleId, int tenantId);
    }
}