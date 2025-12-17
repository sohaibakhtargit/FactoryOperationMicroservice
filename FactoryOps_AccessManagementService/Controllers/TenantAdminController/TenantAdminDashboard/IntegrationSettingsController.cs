using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminDashboard
{
    /// <summary>
    /// Integration Settings Management API
    /// Manages integration configurations, third-party connections, and API settings
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationSettingsController : ControllerBase
    {
        private readonly IIntegrationSettingsRepository _integrationSettingsRepository;

        public IntegrationSettingsController(IIntegrationSettingsRepository integrationSettingsRepository)
        {
            _integrationSettingsRepository = integrationSettingsRepository;
        }

        /// <summary>
        /// Create integration setting
        /// Creates new integration configuration for third-party services
        /// </summary>
        /// <param name="dto">Integration setting data</param>
        /// <returns>Integration creation result</returns>
        /// <response code="200">Integration setting successfully created</response>
        [HttpPost("Create-IntegrationSetting")]
        public async Task<IActionResult> CreateIntegrationSetting([FromBody] CreateIntegrationSettingsDto dto)
        {
            var result = await _integrationSettingsRepository.AddIntegrationSettingAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Update integration setting
        /// Modifies existing integration configuration and settings
        /// </summary>
        /// <param name="dto">Updated integration data</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Integration setting successfully updated</response>
        [HttpPost("Update-IntegrationSetting")]
        public async Task<IActionResult> UpdateIntegrationSetting([FromBody] UpdateIntegrationSettingsDto dto, int tenantId)
        {
            var result = await _integrationSettingsRepository.UpdateIntegrationSettingAsync(dto, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Delete integration setting
        /// Removes integration configuration from system
        /// </summary>
        /// <param name="IntegrationId">Integration identifier</param>
        /// <param name="deletedBy">User identifier performing deletion</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Integration setting successfully deleted</response>
        [HttpPost("Delete-IntegrationSetting")]
        public async Task<IActionResult> DeleteIntegrationSetting(int IntegrationId, int deletedBy, int tenantId)
        {
            var result = await _integrationSettingsRepository.DeleteIntegrationSettingAsync(IntegrationId, deletedBy, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get all integration settings
        /// Retrieves all integration configurations for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of integration settings</returns>
        /// <response code="200">Successfully retrieved all integration settings</response>
        [HttpGet("Get-All-IntegrationSettings")]
        public async Task<IActionResult> GetAllIntegrationSettings(int tenantId)
        {
            var result = await _integrationSettingsRepository.GetAllIntegrationSettingAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get integration setting by ID
        /// Retrieves specific integration configuration details
        /// </summary>
        /// <param name="IntegrationId">Integration identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Integration setting details</returns>
        /// <response code="200">Successfully retrieved integration setting</response>
        [HttpGet("Get-IntegrationSetting-ById")]
        public async Task<IActionResult> GetIntegrationSettingById(int IntegrationId, int tenantId)
        {
            var result = await _integrationSettingsRepository.GetIntegrationSettingByIdAsync(IntegrationId, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get integration settings by category
        /// Retrieves integration configurations filtered by category
        /// </summary>
        /// <param name="category">Integration category filter</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Category-specific integration settings</returns>
        /// <response code="200">Successfully retrieved integration settings by category</response>
        [HttpGet("Get-IntegrationSettings-ByCategory")]
        public async Task<IActionResult> GetIntegrationSettingsByCategory(IntegrationSettingsCategory category, int tenantId)
        {
            var result = await _integrationSettingsRepository.GetIntegrationSettingByCategoryAsync(category, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }
}