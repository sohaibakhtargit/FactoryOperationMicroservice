using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace FactoryOperation_API_Gateway.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Resolve scoped service per-request
            var jwtService = context.RequestServices.GetRequiredService<IJwtService>();

            // Skip authentication for public paths
            if (IsPublicPath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var token = ExtractToken(context.Request);

            if (!string.IsNullOrEmpty(token))
            {
                var user = jwtService.ExtractUserFromToken(token);
                if (user != null)
                {
                    // Add user info to context for routing and authorization
                    context.Items["UserId"] = user.UserId;
                    context.Items["TenantId"] = user.TenantId;
                    context.Items["RoleType"] = user.RoleType;
                    context.Items["Permissions"] = user.Permissions;
                    context.Items["ModuleAccess"] = user.ModuleAccess;
                    context.Items["UserEmail"] = user.Email;

                    await _next(context);
                    return;
                }
            }

            // Token is invalid or missing
            await ReturnUnauthorized(context, "Invalid or missing token");
        }

        private bool IsPublicPath(PathString path)
        {
            var publicPaths = new[]
            {
                "/auth/login",
                "/auth/forget-password",
                "/auth/validate", // allow validate to reach controller
                "/api/authenticate/authenticate",
                "/api/authenticate/Forget-Password",
                "/swagger",
                "/favicon.ico",
                "/api/gateway/status",
                "/api/gateway/health"
            };

            return publicPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
        }

        private string? ExtractToken(HttpRequest request)
        {
            // Try to get token from Authorization header
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var headerValue = authHeader.ToString();
                if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return headerValue.Substring(7);
                }
            }

            // Try to get token from query string
            if (request.Query.TryGetValue("access_token", out var queryToken))
            {
                return queryToken;
            }

            return null;
        }

        private async Task ReturnUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = message,
                timestamp = DateTime.UtcNow
            }));
        }
    }
}
