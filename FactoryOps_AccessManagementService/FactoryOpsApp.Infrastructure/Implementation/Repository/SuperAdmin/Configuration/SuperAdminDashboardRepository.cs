using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Configuration
{
    public class SuperAdminDashboardRepository : ISuperAdminDashboardRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _auditLogger;
        public SuperAdminDashboardRepository(
            MasterFactoryOpsDbContext masterDbcontext,
            TenantDbContextFactory tenantDbContextFactory,
            IConfiguration configuration, IAuditLogService auditLogger)
        {
            _masterDbcontext = masterDbcontext;
            _configuration = configuration;
            _tenantDbContextFactory = tenantDbContextFactory;
            _auditLogger = auditLogger;
        }
        public CountResponseModel GetActiveTenantsAsync()
        {
            CountResponseModel response = new();
            int countactivetenants = _masterDbcontext.FactoryTenants
                              .Where(t => t.IsActive && !t.IsDeleted).Count();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = SuperAdminDashboardStatusMessage.DataFetched;
            response.Count = countactivetenants; ;
            return response;
        }

        public CountResponseModel GetActiveUsersAsync()
        {
            CountResponseModel response = new();
            int countactiveusers = _masterDbcontext.GlobalUsers
                              .Where(t => t.IsActive && !t.IsDeleted).Count();

            response.StatusCode = StatusCode.Success;
            response.StatusMessage = SuperAdminDashboardStatusMessage.DataFetched;
            response.Count = countactiveusers;
            return response;
        }

        public async Task<ReportsAnalyticsSuperAdminDto> GetTenantAnalyticsReportForSuperAdminAsync()
        {
            
            double maxExpectedDbQueryTime = 200;
            double avgDbQueryTime = 120;

            double dbPerformancePercent = 100 - (avgDbQueryTime / maxExpectedDbQueryTime * 100);
            dbPerformancePercent = Math.Clamp(dbPerformancePercent, 0, 100);

            double avgResponseTime = 85;
            string apiResponseTime = $"{avgResponseTime}ms";

            double responseTimePercent = 100 - (avgResponseTime / 500 * 100);
            responseTimePercent = Math.Clamp(responseTimePercent, 0, 100);

           
            DriveInfo drive = DriveInfo.GetDrives().First(d => d.IsReady && d.Name == "C:\\");
            double used = drive.TotalSize - drive.TotalFreeSpace;
            double storageUsagePercent = (used / drive.TotalSize) * 100;
            string storageUsedGB = $"{Math.Round(used / (1024.0 * 1024 * 1024), 2)} GB";


            
            DateTime since = DateTime.UtcNow.AddHours(-24);

            int totalRequests = await _masterDbcontext.Audit_Log_MasterDb
                .CountAsync();

            int failedRequests = await _masterDbcontext.ExceptionLogs
                .CountAsync();

            double errorRatePercent = totalRequests == 0 ? 0 : (double)failedRequests / totalRequests * 100;
            string errorRateString = $"{Math.Round(errorRatePercent, 2)}%";

           
            var performanceMetrics = new PerformanceMetricsDto
            {
                DatabasePerformance = $"{Math.Round(dbPerformancePercent)}%",
                ApiResponseTime = apiResponseTime,
                StorageUsage = storageUsedGB,
                ErrorRate = errorRateString
            };


            int globalApiCalls24h = await _masterDbcontext.Audit_Log_MasterDb
                .Where(a => a.Timestamp >= since)
                .CountAsync();
            var apiKeys = _configuration.GetSection("ApiKeys").Get<List<string>>();
            int activeApiKeys = apiKeys?.Count ?? 0;

            int rateLimitViolations = await _masterDbcontext.ExceptionLogs
                .Where(a => (a.ErrorCode.Contains("429") || a.ExceptionStackTrace.Contains("429")))
                .CountAsync();

            var apiManagement = new ApiManagementDto
            {
                GlobalApiCalls24h = globalApiCalls24h,
                ActiveApiKeys = activeApiKeys,
                RateLimitViolations = rateLimitViolations
            };

            int customDomains = 0;
            int totalTenants = 0;
            int brandedTenants = 0;
            int emailTemplates = 0;

            var whiteLabeling = new WhiteLabelingDto
            {
                CustomDomains = customDomains,
                BrandedTenants = $"{brandedTenants}/{totalTenants}",
                EmailTemplates = emailTemplates
            };

            var recentActivities = await (
                        from audit in _masterDbcontext.Audit_Log_MasterDb
                        join tenant in _masterDbcontext.FactoryTenants
                            on audit.TenantId equals tenant.TenantId
                        where audit.Timestamp >= DateTime.UtcNow.AddDays(-1)
                              && tenant.IsDeleted == false && tenant.IsActive == true
                        orderby audit.Timestamp descending
                        select new RecentTenantActivityDto
                        {
                            TenantName = tenant.TenantName,
                            ActivityDescription = audit.Details,
                            Timestamp = audit.Timestamp
                        }
                    )
                    .Take(5)
                    .ToListAsync();

            var tenants = await _masterDbcontext.FactoryTenants
                .Where(t => t.IsActive && !t.IsDeleted && !t.Suspend)
                .ToListAsync();


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

                }
            }


          
            return new ReportsAnalyticsSuperAdminDto
            {
                ActiveTenants = tenants.Count,
                TotalUsers = totalUsers,
                OpenIssues = openSupportTickets,
                SystemLoad = GetCpuUsage(),
                PerformanceMetrics = performanceMetrics,
                ApiManagement = apiManagement,
                WhiteLabeling = whiteLabeling,
                RecentTenantActivities = recentActivities
            };
        }

        private double GetCpuUsage()
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _ = cpuCounter.NextValue(); 
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

    }
}