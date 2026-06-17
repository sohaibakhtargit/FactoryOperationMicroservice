using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FactoryOpsApp.Infrastructure.Middleware
{
    public class ForceLogoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public ForceLogoutMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var tenantId = context.User.FindFirst("TenantId")?.Value;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(currentUserId))
            {
                await _next(context);
                return;
            }

            if (role == "SuperAdmin")
            {
                await _next(context);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var masterDb = scope.ServiceProvider.GetRequiredService<MasterFactoryOpsDbContext>();
            var tenantDbFactory = scope.ServiceProvider.GetRequiredService<TenantDbContextFactory>();

            if (role == "TenantAdmin")
            {
                var admin = await masterDb.TenantAdminLogins
                    .FirstOrDefaultAsync(a => a.Id == Convert.ToInt32(currentUserId));

                if (admin != null)
                {
                    if (admin.ForceLogout == true)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Tenant Admin has been logged out (ForceLogout).");
                        return;
                    }

                    if (admin.Suspend == true)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Tenant Admin account is suspended.");
                        return;
                    }
                }
            }
            else
            {
                using var tenantDb = tenantDbFactory.GetTenantDbContext(Convert.ToInt32(tenantId));

                var factoryUser = await tenantDb.FactoryUsers
                    .FirstOrDefaultAsync(u => u.UserId == Convert.ToInt32(currentUserId));

                if (factoryUser != null)
                {
                    if (factoryUser.ForceLogout == true)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("User has been logged out (ForceLogout).");
                        return;
                    }

                    if (factoryUser.Suspend == true)
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("User account is suspended.");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
