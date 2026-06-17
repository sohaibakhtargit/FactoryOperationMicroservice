using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.AnalyticReports
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianDashboardController : ControllerBase
    {
        private readonly ITechnicianDashboardService _service;

        public TechnicianDashboardController(ITechnicianDashboardService service)
        {
            _service = service;
        }

        [HttpGet("GetTechnicianDashboard")]
        public async Task<IActionResult> GetTechnicianDashboard(int tenantId,
      int userId,
      DashboardFilter filter = DashboardFilter.Month)
        {
            var result = await _service.GetTechnicianDashboardAsync(tenantId, userId, filter);
            return Ok(result);
        }
    }
}
