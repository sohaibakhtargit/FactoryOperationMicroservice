using FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models;
using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_API_Gateway.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email and Password are required." });
            }

            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var response = await _authService.AuthenticateAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Successful login for email: {Email}", request.Email);

                    // Return the exact response from authentication service
                    return Content(content, "application/json");
                }
                else
                {
                    _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                    return Unauthorized(new { error = "Invalid credentials" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, new { error = "Authentication service unavailable" });
            }
        }

        [AllowAnonymous]
        [HttpPost("forget-password")]
        public async Task<ActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { error = "Email and NewPassword are required." });
            }

            try
            {
                var response = await _authService.ForgetPasswordAsync(request.Email, request.NewPassword);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }
                else
                {
                    return BadRequest(new { error = "Password reset failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for email: {Email}", request.Email);
                return StatusCode(500, new { error = "Authentication service unavailable" });
            }
        }

        [HttpPost("switch-tenant")]
        public async Task<ActionResult> SwitchTenant([FromBody] SwitchTenantRequest request)
        {
            var token = ExtractTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { error = "Token is required" });
            }

            try
            {
                var response = await _authService.SwitchTenantAsync(request.TenantId, token);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Content(content, "application/json");
                }
                else
                {
                    return BadRequest(new { error = "Tenant switch failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tenant switch to: {TenantId}", request.TenantId);
                return StatusCode(500, new { error = "Authentication service unavailable" });
            }
        }

        [AllowAnonymous]
        [HttpPost("validate")]
        public ActionResult ValidateToken([FromBody] ValidateTokenRequest request)
        {
            var user = _jwtService.ExtractUserFromToken(request.Token);

            if (user == null)
            {
                return Unauthorized(new { error = "Invalid token" });
            }

            return Ok(new
            {
                user.UserId,
                user.Email,
                user.RoleType,
                user.TenantId,
                user.Roles,
                user.Permissions,
                user.ModuleAccess,
                isValid = true
            });
        }

        [HttpGet("profile")]
        public ActionResult GetProfile()
        {
            var userId = HttpContext.Items["UserId"] as int?;
            var tenantId = HttpContext.Items["TenantId"] as int?;
            var roleType = HttpContext.Items["RoleType"] as string;
            var permissions = HttpContext.Items["Permissions"] as List<string>;
            var moduleAccess = HttpContext.Items["ModuleAccess"] as List<string>;
            var email = HttpContext.Items["UserEmail"] as string;

            if (userId == null)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            return Ok(new
            {
                UserId = userId,
                TenantId = tenantId,
                RoleType = roleType,
                Email = email,
                Permissions = permissions,
                ModuleAccess = moduleAccess
            });
        }

        private string? ExtractTokenFromHeader()
        {
            if (HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var headerValue = authHeader.ToString();
                if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return headerValue.Substring(7);
                }
            }
            return null;
        }
    }

    public class ForgetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class SwitchTenantRequest
    {
        public int TenantId { get; set; }
    }
}
