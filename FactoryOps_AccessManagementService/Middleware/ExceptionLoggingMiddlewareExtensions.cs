using FactoryOpsApp.Api.Middlewares;

namespace FactoryOps_AccessManagementService.Middleware
{
    public static class ExceptionLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionLoggingMiddleware>();
        }
    }
}