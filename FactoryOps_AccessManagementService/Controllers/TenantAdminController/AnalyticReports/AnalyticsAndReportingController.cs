using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.AnalyticReports
{
    /// <summary>
    /// Analytics and Reporting API
    /// Provides analytics data and reporting capabilities for tenants
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsAndReportingController : ControllerBase
    {
        private readonly IAnalyticsAndReportsServices _repository;

        public AnalyticsAndReportingController(IAnalyticsAndReportsServices repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get analytics and reports
        /// Retrieves analytics data and reports for specified date range and category
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="startDate">Report start date</param>
        /// <param name="endDate">Report end date</param>
        /// <param name="category">Data category filter</param>
        /// <returns>Analytics data and reports</returns>
        /// <response code="200">Successfully retrieved analytics data</response>
        /// <response code="500">Internal server error during data retrieval</response>
        [HttpGet("Get_AnalyticsAndReporting")]
        public async Task<IActionResult> GetAnalyticsAndReportsAsync(
        [FromQuery] int tenantId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] CategoryType category = CategoryType.All)
        {
            try
            {
                // Normalize dates to UTC
                if (startDate.Kind == DateTimeKind.Unspecified)
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

                if (endDate.Kind == DateTimeKind.Unspecified)
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

                var result = await _repository
                     .GetAnalyticsAndReportsAsync(tenantId, startDate, endDate, category);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponseModel
                {
                    StatusCode = "500",
                    StatusMessage = $"Failed to fetch analytics and reports: {ex.Message}"
                });
            }

        }

        /// <summary>
        /// Export analytics and reports
        /// Generates and downloads PDF export of analytics data and reports
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="startDate">Report start date</param>
        /// <param name="endDate">Report end date</param>
        /// <param name="category">Data category filter</param>
        /// <returns>PDF file download</returns>
        /// <response code="200">Successfully exported analytics report</response>
        /// <response code="404">No data found for export</response>
        /// <response code="500">Internal server error during export</response>
        [HttpGet("Export")]
        public async Task<IActionResult> ExportAnalyticsAndReportsAsync(
        [FromQuery] int tenantId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] CategoryType category = CategoryType.All)
        {
            try
            {
                if (startDate.Kind == DateTimeKind.Unspecified)
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

                if (endDate.Kind == DateTimeKind.Unspecified)
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

                var fileBytes = await _repository.ExportAnalyticsAndReportsAsync(
                tenantId, startDate, endDate, category);

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return NotFound(new CommonResponseModel
                    {
                        StatusCode = "404",
                        StatusMessage = "No data found to export."
                    });
                }

                var fileName = $"AnalyticsAndReports_{tenantId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

                // ✅ Return as downloadable file
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponseModel
                {
                    StatusCode = "500",
                    StatusMessage = $"Failed to export analytics and reports: {ex.Message}"
                });
            }
        }
    }
}