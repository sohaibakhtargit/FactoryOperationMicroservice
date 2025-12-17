using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using Microsoft.EntityFrameworkCore;

namespace FactoryOpsApp.Infrastructure.DBContext
{
    public class MasterFactoryOpsDbContext : DbContext
    {
        public MasterFactoryOpsDbContext(DbContextOptions<MasterFactoryOpsDbContext> options) : base(options) { }

        public DbSet<FactoryTenants> FactoryTenants { get; set; }
        public DbSet<TenantMasterMapping> TenantMasterMapping { get; set; }
        public DbSet<Audit_Log_MasterDb> Audit_Log_MasterDb { get; set; }
        public DbSet<TenantAdminLogin> TenantAdminLogins { get; set; }
        public DbSet<ExceptionLogs> ExceptionLogs { get; set; }
        public DbSet<AuditComplianceMetric> AuditComplianceMetrics { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          //  modelBuilder.Entity<SystemMetric>().ToTable("SystemMetrics");

          //  base.OnModelCreating(modelBuilder);

          //  modelBuilder.Entity<TenantIsolation>()
          //.ToTable("TenantIsolation");

          //  modelBuilder.Entity<AnnouncementTemplate>()
          //      .Property(e => e.Type)
          //      .HasConversion<string>() 
          //      .HasMaxLength(50);
        }
    }
}
