using FactoryOperation_IOTDevices.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
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
    public DbSet<FactoryRolePermissions> FactoryRolePermissions { get; set; }
    public DbSet<FactoryTeam> FactoryTeams { get; set; }
    public DbSet<FactorySupportTickets> FactorySupportTickets { get; set; }
    public DbSet<FactoryNotificationRules> FactoryNotificationRules { get; set; }
    public DbSet<FactoryTeamMembers> FactoryTeamMembers { get; set; }
    public DbSet<FeedbackMetric> FeedbackMetrics { get; set; }
    public DbSet<SupportFeedback> SupportFeedback { get; set; }


    #region
    public DbSet<Location> Locations { get; set; }
    public DbSet<FactoryAssetType> FactoryAssetTypes { get; set; }
    public DbSet<AssetRegistry> AssetRegistry { get; set; }
    public DbSet<AssetTracking> AssetTracking { get; set; }
    public DbSet<MaintenanceHistory> MaintenanceHistory { get; set; }
    public DbSet<AssetLifecycle> AssetLifecycles { get; set; }
    public DbSet<AssetFinancialAnalysis> AssetFinancialAnalysis { get; set; }
    public DbSet<AssetDocuments> AssetDocuments { get; set; }
    public DbSet<AssetDashboard_Report> AssetDashboard_Reports { get; set; }

    #endregion

    // New DbSets for IoT Device Management
    public DbSet<FactoryGroup> FactoryGroups { get; set; }
    public DbSet<FactoryDevice> FactoryDevices { get; set; }
    public DbSet<FactoryMqttTopic> FactoryMqttTopics { get; set; }
    public DbSet<FactoryDeviceTopic> FactoryDeviceTopics { get; set; }
    public DbSet<TopicSchemaDefinition> TopicSchemaDefinitions { get; set; }
    public DbSet<FactoryGroupUser> FactoryGroupUsers { get; set; }
    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<Telemetry> Telemetry { get; set; }
    public DbSet<DeviceStatusLog> DeviceStatusLogs { get; set; }
    public DbSet<DeviceConfiguration> DeviceConfiguration { get; set; }
    public DbSet<MessagingTenantSettings> MessagingTenantSettings { get; set; }

    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderProgressUpdates> WorkOrderProgressUpdates { get; set; }

    public DbSet<WorkOrderRequiredTool> WorkOrderRequiredTools { get; set; }
    public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
    public DbSet<WorkOrderSubTask> WorkOrderSubTasks { get; set; }
    public DbSet<AlertNotification> AlertNotifications { get; set; }
    public DbSet<IntegrationSettings> IntegrationSettings { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<InventoryTransaction> InventoryTransaction { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<MaintenanceScheduleOccurrence> MaintenanceScheduleOccurrences { get; set; }
    public DbSet<InventoryCostIntegration> InventoryCostIntegrations { get; set; }
    public DbSet<PointAssignment> PointAssignments { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<FactoryOpsApp.Domain.Entities.FactoryOpsTenants.Challenge> Challenges { get; set; }
    public DbSet<TrainingModule> TrainingModules { get; set; }
    public DbSet<TeamAlertNotification> TeamAlertNotifications { get; set; }



    // NEW: Add these three DbSets for Automated Replenishment
    public DbSet<SupplierManagement> SupplierManagement { get; set; }
    public DbSet<ReorderRule> ReorderRules { get; set; }
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
    public DbSet<MasterNotification> MasterNotifications { get; set; }

    public DbSet<OutboxEvent> OutboxEvents { get; set; } = null!;
    public DbSet<FactoryEventTrace> FactoryEventTraces { get; set; }

    public DbSet<ProcessedMessage> ProcessedMessages { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<FactoryUserRoles>(entity =>
        {
            entity.HasKey(ur => ur.UserRoleId);

            entity.Property(ur => ur.UserRoleId)
                  .ValueGeneratedOnAdd();

            entity.HasIndex(ur => new { ur.UserId, ur.RoleId })
                  .IsUnique()
                  .HasFilter("\"IsDeleted\" = false");

            entity.HasOne(ur => ur.FactoryUsers)
                  .WithMany(u => u.FactoryUserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ur => ur.FactoryRoles)
                  .WithMany(r => r.FactoryUserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FactoryRolePermissions>(entity =>
        {
            entity.HasKey(rp => rp.RolePermissionId);

            entity.Property(rp => rp.RolePermissionId)
                  .ValueGeneratedOnAdd();

            entity.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                  .IsUnique()
                  .HasFilter("\"IsDeleted\" = false");

            entity.HasOne(rp => rp.FactoryRoles)
                  .WithMany(r => r.FactoryRolePermissions)
                  .HasForeignKey(rp => rp.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(rp => rp.FactoryPermissions)
                  .WithMany(p => p.FactoryRolePermissions)
                  .HasForeignKey(rp => rp.PermissionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FactoryTeam>()
             .HasOne(ft => ft.Manager)                  
             .WithMany()                               
             .HasForeignKey(ft => ft.ManagerId)         
             .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FactorySupportTickets>()
            .HasOne(t => t.AssignedTeam)
            .WithMany()
            .HasForeignKey(t => t.AssignedTo)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<FactoryNotificationRules>()
            .Property(e => e.TriggerEvent)
            .HasConversion<string>();

        modelBuilder.Entity<FactoryNotificationRules>()
            .Property(e => e.DeliveryMethod)
            .HasConversion<string>();

        modelBuilder.Entity<FactoryNotificationRules>()
            .Property(e => e.RecipientType)
            .HasConversion<string>();

        modelBuilder.Entity<SupportFeedback>()
            .HasKey(f => f.FeedbackId);

        modelBuilder.Entity<SupportFeedback>()
            .HasOne<FactorySupportTickets>()
            .WithMany()
            .HasForeignKey(f => f.TicketId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupportFeedback>()
            .HasOne<FactoryUsers>() 
            .WithMany()
            .HasForeignKey(f => f.AcknowledgedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FeedbackMetric>()
         .HasKey(f => f.MetricId);

        modelBuilder.Entity<Location>()
        .Property(l => l.LocationType)
        .HasConversion<string>()
        .HasColumnType("text"); 

        // Enum to string conversions
        modelBuilder.Entity<FactoryGroup>()
            .Property(g => g.Type)
            .HasConversion<string>()
            .HasColumnType("varchar(50)");

        modelBuilder.Entity<FactoryDevice>()
            .Property(d => d.Category)
            .HasConversion<string>()
            .HasColumnType("varchar(50)");

        modelBuilder.Entity<FactoryDevice>()
            .Property(d => d.Status)
            .HasConversion<string>()
            .HasColumnType("varchar(50)");

        modelBuilder.Entity<FactoryMqttTopic>()
            .Property(t => t.Type)
            .HasConversion<string>()
            .HasColumnType("varchar(50)");

        // Unique constraints
        modelBuilder.Entity<FactoryDeviceTopic>()
            .HasIndex(dt => new { dt.DeviceId, dt.TopicId })
            .IsUnique();

        modelBuilder.Entity<FactoryGroupUser>()
            .HasIndex(gu => new { gu.GroupId, gu.UserId })
            .IsUnique();


        // TopicSchemaDefinition
        modelBuilder.Entity<TopicSchemaDefinition>()
            .Property(e => e.DataType)
            .HasColumnName("DataType")      
            .HasConversion<string>()
            .HasColumnType("varchar(50)")
            .HasMaxLength(50);            

        // FactoryMqttTopic
        modelBuilder.Entity<FactoryMqttTopic>()
            .Property(e => e.Type)
            .HasColumnName("Type")      
            .HasConversion<string>()        
            .HasMaxLength(50);

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

        // NEW: Maintenance Management Enum Conversions
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
        modelBuilder.Entity<WorkOrderProgressUpdates>()
            .Property(e => e.UpdateType)
            .HasConversion<string>();

        modelBuilder.Entity<WorkOrderProgressUpdates>()
             .Property(e => e.Status)
             .HasConversion<string>();
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


        modelBuilder.Entity<AlertNotification>()
            .Property(a => a.AlertType)
            .HasConversion<string>()
            .HasMaxLength(30);

        modelBuilder.Entity<AlertNotification>()
            .Property(a => a.Severity)
            .HasConversion<string>()
            .HasMaxLength(20);

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

        modelBuilder.Entity<IntegrationSettings>()
              .Property(e => e.Category)
              .HasConversion<string>()
              .HasMaxLength(50);

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

        modelBuilder.Entity<Badge>()
      .Property(b => b.BadgeType)
      .HasConversion<string>();

        modelBuilder.Entity<Badge>()
            .Property(b => b.Rarity)
            .HasConversion<string>();

        modelBuilder.Entity<TeamAlertNotification>()
      .Property(n => n.NotificationType)
      .HasConversion<string>(); 

        modelBuilder.Entity<TeamAlertNotification>()
            .Property(n => n.TriggerType)
            .HasConversion<string>();


        modelBuilder.Entity<IntegrationSettings>()
        .Property(e => e.SettingValue)
        .HasColumnType("jsonb")
        .HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
            v => System.Text.Json.JsonSerializer.Deserialize<SettingValueObject>(v, new System.Text.Json.JsonSerializerOptions())
        );

        modelBuilder.Entity<FactoryEventTrace>(entity =>
        {
            entity.HasKey(x => x.TraceId);

            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.Service);
            entity.HasIndex(x => x.CreatedAt);
        });

    }

}
 
