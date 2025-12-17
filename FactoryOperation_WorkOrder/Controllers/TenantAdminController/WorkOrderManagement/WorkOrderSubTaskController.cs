using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_WorkOrder.Controllers.TenantAdminController.WorkOrderManagement
{    /// <summary>
     /// Work Order SubTask Management API
     /// Manages work order subtasks, task breakdown, and subtask operations
     /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderSubTaskController : ControllerBase
    {
        private readonly IWorkOrderSubTaskService _workOrderSubTaskService;
        public WorkOrderSubTaskController(IWorkOrderSubTaskService workOrderSubTaskService)
        {
            _workOrderSubTaskService = workOrderSubTaskService;
        }

        /// <summary>
        /// Get all work order subtasks
        /// Retrieves all subtasks associated with work orders
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of work order subtasks</returns>
        /// <response code="200">Successfully retrieved all work order subtasks</response>
        [HttpGet("Get-AllWorkOrderSubTask")]
        public async Task<IActionResult> GetAllWorkOrderSubTaskAsync(int tenantId)
        {
            var result = await _workOrderSubTaskService.GetAllWorkOrderSubTaskAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Get work order subtask by ID
        /// Retrieves specific subtask details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="subTaskId">Subtask identifier</param>
        /// <returns>Work order subtask details</returns>
        /// <response code="200">Successfully retrieved work order subtask</response>
        [HttpGet("Get-WorkOrderSubTask-ById")]
        public async Task<IActionResult> GetWorkOrderSubTaskByIdAsync(int tenantId, int subTaskId)
        {
            var result = await _workOrderSubTaskService.GetWorkOrderSubTaskByIdAsync(tenantId, subTaskId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Add work order subtask
        /// Creates new subtask for work order breakdown
        /// </summary>
        /// <param name="dto">Subtask creation data</param>
        /// <returns>Subtask creation result</returns>
        /// <response code="200">Work order subtask successfully added</response>
        [HttpPost("Add-WorkOrderSubTask")]
        public async Task<IActionResult> AddWorkOrderSubTaskAsync([FromBody] CreateWorkOrderSubTaskDto dto)
        {
            var result = await _workOrderSubTaskService.AddWorkOrderSubTaskAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Update work order subtask
        /// Modifies existing subtask details and specifications
        /// </summary>
        /// <param name="dto">Updated subtask data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Work order subtask successfully updated</response>
        [HttpPost("Update-WorkOrderSubTask")]
        public async Task<IActionResult> UpdateWorkOrderSubTaskAsync([FromBody] UpdateWorkOrderSubTaskDto dto)
        {
            var result = await _workOrderSubTaskService.UpdateWorkOrderSubTaskAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        /// <summary>
        /// Delete work order subtask
        /// Removes subtask from work order
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="subTaskId">Subtask identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Work order subtask successfully deleted</response>
        [HttpPost("Delete-WorkOrderSubTask")]
        public async Task<IActionResult> DeleteWorkOrderSubTaskAsync(int tenantId, int subTaskId)
        {
            var result = await _workOrderSubTaskService.DeleteWorkOrderSubTaskAsync(tenantId, subTaskId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }
}
