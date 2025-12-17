using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.IOTDevices
{
    /// <summary>
    /// Factory Device Management API
    /// Manages factory devices, device registration, and device operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FactoryDeviceController : ControllerBase
    {
        private readonly IFactoryDeviceService _deviceService;

        public FactoryDeviceController(IFactoryDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        /// <summary>
        /// Add device
        /// Registers new factory device in the system
        /// </summary>
        /// <param name="dto">Device registration data</param>
        /// <returns>Device creation result</returns>
        /// <response code="200">Device successfully added</response>
        /// <response code="400">Invalid device data provided</response>
        [HttpPost("Add-Device")]
        public async Task<IActionResult> AddDeviceAsync([FromBody] DeviceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _deviceService.AddDeviceAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update device
        /// Modifies existing factory device information
        /// </summary>
        /// <param name="dto">Updated device data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Device successfully updated</response>
        /// <response code="400">Invalid device data provided</response>
        [HttpPost("Update-Device")]
        public async Task<IActionResult> UpdateDeviceAsync([FromBody] DeviceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _deviceService.UpdateDeviceAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete device
        /// Removes factory device from the system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="deviceId">Device identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Device successfully deleted</response>
        [HttpPost("Delete-Device")]
        public async Task<IActionResult> DeleteDeviceAsync(int tenantId, int deviceId)
        {
            var result = await _deviceService.DeleteDeviceAsync(deviceId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all devices
        /// Retrieves all factory devices for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of factory devices</returns>
        /// <response code="200">Successfully retrieved all devices</response>
        [HttpGet("Get-AllDevices")]
        public async Task<IActionResult> GetAllDevices(int tenantId)
        {
            var result = await _deviceService.GetAllDevicesAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get device by ID
        /// Retrieves specific factory device details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="deviceId">Device identifier</param>
        /// <returns>Device details</returns>
        /// <response code="200">Successfully retrieved device</response>
        [HttpGet("Get-DeviceByIdAsync")]
        public async Task<IActionResult> GetDeviceByIdAsync(int tenantId, int deviceId)
        {
            var result = await _deviceService.GetDeviceByIdAsync(deviceId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all devices from all tenants
        /// Retrieves device information across all tenant boundaries
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <param name="deviceCode">Device code</param>
        /// <returns>Cross-tenant device information</returns>
        /// <response code="200">Successfully retrieved cross-tenant devices</response>
        [HttpGet("GetAllDevices_FromAllTenantsAsync")]
        public async Task<IActionResult> GetAllDevicesFromAllTenantsAsync(int deviceId, string deviceCode)
        {
            var result = await _deviceService.GetAllDevicesFromAllTenantsAsync(deviceId, deviceCode);
            return Ok(result);
        }
    }
}