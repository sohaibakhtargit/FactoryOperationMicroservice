using FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models;

namespace FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces
{
    public interface IAuthService
    {
        Task<HttpResponseMessage> AuthenticateAsync(LoginRequest loginRequest);
        Task<HttpResponseMessage> ForgetPasswordAsync(string email, string newPassword);
        Task<HttpResponseMessage> SwitchTenantAsync(int tenantId, string token);
    }
}
