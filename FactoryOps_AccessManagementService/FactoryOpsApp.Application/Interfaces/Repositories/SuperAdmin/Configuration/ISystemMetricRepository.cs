using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration
{
    public interface ISystemMetricRepository
    {
        Task AddAsync(SystemMetric metric);
        Task<SystemMetric?> GetLatestAsync();
        Task<List<SystemMetric>> GetAllAsync();
        Task<int> SaveAsync();
    }
}
