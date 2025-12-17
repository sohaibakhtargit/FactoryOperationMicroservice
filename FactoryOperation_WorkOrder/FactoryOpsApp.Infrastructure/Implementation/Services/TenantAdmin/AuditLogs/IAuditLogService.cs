using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.AuditLogs
{
    public class AuditLogService : IAuditLogService
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuditLogService(MasterFactoryOpsDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _masterDbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAuditAsync(string action, string details, int? tenantId, string? email, string? eventType)
        {
            var ctx = _httpContextAccessor.HttpContext;

            var auditLog = new Audit_Log_MasterDb
            {
                Action = action,
                Details = details,
                TenantId = tenantId,
                Email = email,
                EventType = eventType ?? "",
                Timestamp = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                UserName = Environment.UserName,
                Ipaddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
            };

            await _masterDbContext.Audit_Log_MasterDb.AddAsync(auditLog);
            await _masterDbContext.SaveChangesAsync();
        }
    }
}
