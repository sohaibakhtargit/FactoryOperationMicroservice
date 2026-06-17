using FactoryOperation_API_Gateway.Controllers;
using FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models;

namespace FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces
{
    public interface IAuthService
    {
        Task<HttpResponseMessage> AuthenticateAsync(LoginRequest loginRequest);
        Task<HttpResponseMessage> ForgetPasswordAsync(string email);
        Task<HttpResponseMessage> SwitchTenantAsync(int tenantId, string token);
        Task<HttpResponseMessage> VerifyOtp(VerifyOTPDto otp);
        Task<HttpResponseMessage> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}
