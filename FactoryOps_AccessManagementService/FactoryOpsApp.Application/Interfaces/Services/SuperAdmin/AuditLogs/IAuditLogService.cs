namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs
{
    public interface IAuditLogService
    {
        public Task LogAuditAsync(string action, string details, int? tenantId, string? email, string? eventType);
    }
}
