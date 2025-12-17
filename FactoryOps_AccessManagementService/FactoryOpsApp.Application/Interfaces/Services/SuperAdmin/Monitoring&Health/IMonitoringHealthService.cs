using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Monitoring_Health
{
    public interface IMonitoringHealthService
    {
        Task<MonitoringHealthDto> GetMonitoringHealthAsync();
    }
}
