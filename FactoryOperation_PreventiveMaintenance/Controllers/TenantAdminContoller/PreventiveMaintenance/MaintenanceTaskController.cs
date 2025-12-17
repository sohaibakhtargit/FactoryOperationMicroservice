using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FactoryOpsApp.Application.DTOs;
using System.Threading.Tasks;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.PreventiveMaintenance;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.PreventiveMaintenance
{
    /// <summary>
    /// Maintenance Task Management API
    /// Manages maintenance tasks, task execution, and task verification processes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceTaskController : ControllerBase
    {
        private readonly IMaintenanceTaskService _maintenanceTaskService;

        public MaintenanceTaskController(IMaintenanceTaskService maintenanceTaskService)
        {
            _maintenanceTaskService = maintenanceTaskService;
        }

        /// <summary>
        /// Add maintenance task
        /// Creates new maintenance task for work order execution
        /// </summary>
        /// <param name="dto">Maintenance task data</param>
        /// <returns>Task creation result</returns>
        /// <response code="200">Maintenance task successfully added</response>
        /// <response code="400">Invalid task data provided</response>
        [HttpPost("Add-MaintenanceTask")]
        public async Task<IActionResult> AddMaintenanceTaskAsync([FromBody] MaintenanceTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceTaskService.AddMaintenanceTaskAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update maintenance task
        /// Modifies existing maintenance task details and specifications
        /// </summary>
        /// <param name="dto">Updated task data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Maintenance task successfully updated</response>
        /// <response code="400">Invalid task data provided</response>
        [HttpPost("Update-MaintenanceTask")]
        public async Task<IActionResult> UpdateMaintenanceTaskAsync([FromBody] MaintenanceTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceTaskService.UpdateMaintenanceTaskAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete maintenance task
        /// Removes maintenance task from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Maintenance task successfully deleted</response>
        [HttpPost("Delete-MaintenanceTask")]
        public async Task<IActionResult> DeleteMaintenanceTaskAsync(int tenantId, int taskId)
        {
            var result = await _maintenanceTaskService.DeleteMaintenanceTaskAsync(taskId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Update task status
        /// Updates maintenance task status and progress tracking
        /// </summary>
        /// <param name="dto">Task status update data</param>
        /// <returns>Status update result</returns>
        /// <response code="200">Task status successfully updated</response>
        /// <response code="400">Invalid status data provided</response>
        [HttpPost("Update-TaskStatus")]
        public async Task<IActionResult> UpdateTaskStatusAsync([FromBody] TaskStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceTaskService.UpdateTaskStatusAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Verify task
        /// Verifies completion and quality of maintenance task
        /// </summary>
        /// <param name="dto">Task verification data</param>
        /// <returns>Verification operation result</returns>
        /// <response code="200">Task successfully verified</response>
        /// <response code="400">Invalid verification data provided</response>
        [HttpPost("Verify-Task")]
        public async Task<IActionResult> VerifyTaskAsync([FromBody] TaskVerificationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _maintenanceTaskService.VerifyTaskAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get tasks by work order
        /// Retrieves all tasks associated with specific work order
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="workOrderId">Work order identifier</param>
        /// <returns>List of work order tasks</returns>
        /// <response code="200">Successfully retrieved tasks by work order</response>
        [HttpGet("Get-TasksByWorkOrder")]
        public async Task<IActionResult> GetTasksByWorkOrderAsync(int tenantId, int workOrderId)
        {
            var result = await _maintenanceTaskService.GetTasksByWorkOrderAsync(workOrderId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get tasks by status
        /// Retrieves maintenance tasks grouped by current status
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Tasks grouped by status</returns>
        /// <response code="200">Successfully retrieved tasks by status</response>
        [HttpGet("Get-TasksByStatus")]
        public async Task<IActionResult> GetTasksByStatusAsync(int tenantId)
        {
            var result = await _maintenanceTaskService.GetTasksByStatusAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get maintenance task by ID
        /// Retrieves specific maintenance task details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Maintenance task details</returns>
        /// <response code="200">Successfully retrieved maintenance task</response>
        [HttpGet("Get-MaintenanceTaskById")]
        public async Task<IActionResult> GetMaintenanceTaskByIdAsync(int tenantId, int taskId)
        {
            var result = await _maintenanceTaskService.GetMaintenanceTaskByIdAsync(taskId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get tasks requiring verification
        /// Retrieves maintenance tasks pending quality verification
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of tasks requiring verification</returns>
        /// <response code="200">Successfully retrieved tasks requiring verification</response>
        [HttpGet("Get-TasksRequiringVerification")]
        public async Task<IActionResult> GetTasksRequiringVerificationAsync(int tenantId)
        {
            var result = await _maintenanceTaskService.GetTasksRequiringVerificationAsync(tenantId);
            return Ok(result);
        }
    }
}