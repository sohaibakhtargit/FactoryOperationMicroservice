using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Analytic_Reports;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Analytic_Reports
{
    public class TechnicianDashboardRepository : ITechnicianDashboardRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;

        public TechnicianDashboardRepository(TenantDbContextFactory tenantDbContext)
        {
            _tenantDbContext = tenantDbContext;
        }

        //  public async Task<TechnicianDashboardDto> GetTechnicianDashboardAsync(
        //int tenantId,
        //int userId,
        //DashboardFilter filter = DashboardFilter.Month) // default so existing calls won't break
        //  {
        //      using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

        //      var result = new TechnicianDashboardDto();

        //      var (startDate, endDate) = GetDateRange(filter);

        //      // Base Query (REUSED everywhere)
        //      var baseQuery = tenantDb.WorkOrders
        //          .Where(x => x.TenantId == tenantId
        //                   && x.AssignedToUserId == userId
        //                   && !x.IsDeleted);

        //      // Filtered Query (ONLY for KPI calculations)
        //      var filteredQuery = baseQuery
        //          .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);

        //      // =========================
        //      // EXISTING FUNCTIONALITY (UNCHANGED)
        //      // =========================

        //      var total = await baseQuery.CountAsync();
        //      result.TotalActiveWorkOrders = total;

        //      var typeData = await baseQuery
        //          .GroupBy(x => x.WorkOrderType)
        //          .Select(g => new
        //          {
        //              Type = g.Key,
        //              Count = g.Count()
        //          })
        //          .ToListAsync();

        //      result.WorkOrdersByType = typeData.Select(x => new WorkOrderTypeSummaryForTechinicianDto
        //      {
        //          Type = x.Type!.ToString()!,
        //          Count = x.Count,
        //          Percentage = total == 0 ? 0 : Math.Round((decimal)x.Count * 100 / total, 1)
        //      }).ToList();

        //      var priorityData = await baseQuery
        //          .GroupBy(x => x.Priority)
        //          .Select(g => new
        //          {
        //              Priority = g.Key,
        //              Count = g.Count()
        //          })
        //          .ToListAsync();

        //      result.WorkOrdersByPriority = priorityData.Select(x => new WorkOrderPrioritySummaryForTechinicianDto
        //      {
        //          Priority = x.Priority!.ToString()!,
        //          Count = x.Count,
        //          Percentage = total == 0 ? 0 : Math.Round((decimal)x.Count * 100 / total, 1)
        //      }).ToList();

        //      result.TotalTeams = await tenantDb.FactoryTeamMembers
        //          .Where(x => x.UserId == userId && !x.IsDeleted)
        //          .Select(x => x.TeamId)
        //          .Distinct()
        //          .CountAsync();

        //      // =========================
        //      // NEW: PERFORMANCE KPIs
        //      // =========================

        //      result.Performance ??= new TechnicianPerformanceDto();

        //      // 1. Assigned Work Orders
        //      var assigned = await filteredQuery.CountAsync();
        //      result.Performance.AssignedWorkOrders = assigned;

        //      // 2. Completed Work Orders
        //      var completedList = await filteredQuery
        //          .Where(x => x.Status == WorkOrderStatus.Completed)
        //          .ToListAsync();

        //      result.Performance.CompletedWorkOrders = completedList.Count;

        //      result.Performance.CancelledWorkOrders = await filteredQuery
        //          .Where(x => x.Status == WorkOrderStatus.Cancelled)
        //          .CountAsync();

        //      result.Performance.ActiveWorkOrders = await filteredQuery
        //          .Where(x => x.Status == WorkOrderStatus.Active)
        //          .CountAsync();

        //      result.Performance.InactiveWorkOrders = await filteredQuery
        //          .Where(x => x.Status == WorkOrderStatus.Inactive)
        //          .CountAsync();

        //      var validStatuses = new[]
        //          {
        //              WorkOrderStatus.Started,
        //              WorkOrderStatus.Pending,
        //              WorkOrderStatus.Assigned,
        //              WorkOrderStatus.InProgress,
        //              WorkOrderStatus.Overdue
        //          };

        //      result.Performance.PendingTasks = await filteredQuery
        //          .Where(x => x.Status != null && validStatuses.Contains(x.Status.Value))
        //          .CountAsync();

        //      // 4. On-Time Completion Rate
        //      var onTimeCompleted = completedList
        //          .Where(x => x.DueDate != null && x.UpdatedAt <= x.DueDate)
        //          .Count();

        //      result.Performance.OnTimeCompletionRate =
        //          completedList.Count == 0 ? 0 :
        //          Math.Round((decimal)onTimeCompleted * 100 / completedList.Count, 2);

        //      // 5. Average Completion Time (Hours)
        //      var avgTimeList = completedList
        //          .Where(x => x.StartTime != null && x.UpdatedAt != null)
        //          .Select(x => (x.UpdatedAt - x.StartTime)!.Value.TotalHours);

        //      result.Performance.AverageCompletionTimeInHours =
        //          avgTimeList.Any() ? Math.Round((decimal)avgTimeList.Average(), 2) : 0;

        //      // 6. Rework Rate (based on multiple updates)
        //      var reworkedCount = await tenantDb.WorkOrderProgressUpdates
        //          .Where(x => x.WorkOrder.TenantId == tenantId
        //                   && x.WorkOrder.AssignedToUserId == userId
        //                   && x.WorkOrder.CreatedAt >= startDate
        //                   && x.WorkOrder.CreatedAt <= endDate)
        //          .GroupBy(x => x.WorkOrderId)
        //          .Where(g => g.Count() > 1)
        //          .CountAsync();

        //      result.Performance.ReworkRate =
        //          assigned == 0 ? 0 :
        //          Math.Round((decimal)reworkedCount * 100 / assigned, 2);

        //      return result;
        //  }

        public async Task<TechnicianDashboardDto> GetTechnicianDashboardAsync(
    int tenantId,
    int userId,
    DashboardFilter filter = DashboardFilter.Month)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var result = new TechnicianDashboardDto();

            var (startDate, endDate) = GetDateRange(filter);

            // 🔥 FILTER APPLIED TO ALL
            var baseQuery = tenantDb.WorkOrders
                .Where(x => x.TenantId == tenantId
                         && x.AssignedToUserId == userId
                         && !x.IsDeleted
                         && x.CreatedAt >= startDate
                         && x.CreatedAt <= endDate);

            // =========================
            // ALL DATA NOW FILTERED
            // =========================

            var total = await baseQuery.CountAsync();
            result.TotalActiveWorkOrders = total;

            var typeData = await baseQuery
                .GroupBy(x => x.WorkOrderType)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            result.WorkOrdersByType = typeData.Select(x => new WorkOrderTypeSummaryForTechinicianDto
            {
                Type = x.Type!.ToString()!,
                Count = x.Count,
                Percentage = total == 0 ? 0 : Math.Round((decimal)x.Count * 100 / total, 1)
            }).ToList();

            var priorityData = await baseQuery
                .GroupBy(x => x.Priority)
                .Select(g => new
                {
                    Priority = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            result.WorkOrdersByPriority = priorityData.Select(x => new WorkOrderPrioritySummaryForTechinicianDto
            {
                Priority = x.Priority!.ToString()!,
                Count = x.Count,
                Percentage = total == 0 ? 0 : Math.Round((decimal)x.Count * 100 / total, 1)
            }).ToList();

            // Teams usually NOT filtered (keep as is unless client says)
            result.TotalTeams = await tenantDb.FactoryTeamMembers
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .Select(x => x.TeamId)
                .Distinct()
                .CountAsync();

            // =========================
            // PERFORMANCE (ALREADY FILTERED)
            // =========================

            result.Performance ??= new TechnicianPerformanceDto();

            var assigned = total;
            result.Performance.AssignedWorkOrders = assigned;

            var completedList = await baseQuery
                .Where(x => x.Status == WorkOrderStatus.Completed)
                .ToListAsync();

            result.Performance.CompletedWorkOrders = completedList.Count;

            result.Performance.CancelledWorkOrders = await baseQuery
                .Where(x => x.Status == WorkOrderStatus.Cancelled)
                .CountAsync();

            result.Performance.ActiveWorkOrders = await baseQuery
                .Where(x => x.Status == WorkOrderStatus.Active)
                .CountAsync();

            result.Performance.InactiveWorkOrders = await baseQuery
                .Where(x => x.Status == WorkOrderStatus.Inactive)
                .CountAsync();

            var validStatuses = new[]
            {
        WorkOrderStatus.Started,
        WorkOrderStatus.Pending,
        WorkOrderStatus.Assigned,
        WorkOrderStatus.InProgress,
        WorkOrderStatus.Overdue
    };

            result.Performance.PendingTasks = await baseQuery
                .Where(x => x.Status != null && validStatuses.Contains(x.Status.Value))
                .CountAsync();

            var onTimeCompleted = completedList
                .Where(x => x.DueDate != null && x.UpdatedAt <= x.DueDate)
                .Count();

            result.Performance.OnTimeCompletionRate =
                completedList.Count == 0 ? 0 :
                Math.Round((decimal)onTimeCompleted * 100 / completedList.Count, 2);

            var avgTimeList = completedList
                .Where(x => x.StartTime != null && x.UpdatedAt != null)
                .Select(x => (x.UpdatedAt - x.StartTime)!.Value.TotalHours);

            result.Performance.AverageCompletionTimeInHours =
                avgTimeList.Any() ? Math.Round((decimal)avgTimeList.Average(), 2) : 0;

            var reworkedCount = await tenantDb.WorkOrderProgressUpdates
                .Where(x => x.WorkOrder.TenantId == tenantId
                         && x.WorkOrder.AssignedToUserId == userId
                         && x.WorkOrder.CreatedAt >= startDate
                         && x.WorkOrder.CreatedAt <= endDate)
                .GroupBy(x => x.WorkOrderId)
                .Where(g => g.Count() > 1)
                .CountAsync();

            result.Performance.ReworkRate =
                assigned == 0 ? 0 :
                Math.Round((decimal)reworkedCount * 100 / assigned, 2);

            return result;
        }
        private (DateTime start, DateTime end) GetDateRange(DashboardFilter filter)
        {
            var now = DateTime.UtcNow;

            return filter switch
            {
                DashboardFilter.Day => (now.Date, now),
                DashboardFilter.Week => (now.AddDays(-7), now),
                DashboardFilter.Month => (now.AddMonths(-1), now),
                DashboardFilter.Year => (now.AddYears(-1), now),
                _ => (now.AddMonths(-1), now)
            };
        }
    }
}
