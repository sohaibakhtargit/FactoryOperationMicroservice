using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Monitoring_Health;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Monitoring_Health;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Monitoring_Health
{
    public class MonitoringHealthService : IMonitoringHealthService
    {
        private readonly IMonitoringHealthRepository _repository;

        public MonitoringHealthService(IMonitoringHealthRepository repository)
        {
            _repository = repository;
        }

        public async Task<MonitoringHealthDto> GetMonitoringHealthAsync()
        {
            return await _repository.GetMonitoringHealthMetricsAsync();
        }
    }
}
