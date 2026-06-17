using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Configuration
{
    public class TenantMigrationRepository : ITenantMigrationRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbcontext;
        private readonly TenantDbContextFactory _tenantDbContext;

        public TenantMigrationRepository(
             MasterFactoryOpsDbContext masterDbcontext,
             TenantDbContextFactory tenantDbContext)
        {
            _masterDbcontext = masterDbcontext;
            _tenantDbContext = tenantDbContext;
        }


        /*        private const string migrationSql = @"
                    CREATE TABLE IF NOT EXISTS ""ServiceRequestWorkflowLogs""
                    (
                        ""LogId"" SERIAL PRIMARY KEY,
                        ""TenantId"" INT NOT NULL,

                        ""ServiceRequestId"" INT NOT NULL,

                        ""ActionType"" VARCHAR(100) NULL,

                        ""PerformedBy"" INT NULL,
                        ""PerformedAt"" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

                        ""Notes"" TEXT NULL,

                        CONSTRAINT ""FK_SRWorkflowLogs_ServiceRequests""
                            FOREIGN KEY (""ServiceRequestId"")
                            REFERENCES ""ServiceRequests"" (""ServiceRequestId"")
                            ON DELETE CASCADE
                    );
                    ";*/

        //private const string migrationSql = @"
        //CREATE TABLE ""ProcessedMessage""(
        //    ""MessageId"" TEXT PRIMARY KEY,
        //    ""ProcessedAt"" TIMESTAMPTZ DEFAULT NOW()
        //);
        //";


        //   private const string migrationSql = @"
        //     ALTER TABLE ""WorkOrders""
        //     RENAME COLUMN ""WorkOrderPhoto"" TO ""WorkOrderMedia"";

        //     ALTER TABLE ""WorkOrders""
        //    RENAME COLUMN ""WorkOrderPhotoPath"" TO ""WorkOrderMediaPath"";
        //        ";

        private const string migrationSql = @"
           ALTER TABLE ""MasterNotification""
           Add COLUMN ""OutgoingNotification"" INT;
            ";


        //CREATE TABLE ""AssetBillOfMaterials""
        //(
        //    ""BomPartId"" SERIAL PRIMARY KEY,

        //    ""TenantId"" INT NOT NULL,
        //    ""AssetId"" INT NOT NULL,

        //    ""PartNumber"" VARCHAR(100) NOT NULL,
        //    ""PartName"" VARCHAR(150) NOT NULL,
        //    ""Description"" TEXT,
        //    ""Category"" VARCHAR(100),

        //    ""Quantity"" INT NOT NULL CHECK (""Quantity"" >= 0),
        //    ""UnitCost"" NUMERIC(12,2),
        //    ""MinimumStockLevel"" INT,
        //    ""LeadTimeDays"" INT,

        //    ""Supplier"" VARCHAR(150),
        //    ""StorageLocation"" VARCHAR(150),
        //    ""CompatibleModels"" TEXT,

        //    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE,
        //    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,

        //    ""CreatedBy"" INT,
        //    ""UpdatedBy"" INT,
        //    ""DeletedBy"" INT,
        //    ""CreatedAt"" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
        //    ""UpdatedAt"" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
        //    ""DeletedAt"" TIMESTAMPTZ,

        //    CONSTRAINT ""FK_AssetBillOfMaterials_AssetRegistry""
        //        FOREIGN KEY (""AssetId"")
        //        REFERENCES ""AssetRegistry"" (""AssetId"")
        //        ON DELETE CASCADE);
        //";


        //        private const string migrationSql = @"
        //-----------------------------------------------------
        //-- RESET PARENT PERMISSIONS
        //-----------------------------------------------------
        //TRUNCATE TABLE ""FactorySubPermission"" RESTART IDENTITY CASCADE;
        //TRUNCATE TABLE ""FactoryPermissions"" RESTART IDENTITY CASCADE;

        //-----------------------------------------------------
        //-- INSERT PARENT MODULES
        //-----------------------------------------------------
        //INSERT INTO ""FactoryPermissions""
        //(""Name"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""CreatedBy"", 
        // ""UpdatedAt"", ""UpdatedBy"", ""DeletedAt"", ""DeletedBy"")
        //VALUES
        //('Workspace', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('TenantAdmin', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('WorkOrders', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('Preventive Maintenance', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('Team Management', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('Asset Management', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('Inventory Management', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('IOT Devices', true, false, NOW(), 1, NULL, NULL, NULL, NULL),
        //('Analytics & Reporting', true, false, NOW(), 1, NULL, NULL, NULL, NULL);

        //-----------------------------------------------------
        //-- INSERT SUB PERMISSIONS
        //-----------------------------------------------------
        //INSERT INTO ""FactorySubPermission"" 
        //(""ParentPermissionId"", ""Name"", ""IsActive"", ""IsDeleted"", ""CreatedAt"", ""CreatedBy"")
        //VALUES

        //-- ========= Workspace: Parent 1 =========
        //(1,'Dashboard & KPIs',true,false,NOW(),1),
        //(1,'Service Requests',true,false,NOW(),1),
        //(1,'Tracking & Updates',true,false,NOW(),1),
        //(1,'Schedule Management',true,false,NOW(),1),
        //(1,'Asset Tracking',true,false,NOW(),1),
        //(1,'Inventory Tracking',true,false,NOW(),1),
        //(1,'Progress Tracking',true,false,NOW(),1),

        //-- ========= Tenant Admin: Parent 2 =========
        //(2,'KPIs & Dashboard',true,false,NOW(),1),
        //(2,'Roles & Permissions',true,false,NOW(),1),
        //(2,'User Management',true,false,NOW(),1),
        //(2,'Team Management',true,false,NOW(),1),
        //(2,'Location Management',true,false,NOW(),1),
        //(2,'Space Management',true,false,NOW(),1),
        //(2,'Notifications',true,false,NOW(),1),
        //(2,'Support Tickets',true,false,NOW(),1),
        //(2,'Tickets Feedback',true,false,NOW(),1),
        //(2,'Audit Logs',true,false,NOW(),1),
        //(2,'Integration & Linkage',true,false,NOW(),1),
        //(2,'TenantAdmin Analytics & Reporting',true,false,NOW(),1),

        //-- ========= WorkOrder: Parent 3 =========
        //(3,'WO Management',true,false,NOW(),1),
        //(3,'Service Requests',true,false,NOW(),1),
        //(3,'Resource Utilization',true,false,NOW(),1),
        //(3,'Assignment & Dispatch',true,false,NOW(),1),
        //(3,'Task Management',true,false,NOW(),1),
        //(3,'Team Management',true,false,NOW(),1),
        //(3,'Tracking & Updates',true,false,NOW(),1),
        //(3,'Inventory & Costs',true,false,NOW(),1),
        //(3,'Integrations & Linkages',true,false,NOW(),1),
        //(3,'WorkOrder Analytics & Reporting',true,false,NOW(),1),

        //-- ========= Preventive Maintenance: Parent 4 =========
        //(4,'Schedule Management',true,false,NOW(),1),
        //(4,'Work Order',true,false,NOW(),1),
        //(4,'Task Management',true,false,NOW(),1),
        //(4,'Alerts & Notifications',true,false,NOW(),1),
        //(4,'Integrations & Linkages',true,false,NOW(),1),
        //(4,'Maintenance Analytics & Reporting',true,false,NOW(),1),

        //-- ========= Team Management: Parent 5 =========
        //(5,'Points System',true,false,NOW(),1),
        //(5,'Achievements',true,false,NOW(),1),
        //(5,'Progress Tracking',true,false,NOW(),1),
        //(5,'Leaderboards',true,false,NOW(),1),
        //(5,'Challenges',true,false,NOW(),1),
        //(5,'Notifications',true,false,NOW(),1),
        //(5,'Training',true,false,NOW(),1),
        //(5,'Integrations & Linkages',true,false,NOW(),1),
        //(5,'TeamManagement Analytics & Reporting',true,false,NOW(),1),

        //-- ========= Asset: Parent 6 =========
        //(6,'Asset Registry',true,false,NOW(),1),
        //(6,'Asset Tracking',true,false,NOW(),1),
        //(6,'Maintenance History',true,false,NOW(),1),
        //(6,'Lifecycle & Financials',true,false,NOW(),1),
        //(6,'Documents & Compliance',true,false,NOW(),1),
        //(6,'Asset Analytics & Reporting',true,false,NOW(),1),
        //(6,'Parts / BOM',true,false,NOW(),1),

        //-- ========= Inventory: Parent 7 =========
        //(7,'Inventory Registry',true,false,NOW(),1),
        //(7,'Inventory Tracking',true,false,NOW(),1),
        //(7,'Inventory Transactions',true,false,NOW(),1),
        //(7,'Procurement',true,false,NOW(),1),
        //(7,'Costing & Valuation',true,false,NOW(),1),
        //(7,'Integrations & Linkages',true,false,NOW(),1),
        //(7,'Inventory Analytics & Reporting',true,false,NOW(),1),

        //-- ========= IoT: Parent 8 =========
        //(8,'Dashboard',true,false,NOW(),1),
        //(8,'Device Management',true,false,NOW(),1),
        //(8,'Topic Management',true,false,NOW(),1),
        //(8,'Data Hub',true,false,NOW(),1),
        //(8,'Alerts & Rules',true,false,NOW(),1),
        //(8,'Configuration',true,false,NOW(),1),
        //(8,'Integrations & Linkages',true,false,NOW(),1),
        //(8,'IOT Analytics & Reporting',true,false,NOW(),1),

        //-- ========= Global Analytics: Parent 9 =========
        //(9,'GA Dashboards & KPIs',true,false,NOW(),1),
        //(9,'GA Work Orders',true,false,NOW(),1),
        //(9,'GA Preventive Maintenance',true,false,NOW(),1),
        //(9,'GA Predictive Maintenance',true,false,NOW(),1),
        //(9,'GA Teams & Workforce',true,false,NOW(),1),
        //(9,'GA Asset Performance',true,false,NOW(),1),
        //(9,'GA Inventory Analytics',true,false,NOW(),1),
        //(9,'GA IoT Devices',true,false,NOW(),1),
        //(9,'Financial Reporting',true,false,NOW(),1),
        //(9,'Utilization & OEE',true,false,NOW(),1),
        //(9,'Compliance & Safety',true,false,NOW(),1),
        //(9,'AI Insights',true,false,NOW(),1);
        //";


        /* private const string migrationSql = @"
         ALTER TABLE ""WorkOrders""
         DROP COLUMN ""TotalTime"";

         ALTER TABLE ""WorkOrders""
         ADD COLUMN ""TotalTime"" VARCHAR(255) NULL ;

         ALTER TABLE ""PurchaseRequisition""
         DROP COLUMN ""SupplierAcceptanceStatus"";

         ALTER TABLE ""PurchaseRequisition""
         ADD COLUMN ""SupplierAcceptanceStatus"" INTEGER NULL ;
         ";

        /*private const string migrationSql = @"
        ALTER TABLE ""FactoryPermissions""
ALTER COLUMN ""TenantId"" DROP NOT NULL;

        ALTER TABLE ""FactoryPermissions""
ALTER COLUMN ""TenantId"" DROP DEFAULT;
        ";*/



        //private const string migrationSql = @"
        //    ALTER TABLE ""AssetRegistry""
        //    ADD COLUMN IF NOT EXISTS ""AssetUniqueId"" VARCHAR(255) UNIQUE;";

        //private const string migrationSql = @"";

        public async Task<CommonResponseModel> ApplySchemaMigrationAsync()
        {
            CommonResponseModel response = new();
            try
            {
                var tenants = await _masterDbcontext.FactoryTenants
                    .Where(l => l.IsActive && !l.IsDeleted)
                    .ToListAsync();

                foreach (var tenant in tenants)
                {
                    using var tenantDb = _tenantDbContext.GetTenantDbContext(tenant.TenantId);
                    await tenantDb.Database.ExecuteSqlRawAsync(migrationSql);

                }

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantMigrationStatusMessage.MigrationAppilied;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Migration failed: {ex.Message}";
            }
            return response;
        }

        public async Task<CommonResponseModel> ApplySchemaMigrationByTenantAsync(int tenantId)
        {
            CommonResponseModel response = new();
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
                await tenantDb.Database.ExecuteSqlRawAsync(migrationSql);
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = TenantMigrationStatusMessage.MigrationAppilied;
            }
            catch (Exception ex)
            {
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Migration failed: {ex.Message}";
            }
            return response;
        }
    }
}
