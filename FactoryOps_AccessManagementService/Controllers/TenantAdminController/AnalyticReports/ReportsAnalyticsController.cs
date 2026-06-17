using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.AnalyticReports
{
    /// <summary>
    /// Reports and Analytics API
    /// Provides comprehensive reporting and analytics data for tenant operations
    /// </summary>
    [Route("api/tenant/reports-analytics")]
    [ApiController]
    public class ReportsAnalyticsController : ControllerBase
    {
        private readonly IReportsAnalyticsService _reportsAnalyticsService;

        public ReportsAnalyticsController(IReportsAnalyticsService reportsAnalyticsService)
        {
            _reportsAnalyticsService = reportsAnalyticsService;
        }

        /// <summary>
        /// Get tenant analytics report
        /// Retrieves comprehensive analytics and reporting data for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Tenant analytics report data</returns>
        /// <response code="200">Successfully retrieved tenant analytics report</response>
        /// <response code="400">Invalid tenant identifier provided</response>
        [HttpGet]
        public async Task<IActionResult> GetTenantAnalyticsReportAsync([FromHeader(Name = "TenantId")] int tenantId)
        {
            if (tenantId <= 0)
                return BadRequest("Invalid TenantId");

            var result = await _reportsAnalyticsService.GetTenantAnalyticsReportAsync(tenantId);

            return Ok(result);
        }
    }
}