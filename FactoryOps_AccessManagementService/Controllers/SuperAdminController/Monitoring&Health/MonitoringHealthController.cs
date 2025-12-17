using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Monitoring_Health;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Monitoring_Health
{
    /// <summary>
    /// Monitoring Health API for Super Admin
    /// Provides system health monitoring and performance metrics
    /// </summary>
    [Route("api/superadmin/[controller]")]
    [ApiController]
    public class MonitoringHealthController : ControllerBase
    {
        private readonly IMonitoringHealthService _monitoringHealthService;

        public MonitoringHealthController(IMonitoringHealthService monitoringHealthService)
        {
            _monitoringHealthService = monitoringHealthService;
        }

        /// <summary>
        /// Get system health metrics
        /// Retrieves comprehensive health and performance data for system monitoring
        /// </summary>
        /// <returns>System health metrics and status information</returns>
        /// <response code="200">Successfully retrieved health metrics</response>
        [HttpGet("get-health-metrics")]
        public async Task<IActionResult> GetMonitoringAndHealthMetrics()
        {
            var result = await _monitoringHealthService.GetMonitoringHealthAsync();
            return Ok(result);
        }
    }
}