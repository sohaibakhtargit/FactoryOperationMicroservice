using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
            // 🔥 IMPORTANT: Respect [AllowAnonymous]
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
            var token = ExtractToken(context.Request);

            if (!string.IsNullOrEmpty(token))
            {
                var user = jwtService.ExtractUserFromToken(token);

                if (user != null)
                {
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

            await ReturnUnauthorized(context, "Invalid or missing token");
        }

        private string? ExtractToken(HttpRequest request)
        {
            if (request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var headerValue = authHeader.ToString();
                if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return headerValue.Substring(7);
                }
            }

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