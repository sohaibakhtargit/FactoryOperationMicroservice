using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_API_Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GatewayController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("status")]
        public ActionResult GetStatus()
        {
            var microservices = _configuration.GetSection("Microservices").GetChildren()
                .ToDictionary(x => x.Key, x => x.GetValue<string>("BaseUrl"));

            return Ok(new
            {
                message = "API Gateway is running",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                authentication = "JWT Token Validation",
                microservices = microservices
            });
        }

        [HttpGet("health")]
        public ActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "API Gateway"
            });
        }

        [HttpGet("user-info")]
        public ActionResult GetUserInfo()
        {
            var userId = HttpContext.Items["UserId"] as int?;
            var tenantId = HttpContext.Items["TenantId"] as int?;
            var roleType = HttpContext.Items["RoleType"] as string;
            var permissions = HttpContext.Items["Permissions"] as List<string>;
            var moduleAccess = HttpContext.Items["ModuleAccess"] as List<string>;

            return Ok(new
            {
                UserId = userId,
                TenantId = tenantId,
                RoleType = roleType,
                Permissions = permissions?.Count ?? 0,
                ModuleAccess = moduleAccess?.Count ?? 0,
                IsAuthenticated = userId != null
            });
        }
    }
}
