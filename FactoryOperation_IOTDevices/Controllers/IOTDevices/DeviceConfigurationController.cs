using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.IOTDevices
{
    /// <summary>
    /// Device Configuration Management API
    /// Manages device configurations, settings, and configuration profiles
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceConfigurationController : ControllerBase
    {
        private readonly IDeviceConfigurationService _service;

        public DeviceConfigurationController(IDeviceConfigurationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Add device configuration
        /// Creates new device configuration profile and settings
        /// </summary>
        /// <param name="model">Device configuration data</param>
        /// <returns>Configuration creation result</returns>
        /// <response code="200">Configuration successfully added</response>
        /// <response code="400">Invalid configuration data provided</response>
        [HttpPost("add-configuration")]
        public async Task<IActionResult> Add(DeviceConfigurationDto model)
        {
            if (model == null) return BadRequest("Invalid data");

            var result = await _service.AddAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Update device configuration
        /// Modifies existing device configuration settings
        /// </summary>
        /// <param name="model">Updated configuration data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Configuration successfully updated</response>
        /// <response code="400">Invalid configuration data provided</response>
        [HttpPost("update-configuration")]
        public async Task<IActionResult> Update(DeviceConfigurationDto model)
        {
            if (model == null || model.ConfigId == 0) return BadRequest("Invalid data");

            var result = await _service.UpdateAsync(model);
            return Ok(result);
        }

        /// <summary>
        /// Get configuration by ID
        /// Retrieves specific device configuration details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="configId">Configuration identifier</param>
        /// <param name="deviceId">Device identifier</param>
        /// <returns>Device configuration details</returns>
        /// <response code="200">Successfully retrieved configuration</response>
        /// <response code="404">Configuration not found</response>
        [HttpGet("get-configuration")]
        public async Task<IActionResult> GetById(int tenantId, int configId, int deviceId)
        {
            var result = await _service.GetByIdAsync(tenantId, configId, deviceId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Get all configurations
        /// Retrieves all device configurations for specific device
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="deviceId">Device identifier</param>
        /// <returns>List of device configurations</returns>
        /// <response code="200">Successfully retrieved all configurations</response>
        [HttpGet("get-all-configurations")]
        public async Task<IActionResult> GetAll(int tenantId, int deviceId)
        {
            var result = await _service.GetAllAsync(tenantId, deviceId);
            return Ok(result);
        }
    }
}