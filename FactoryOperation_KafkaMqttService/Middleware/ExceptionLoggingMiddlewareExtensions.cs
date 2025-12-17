using FactoryOpsApp.Api.Middlewares;

namespace FactoryOperation_KafkaMqttService.Middleware
{
    public static class ExceptionLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionLoggingMiddleware>();
        }
    }
}