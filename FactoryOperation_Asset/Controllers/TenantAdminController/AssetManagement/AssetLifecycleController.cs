using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.AssetManagement
{
    /// <summary>
    /// Asset Lifecycle Management API
    /// Manages asset lifecycle stages, transitions, and lifecycle metrics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetLifecycleController : ControllerBase
    {
        private readonly IAssetLifecycleService _assetLifecycleService;

        public AssetLifecycleController(IAssetLifecycleService assetLifecycleService)
        {
            _assetLifecycleService = assetLifecycleService;
        }

        /// <summary>
        /// Add asset lifecycle
        /// Creates new asset lifecycle stage or transition
        /// </summary>
        /// <param name="dto">Asset lifecycle data</param>
        /// <returns>Lifecycle creation result</returns>
        /// <response code="200">Lifecycle successfully added</response>
        /// <response code="400">Invalid input data provided</response>
        [HttpPost("Add-AssetLifecycle")]
        public async Task<IActionResult> AddAssetLifecycleAsync([FromBody] AssetLifecycleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _assetLifecycleService.AddAssetLifecycleAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update asset lifecycle
        /// Modifies existing asset lifecycle information
        /// </summary>
        /// <param name="dto">Updated lifecycle data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Lifecycle successfully updated</response>
        /// <response code="400">Invalid input data provided</response>
        [HttpPost("Update-AssetLifecycle")]
        public async Task<IActionResult> UpdateAssetLifecycleAsync([FromBody] AssetLifecycleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _assetLifecycleService.UpdateAssetLifecycleAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete asset lifecycle
        /// Removes asset lifecycle record from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="lifecycleId">Lifecycle identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Lifecycle successfully deleted</response>
        [HttpPost("Delete-AssetLifecycle")]
        public async Task<IActionResult> DeleteAssetLifecycleAsync(int tenantId, long lifecycleId)
        {
            var result = await _assetLifecycleService.DeleteAssetLifecycleAsync(lifecycleId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all asset lifecycles
        /// Retrieves all asset lifecycles with optional stage filtering
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="stageFilter">Optional lifecycle stage filter</param>
        /// <returns>List of asset lifecycles</returns>
        /// <response code="200">Successfully retrieved lifecycles</response>
        [HttpGet("Get-AllAssetLifecycles")]
        public async Task<IActionResult> GetAllAssetLifecycles(int tenantId, string? stageFilter = null)
        {
            var result = await _assetLifecycleService.GetAllAssetLifecyclesAsync(tenantId, stageFilter);
            return Ok(result);
        }

        /// <summary>
        /// Get asset lifecycle by ID
        /// Retrieves specific asset lifecycle details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="lifecycleId">Lifecycle identifier</param>
        /// <returns>Lifecycle details</returns>
        /// <response code="200">Successfully retrieved lifecycle</response>
        [HttpGet("Get-AssetLifecycleById")]
        public async Task<IActionResult> GetAssetLifecycleByIdAsync(int tenantId, long lifecycleId)
        {
            var result = await _assetLifecycleService.GetAssetLifecycleByIdAsync(lifecycleId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get asset lifecycle by asset ID
        /// Retrieves lifecycle information for specific asset
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="assetId">Asset identifier</param>
        /// <returns>Asset lifecycle data</returns>
        /// <response code="200">Successfully retrieved asset lifecycle</response>
        [HttpGet("Get-AssetLifecycleByAssetId")]
        public async Task<IActionResult> GetAssetLifecycleByAssetIdAsync(int tenantId, int assetId)
        {
            var result = await _assetLifecycleService.GetAssetLifecycleByAssetIdAsync(assetId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get asset lifecycle metrics
        /// Retrieves lifecycle performance metrics and analytics
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Lifecycle metrics and analytics</returns>
        /// <response code="200">Successfully retrieved lifecycle metrics</response>
        [HttpGet("Get-AssetLifecycleMetrics")]
        public async Task<IActionResult> GetAssetLifecycleMetricsAsync(int tenantId)
        {
            var result = await _assetLifecycleService.GetAssetLifecycleMetricsAsync(tenantId);
            return Ok(result);
        }

        [HttpGet("Get-AssetLifeSummery")]
        public async Task<IActionResult> GetAssetLifeCycleSummery(int tenantId)
        {
            var result = await _assetLifecycleService.GetAssetLifeCycleSummery(tenantId);
            return Ok(result);
        }

        [HttpGet("GetAssetLifeHistoryReport")]
        public async Task<IActionResult> GetAssetLifeHistoryReport(int tenantId, int assetId)
        {
            var result = await _assetLifecycleService.GetAssetLifeHistoryReport(tenantId, assetId);
            return Ok(result);
        }

    }

    /// <summary>
    /// Asset Financial Analysis API
    /// Manages financial analysis, cost tracking, and ROI calculations for assets
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssetFinancialAnalysisController : ControllerBase
    {
        private readonly IAssetFinancialAnalysisService _financialAnalysisService;

        public AssetFinancialAnalysisController(IAssetFinancialAnalysisService financialAnalysisService)
        {
            _financialAnalysisService = financialAnalysisService;
        }

        /// <summary>
        /// Add financial analysis
        /// Creates new financial analysis record for asset
        /// </summary>
        /// <param name="dto">Financial analysis data</param>
        /// <returns>Analysis creation result</returns>
        /// <response code="200">Analysis successfully added</response>
        /// <response code="400">Invalid input data provided</response>
        [HttpPost("Add-FinancialAnalysis")]
        public async Task<IActionResult> AddFinancialAnalysisAsync([FromBody] AssetFinancialAnalysisDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _financialAnalysisService.AddFinancialAnalysisAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete financial analysis
        /// Removes financial analysis record from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="analysisId">Analysis identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Analysis successfully deleted</response>
        [HttpPost("Delete-FinancialAnalysis")]
        public async Task<IActionResult> DeleteFinancialAnalysisAsync(int tenantId, long analysisId)
        {
            var result = await _financialAnalysisService.DeleteFinancialAnalysisAsync(analysisId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get financial analyses by asset ID
        /// Retrieves financial analyses for specific asset with optional type filtering
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="analysisType">Optional analysis type filter</param>
        /// <returns>Asset financial analyses</returns>
        /// <response code="200">Successfully retrieved financial analyses</response>
        [HttpGet("Get-FinancialAnalysesByAssetId")]
        public async Task<IActionResult> GetFinancialAnalysesByAssetIdAsync(int tenantId, int assetId, string? analysisType = null)
        {
            var result = await _financialAnalysisService.GetFinancialAnalysesByAssetIdAsync(assetId, tenantId, analysisType);
            return Ok(result);
        }

        /// <summary>
        /// Get all financial analyses
        /// Retrieves all financial analyses with optional type filtering
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="analysisType">Optional analysis type filter</param>
        /// <returns>List of financial analyses</returns>
        /// <response code="200">Successfully retrieved all analyses</response>
        [HttpGet("Get-AllFinancialAnalyses")]
        public async Task<IActionResult> GetAllFinancialAnalysesAsync(int tenantId, string? analysisType = null)
        {
            var result = await _financialAnalysisService.GetAllFinancialAnalysesAsync(tenantId, analysisType);
            return Ok(result);
        }
    }
}