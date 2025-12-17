namespace FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class GatewayUser
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string RoleType { get; set; } = string.Empty;
        public int? TenantId { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<string> ModuleAccess { get; set; } = new();
    }

    public class AuthResponse
    {
        public string StatusCode { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}