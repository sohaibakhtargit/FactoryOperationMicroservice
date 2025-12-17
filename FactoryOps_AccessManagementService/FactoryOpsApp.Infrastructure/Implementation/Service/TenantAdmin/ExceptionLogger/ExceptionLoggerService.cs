using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.ExceptionLogger
{
    public class ExceptionLoggerService : IExceptionLoggerService
    {
        private readonly MasterFactoryOpsDbContext _dbContext;

        public ExceptionLoggerService(MasterFactoryOpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogExceptionAsync(Exception ex, string sourceModule, string apiName, int? tenantId = null, int? userId = null)
        {
            try
            {
                var log = new ExceptionLogs
                {
                    LogTimestamp = DateTime.UtcNow,
                    SourceModule = sourceModule,
                    ApiName = apiName,
                    ErrorCode = ex.HResult.ToString(),
                    ExceptionStackTrace = ex.StackTrace,
                    ErrorMessage = ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : ""),
                    TenantId = tenantId,
                    UserId = userId,
                    IsActive = true,
                    IsDeleted = false
                };

                _dbContext.ExceptionLogs.Add(log);
                await _dbContext.SaveChangesAsync();
            }
            catch
            {

            }
        }
    }
}
