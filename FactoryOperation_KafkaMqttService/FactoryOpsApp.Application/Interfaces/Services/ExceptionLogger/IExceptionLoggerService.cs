namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger
{
    public interface IExceptionLoggerService
    {
        Task LogExceptionAsync(Exception ex, string sourceModule, string apiName, int? tenantId = null, int? userId = null);
    }
}
