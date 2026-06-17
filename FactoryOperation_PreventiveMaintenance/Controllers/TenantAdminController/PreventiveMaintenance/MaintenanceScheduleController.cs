using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.PreventiveMaintenance
{
    /// <summary>
    /// Maintenance Schedule Management API
    /// Manages maintenance schedules, scheduling operations, and maintenance planning
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceScheduleController : ControllerBase
    {
        private readonly IMaintenanceScheduleService _maintenanceScheduleService;

        public MaintenanceScheduleController(IMaintenanceScheduleService maintenanceScheduleService)
        {
            _maintenanceScheduleService = maintenanceScheduleService;
        }

        /// <summary>
        /// Add maintenance schedule
        /// Creates new maintenance schedule and planning entry
        /// </summary>
        /// <param name="dto">Maintenance schedule data</param>
        /// <returns>Schedule creation result</returns>
        /// <response code="200">Maintenance schedule successfully added</response>
        /// <response code="400">Invalid schedule data provided</response>
        [HttpPost("Add-MaintenanceSchedule")]
        public async Task<IActionResult> AddMaintenanceScheduleAsync([FromForm] MaintenanceScheduleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceScheduleService.AddMaintenanceScheduleAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update maintenance schedule
        /// Modifies existing maintenance schedule and planning details
        /// </summary>
        /// <param name="dto">Updated schedule data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Maintenance schedule successfully updated</response>
        /// <response code="400">Invalid schedule data provided</response>
        [HttpPost("Update-MaintenanceSchedule")]
        public async Task<IActionResult> UpdateMaintenanceScheduleAsync([FromForm] MaintenanceScheduleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceScheduleService.UpdateMaintenanceScheduleAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete maintenance schedule
        /// Removes maintenance schedule from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="scheduleId">Schedule identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Maintenance schedule successfully deleted</response>
        [HttpPost("Delete-MaintenanceSchedule")]
        public async Task<IActionResult> DeleteMaintenanceScheduleAsync(int tenantId, int scheduleId)
        {
            var result = await _maintenanceScheduleService.DeleteMaintenanceScheduleAsync(scheduleId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Approve schedule
        /// Approves maintenance schedule for execution
        /// </summary>
        /// <param name="dto">Schedule approval data</param>
        /// <returns>Approval operation result</returns>
        /// <response code="200">Schedule successfully approved</response>
        /// <response code="400">Invalid approval data provided</response>
        [HttpPost("Approve-Schedule")]
        public async Task<IActionResult> ApproveScheduleAsync([FromBody] ScheduleApprovalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceScheduleService.ApproveScheduleAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get all maintenance schedules
        /// Retrieves all maintenance schedules with optional status filtering
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="statusFilter">Optional work order status filter</param>
        /// <returns>List of maintenance schedules</returns>
        /// <response code="200">Successfully retrieved all maintenance schedules</response>
        [HttpGet("Get-AllMaintenanceSchedules")]
        public async Task<IActionResult> GetAllMaintenanceSchedules(int tenantId, WorkOrderStatus? statusFilter = null)
        {
            var result = await _maintenanceScheduleService.GetAllMaintenanceSchedulesAsync(tenantId, statusFilter);
            return Ok(result);
        }

        /// <summary>
        /// Get maintenance schedule by ID
        /// Retrieves specific maintenance schedule details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="scheduleId">Schedule identifier</param>
        /// <returns>Maintenance schedule details</returns>
        /// <response code="200">Successfully retrieved maintenance schedule</response>
        [HttpGet("Get-MaintenanceScheduleById")]
        public async Task<IActionResult> GetMaintenanceScheduleByIdAsync(int tenantId, int scheduleId)
        {
            var result = await _maintenanceScheduleService.GetMaintenanceScheduleByIdAsync(scheduleId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Calculate next due date
        /// Calculates next maintenance due date based on schedule
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="scheduleId">Schedule identifier</param>
        /// <returns>Next due date calculation result</returns>
        /// <response code="200">Successfully calculated next due date</response>
        [HttpPost("Calculate-NextDueDate")]
        public async Task<IActionResult> CalculateNextDueDateAsync(int tenantId, int scheduleId)
        {
            var result = await _maintenanceScheduleService.CalculateNextDueDateAsync(scheduleId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get occurrences by schedule ID
        /// Retrieves all schedule occurrences for specific maintenance schedule
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="scheduleId">Schedule identifier</param>
        /// <returns>List of schedule occurrences</returns>
        /// <response code="200">Successfully retrieved schedule occurrences</response>
        [HttpGet("Get-OccurrencesByScheduleId")]
        public async Task<IActionResult> GetOccurrencesByScheduleIdAsync(int tenantId, int scheduleId)
        {
            var result = await _maintenanceScheduleService.GetOccurrencesByScheduleIdAsync(scheduleId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get upcoming occurrences
        /// Retrieves upcoming maintenance occurrences within specified days
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="daysAhead">Number of days to look ahead</param>
        /// <returns>List of upcoming occurrences</returns>
        /// <response code="200">Successfully retrieved upcoming occurrences</response>
        [HttpGet("Get-UpcomingOccurrences")]
        public async Task<IActionResult> GetUpcomingOccurrencesAsync(int tenantId, int daysAhead = 7)
        {
            var result = await _maintenanceScheduleService.GetUpcomingOccurrencesAsync(tenantId, daysAhead);
            return Ok(result);
        }

        /// <summary>
        /// Regenerate occurrences
        /// Manually regenerates schedule occurrences for maintenance planning
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="scheduleId">Schedule identifier</param>
        /// <returns>Regeneration operation result</returns>
        /// <response code="200">Successfully regenerated occurrences</response>
        [HttpPost("Regenerate-Occurrences")]
        public async Task<IActionResult> RegenerateOccurrencesAsync(int tenantId, int scheduleId)
        {
            var result = await _maintenanceScheduleService.RegenerateOccurrencesAsync(scheduleId, tenantId);
            return Ok(result);
        }
    }
}