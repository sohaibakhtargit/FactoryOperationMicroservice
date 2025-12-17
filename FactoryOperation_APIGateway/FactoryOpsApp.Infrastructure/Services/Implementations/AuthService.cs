using FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models;
using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<HttpResponseMessage> AuthenticateAsync(LoginRequest loginRequest)
        {
            var authServiceUrl = _configuration["Microservices:AccessManagement:BaseUrl"];
            var content = new StringContent(
                JsonSerializer.Serialize(new { loginRequest.Email, loginRequest.Password }),
                Encoding.UTF8,
                "application/json");

            return await _httpClient.PostAsync($"{authServiceUrl}/api/Authenticate/authenticate", content);
        }

        public async Task<HttpResponseMessage> ForgetPasswordAsync(string email, string newPassword)
        {
            var authServiceUrl = _configuration["Microservices:AccessManagement:BaseUrl"];
            var content = new StringContent(
                JsonSerializer.Serialize(new { email, newPassword }),
                Encoding.UTF8,
                "application/json");

            return await _httpClient.PostAsync($"{authServiceUrl}/api/Authenticate/Forget-Password", content);
        }

        public async Task<HttpResponseMessage> SwitchTenantAsync(int tenantId, string token)
        {
            var authServiceUrl = _configuration["Microservices:AccessManagement:BaseUrl"];
            var request = new HttpRequestMessage(HttpMethod.Post, $"{authServiceUrl}/api/Authenticate/switch-to-tenant");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(
                JsonSerializer.Serialize(new { TenantId = tenantId }),
                Encoding.UTF8,
                "application/json");
            request.Content = content;

            return await _httpClient.SendAsync(request);
        }
    }
}
