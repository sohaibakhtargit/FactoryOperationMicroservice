using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Monitoring_Health;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Monitoring_Health
{
    public class MonitoringHealthRepository : IMonitoringHealthRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly TenantDbContextFactory _tenantDbContextFactory;
        private readonly IConnectionMultiplexer _redis;
        private readonly IExceptionLoggerService _exceptionLogger;

        public MonitoringHealthRepository(
            MasterFactoryOpsDbContext masterDbContext,
            TenantDbContextFactory tenantDbContextFactory,
            IConnectionMultiplexer redis,
            IExceptionLoggerService exceptionLogger)
        {
            _masterDbContext = masterDbContext;
            _tenantDbContextFactory = tenantDbContextFactory;
            _redis = redis;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<MonitoringHealthDto> GetMonitoringHealthMetricsAsync()
        {
            var dto = new MonitoringHealthDto();

            // 1. Tenant/User/Support Ticket Metrics
            var tenants = await _masterDbContext.FactoryTenants
                .Where(t => t.IsActive && !t.IsDeleted)
                .ToListAsync();

            dto.ActiveTenants = tenants.Count;

            int totalUsers = 0, totalSupportTickets = 0, openSupportTickets = 0;

            foreach (var tenant in tenants)
            {
                try
                {
                    var tenantDb = _tenantDbContextFactory.GetTenantDbContext(tenant.TenantId);

                    totalUsers += await tenantDb.FactoryUsers.CountAsync(u => !u.IsDeleted);

                    var tickets = await tenantDb.FactorySupportTickets
                        .Where(t => !t.IsDeleted)
                        .ToListAsync();

                    totalSupportTickets += tickets.Count;
                    openSupportTickets += tickets.Count(t => !string.IsNullOrEmpty(t.Status) &&
                                                             !t.Status.ToLower().Contains("resolved"));
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(
                  ex,
                  sourceModule: "MonitoringHealth",
                  apiName: "GetMonitoringHealthMetricsAsync",
                  tenantId: tenant.TenantId
              );
                }
            }

            dto.TotalUsers = totalUsers;
            dto.TotalSupportTickets = totalSupportTickets;
            dto.OpenSupportTickets = openSupportTickets;

            // 2. System Load (CPU Usage)
            dto.SystemLoad = GetCpuUsage();

            // 3. Security Monitoring (Placeholder - Implement your logic)
            dto.FailedLoginRate = GetFailedLoginRate();
            dto.UnusualActivityCount = GetUnusualActivityCount();
            dto.RateLimitViolations = GetRateLimitViolations();

            // 4. API Performance (Placeholder - Implement from API monitoring)
            dto.P50Latency = GetLatencyPercentile(50);
            dto.P95Latency = GetLatencyPercentile(95);
            dto.P99Latency = GetLatencyPercentile(99);
            dto.TimeoutRate = GetTimeoutRate();

            // 5. Infrastructure Health (Redis, Memory, Disk, Network)
            dto.CacheHitRate = await GetCacheHitRateAsync();
            dto.MemoryUsage = GetMemoryUsage();
            dto.DiskUsage = GetDiskUsage();
            dto.NetworkLatency = GetNetworkLatency();

            return dto;
        }

        #region === Helper Methods ===

        private async Task<double> GetCacheHitRateAsync()
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var info = await server.InfoAsync("stats");
                var stats = info.FirstOrDefault(section => section.Key == "stats").FirstOrDefault().Value;

                long hits = ExtractRedisStat(stats, "keyspace_hits");
                long misses = ExtractRedisStat(stats, "keyspace_misses");

                return (hits + misses) > 0 ? (double)hits / (hits + misses) * 100 : 0;
            }
            catch { return 0; }
        }

        private long ExtractRedisStat(string stats, string key)
        {
            var match = Regex.Match(stats, $@"{key}:(\d+)");
            return match.Success ? long.Parse(match.Groups[1].Value) : 0;
        }

        private double GetCpuUsage()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _ = cpuCounter.NextValue(); // First call always returns 0
                    Thread.Sleep(500);
                    return Math.Round(cpuCounter.NextValue(), 2);
                }
                else
                {
                    string[] cpuStats1 = File.ReadAllLines("/proc/stat")
                                              .First()
                                              .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    long idle1 = long.Parse(cpuStats1[4]);
                    long total1 = cpuStats1.Skip(1).Sum(long.Parse);

                    Thread.Sleep(500);

                    string[] cpuStats2 = File.ReadAllLines("/proc/stat")
                                              .First()
                                              .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    long idle2 = long.Parse(cpuStats2[4]);
                    long total2 = cpuStats2.Skip(1).Sum(long.Parse);

                    long idle = idle2 - idle1;
                    long total = total2 - total1;

                    return Math.Round((1.0 - (double)idle / total) * 100, 2);
                }
            }
            catch
            {
                return -1;
            }
        }


        private double GetMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            double totalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024.0 * 1024.0);
            double usedMB = process.WorkingSet64 / (1024.0 * 1024.0);
            return (usedMB / totalMemoryMB) * 100;
        }

        private double GetDiskUsage()
        {
            try
            {
                DriveInfo drive = DriveInfo.GetDrives().First(d => d.IsReady && d.Name == "C:\\");
                double used = drive.TotalSize - drive.TotalFreeSpace;
                double storageUsagePercent = (used / drive.TotalSize) * 100;
                double storageUsedGB = Math.Round(used / (1024.0 * 1024 * 1024), 2);
                string rootPath = OperatingSystem.IsWindows() ? "C:\\" : "/";

                return storageUsedGB;
            }
            catch
            {
                return -1;
            }
        }

        private double GetNetworkLatency()
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send("8.8.8.8", 2000);
                return reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
            }
            catch { return -1; }
        }

        private double GetFailedLoginRate() => 0.5;
        private int GetUnusualActivityCount() => 2;
        private int GetRateLimitViolations() => 1;

        private int GetLatencyPercentile(int percentile) => 100 + percentile;
        private double GetTimeoutRate() => 0.2;

        #endregion

    }
}
