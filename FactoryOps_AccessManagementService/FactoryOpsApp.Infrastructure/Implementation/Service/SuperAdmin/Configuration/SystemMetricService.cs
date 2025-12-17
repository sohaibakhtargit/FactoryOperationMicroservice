using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Configuration
{
    public class SystemMetricService : ISystemMetricService
    {
        private readonly ISystemMetricRepository _metricRepo;
        private readonly FactoryOpsDBContext _tenantDb;

        public SystemMetricService(
            ISystemMetricRepository metricRepo,
            FactoryOpsDBContext tenantDb)
        {
            _metricRepo = metricRepo;
            _tenantDb = tenantDb;
        }

        //public async Task<CommonResponseModel> CreateMetricAsync()
        //{
        //    var metric = new SystemMetric
        //    {
        //        ActiveTenants = await _tenantDb.FactoryTenants.CountAsync(t => t.IsActive),
        //        ActiveUsers = await _tenantDb.FactoryUsers.CountAsync(u => u.IsActive),
        //        TotalStorage = 1024L * 1024L * 1024L * 5, // Simulated 5 GB
        //    //    OpenWorkOrders = await _tenantDb.FactoryWorkOrders.CountAsync(w => w.Status == "Open"),
        //       // ErrorCount = await _tenantDb.Audit_Logs.CountAsync(l => l.LogLevel == "Error" && l.CreatedAt >= DateTime.UtcNow.AddDays(-1))
        //    };

        //    await _metricRepo.AddAsync(metric);
        //    await _metricRepo.SaveAsync();

        //    return new CommonResponseModel
        //    {
        //        StatusCode = "200",
        //        StatusMessage = "System metrics recorded successfully."
        //    };
        //}

        public async Task<GetSystemMetricDto?> GetLatestMetricAsync()
        {
            var metric = await _metricRepo.GetLatestAsync();

            if (metric == null) return null;

            return new GetSystemMetricDto
            {
                MetricId = metric.MetricId,
                Timestamp = metric.Timestamp,
                ActiveTenants = metric.ActiveTenants,
                ActiveUsers = metric.ActiveUsers,
                TotalStorage = metric.TotalStorage,
                // OpenWorkOrders = metric.OpenWorkOrders,
                ErrorCount = metric.ErrorCount
            };
        }

        public async Task<List<GetSystemMetricDto>> GetAllMetricsAsync()
        {
            var list = await _metricRepo.GetAllAsync();

            return list.Select(m => new GetSystemMetricDto
            {
                MetricId = m.MetricId,
                Timestamp = m.Timestamp,
                ActiveTenants = m.ActiveTenants,
                ActiveUsers = m.ActiveUsers,
                TotalStorage = m.TotalStorage,
                //   OpenWorkOrders = m.OpenWorkOrders,
                ErrorCount = m.ErrorCount
            }).ToList();
        }
    }
}
