using Microsoft.AspNetCore.Mvc;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.AssetManagement
{
    /// <summary>
    /// Asset Type Management API
    /// Manages asset type definitions, classifications, and type configurations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetTypeController : ControllerBase
    {
        private readonly IAssetTypeService _service;

        public AssetTypeController(IAssetTypeService service)
        {
            _service = service;
        }

        /// <summary>
        /// Create asset type
        /// Creates new asset type definition and classification
        /// </summary>
        /// <param name="dto">Asset type creation data</param>
        /// <returns>Asset type creation result</returns>
        /// <response code="200">Asset type successfully created</response>
        [HttpPost("Create-AssetType")]
        public async Task<IActionResult> CreateAssetType([FromBody] CreateAssetTypeDto dto)
            => Ok(await _service.CreateAssetTypeAsync(dto));

        /// <summary>
        /// Update asset type
        /// Modifies existing asset type definition and properties
        /// </summary>
        /// <param name="dto">Updated asset type data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Asset type successfully updated</response>
        [HttpPost("Update-AssetType")]
        public async Task<IActionResult> UpdateAssetType([FromBody] UpdateAssetTypeDto dto)
            => Ok(await _service.UpdateAssetTypeAsync(dto));

        /// <summary>
        /// Delete asset type
        /// Removes asset type definition from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Asset type identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Asset type successfully deleted</response>
        [HttpPost("Delete-AssetType")]
        public async Task<IActionResult> DeleteAssetType(int tenantId, int id)
            => Ok(await _service.DeleteAssetTypeAsync(id, tenantId));

        /// <summary>
        /// Get all asset types
        /// Retrieves complete list of all asset types for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of all asset types</returns>
        /// <response code="200">Successfully retrieved all asset types</response>
        [HttpGet("Getall-AssetType")]
        public async Task<IActionResult> GetAllAssetTypes(int tenantId)
            => Ok(await _service.GetAllAssetTypesAsync(tenantId));

        /// <summary>
        /// Get asset type by ID
        /// Retrieves specific asset type details and configuration
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Asset type identifier</param>
        /// <returns>Asset type details</returns>
        /// <response code="200">Successfully retrieved asset type</response>
        /// <response code="404">Asset type not found</response>
        [HttpGet("GetAsset-Type-ById")]
        public async Task<IActionResult> GetAssetTypeById(int tenantId, int id)
        {
            var result = await _service.GetAssetTypeByIdAsync(id, tenantId);
            return result == null ? NotFound() : Ok(result);
        }
    }
}