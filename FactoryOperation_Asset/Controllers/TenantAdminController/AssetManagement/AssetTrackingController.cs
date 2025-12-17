using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.AssetManagement
{
    /// <summary>
    /// Asset Tracking API
    /// Manages real-time asset tracking, location monitoring, and tracking history
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetTrackingController : ControllerBase
    {
        private readonly IAsssetTrackingRepository _assetTrackingRepository;

        public AssetTrackingController(IAsssetTrackingRepository assetTrackingRepository)
        {
            _assetTrackingRepository = assetTrackingRepository;
        }

        /// <summary>
        /// Create asset tracking
        /// Creates new asset tracking record with location and status data
        /// </summary>
        /// <param name="dto">Asset tracking creation data</param>
        /// <returns>Tracking creation result</returns>
        /// <response code="200">Asset tracking successfully created</response>
        [HttpPost("Create-AssetTracking")]
        public async Task<IActionResult> CreateAssetTracking([FromBody] AssetTrackingCreateDto dto)
        {
            var result = await _assetTrackingRepository.CreateAssetTrackingAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Update asset tracking
        /// Modifies existing asset tracking information and location data
        /// </summary>
        /// <param name="dto">Updated tracking data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Asset tracking successfully updated</response>
        [HttpPost("Update-AssetTracking")]
        public async Task<IActionResult> UpdateAssetTracking([FromBody] AssetTrackingUpdateDto dto)
        {
            var result = await _assetTrackingRepository.UpdateAssetTrackingAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Delete asset tracking
        /// Removes asset tracking record from system
        /// </summary>
        /// <param name="trackingId">Tracking identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Asset tracking successfully deleted</response>
        [HttpPost("delete-AssetTracking")]
        public async Task<IActionResult> DeleteAssetTracking(long trackingId, int tenantId)
        {
            var result = await _assetTrackingRepository.DeleteAssetTrackingAsync(trackingId, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get all asset trackings
        /// Retrieves complete history of all asset tracking records
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of all asset trackings</returns>
        /// <response code="200">Successfully retrieved all asset trackings</response>
        [HttpGet("Get-all-AssetTracking")]
        public async Task<IActionResult> GetAllAssetTrackings(int tenantId)
        {
            var result = await _assetTrackingRepository.GetAllAssetTrackingsAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get asset tracking by ID
        /// Retrieves specific asset tracking details
        /// </summary>
        /// <param name="trackingId">Tracking identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Asset tracking details</returns>
        /// <response code="200">Successfully retrieved asset tracking</response>
        [HttpGet("GetAssetTracking-ById")]
        public async Task<IActionResult> GetAssetTrackingById(long trackingId, int tenantId)
        {
            var result = await _assetTrackingRepository.GetAssetTrackingByIdAsync(trackingId, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get latest asset trackings
        /// Retrieves most recent asset tracking records
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="count">Number of recent records to retrieve</param>
        /// <returns>Latest asset tracking records</returns>
        /// <response code="200">Successfully retrieved latest asset trackings</response>
        [HttpGet("GetLatestAssetTracking")]
        public async Task<IActionResult> GetLatestAssetTrackingsAsync(int tenantId, int count = 3)
        {
            var result = await _assetTrackingRepository.GetLatestAssetTrackingsAsync(tenantId, count);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }
}