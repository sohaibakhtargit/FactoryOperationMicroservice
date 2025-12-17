using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminManagement
{
    public class ReportsAnalyticsRepository : IReportsAnalyticsRepository
    {
        private readonly MasterFactoryOpsDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IExceptionLoggerService _exceptionLogger;

        public ReportsAnalyticsRepository(MasterFactoryOpsDbContext context,
            IConfiguration configuration,
            IExceptionLoggerService exceptionLogger)
        {
            _context = context;
            _configuration = configuration;
            _exceptionLogger = exceptionLogger;

        }
        public async Task<ReportsAnalyticsDto> GetReportsAnalyticsData(int tenantId)
        {
            try
            {
                double maxExpectedDbQueryTime = 200;
                double avgDbQueryTime = 120;
                double dbPerformancePercent = Math.Clamp(100 - avgDbQueryTime / maxExpectedDbQueryTime * 100, 0, 100);

                double avgResponseTime = 85;
                string apiResponseTime = $"{avgResponseTime}ms";
                double responseTimePercent = Math.Clamp(100 - avgResponseTime / 500 * 100, 0, 100);

                string storageUsedGB = "N/A";
                double storageUsagePercent = 0;
                try
                {
                    DriveInfo drive = DriveInfo.GetDrives().First(d => d.IsReady && d.Name == "C:\\");
                    double used = drive.TotalSize - drive.TotalFreeSpace;
                    storageUsagePercent = used / drive.TotalSize * 100;
                    storageUsedGB = $"{Math.Round(used / (1024.0 * 1024 * 1024), 2)} GB";
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(ex, "ReportsAnalyticsRepository", "GetReportsAnalyticsData - Storage Usage", null, null);
                }

                DateTime since = DateTime.UtcNow.AddHours(-24);
                int totalRequests = 0;
                int failedRequests = 0;
                try
                {
                    totalRequests = await _context.Audit_Log_MasterDb.CountAsync();
                    failedRequests = await _context.ExceptionLogs.CountAsync();
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(ex, "ReportsAnalyticsRepository", "GetReportsAnalyticsData - Requests/Failures", null, null);
                }

                double errorRatePercent = totalRequests == 0 ? 0 : (double)failedRequests / totalRequests * 100;
                string errorRateString = $"{Math.Round(errorRatePercent, 2)}%";

                var performanceMetrics = new PerformanceMetricsDto
                {
                    DatabasePerformance = $"{Math.Round(dbPerformancePercent)}%",
                    ApiResponseTime = apiResponseTime,
                    StorageUsage = storageUsedGB,
                    ErrorRate = errorRateString
                };

                int globalApiCalls24h = 0;
                int activeApiKeys = 0;
                int rateLimitViolations = 0;
                try
                {
                    globalApiCalls24h = await _context.Audit_Log_MasterDb
                        .Where(a => a.Timestamp >= since)
                        .CountAsync();

                    var apiKeys = _configuration.GetSection("ApiKeys").Get<List<string>>();
                    activeApiKeys = apiKeys?.Count ?? 0;

                    rateLimitViolations = await _context.ExceptionLogs
                        .Where(a => a.ErrorCode.Contains("429") || a.ExceptionStackTrace.Contains("429"))
                        .CountAsync();
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(ex, "ReportsAnalyticsRepository", "GetReportsAnalyticsData - API Management", null, null);
                }

                var apiManagement = new ApiManagementDto
                {
                    GlobalApiCalls24h = globalApiCalls24h,
                    ActiveApiKeys = activeApiKeys,
                    RateLimitViolations = rateLimitViolations
                };

                var whiteLabeling = new WhiteLabelingDto
                {
                    CustomDomains = 0,
                    BrandedTenants = "0/0",
                    EmailTemplates = 0
                };

                List<RecentTenantActivityDto> recentActivities = new();
                try
                {
                    recentActivities = await (
                        from audit in _context.Audit_Log_MasterDb
                        join tenant in _context.FactoryTenants
                            on tenantId equals tenant.TenantId
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
                    .Take(3)
                    .ToListAsync();
                }
                catch (Exception ex)
                {
                    await _exceptionLogger.LogExceptionAsync(ex, "ReportsAnalyticsRepository", "GetReportsAnalyticsData - RecentTenantActivities", null, null);
                }

                return new ReportsAnalyticsDto
                {
                    PerformanceMetrics = performanceMetrics,
                    ApiManagement = apiManagement,
                    WhiteLabeling = whiteLabeling,
                    RecentTenantActivities = recentActivities
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "ReportsAnalyticsRepository", "GetReportsAnalyticsData - General", null, null);
                throw;
            }
        }
    }
}
