using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using Microsoft.EntityFrameworkCore;

namespace FactoryOpsApp.Infrastructure.DBContext
{
    public class MasterFactoryOpsDbContext : DbContext
    {
        public MasterFactoryOpsDbContext(DbContextOptions<MasterFactoryOpsDbContext> options) : base(options) { }

       
        public DbSet<TenantMasterMapping> TenantMasterMapping { get; set; }
        public DbSet<FactoryTenants> FactoryTenants { get; set; }
        public DbSet<Audit_Log_MasterDb> Audit_Log_MasterDb { get; set; }
        public DbSet<AdminLogin> AdminLogins { get; set; }
        public DbSet<AdminRoles> AdminRoles { get; set; }
        public DbSet<SystemMetric> SystemMetrics { get; set; }
        public DbSet<TenantIsolation> TenantIsolations { get; set; }
        public DbSet<GlobalUsers> GlobalUsers { get; set; }
        public DbSet<TenantAdminLogin> TenantAdminLogins { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementChannel> AnnouncementChannels { get; set; }
        public DbSet<AnnouncementTenant> AnnouncementTenants { get; set; }
        public DbSet<AnnouncementTemplate> AnnouncementTemplates { get; set; }
        public DbSet<ExceptionLogs> ExceptionLogs { get; set; }
        public DbSet<BackupJob> BackupJobs { get; set; }
        public DbSet<BackupSchedule> BackupSchedules { get; set; }
        public DbSet<BackupStat> BackupStats { get; set; }
        public DbSet<FactoryComplianceAndAudit> FactoryComplianceAndAudits { get; set; }
        public DbSet<AuditComplianceMetric> AuditComplianceMetrics { get; set; }
        public DbSet<ModuleMaster> ModuleMaster { get; set; }
        public DbSet<ModuleMasterMapping> ModuleMasterMapping { get; set; }
        public DbSet<CompanyBrandingInfo> CompanyBrandingInfo { get; set; }
        public DbSet<GlobalDropdown> GlobalDropdowns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SystemMetric>().ToTable("SystemMetrics");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantIsolation>()
          .ToTable("TenantIsolation");

            modelBuilder.Entity<AnnouncementTemplate>()
                .Property(e => e.Type)
                .HasConversion<string>() 
                .HasMaxLength(50);
        }
    }
}
