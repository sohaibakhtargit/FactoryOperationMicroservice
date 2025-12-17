using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Monitoring_Health
{
    public interface IMonitoringHealthRepository
    {
        Task<MonitoringHealthDto> GetMonitoringHealthMetricsAsync();
    }
}
