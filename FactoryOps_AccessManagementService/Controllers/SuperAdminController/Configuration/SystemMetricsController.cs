using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Configuration
{
    /// <summary>
    /// System Metrics API for Super Admin
    /// Provides real-time system performance monitoring and metric tracking
    /// </summary>
    [Route("api/superadmin/[controller]")]
    [ApiController]
    //[Authorize(Roles = "SuperAdmin")]
    public class SystemMetricsController : ControllerBase
    {
        private readonly ISystemMetricService _metricService;

        public SystemMetricsController(ISystemMetricService metricService)
        {
            _metricService = metricService;
        }

        /// <summary>
        /// Get the latest system metrics
        /// Retrieves current system performance data including resource usage and health status
        /// </summary>
        /// <returns>Latest system metric data</returns>
        /// <response code="200">Successfully retrieved latest system metrics</response>
        /// <response code="404">No system metrics data available</response>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestMetric()
        {
            var result = await _metricService.GetLatestMetricAsync();
            if (result == null)
                return NotFound("No metrics found.");

            return Ok(result);
        }

        /// <summary>
        /// Get all historical system metrics
        /// Retrieves complete history of system performance data for analysis and trending
        /// </summary>
        /// <returns>Complete collection of system metrics</returns>
        /// <response code="200">Successfully retrieved all system metrics</response>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllMetrics()
        {
            var result = await _metricService.GetAllMetricsAsync();
            return Ok(result);
        }
    }
}