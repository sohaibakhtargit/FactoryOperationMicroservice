using FactoryOperation_Asset.FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;


namespace FactoryOpsApp.Infrastructure.DBContext;

public class FactoryOpsDBContext : DbContext
{
    public FactoryOpsDBContext(DbContextOptions<FactoryOpsDBContext> options) : base(options) { }

    #region
    public DbSet<Location> Locations { get; set; }
    public DbSet<FactoryAssetType> FactoryAssetTypes { get; set; }
    public DbSet<AssetRegistry> AssetRegistry { get; set; }
    public DbSet<AssetBillOfMaterials> AssetBillOfMaterials { get; set; }
    public DbSet<AssetTracking> AssetTracking { get; set; }
    public DbSet<MaintenanceHistory> MaintenanceHistory { get; set; }
    public DbSet<AssetLifecycle> AssetLifecycles { get; set; }
    public DbSet<AssetFinancialAnalysis> AssetFinancialAnalysis { get; set; }
    public DbSet<AssetDocuments> AssetDocuments { get; set; }
    public DbSet<AssetDashboard_Report> AssetDashboard_Reports { get; set; }
    public DbSet<AssetBulkImport> AssetBulkImport { get; set; }
    public DbSet<AssetLifecycleMappings> AssetLifecycleMappings { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderProgressUpdates> WorkOrderProgressUpdates { get; set; }

    public DbSet<Inventory> Inventory { get; set; }

    public DbSet<FactoryUserRoles> FactoryUserRoles { get; set; }
    public DbSet<FactoryUsers> FactoryUsers { get; set; }
    public DbSet<FactoryRoles> FactoryRoles { get; set; }
    #endregion


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

        /*   modelBuilder.Entity<SupportFeedback>()
               .HasOne<FactoryUsers>()
               .WithMany()
               .HasForeignKey(f => f.AcknowledgedBy)
               .OnDelete(DeleteBehavior.Restrict);*/

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

        modelBuilder.Entity<AssetTracking>()
            .Property(at => at.Status)
            .HasConversion<string>();

        modelBuilder.Entity<AssetTracking>()
        .HasOne(at => at.AssignedUser)
        .WithMany()
        .HasForeignKey(at => at.AssignedTo)
        .HasConstraintName("FK_AssetTracking_FactoryUsers_AssignedTo");

        modelBuilder.Entity<AssetLifecycle>()
       .Property(a => a.Stage)
       .HasConversion<string>()
       .HasMaxLength(50);

        modelBuilder.Entity<AssetLifecycle>()
            .Property(a => a.AnalysisType)
            .HasConversion<string>()
            .HasMaxLength(50);

        modelBuilder.Entity<AssetLifecycleMappings>()
            .Property(e => e.AssetStage)
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

        modelBuilder.Entity<AssetRegistry>()
             .Property(e => e.Criticality)
             .HasConversion<string>()
             .HasMaxLength(10);
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

    }
}

