using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.Common
{
    public class NotificationBuilderService : INotificationBuilderService
    {
        // public async Task<NotificationUsersResult> BuildAsync(
        //TenantDbContextFactory _tenantDbContext,
        //int tenantId,
        //int? teamId,
        //int? actionUserId, //senderId
        //int? assignedToUserId,
        //int? createdBy,
        //int? assetAssignedUserId = null)
        // {
        //     using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

        //     var result = new NotificationUsersResult
        //     {
        //         Outgoing = actionUserId
        //     };
        //     var assignedUserTeamIds = new List<int>();

        //     if (assignedToUserId.HasValue)
        //     {
        //         assignedUserTeamIds = await tenantDb.FactoryTeamMembers
        //             .Where(x => x.UserId == assignedToUserId && !x.IsDeleted && x.IsActive)
        //             .Select(x => x.TeamId)
        //             .ToListAsync();
        //     }

        //     var maintenanceRoleId = await tenantDb.FactoryRoles
        //         .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
        //         .Select(x => x.RoleId)
        //         .FirstOrDefaultAsync();

        //     var productionRoleId = await tenantDb.FactoryRoles
        //         .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
        //         .Select(x => x.RoleId)
        //         .FirstOrDefaultAsync();
        //     var maintenanceUsers = await (
        //         from fur in tenantDb.FactoryUserRoles
        //         join tm in tenantDb.FactoryTeamMembers on fur.UserId equals tm.UserId
        //         where fur.TenantId == tenantId
        //               && fur.RoleId == maintenanceRoleId
        //               && !fur.IsDeleted
        //               && !tm.IsDeleted
        //               && tm.IsActive
        //               && assignedUserTeamIds.Contains(tm.TeamId)
        //         select fur.UserId
        //     ).Distinct().ToListAsync();


        //     var productionUsers = await (
        //         from fur in tenantDb.FactoryUserRoles
        //         join tm in tenantDb.FactoryTeamMembers on fur.UserId equals tm.UserId
        //         where fur.TenantId == tenantId
        //               && fur.RoleId == productionRoleId
        //               && !fur.IsDeleted
        //               && !tm.IsDeleted
        //               && tm.IsActive
        //               && assignedUserTeamIds.Contains(tm.TeamId)
        //         select fur.UserId
        //     ).Distinct().ToListAsync();

        //     var incoming = maintenanceUsers
        //         .Concat(productionUsers)
        //         .Concat(assignedToUserId.HasValue ? new[] { assignedToUserId.Value } : Array.Empty<int>())
        //         .Concat(createdBy.HasValue ? new[] { createdBy.Value } : Array.Empty<int>())
        //         .Concat(assetAssignedUserId.HasValue ? new[] { assetAssignedUserId.Value } : Array.Empty<int>())
        //         .Distinct()
        //         .ToList();

        //     if (actionUserId.HasValue)
        //     {
        //         incoming = incoming.Where(x => x != actionUserId.Value).ToList();
        //     }

        //     result.Incoming = incoming.Select(x => (int?)x).ToList();

        //     result.AllUsers = result.Incoming
        //         .Concat(actionUserId.HasValue ? new List<int?> { actionUserId } : new List<int?>())
        //         .Where(x => x.HasValue)
        //         .Distinct()
        //         .ToList();

        //     var emails = await (
        //          from fur in tenantDb.FactoryUserRoles
        //          join tm in tenantDb.FactoryTeamMembers on fur.UserId equals tm.UserId
        //          join fu in tenantDb.FactoryUsers on fur.UserId equals fu.UserId
        //          where fur.TenantId == tenantId
        //                && (fur.RoleId == maintenanceRoleId || fur.RoleId == productionRoleId)
        //                && !fur.IsDeleted
        //                && !tm.IsDeleted
        //                && tm.IsActive
        //                && assignedUserTeamIds.Contains(tm.TeamId)
        //          select fu.Email
        //      ).Distinct().ToListAsync();

        //     if (assignedToUserId.HasValue)
        //     {
        //         var assignedEmail = await tenantDb.FactoryUsers
        //             .Where(x => x.UserId == assignedToUserId && !x.IsDeleted)
        //             .Select(x => x.Email)
        //             .FirstOrDefaultAsync();

        //         if (!string.IsNullOrEmpty(assignedEmail))
        //             emails.Add(assignedEmail);
        //     }

        //     if (createdBy.HasValue)
        //     {
        //         var createdByEmail = await tenantDb.FactoryUsers
        //             .Where(x => x.UserId == createdBy && !x.IsDeleted)
        //             .Select(x => x.Email)
        //             .FirstOrDefaultAsync();

        //         if (!string.IsNullOrEmpty(createdByEmail))
        //             emails.Add(createdByEmail);
        //     }

        //     if (assetAssignedUserId.HasValue)
        //     {
        //         var assetEmail = await tenantDb.FactoryUsers
        //             .Where(x => x.UserId == assetAssignedUserId && !x.IsDeleted)
        //             .Select(x => x.Email)
        //             .FirstOrDefaultAsync();

        //         if (!string.IsNullOrEmpty(assetEmail))
        //             emails.Add(assetEmail);
        //     }

        //     result.Emails = emails.Distinct().ToList();

        //     return result;
        // }

        //public async Task<NotificationUsersResult> BuildAsync(
        //    TenantDbContextFactory _tenantDbContext,
        //    int tenantId,
        //    int? teamId,
        //    int? actionUserId, // sender
        //    int? assignedToUserId,
        //    int? createdBy,
        //    int? assetAssignedUserId = null)
        //{
        //    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

        //    var result = new NotificationUsersResult
        //    {
        //        Outgoing = actionUserId
        //    };

        //    // =========================
        //    // ROLE IDS
        //    // =========================
        //    var maintenanceRoleId = await tenantDb.FactoryRoles
        //        .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
        //        .Select(x => x.RoleId)
        //        .FirstOrDefaultAsync();

        //    var productionRoleId = await tenantDb.FactoryRoles
        //        .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
        //        .Select(x => x.RoleId)
        //        .FirstOrDefaultAsync();

        //    List<int> maintenanceUsers = new();
        //    List<int> productionUsers = new();

        //    // =========================
        //    // TEAM SPECIFIC LOGIC
        //    // =========================
        //    if (teamId.HasValue)
        //    {
        //        // Team users
        //        var teamUserIds = await tenantDb.FactoryTeamMembers
        //            .Where(x =>
        //                x.TeamId == teamId.Value &&
        //                x.IsActive &&
        //                !x.IsDeleted &&
        //                x.UserId.HasValue)
        //            .Select(x => x.UserId!.Value)
        //            .Distinct()
        //            .ToListAsync();

        //        // Maintenance Supervisor only from team
        //        maintenanceUsers = await tenantDb.FactoryUserRoles
        //            .Where(x =>
        //                x.TenantId == tenantId &&
        //                x.RoleId == maintenanceRoleId &&
        //                !x.IsDeleted &&
        //                teamUserIds.Contains(x.UserId))
        //            .Select(x => x.UserId)
        //            .Distinct()
        //            .ToListAsync();

        //        // Production Supervisor only from team
        //        productionUsers = await tenantDb.FactoryUserRoles
        //            .Where(x =>
        //                x.TenantId == tenantId &&
        //                x.RoleId == productionRoleId &&
        //                !x.IsDeleted &&
        //                teamUserIds.Contains(x.UserId))
        //            .Select(x => x.UserId)
        //            .Distinct()
        //            .ToListAsync();

        //        // Include ALL team users
        //        maintenanceUsers.AddRange(teamUserIds);
        //    }
        //    else
        //    {
        //        // =========================
        //        // EXISTING FUNCTIONALITY
        //        // (ALL TENANT SUPERVISORS)
        //        // =========================
        //        maintenanceUsers = await tenantDb.FactoryUserRoles
        //            .Where(x =>
        //                x.TenantId == tenantId &&
        //                x.RoleId == maintenanceRoleId &&
        //                !x.IsDeleted)
        //            .Select(x => x.UserId)
        //            .Distinct()
        //            .ToListAsync();

        //        productionUsers = await tenantDb.FactoryUserRoles
        //            .Where(x =>
        //                x.TenantId == tenantId &&
        //                x.RoleId == productionRoleId &&
        //                !x.IsDeleted)
        //            .Select(x => x.UserId)
        //            .Distinct()
        //            .ToListAsync();
        //    }

        //    // =========================
        //    // BUILD USER LIST
        //    // =========================
        //    var incoming = maintenanceUsers
        //        .Concat(productionUsers)
        //        .Concat(assignedToUserId.HasValue ? new[] { assignedToUserId.Value } : Array.Empty<int>())
        //        .Concat(createdBy.HasValue ? new[] { createdBy.Value } : Array.Empty<int>())
        //        .Concat(assetAssignedUserId.HasValue ? new[] { assetAssignedUserId.Value } : Array.Empty<int>())
        //        .Distinct()
        //        .ToList();

        //    // Remove sender
        //    if (actionUserId.HasValue)
        //    {
        //        incoming = incoming
        //            .Where(x => x != actionUserId.Value)
        //            .ToList();
        //    }

        //    result.Incoming = incoming
        //        .Select(x => (int?)x)
        //        .Distinct()
        //        .ToList();

        //    result.AllUsers = result.Incoming
        //        .Concat(actionUserId.HasValue
        //            ? new List<int?> { actionUserId }
        //            : new List<int?>())
        //        .Where(x => x.HasValue)
        //        .Distinct()
        //        .ToList();

        //    // =========================
        //    // EMAILS
        //    // =========================
        //    var emailUserIds = result.AllUsers
        //        .Where(x => x.HasValue)
        //        .Select(x => x!.Value)
        //        .Distinct()
        //        .ToList();

        //    result.Emails = await tenantDb.FactoryUsers
        //        .Where(x =>
        //            emailUserIds.Contains(x.UserId) &&
        //            !x.IsDeleted)
        //        .Select(x => x.Email)
        //        .Distinct()
        //        .ToListAsync();

        //    return result;
        //}



    public async Task<NotificationUsersResult> BuildAsync(
    TenantDbContextFactory _tenantDbContext,
    int tenantId,
    int? actionUserId,
    int? assignedToUserId,
    int? createdBy,
    int? assetAssignedUserId = null)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var result = new NotificationUsersResult
            {
                Outgoing = actionUserId
            };

            // =========================
            // ROLE IDS
            // =========================
            var maintenanceRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            var productionRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            List<int> maintenanceUsers;
            List<int> productionUsers;

            // =========================
            // CASE 1: Assigned User Exists → TEAM BASED
            // =========================

            //if (assignedToUserId.HasValue)
            //{
            //    var teamIds = await tenantDb.FactoryTeamMembers
            //        .Where(x => x.UserId == assignedToUserId && !x.IsDeleted && x.IsActive)
            //        .Select(x => x.TeamId)
            //        .ToListAsync();

            //    maintenanceUsers = await (
            //        from fur in tenantDb.FactoryUserRoles
            //        join tm in tenantDb.FactoryTeamMembers on fur.UserId equals tm.UserId
            //        where fur.TenantId == tenantId
            //              && fur.RoleId == maintenanceRoleId
            //              && !fur.IsDeleted
            //              && !tm.IsDeleted
            //              && tm.IsActive
            //              && teamIds.Contains(tm.TeamId)
            //        select fur.UserId
            //    ).Distinct().ToListAsync();

            //    productionUsers = await (
            //        from fur in tenantDb.FactoryUserRoles
            //        join tm in tenantDb.FactoryTeamMembers on fur.UserId equals tm.UserId
            //        where fur.TenantId == tenantId
            //              && fur.RoleId == productionRoleId
            //              && !fur.IsDeleted
            //              && !tm.IsDeleted
            //              && tm.IsActive
            //              && teamIds.Contains(tm.TeamId)
            //        select fur.UserId
            //    ).Distinct().ToListAsync();
            //}
            //else
            //{
            //    // =========================
            //    // ✅ CASE 2: No Assigned User → ALL SUPERVISORS
            //    // =========================
            //    maintenanceUsers = await tenantDb.FactoryUserRoles
            //        .Where(x => x.TenantId == tenantId &&
            //                    x.RoleId == maintenanceRoleId &&
            //                    !x.IsDeleted)
            //        .Select(x => x.UserId)
            //        .Distinct()
            //        .ToListAsync();

            //    productionUsers = await tenantDb.FactoryUserRoles
            //        .Where(x => x.TenantId == tenantId &&
            //                    x.RoleId == productionRoleId &&
            //                    !x.IsDeleted)
            //        .Select(x => x.UserId)
            //        .Distinct()
            //        .ToListAsync();
            //}

            // =========================
            // TEAM BASED FILTER
            // =========================
            List<int> teamIds = new();

            if (assignedToUserId.HasValue)
            {
                teamIds = await tenantDb.FactoryTeamMembers
                    .Where(x => x.UserId == assignedToUserId && !x.IsDeleted && x.IsActive)
                    .Select(x => x.TeamId)
                    .ToListAsync();
            }
            else if (actionUserId.HasValue)
            {
                teamIds = await tenantDb.FactoryTeamMembers
                    .Where(x => x.UserId == actionUserId && !x.IsDeleted && x.IsActive)
                    .Select(x => x.TeamId)
                    .ToListAsync();
            }

            // =========================
            // USERS FETCH
            // =========================
            if (teamIds.Any())
            {
                maintenanceUsers = await (
                    from fur in tenantDb.FactoryUserRoles
                    join tm in tenantDb.FactoryTeamMembers on fur.UserId equals tm.UserId
                    where fur.TenantId == tenantId
                          && fur.RoleId == maintenanceRoleId
                          && !fur.IsDeleted
                          && !tm.IsDeleted
                          && tm.IsActive
                          && teamIds.Contains(tm.TeamId)
                    select fur.UserId
                ).Distinct().ToListAsync();

                productionUsers = await (
                    from fur in tenantDb.FactoryUserRoles
                    join tm in tenantDb.FactoryTeamMembers on fur.UserId equals tm.UserId
                    where fur.TenantId == tenantId
                          && fur.RoleId == productionRoleId
                          && !fur.IsDeleted
                          && !tm.IsDeleted
                          && tm.IsActive
                          && teamIds.Contains(tm.TeamId)
                    select fur.UserId
                ).Distinct().ToListAsync();
            }
            else
            {
                // fallback (same as before)
                maintenanceUsers = await tenantDb.FactoryUserRoles
                    .Where(x => x.TenantId == tenantId &&
                                x.RoleId == maintenanceRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync();

                productionUsers = await tenantDb.FactoryUserRoles
                    .Where(x => x.TenantId == tenantId &&
                                x.RoleId == productionRoleId &&
                                !x.IsDeleted)
                    .Select(x => x.UserId)
                    .Distinct()
                    .ToListAsync();
            }


            // =========================
            // BUILD USER LIST
            // =========================
            var incoming = maintenanceUsers
                .Concat(productionUsers)
                .Concat(assignedToUserId.HasValue ? new[] { assignedToUserId.Value } : Array.Empty<int>())
                .Concat(createdBy.HasValue ? new[] { createdBy.Value } : Array.Empty<int>())
                .Concat(assetAssignedUserId.HasValue ? new[] { assetAssignedUserId.Value } : Array.Empty<int>())
                .Distinct()
                .ToList();

            // Remove sender
            if (actionUserId.HasValue)
            {
                incoming = incoming.Where(x => x != actionUserId.Value).ToList();
            }

            result.Incoming = incoming.Select(x => (int?)x).ToList();

            result.AllUsers = result.Incoming
                .Concat(actionUserId.HasValue ? new List<int?> { actionUserId } : new List<int?>())
                .Where(x => x.HasValue)
                .Distinct()
                .ToList();

            // =========================
            // EMAILS (OPTIMIZED)
            // =========================
            var userIds = result.AllUsers
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            result.Emails = await tenantDb.FactoryUsers
                .Where(x => userIds.Contains(x.UserId) && !x.IsDeleted)
                .Select(x => x.Email)
                .Distinct()
                .ToListAsync();

            return result;
        }

        /*public async Task<NotificationUsersResult> BuildAsync(
           TenantDbContextFactory _tenantDbContext,
           int tenantId,
           int? actionUserId,
           int? TeamId,
           int? assignedToUserId,
           int? createdBy,
           int? assetAssignedUserId = null)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

            var result = new NotificationUsersResult
            {
                Outgoing = actionUserId
            };

            var maintenanceRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Maintenance Supervisor" && !x.IsDeleted)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            var productionRoleId = await tenantDb.FactoryRoles
                .Where(x => x.RoleName == "Production Supervisor" && !x.IsDeleted)
                .Select(x => x.RoleId)
                .FirstOrDefaultAsync();

            var maintenanceUsers = await tenantDb.FactoryUserRoles
                .Where(x => x.TenantId == tenantId && x.RoleId == maintenanceRoleId && !x.IsDeleted)
                .Select(x => x.UserId)
                .ToListAsync();

            var productionUsers = await tenantDb.FactoryUserRoles
                .Where(x => x.TenantId == tenantId && x.RoleId == productionRoleId && !x.IsDeleted)
                .Select(x => x.UserId)
                .ToListAsync();

            var incoming = maintenanceUsers
                .Concat(productionUsers)
                .Concat(assignedToUserId.HasValue ? new[] { assignedToUserId.Value } : Array.Empty<int>())
                .Concat(createdBy.HasValue ? new[] { createdBy.Value } : Array.Empty<int>())
                .Concat(assetAssignedUserId.HasValue ? new[] { assetAssignedUserId.Value } : Array.Empty<int>())
                .Distinct()
                .ToList();

            if (actionUserId.HasValue)
            {
                incoming = incoming.Where(x => x != actionUserId.Value).ToList();
            }

            result.Incoming = incoming.Select(x => (int?)x).ToList();

            result.AllUsers = result.Incoming
                .Concat(actionUserId.HasValue ? new List<int?> { actionUserId } : new List<int?>())
                .Where(x => x.HasValue)
                .Distinct()
                .ToList();

            var emails = await tenantDb.FactoryUserRoles
                .Where(x => x.TenantId == tenantId &&
                           (x.RoleId == maintenanceRoleId || x.RoleId == productionRoleId) &&
                            !x.IsDeleted)
                .Select(x => x.FactoryUsers.Email)
                .ToListAsync();

            if (assignedToUserId.HasValue)
            {
                var assignedEmail = await tenantDb.FactoryUsers
                    .Where(x => x.UserId == assignedToUserId && !x.IsDeleted)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(assignedEmail))
                    emails.Add(assignedEmail);
            }

            if (createdBy.HasValue)
            {
                var createdByEmail = await tenantDb.FactoryUsers
                    .Where(x => x.UserId == createdBy && !x.IsDeleted)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(createdByEmail))
                    emails.Add(createdByEmail);
            }

            if (assetAssignedUserId.HasValue)
            {
                var assetEmail = await tenantDb.FactoryUsers
                    .Where(x => x.UserId == assetAssignedUserId && !x.IsDeleted)
                    .Select(x => x.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(assetEmail))
                    emails.Add(assetEmail);
            }

            result.Emails = emails.Distinct().ToList();

            return result;
        }*/
    }
}
