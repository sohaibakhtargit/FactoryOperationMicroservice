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
        public DbSet<TenantIsolation> TenantIsolations { get; set; }
        public DbSet<GlobalUsers> GlobalUsers { get; set; }
        public DbSet<TenantAdminLogin> TenantAdminLogins { get; set; }
        public DbSet<ExceptionLogs> ExceptionLogs { get; set; }
        public DbSet<FactoryComplianceAndAudit> FactoryComplianceAndAudits { get; set; }
        public DbSet<ModuleMaster> ModuleMaster { get; set; }
        public DbSet<ModuleMasterMapping> ModuleMasterMapping { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TenantIsolation>()
          .ToTable("TenantIsolation");
        }
    }
}
