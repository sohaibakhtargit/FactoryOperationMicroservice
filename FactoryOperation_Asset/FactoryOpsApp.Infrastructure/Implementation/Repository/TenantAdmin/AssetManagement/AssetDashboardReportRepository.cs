using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOpsApp.Common.CommonConstant;

using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.AssetManagement
{
    public class AssetDashboardReportRepository : IAssetDashboardReportRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;

        public AssetDashboardReportRepository(
            TenantDbContextFactory tenantDbContext,
            IExceptionLoggerService exceptionLogger,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<GetAllRecord<DashboardSummaryDto>> GetDashboardSummaryAsync(int tenantId)
        {
            var response = new GetAllRecord<DashboardSummaryDto>();

            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                var today = DateTime.UtcNow;

                // Basic counts
                int totalAssets = await tenantDb.AssetRegistry.CountAsync(x => !x.IsDeleted);
                int running = await tenantDb.AssetRegistry.CountAsync(x => x.IsActive && !x.IsDeleted);
                int maintenance = await tenantDb.AssetRegistry.CountAsync(x => x.Department == "Maintenance" && !x.IsDeleted);
                int idle = totalAssets - running - maintenance;

                // Retirement planning
                var next12Months = await tenantDb.AssetRegistry
                    .CountAsync(x => !x.IsDeleted && x.WarrantyExpiry != null && x.WarrantyExpiry <= today.AddMonths(12));

                var twoToFiveYears = await tenantDb.AssetRegistry
                    .CountAsync(x => !x.IsDeleted && x.WarrantyExpiry != null
                        && x.WarrantyExpiry > today.AddMonths(12)
                        && x.WarrantyExpiry <= today.AddYears(5));

                var fivePlusYears = await tenantDb.AssetRegistry
                    .CountAsync(x => !x.IsDeleted && x.WarrantyExpiry > today.AddYears(5));

                // Utilization DTO
                var utilization = new AssetUtilizationDto
                {
                    RunningAssets = running,
                    RunningPercentage = totalAssets > 0 ? running * 100.0 / totalAssets : 0,
                    UnderMaintenance = maintenance,
                    MaintenancePercentage = totalAssets > 0 ? maintenance * 100.0 / totalAssets : 0,
                    IdleAssets = idle,
                    IdlePercentage = totalAssets > 0 ? idle * 100.0 / totalAssets : 0
                };

                // Categories
                var categories = await tenantDb.AssetRegistry
                    .Where(x => !x.IsDeleted)
                    .GroupBy(x => x.FactoryAssetType.Type_Name)
                    .Select(g => new AssetCategoryDto
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Utilization = 100 * g.Count(a => a.IsActive) / (double)g.Count()
                    })
                    .ToListAsync();

                // Retirement planning DTO
                var retirementPlanning = new RetirementPlanningDto
                {
                    Next12Months = next12Months,
                    TwoToFiveYears = twoToFiveYears,
                    FivePlusYears = fivePlusYears
                };

                // Top maintenance costs
                var topMaintenanceCosts = await (
                    from m in tenantDb.MaintenanceHistory
                    join a in tenantDb.AssetRegistry on m.AssetId equals a.AssetId
                    where m.TenantId == tenantId
                    group new { m, a } by new { a.AssetId, a.AssetName, a.FactoryAssetType.Type_Name } into g
                    select new TopMaintenanceCostDto
                    {
                        AssetName = g.Key.AssetName,
                        AssetType = g.Key.Type_Name,
                        YTDCost = g.Sum(x => x.m.Cost) ?? 0,
                        Frequency = g.Count(),
                        Trend = g.Sum(x => x.m.Cost) > 50000 ? "High" : "Normal"
                    })
                    .OrderByDescending(x => x.YTDCost)
                    .Take(5)
                    .ToListAsync();

                // Core metrics
                double overallUptime = totalAssets > 0 ? running * 100.0 / totalAssets : 0;

                int totalMaintenances = await tenantDb.MaintenanceHistory
                    .CountAsync(x => x.TenantId == tenantId && !x.IsDeleted);

                double reliabilityIndex = totalAssets > 0
                    ? 100 - (double)totalMaintenances / totalAssets * 10
                    : 100;

                decimal totalAcquisitionCost = await tenantDb.AssetRegistry
                    .Where(x => !x.IsDeleted)
                    .SumAsync(x => x.AcquisitionCost ?? 0);

                decimal totalMaintenanceCost = await tenantDb.MaintenanceHistory
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted)
                    .SumAsync(x => x.Cost ?? 0);

                double costEfficiency = totalAcquisitionCost > 0
                    ? (double)(totalAcquisitionCost - totalMaintenanceCost) / (double)totalAcquisitionCost * 100
                    : 0;

                int compliantAssets = await tenantDb.AssetRegistry
                    .CountAsync(x => !x.IsDeleted && x.WarrantyExpiry != null && x.WarrantyExpiry >= today);

                double complianceRate = totalAssets > 0
                    ? compliantAssets * 100.0 / totalAssets
                    : 0;

                var metrics = new DashboardMetricsDto
                {
                    OverallUptime = overallUptime,
                    ReliabilityIndex = reliabilityIndex,
                    CostEfficiency = costEfficiency,
                    ComplianceRate = complianceRate
                };

                // -------------------- Downtime calculation using AssetTracking --------------------
                // Fetch asset tracking rows that have any accumulated downtime OR are currently down
                var tracked = await tenantDb.AssetTracking
                    .Where(x => !x.IsDeleted && (x.TotalDownAccumulatedMinutes != null || x.Status == Domain.Entities.FactoryOpsTenants.AssetTrackingStatusEnum.Down))
                    .Select(x => new
                    {
                        x.TrackingId,
                        x.AssetId,
                        x.Status,
                        DownAccumulated = x.TotalDownAccumulatedMinutes ?? 0,
                        DownStart = x.DownStartTime
                    })
                    .ToListAsync();

                // total minutes from accumulated stored values
                double totalDowntimeMinutes = tracked.Sum(t => t.DownAccumulated);

                // add live duration for currently-down assets
                var now = DateTime.UtcNow;
                var currentlyDownList = tracked.Where(t => t.Status == Domain.Entities.FactoryOpsTenants.AssetTrackingStatusEnum.Down && t.DownStart.HasValue).ToList();

                foreach (var cd in currentlyDownList)
                {
                    var liveMinutes = (now - cd.DownStart.Value).TotalMinutes;
                    if (liveMinutes > 0) totalDowntimeMinutes += liveMinutes;
                }

                // assets currently down count
                int assetsCurrentlyDown = await tenantDb.AssetTracking
                    .CountAsync(x => x.Status == Domain.Entities.FactoryOpsTenants.AssetTrackingStatusEnum.Down && !x.IsDeleted);

                // assets that have any downtime record (accumulated or currently down)
                int assetsWithDowntime = tracked.Select(t => t.AssetId).Distinct().Count();

                double averageDowntimeMinutes = assetsWithDowntime > 0 ? totalDowntimeMinutes / assetsWithDowntime : 0;
                double totalDowntimeHours = totalDowntimeMinutes / 60.0;

                // downtime percentage relative to total assets in one day baseline
                double downtimePercentage = totalAssets > 0
                    ? (totalDowntimeMinutes / (totalAssets * 24 * 60)) * 100
                    : 0;

                // Build downtime DTO: we will place it inside DashboardSummaryDto as DowntimeMetrics
                var downtimeDto = new AssetDashboardReportDto
                {
                    AssetsCurrentlyDown = assetsCurrentlyDown,
                    AverageDowntimeMinutes = Math.Round(averageDowntimeMinutes, 2),
                    TotalDowntimeHours = Math.Round(totalDowntimeHours, 2),
                    DowntimePercentage = Math.Round(downtimePercentage, 2),

                    // keep asset status & planning fields in top-level report dto to match your flattened DTO
                    RunningAssets = running,
                    MaintenanceAssets = maintenance,
                    IdleAssets = idle,
                    Next12MonthsReplacements = next12Months,
                    TwoToFiveYearsReplacements = twoToFiveYears,
                    FivePlusYearsReplacements = fivePlusYears,
                    CreatedBy = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Final summary DTO
                var dashboardSummary = new DashboardSummaryDto
                {
                    AssetUtilization = utilization,
                    AssetCategories = categories,
                    RetirementPlanning = retirementPlanning,
                    TopMaintenanceCosts = topMaintenanceCosts,
                    Metrics = metrics,
                    DowntimeMetrics = downtimeDto
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AssetDashboardReportStatusMessage.DataFetched;
                response.GetAllData = new List<DashboardSummaryDto> { dashboardSummary };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AssetDashboardReport-Module", "Get-DashboardSummary", tenantId, null);
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{AssetDashboardReportStatusMessage.FetchFailed}: {ex.Message}";
            }

            return response;
        }
    }
}
