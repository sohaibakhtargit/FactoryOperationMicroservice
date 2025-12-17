using FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models;
using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;

        // FIXED: Use IOptions<JwtSettings> instead of direct JwtSettings
        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public GatewayUser? ExtractUserFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                if (principal == null) return null;

                return new GatewayUser
                {
                    UserId = int.Parse(principal.FindFirst("UserId")?.Value ?? "0"),
                    Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                    RoleType = principal.FindFirst("RoleType")?.Value ?? string.Empty,
                    TenantId = principal.FindFirst("TenantId") != null ?
                              int.Parse(principal.FindFirst("TenantId")!.Value) : null,
                    Roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                    Permissions = principal.FindAll("Permission").Select(c => c.Value).ToList(),
                    ModuleAccess = principal.FindAll("ModuleAccess").Select(c => c.Value).ToList()
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
