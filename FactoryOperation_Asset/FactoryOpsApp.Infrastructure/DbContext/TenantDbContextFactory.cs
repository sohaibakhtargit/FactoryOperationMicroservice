using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FactoryOpsApp.Infrastructure.DBContext
{
    public class TenantDbContextFactory
    {
        private readonly MasterFactoryOpsDbContext _masterDb;
        private readonly IConfiguration _configuration;

        public TenantDbContextFactory(
            MasterFactoryOpsDbContext masterDb,
            IConfiguration configuration)
        {
            _masterDb = masterDb;
            _configuration = configuration;
        }
        public FactoryOpsDBContext GetTenantDbContext(int tenantId)
        {
            string dbName;

            var tenant = _masterDb.TenantMasterMapping
                .AsNoTracking()
                .FirstOrDefault(x => x.TenantId == tenantId && x.IsActive);

            if (tenant != null)
            {
                dbName = tenant.DbName;
            }
            else
            {
                dbName = "FactoryOperation";
            }

            var template = _configuration.GetConnectionString("TenantDbConnection");
            var connectionString = string.Format(template, dbName);

            var options = new DbContextOptionsBuilder<FactoryOpsDBContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new FactoryOpsDBContext(options);
        }

    }
}
