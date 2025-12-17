using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Configuration
{
    public class SystemMetricRepository : ISystemMetricRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;

        public SystemMetricRepository(MasterFactoryOpsDbContext masterDbcontext)
        {
            _masterDbcontext = masterDbcontext;
        }

        public async Task AddAsync(SystemMetric metric)
        {
            await _masterDbcontext.SystemMetrics.AddAsync(metric);
        }

        public async Task<SystemMetric?> GetLatestAsync()
        {
            return await _masterDbcontext.SystemMetrics
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SystemMetric>> GetAllAsync()
        {
            return await _masterDbcontext.SystemMetrics
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<int> SaveAsync()
        {
            return await _masterDbcontext.SaveChangesAsync();
        }
    }
}

