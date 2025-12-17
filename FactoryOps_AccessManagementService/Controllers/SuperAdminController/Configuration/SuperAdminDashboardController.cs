using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Configuration
{
    /// <summary>
    /// Super Admin Dashboard API
    /// Provides comprehensive analytics and overview data for super admin dashboard
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminDashboardController : ControllerBase
    {
        private readonly ISuperAdminDashboardService _tenantService;

        public SuperAdminDashboardController(ISuperAdminDashboardService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// Get all active tenants
        /// Retrieves list of currently active tenants in the system
        /// </summary>
        /// <returns>List of active tenants</returns>
        /// <response code="200">Successfully retrieved active tenants</response>
        [HttpGet]
        [Route("Get-ActiveTenants")]
        public IActionResult GetActiveTenants()
        {
            var result = _tenantService.GetActiveTenantsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get all active users
        /// Retrieves list of currently active users across all tenants
        /// </summary>
        /// <returns>List of active users</returns>
        /// <response code="200">Successfully retrieved active users</response>
        [HttpGet]
        [Route("Get-ActiveUsers")]
        public IActionResult GetActiveUsers()
        {
            var result = _tenantService.GetActiveUsersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get comprehensive analytics report
        /// Retrieves detailed analytics and performance metrics for all tenants
        /// </summary>
        /// <returns>Complete analytics report for super admin</returns>
        /// <response code="200">Successfully retrieved analytics report</response>
        [HttpGet]
        [Route("GetAnalytic-Report-SuperAdmin")]
        public async Task<IActionResult> GetTenantAnalyticsReportForSuperAdminAsync()
        {
            var result = await _tenantService.GetTenantAnalyticsReportForSuperAdminAsync();
            return Ok(result);
        }
    }
}