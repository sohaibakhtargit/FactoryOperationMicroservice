using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_WorkOrder.Controllers.TenantAdminController.WorkOrderManagement
{
    /// <summary>
    /// Technician Assignment Management API
    /// Manages technician assignments, work order distribution, and technician scheduling
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TechnicianAssignmentController : ControllerBase
    {
        private readonly ITechnicianAssignmentService _technicianAssignmentService;

        public TechnicianAssignmentController(ITechnicianAssignmentService technicianAssignmentService)
        {
            _technicianAssignmentService = technicianAssignmentService;
        }

        /// <summary>
        /// Get technician dashboard summary
        /// Retrieves comprehensive dashboard data for technician operations
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Technician dashboard summary</returns>
        /// <response code="200">Successfully retrieved technician dashboard</response>
        [HttpGet]
        [Route("Get-TechnicianDashboardSummary")]
        public async Task<IActionResult> GetTechnicianDashboardSummaryAsync(int tenantId)
        {
            var result = await _technicianAssignmentService.GetTechnicianDashboardSummaryAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get technician work orders
        /// Retrieves all work orders assigned to technicians
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of technician work orders</returns>
        /// <response code="200">Successfully retrieved technician work orders</response>
        [HttpGet]
        [Route("Get-Technician-WorkOrders")]
        public async Task<IActionResult> GetTechnicianWorkOrdersAsync(int tenantId)
        {
            var result = await _technicianAssignmentService.GetTechnicianWorkOrdersAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get technician details
        /// Retrieves comprehensive information about all technicians
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of technician details</returns>
        /// <response code="200">Successfully retrieved technician details</response>
        [HttpGet]
        [Route("Get-Technician-Details")]
        public async Task<IActionResult> GetTechniciansAsync(int tenantId)
        {
            var result = await _technicianAssignmentService.GetTechniciansAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get latest assignment history
        /// Retrieves recent technician assignment history and activities
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Latest assignment history</returns>
        /// <response code="200">Successfully retrieved assignment history</response>
        [HttpGet]
        [Route("Get-TechnicianetLatestAssignmentHistory")]
        public async Task<IActionResult> GetLatestAssignmentHistoryAsync(int tenantId)
        {
            var result = await _technicianAssignmentService.GetLatestAssignmentHistoryAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Update and assign technician
        /// Assigns technician to work order and updates assignment details
        /// </summary>
        /// <param name="dto">Technician assignment data</param>
        /// <returns>Assignment operation result</returns>
        /// <response code="200">Technician successfully assigned</response>
        [HttpPost]
        [Route("Update-AssignTechnician")]
        public async Task<IActionResult> AssignTechnicianAsync(AssignTechnicianUpdateWorkOrder dto)
        {
            var result = await _technicianAssignmentService.AssignTechnicianAsync(dto);
            return Ok(result);
        }
    }
}
