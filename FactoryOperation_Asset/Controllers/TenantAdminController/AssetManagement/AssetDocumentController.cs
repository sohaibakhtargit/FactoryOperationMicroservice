using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.AssetManagement
{
    /// <summary>
    /// Asset Documents Management API
    /// Manages asset-related documents, compliance files, and document tracking
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AssetDocumentsController : ControllerBase
    {
        private readonly IAssetDocumentService _assetDocumentService;

        public AssetDocumentsController(IAssetDocumentService assetDocumentService)
        {
            _assetDocumentService = assetDocumentService;
        }

        /// <summary>
        /// Add document to asset
        /// Uploads and attaches document to specific asset
        /// </summary>
        /// <param name="dto">Asset document data</param>
        /// <returns>Document creation result</returns>
        /// <response code="200">Document successfully added</response>
        [HttpPost]
        [Route("Add-Document")]
        public async Task<IActionResult> AddDocument([FromForm] CreateAssetDocumentDto dto)
        {
            var result = await _assetDocumentService.AddAssetDocumentAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update asset document
        /// Modifies existing asset document information and metadata
        /// </summary>
        /// <param name="dto">Updated document data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Document successfully updated</response>
        [HttpPost("Update-Asset-Document")]
        public async Task<IActionResult> UpdateAssetDocument([FromForm] UpdateAssetDocumentDto dto)
        {
            var result = await _assetDocumentService.UpdateAssetDocument(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get all documents
        /// Retrieves all asset documents for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of all asset documents</returns>
        /// <response code="200">Successfully retrieved all documents</response>
        [HttpGet("Get-AllDocuments")]
        public IActionResult GetAllAssetDocuments(int tenantId)
        {
            var result = _assetDocumentService.GetAllAssetDocuments(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get documents by asset ID
        /// Retrieves documents specific to a particular asset
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="assetId">Asset identifier</param>
        /// <returns>Asset-specific documents</returns>
        /// <response code="200">Successfully retrieved asset documents</response>
        [HttpGet("Get-DocumentsByAssetId")]
        public IActionResult GetAssetDocumentsByAssetId(int tenantId, int assetId)
        {
            var result = _assetDocumentService.GetAssetDocumentsByAssetId(tenantId, assetId);
            return Ok(result);
        }

        /// <summary>
        /// Get all document URLs
        /// Retrieves download URLs for all asset documents
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of document URLs</returns>
        /// <response code="200">Successfully retrieved document URLs</response>
        [HttpGet("Get-AllDocumentsUrl")]
        public IActionResult GetAssetDocumentsUrl(int tenantId)
        {
            var result = _assetDocumentService.GetAssetDocumentsUrl(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all compliance documents
        /// Retrieves asset documents with compliance status and information
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Compliance document data</returns>
        /// <response code="200">Successfully retrieved compliance documents</response>
        [HttpGet("Get-AllDocumentsCompliance")]
        public IActionResult GetAllAssetDocumentsCompliance(int tenantId)
        {
            var result = _assetDocumentService.GetAllAssetDocumentsCompliance(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Delete asset document
        /// Removes asset document from the system
        /// </summary>
        /// <param name="id">Document identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Document successfully deleted</response>
        [HttpPost("Delete-Asset-Document")]
        public async Task<IActionResult> DeleteAssetDocument(int id, int tenantId)
        {
            var result = await _assetDocumentService.DeleteAssetDocument(id, tenantId);
            return Ok(result);
        }
    }
}