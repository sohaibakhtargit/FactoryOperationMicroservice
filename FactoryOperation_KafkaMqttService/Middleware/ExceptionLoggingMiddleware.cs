using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Common;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using System.Net;
using System.Text.Json;
using static FactoryOperation_KafkaMqttService.FactoryOpsApp.Common.CommonConstantFiles.CommonConstant;

namespace FactoryOpsApp.Api.Middlewares
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ⬇ Updated method signature: DI injects IExceptionLoggerService correctly (Scoped)
        public async Task Invoke(HttpContext context, IExceptionLoggerService exceptionLogger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                string path = context.Request.Path.Value ?? string.Empty;
                string method = context.Request.Method;

                string sourceModule = GetSourceModuleFromPath(path);
                string apiName = $"{method} {path}";

                int? tenantId = GetTenantId(context);
                int? userId = GetUserId(context);

                // Log the error to database
                await exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule,
                    apiName,
                    tenantId,
                    userId
                );

                await WriteErrorResponseAsync(context);
            }
        }

        private static string GetSourceModuleFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "Unknown-Module";

            var segments = path.Trim('/').Split('/');
            if (segments.Length > 1)
                return $"{segments[1]}-Module";

            return $"{segments[0]}-Module";
        }

        private static int? GetTenantId(HttpContext context)
        {
            var tenantClaim = context.User?.FindFirst("TenantId")?.Value;
            if (int.TryParse(tenantClaim, out var tenantIdFromClaim))
                return tenantIdFromClaim;

            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader) &&
                int.TryParse(tenantHeader, out var tenantIdHeader))
            {
                return tenantIdHeader;
            }

            return null;
        }

        private static int? GetUserId(HttpContext context)
        {
            var userIdClaim = context.User?.FindFirst("UserId")?.Value
                ?? context.User?.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        private static async Task WriteErrorResponseAsync(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new CommonResponseModel
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = "Unexpected Server Error Logged Successfully"
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
