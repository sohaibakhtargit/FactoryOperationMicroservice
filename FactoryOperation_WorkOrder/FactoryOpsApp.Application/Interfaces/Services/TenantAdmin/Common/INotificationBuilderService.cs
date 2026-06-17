using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Models;
using FactoryOpsApp.Infrastructure.DBContext;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common
{
    public interface INotificationBuilderService
    {


        Task<NotificationUsersResult> BuildAsync(
            TenantDbContextFactory _tenantDbContext,
            int tenantId,
            int? actionUserId,
            int? assignedToUserId,
            int? createdBy,
            int? assetAssignedUserId = null
        );
        /* Task<NotificationUsersResult> BuildAsync(
             TenantDbContextFactory _tenantDbContext,
             int tenantId,
             int? TeamId,
             int? actionUserId,      // UpdatedBy / DeletedBy / CreatedBy
             int? assignedToUserId,
             int? createdBy,
             int? assetAssignedUserId = null
         );*/
    }
}
