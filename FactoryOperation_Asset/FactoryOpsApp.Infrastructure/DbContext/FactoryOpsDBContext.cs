using FactoryOpsApp.Domain.Entities;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using Microsoft.EntityFrameworkCore;


namespace FactoryOpsApp.Infrastructure.DBContext;

public class FactoryOpsDBContext : DbContext
{
    public FactoryOpsDBContext(DbContextOptions<FactoryOpsDBContext> options) : base(options) { }

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


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

    }

}

