using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TenantManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.TenantManagement
{
    /// <summary>
    /// Company Branding Management API
    /// Manages company branding, logos, and visual identity across the platform
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyBrandingController : ControllerBase
    {
        private readonly ICompanyBrandingInfoService _iCompanyBrandingInfoService;
        public CompanyBrandingController(ICompanyBrandingInfoService iCompanyBrandingInfoService)
        {
            _iCompanyBrandingInfoService = iCompanyBrandingInfoService;
        }

        /// <summary>
        /// Create company branding
        /// Sets up company visual identity including logos and branding elements
        /// </summary>
        /// <param name="dto">Company branding data</param>
        /// <returns>Branding creation result</returns>
        /// <response code="200">Company branding successfully created</response>
        /// <response code="500">Internal server error during creation</response>
        [HttpPost("CreateCompanyBranding")]
        public async Task<IActionResult> CreateCompanyBranding([FromForm] CreateCompanyBrandingDto dto)
        {
            var result = await _iCompanyBrandingInfoService.CreateCompanyBrandingAsync(dto);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }

        /// <summary>
        /// Update company branding
        /// Modifies existing company branding and visual identity elements
        /// </summary>
        /// <param name="dto">Updated branding data</param>
        /// <returns>Branding update result</returns>
        /// <response code="200">Company branding successfully updated</response>
        /// <response code="500">Internal server error during update</response>
        [HttpPost("UpdateCompanyBranding")]
        public async Task<IActionResult> UpdateCompanyBranding([FromForm] UpdateCompanyBrandingDto dto)
        {
            var result = await _iCompanyBrandingInfoService.UpdateCompanyBrandingAsync(dto);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }

        /// <summary>
        /// Get company branding
        /// Retrieves all company branding configurations and visual assets
        /// </summary>
        /// <returns>List of company branding configurations</returns>
        /// <response code="200">Successfully retrieved company branding</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("GetCompanyBranding")]
        public IActionResult GetCompanyBranding()
        {
            var result = _iCompanyBrandingInfoService.GetAllCompanyBrandings();
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }
    }
}