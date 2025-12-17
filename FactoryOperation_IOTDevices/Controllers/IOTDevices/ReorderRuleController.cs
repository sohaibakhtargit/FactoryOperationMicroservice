using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOpsApp.API.Controllers.TenantAdminContoller.IOTDevices
{
    /// <summary>
    /// Reorder Rule Management API
    /// Manages reorder rules, automated replenishment, and inventory restocking logic
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReorderRuleController : ControllerBase
    {
        private readonly IReorderRuleService _reorderRuleService;

        public ReorderRuleController(IReorderRuleService reorderRuleService)
        {
            _reorderRuleService = reorderRuleService;
        }

        /// <summary>
        /// Get reorder dashboard
        /// Retrieves automated replenishment dashboard data and metrics
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Reorder dashboard data</returns>
        /// <response code="200">Successfully retrieved reorder dashboard</response>
        [HttpGet("reorder-dashboard")]
        public async Task<ActionResult<GetSpecificRecord<AutomatedReplenishmentDashboardDto>>> GetDashboard(int tenantId)
        {
            var result = await _reorderRuleService.GetDashboardDataAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get reorder rules
        /// Retrieves all reorder rules for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of reorder rules</returns>
        /// <response code="200">Successfully retrieved reorder rules</response>
        [HttpGet("get-ReorderRule")]
        public async Task<ActionResult<GetAllRecord<ReorderRuleResponseDto>>> GetAll(int tenantId)
        {
            var result = await _reorderRuleService.GetAllAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get reorder rule by ID
        /// Retrieves specific reorder rule details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Reorder rule identifier</param>
        /// <returns>Reorder rule details</returns>
        /// <response code="200">Successfully retrieved reorder rule</response>
        /// <response code="404">Reorder rule not found</response>
        [HttpGet("get-reorderRule-byId")]
        public async Task<ActionResult<GetSpecificRecord<ReorderRuleResponseDto>>> GetById(int tenantId, int id)
        {
            var result = await _reorderRuleService.GetByIdAsync(tenantId, id);
            if (result.StatusCode == "404")
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>
        /// Create reorder rule
        /// Creates new reorder rule for automated inventory replenishment
        /// </summary>
        /// <param name="createDto">Reorder rule creation data</param>
        /// <returns>Rule creation result</returns>
        /// <response code="200">Reorder rule successfully created</response>
        /// <response code="400">Invalid rule data provided</response>
        [HttpPost("create-reorder-rule")]
        public async Task<ActionResult<CommonResponseModel>> Create([FromBody] CreateReorderRuleDto createDto)
        {
            createDto.CreatedBy = 1;
            var result = await _reorderRuleService.CreateAsync(createDto);
            if (result.StatusCode == "200")
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Update reorder rule
        /// Modifies existing reorder rule parameters and thresholds
        /// </summary>
        /// <param name="updateDto">Updated reorder rule data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Reorder rule successfully updated</response>
        /// <response code="400">Invalid rule data provided</response>
        [HttpPost("update-reorder-rule")]
        public async Task<ActionResult<CommonResponseModel>> Update([FromBody] UpdateReorderRuleDto updateDto)
        {
            updateDto.UpdatedBy = 1;
            var result = await _reorderRuleService.UpdateAsync(updateDto);
            if (result.StatusCode == "200")
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Delete reorder rule
        /// Removes reorder rule from automated replenishment system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Reorder rule identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Reorder rule successfully deleted</response>
        /// <response code="400">Invalid deletion request</response>
        [HttpPost("delete-reorder-rule")]
        public async Task<ActionResult<CommonResponseModel>> Delete(int tenantId, int id)
        {
            var deletedBy = 1;
            var result = await _reorderRuleService.DeleteAsync(tenantId, id, deletedBy);
            if (result.StatusCode == "200")
                return Ok(result);
            return BadRequest(result);
        }
    }
}