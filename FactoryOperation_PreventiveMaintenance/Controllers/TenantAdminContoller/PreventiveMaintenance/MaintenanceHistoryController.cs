using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.PreventiveMaintenance
{
    /// <summary>
    /// Maintenance History Management API
    /// Manages maintenance history, service records, and maintenance metrics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceHistoryController : ControllerBase
    {
        private readonly IMaintenanceHistoryService _maintenanceHistoryService;

        public MaintenanceHistoryController(IMaintenanceHistoryService maintenanceHistoryService)
        {
            _maintenanceHistoryService = maintenanceHistoryService;
        }

        /// <summary>
        /// Add maintenance history
        /// Creates new maintenance record and service history entry
        /// </summary>
        /// <param name="dto">Maintenance history data</param>
        /// <returns>Maintenance creation result</returns>
        /// <response code="200">Maintenance history successfully added</response>
        /// <response code="400">Invalid maintenance data provided</response>
        [HttpPost("Add-MaintenanceHistory")]
        public async Task<IActionResult> AddMaintenanceHistoryAsync([FromBody] MaintenanceHistoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceHistoryService.AddMaintenanceHistoryAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update maintenance history
        /// Modifies existing maintenance record and service details
        /// </summary>
        /// <param name="dto">Updated maintenance data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Maintenance history successfully updated</response>
        /// <response code="400">Invalid maintenance data provided</response>
        [HttpPost("Update-MaintenanceHistory")]
        public async Task<IActionResult> UpdateMaintenanceHistoryAsync([FromBody] MaintenanceHistoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceHistoryService.UpdateMaintenanceHistoryAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete maintenance history
        /// Removes maintenance record from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="maintenanceId">Maintenance identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Maintenance history successfully deleted</response>
        [HttpPost("Delete-MaintenanceHistory")]
        public async Task<IActionResult> DeleteMaintenanceHistoryAsync(int tenantId, long maintenanceId)
        {
            var result = await _maintenanceHistoryService.DeleteMaintenanceHistoryAsync(maintenanceId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all maintenance history
        /// Retrieves complete maintenance history with optional filtering
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="searchTerm">Optional search term</param>
        /// <param name="statusFilter">Optional status filter</param>
        /// <param name="typeFilter">Optional type filter</param>
        /// <returns>List of maintenance history records</returns>
        /// <response code="200">Successfully retrieved maintenance history</response>
        [HttpGet("Get-AllMaintenanceHistory")]
        public async Task<IActionResult> GetAllMaintenanceHistory(int tenantId, string? searchTerm = null, string? statusFilter = null, string? typeFilter = null)
        {
            var result = await _maintenanceHistoryService.GetAllMaintenanceHistoryAsync(tenantId, searchTerm, statusFilter, typeFilter);
            return Ok(result);
        }

        /// <summary>
        /// Get maintenance history by ID
        /// Retrieves specific maintenance record details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="maintenanceId">Maintenance identifier</param>
        /// <returns>Maintenance history details</returns>
        /// <response code="200">Successfully retrieved maintenance history</response>
        [HttpGet("Get-MaintenanceHistoryById")]
        public async Task<IActionResult> GetMaintenanceHistoryByIdAsync(int tenantId, long maintenanceId)
        {
            var result = await _maintenanceHistoryService.GetMaintenanceHistoryByIdAsync(maintenanceId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get maintenance metrics
        /// Retrieves maintenance performance metrics and analytics
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Maintenance metrics and analytics</returns>
        /// <response code="200">Successfully retrieved maintenance metrics</response>
        [HttpGet("Get-MaintenanceMetrics")]
        public async Task<IActionResult> GetMaintenanceMetricsAsync(int tenantId)
        {
            var result = await _maintenanceHistoryService.GetMaintenanceMetricsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get maintenance history by asset ID
        /// Retrieves maintenance records for specific asset
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="assetId">Asset identifier</param>
        /// <returns>Asset-specific maintenance history</returns>
        /// <response code="200">Successfully retrieved asset maintenance history</response>
        [HttpGet("Get-MaintenanceHistoryByAssetId")]
        public async Task<IActionResult> GetMaintenanceHistoryByAssetIdAsync(int tenantId, int assetId)
        {
            var result = await _maintenanceHistoryService.GetMaintenanceHistoryByAssetIdAsync(assetId, tenantId);
            return Ok(result);
        }
    }
}