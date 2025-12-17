using FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models;
using System.Security.Claims;

namespace FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces
{
    public interface IJwtService
    {
        ClaimsPrincipal? ValidateToken(string token);
        GatewayUser? ExtractUserFromToken(string token);
    }
}
