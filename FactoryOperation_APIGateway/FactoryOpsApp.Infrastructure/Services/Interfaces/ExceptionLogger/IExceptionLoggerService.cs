namespace FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces.ExceptionLogger
{
    public interface IExceptionLoggerService
    {
        Task LogExceptionAsync(Exception ex, string sourceModule, string apiName, int? tenantId = null, int? userId = null);
    }
}
