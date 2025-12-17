using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Infrastructure.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_Inventory.Controllers.TenantAdminContoller.InventoryManagement
{
    /// <summary>
    /// Inventory Cost Integration API
    /// Manages inventory cost tracking, cost integration, and financial inventory data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryCostIntegrationController : ControllerBase
    {
        private readonly IInventoryCostIntegrationService _inventoryCostIntegrationService;
        public InventoryCostIntegrationController(IInventoryCostIntegrationService inventoryCostIntegrationService)
        {
            _inventoryCostIntegrationService = inventoryCostIntegrationService;
        }

        /// <summary>
        /// Get all inventory costs
        /// Retrieves complete list of inventory cost records and calculations
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of inventory costs</returns>
        /// <response code="200">Successfully retrieved all inventory costs</response>
        [HttpGet("Get-All-InventoryCost")]
        public async Task<IActionResult> GetInventoryCostsAsync(int tenantId)
        {
            var result = await _inventoryCostIntegrationService.GetInventoryCostsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all inventory information
        /// Retrieves comprehensive inventory item data and details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Inventory item information</returns>
        /// <response code="200">Successfully retrieved inventory information</response>
        [HttpGet("Get-All-InventoryInfo")]
        public async Task<IActionResult> GetInventoryItemInfo(int tenantId)
        {
            var result = await _inventoryCostIntegrationService.GetInventoryItemInfo(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all work order integration
        /// Retrieves work order integration data with inventory costs
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Work order integration data</returns>
        /// <response code="200">Successfully retrieved work order integration</response>
        [HttpGet("Get-All-WorkOrderIntegration")]
        public async Task<IActionResult> GetWorkOrderIntegration(int tenantId)
        {
            var result = await _inventoryCostIntegrationService.GetWorkOrderIntegration(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get inventory cost by ID
        /// Retrieves specific inventory cost details and calculations
        /// </summary>
        /// <param name="id">Inventory cost identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Inventory cost details</returns>
        /// <response code="200">Successfully retrieved inventory cost</response>
        [HttpGet("Get-InventoryCost-ById")]
        public async Task<IActionResult> GetInventoryCostByIdAsync(int id, int tenantId)
        {
            var result = await _inventoryCostIntegrationService.GetInventoryCostByIdAsync(id, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Create inventory cost
        /// Creates new inventory cost record and calculation
        /// </summary>
        /// <param name="dto">Inventory cost creation data</param>
        /// <returns>Cost creation result</returns>
        /// <response code="200">Inventory cost successfully created</response>
        [HttpPost("Create-InventoryCost")]
        public async Task<IActionResult> AddInventoryCostAsync([FromBody] CreateInventoryCostIntegrationDto dto)
        {
            var result = await _inventoryCostIntegrationService.AddInventoryCostAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update inventory cost
        /// Modifies existing inventory cost calculations and data
        /// </summary>
        /// <param name="dto">Updated inventory cost data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Inventory cost successfully updated</response>
        [HttpPost("Update-InventoryCost")]
        public async Task<IActionResult> UpdateInventoryCostAsync([FromBody] CreateInventoryCostIntegrationDto dto)
        {
            var result = await _inventoryCostIntegrationService.UpdateInventoryCostAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete inventory cost
        /// Removes inventory cost record from system
        /// </summary>
        /// <param name="id">Inventory cost identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Inventory cost successfully deleted</response>
        [HttpPost("Delete-InventoryCost")]
        public async Task<IActionResult> DeleteInventoryCostAsync([FromBody] int id, int tenantId)
        {
            var result = await _inventoryCostIntegrationService.DeleteInventoryCostAsync(id, tenantId);
            return Ok(result);
        }
    }
}