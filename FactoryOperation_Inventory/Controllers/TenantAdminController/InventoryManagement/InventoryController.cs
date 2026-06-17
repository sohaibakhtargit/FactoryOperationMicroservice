using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FactoryOperation_Inventory.Controllers.TenantAdminContoller.InventoryManagement
{
    /// <summary>
    /// Inventory Management API
    /// Manages inventory items, stock tracking, and inventory operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Add inventory items
        /// Creates new inventory items and stock entries
        /// </summary>
        /// <param name="dto">Inventory item data</param>
        /// <returns>Inventory creation result</returns>
        /// <response code="200">Inventory item successfully added</response>
        [HttpPost("add-Inventory-Items")]
        public async Task<IActionResult> Create([FromBody] InventoryDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get all inventory items
        /// Retrieves complete list of all inventory items for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of inventory items</returns>
        /// <response code="200">Successfully retrieved all inventory items</response>
        [HttpGet("getAllItemsInventory")]
        public async Task<IActionResult> GetAll(int tenantId)
        {
            var result = await _service.GetAllAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get stock tracking
        /// Retrieves real-time stock movement and tracking data
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Stock tracking information</returns>
        /// <response code="200">Successfully retrieved stock tracking</response>
        [HttpGet("getStockTracking")]
        public async Task<IActionResult> GetStockTracking(int tenantId)
        {
            var result = await _service.GetStockTrackingAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get stock reservations
        /// Retrieves reserved stock items and allocation data
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Stock reservation information</returns>
        /// <response code="200">Successfully retrieved stock reservations</response>
        [HttpGet("getStockReservations")]
        public async Task<IActionResult> GetStockReservations(int tenantId)
        {
            var result = await _service.GetStockReservationsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get serial batch tracking
        /// Retrieves serial and batch number tracking information
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Serial batch tracking data</returns>
        /// <response code="200">Successfully retrieved serial batch tracking</response>
        [HttpGet("getSerialBatchTracking")]
        public async Task<IActionResult> GetSerialBatchTracking(int tenantId)
        {
            var result = await _service.GetSerialBatchTrackingAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get stock tracking summary
        /// Retrieves summarized stock tracking data by type
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="type">Stock tracking type filter</param>
        /// <returns>Stock tracking summary</returns>
        /// <response code="200">Successfully retrieved stock tracking summary</response>
        [HttpGet("getStockTrackingSummary")]
        public async Task<IActionResult> GetStockTrackingSummary(int tenantId, StockTrackingType type)
        {
            var result = await _service.GetStockTrackingSummaryAsync(tenantId, type);
            return Ok(result);
        }

        /// <summary>
        /// Get item by ID
        /// Retrieves specific inventory item details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Inventory item identifier</param>
        /// <returns>Inventory item details</returns>
        /// <response code="200">Successfully retrieved inventory item</response>
        [HttpGet("getItemById")]
        public async Task<IActionResult> GetById(int tenantId, int id)
        {
            var result = await _service.GetByIdAsync(id, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Update inventory items
        /// Modifies existing inventory item information and stock data
        /// </summary>
        /// <param name="dto">Updated inventory data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Inventory item successfully updated</response>
        [HttpPost("update-Inventory-Items")]
        public async Task<IActionResult> Update([FromBody] InventoryDto dto)
        {
            var result = await _service.UpdateAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete inventory items
        /// Removes inventory items from the system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Inventory item identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Inventory item successfully deleted</response>
        [HttpPost("delete-Inventory-Items")]
        public async Task<IActionResult> Delete(int tenantId, int id)
        {
            var result = await _service.DeleteAsync(id, tenantId);
            return Ok(result);
        }
    }
}