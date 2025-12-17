using FactoryOperation_WorkOrder.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using Microsoft.EntityFrameworkCore;


namespace FactoryOpsApp.Infrastructure.DBContext;

public class FactoryOpsDBContext : DbContext
{
    public FactoryOpsDBContext(DbContextOptions<FactoryOpsDBContext> options) : base(options) { }
    
    public DbSet<FactoryUsers> FactoryUsers { get; set; }
    public DbSet<FactoryRoles> FactoryRoles { get; set; }
    public DbSet<FactoryPermission> FactoryPermissions { get; set; }
    public DbSet<FactoryUserRoles> FactoryUserRoles { get; set; }
    public DbSet<FactoryTeam> FactoryTeams { get; set; }
    public DbSet<Location> Locations { get; set; }

    
    public DbSet<AssetRegistry> AssetRegistry { get; set; }
    public DbSet<AssetTracking> AssetTracking { get; set; }

    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderBulkImport> WorkOrderBulkImports { get; set; }
    public DbSet<WorkOrderRequiredTool> WorkOrderRequiredTools { get; set; }
    public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
    public DbSet<WorkOrderSubTask> WorkOrderSubTasks { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<InventoryTransaction> InventoryTransaction { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<MaintenanceScheduleOccurrence> MaintenanceScheduleOccurrences { get; set; }
    public DbSet<InventoryCostIntegration> InventoryCostIntegrations { get; set; }
    public DbSet<PointAssignment> PointAssignments { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<FactoryOpsApp.Domain.Entities.FactoryOpsTenants.Challenge> Challenges { get; set; }
    public DbSet<TeamAlertNotification> TeamAlertNotifications { get; set; }
    public DbSet<SupplierManagement> SupplierManagement { get; set; }
    public DbSet<ReorderRule> ReorderRules { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<MasterNotification> MasterNotifications { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<FactoryUserRoles>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<FactoryUserRoles>()
                .HasOne(ur => ur.FactoryUsers)
                .WithMany(u => u.FactoryUserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<FactoryUserRoles>()
                .HasOne(ur => ur.FactoryRoles)
                .WithMany(r => r.FactoryUserRoles)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<FactoryTeam>()
                 .HasOne(ft => ft.Manager)                  
                 .WithMany()                               
                 .HasForeignKey(ft => ft.ManagerId)         
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Location>()
                .Property(l => l.LocationType)
                .HasConversion<string>()
                .HasColumnType("text"); 

            modelBuilder.Entity<AssetDocuments>()
                .Property(d => d.DocumentType)
                .HasConversion<string>();

            modelBuilder.Entity<AssetLifecycle>()
                .Property(a => a.Stage)
                .HasConversion<string>();

            modelBuilder.Entity<MaintenanceHistory>()
                .Property(m => m.MaintenanceType)
                .HasConversion<string>();

            modelBuilder.Entity<AssetTracking>()
                .Property(at => at.Status)
                .HasConversion<string>();

            modelBuilder.Entity<AssetTracking>()
                .HasOne(at => at.AssignedUser)
                .WithMany()
                .HasForeignKey(at => at.AssignedTo)
                .HasConstraintName("FK_AssetTracking_FactoryUsers_AssignedTo");

            modelBuilder.Entity<MaintenanceHistory>()
                .Property(m => m.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<AssetLifecycle>()
               .Property(a => a.Stage)
               .HasConversion<string>()
               .HasMaxLength(50);

            modelBuilder.Entity<AssetLifecycle>()
                .Property(a => a.AnalysisType)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<AssetDocuments>()
                .Property(d => d.DocumentType)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<AssetDocuments>()
                .Property(d => d.Category)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<AssetDocuments>()
                .Property(d => d.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<AssetDashboard_Report>()
                .Property(d => d.SelectedReportType)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<AssetDashboard_Report>()
                .Property(d => d.SelectedDateRangeOption)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<AssetDashboard_Report>()
                .Property(d => d.SelectedExportFormat)
                .HasConversion<string>()
                .HasMaxLength(50);


            modelBuilder.Entity<MaintenanceSchedule>()
                .Property(s => s.ScheduleType)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<MaintenanceSchedule>()
                .Property(s => s.Frequency)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<MaintenanceSchedule>()
                .Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
            modelBuilder.Entity<MaintenanceSchedule>()
                .HasMany(s => s.Occurrences)
                .WithOne(o => o.MaintenanceSchedule)
                .HasForeignKey(o => o.ScheduleId);
            modelBuilder.Entity<MaintenanceScheduleOccurrence>()
               .Property(o => o.FrequencyType)
               .HasConversion<string>() 
               .HasMaxLength(50);

            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.Priority)
                .HasConversion<string>()
                .HasMaxLength(20); 
        
            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.WorkOrderType)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<MaintenanceTask>()
                .Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<WorkOrderSubTask>()
                .Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<WorkOrderSubTask>()
               .Property(t => t.Priority)
               .HasConversion<string>()
               .HasMaxLength(20);
            modelBuilder.Entity<WorkOrderSubTask>()
                .Property(w => w.StartDate)
                .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<WorkOrderSubTask>()
               .Property(w => w.EndDate)
               .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<WorkOrderSubTask>()
              .Property(w => w.CompletedDate)
              .HasColumnType("timestamp without time zone");
            modelBuilder.Entity<WorkOrderSubTask>()
              .Property(w => w.CreatedAt)
              .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<WorkOrderSubTask>()
                .Property(w => w.UpdatedAt)
                .HasColumnType("timestamp without time zone");
   
            modelBuilder.Entity<InventoryCostIntegration>(entity =>
            {
                entity.ToTable("InventoryCostIntegration");
                entity.Property(e => e.UnitCost)
                    .HasColumnType("numeric(18,2)");

                modelBuilder.Entity<InventoryCostIntegration>()
                    .Property(p => p.IsActive)
                    .HasColumnType("boolean")
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp without time zone");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp without time zone");
                entity.HasOne(e => e.WorkOrder)
                    .WithMany()
                    .HasForeignKey(e => e.WorkOrderId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Inventory)
                    .WithMany()
                    .HasForeignKey(e => e.InventoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Inventory>()
                  .Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(50);

            modelBuilder.Entity<Inventory>()
                  .Property(e => e.Category)
                  .HasConversion<string>()
                  .HasMaxLength(50);
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.Priority)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.RequestType)
                .HasConversion<string>()
                .HasMaxLength(30);

            modelBuilder.Entity<InventoryTransaction>()
                  .Property(e => e.TransactionType)
                  .HasConversion<string>()
                  .HasMaxLength(50);

            modelBuilder.Entity<InventoryTransaction>()
                  .Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(50);

            modelBuilder.Entity<ReorderRule>()
                .Property(r => r.Priority)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<PurchaseRequisition>()
                .Property(p => p.Priority)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<PurchaseRequisition>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
    
            modelBuilder.Entity<AssetRegistry>()
                .Property(e => e.Criticality)
                .HasConversion<string>() 
                .HasMaxLength(10);

            modelBuilder.Entity<TeamAlertNotification>()
          .Property(n => n.NotificationType)
          .HasConversion<string>(); 

            modelBuilder.Entity<TeamAlertNotification>()
                .Property(n => n.TriggerType)
                .HasConversion<string>();

        }

}
 
