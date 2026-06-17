using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.AssetManagement
{
    /// <summary>
    /// Asset Dashboard Report API
    /// Provides asset management dashboard summaries and reports for tenants
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AssetDashboardReportController : ControllerBase
    {
        private readonly IAssetDashboardReportService _dashboardReportService;

        public AssetDashboardReportController(IAssetDashboardReportService dashboardReportService)
        {
            _dashboardReportService = dashboardReportService;
        }

        /// <summary>
        /// Get dashboard summary
        /// Retrieves comprehensive asset management dashboard summary for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Asset dashboard summary data</returns>
        /// <response code="200">Successfully retrieved dashboard summary</response>
        [HttpGet("Get_Dashboard_Summary")]
        public async Task<IActionResult> GetDashboardSummary(int tenantId)
        {
            var result = await _dashboardReportService.GetDashboardSummaryAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
        [HttpGet("Fetch_Dashboard_Data")]
        public async Task<IActionResult> FetchDashboardDataAsync(int tenantId)
        {
            var result = await _dashboardReportService.FetchDashboardDataAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }
}