using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.IOTDevices
{
    /// <summary>
    /// Telemetry Data API
    /// Provides access to device telemetry data and monitoring information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryRepository _telemetryRepository;

        public TelemetryController(ITelemetryRepository telemetryRepository)
        {
            _telemetryRepository = telemetryRepository;
        }

        /// <summary>
        /// Get telemetry report
        /// Retrieves telemetry data and monitoring information for specific device
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Device telemetry data</returns>
        /// <response code="200">Successfully retrieved telemetry report</response>
        [HttpGet("Get-Telemetry-Report")]
        public async Task<IActionResult> GetTelemetry(int deviceId, int TenantId)
        {
            var data = await _telemetryRepository.GetTelemetryByDeviceIdAsync(deviceId, TenantId);
            return Ok(data);
        }

        ///// <summary>
        ///// Get device status logs
        ///// Retrieves device status history and operational logs
        ///// </summary>
        ///// <param name="tenantId">Tenant identifier</param>
        ///// <param name="deviceId">Device identifier</param>
        ///// <returns>Device status logs</returns>
        ///// <response code="200">Successfully retrieved device status logs</response>
        //[HttpGet("{tenantId}/{deviceId}")]
        //public async Task<IActionResult> GetDeviceStatusLogs(int tenantId, int deviceId)
        //{
        //    var logs = await _telemetryRepository.GetDeviceStatusLogsAsync(tenantId, deviceId);
        //    return Ok(logs);
        //}
    }
}