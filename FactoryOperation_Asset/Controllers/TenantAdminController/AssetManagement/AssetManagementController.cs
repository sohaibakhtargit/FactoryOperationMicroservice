using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.AssetManagement
{
    /// <summary>
    /// Asset Management API
    /// Manages asset registry, asset information, and asset lifecycle operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetManagementController : ControllerBase
    {
        private readonly IAssetManagementService _assetManagementService;

        public AssetManagementController(IAssetManagementService assetManagementService)
        {
            _assetManagementService = assetManagementService;
        }

        /// <summary>
        /// Add asset to registry
        /// Creates new asset record in the asset registry
        /// </summary>
        /// <param name="dto">Asset registration data</param>
        /// <returns>Asset creation result</returns>
        /// <response code="200">Asset successfully added to registry</response>
        [HttpPost("Add-Asset-Registry")]
        public async Task<IActionResult> AddAsset([FromForm] AssetRegistryDto dto)
        {
            var result = await _assetManagementService.AddAsset(dto);
            return Ok(result);
        }


        [HttpPost("bulk-import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportAssets([FromForm] BulkAssetImportRequest request)
        {
            var result = await _assetManagementService.ImportBulkAssetAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Update asset registry
        /// Modifies existing asset information in the registry
        /// </summary>
        /// <param name="dto">Updated asset data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Asset successfully updated</response>
        [HttpPost("Update-Asset-Registry")]
        public async Task<IActionResult> UpdateAsset([FromForm] AssetRegistryDto dto)
        {
            var result = await _assetManagementService.UpdateAsset(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get all assets
        /// Retrieves complete list of all assets for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of all assets</returns>
        /// <response code="200">Successfully retrieved all assets</response>
        [HttpGet("Get-AllAssets")]
        public IActionResult GetAllAssets(int tenantId)
        {
            var result = _assetManagementService.GetAllAssets(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Delete asset from registry
        /// Removes asset record from the asset registry
        /// </summary>
        /// <param name="id">Asset identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Asset successfully deleted</response>
        [HttpPost("Delete-Asset-Registry")]
        public async Task<IActionResult> DeleteAsset(int id, int tenantId)
        {
            var result = await _assetManagementService.DeleteAsset(id, tenantId);
            return Ok(result);
        }

        [HttpPost("Bulk-Asset-Delete")]
        public async Task<IActionResult>BulkAssetDeleteAsync(int tenantId, List<int> AssetId)
        {
            var result = await _assetManagementService.BulkAssetDeleteAsync(tenantId, AssetId);
            return Ok(result);
        }

        [HttpGet("Download-Asset-Billing-Pdf")]
        public async Task<IActionResult> DownloadAssetBillingPdf(int tenantId, int assetId)
        {
            var result = await _assetManagementService
                .DownloadAssetBillingPdf(tenantId, assetId);

            return File(
                result.fileBytes,
                "application/pdf",
                result.fileName
            );
        }

    }
}