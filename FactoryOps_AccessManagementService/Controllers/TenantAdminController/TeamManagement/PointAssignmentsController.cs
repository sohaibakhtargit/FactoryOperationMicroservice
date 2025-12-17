using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TeamManagement
{
    /// <summary>
    /// Point Assignments Management API
    /// Manages point assignments, scoring systems, and reward point allocations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PointAssignmentsController : ControllerBase
    {
        private readonly IPointAssignmentService _pointAssignmentService;

        public PointAssignmentsController(IPointAssignmentService pointAssignmentService)
        {
            _pointAssignmentService = pointAssignmentService;
        }

        /// <summary>
        /// Create point assignment
        /// Creates new point assignment for user scoring and rewards
        /// </summary>
        /// <param name="dto">Point assignment data</param>
        /// <returns>Assignment creation result</returns>
        /// <response code="200">Point assignment successfully created</response>
        /// <response code="400">Invalid assignment data provided</response>
        [HttpPost("CreatePointAssignment")]
        public async Task<ActionResult<CommonResponseModel>> CreatePointAssignment([FromBody] PointAssignmentDto dto)
        {
            var result = await _pointAssignmentService.AddPointAssignmentAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update point assignment
        /// Modifies existing point assignment values and criteria
        /// </summary>
        /// <param name="dto">Updated point assignment data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Point assignment successfully updated</response>
        /// <response code="400">Invalid assignment data provided</response>
        [HttpPost("UpdatePointAssignment")]
        public async Task<ActionResult<CommonResponseModel>> UpdatePointAssignment([FromBody] PointAssignmentDto dto)
        {
            var result = await _pointAssignmentService.UpdatePointAssignmentAsync(dto);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete point assignment
        /// Removes point assignment from scoring system
        /// </summary>
        /// <param name="id">Point assignment identifier</param>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Point assignment successfully deleted</response>
        /// <response code="400">Invalid deletion request</response>
        [HttpPost("DeletePointAssignment")]
        public async Task<ActionResult<CommonResponseModel>> DeletePointAssignment(int id, int tenantId)
        {
            var result = await _pointAssignmentService.DeletePointAssignmentAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get point assignments
        /// Retrieves all point assignments for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of point assignments</returns>
        /// <response code="200">Successfully retrieved point assignments</response>
        /// <response code="400">Invalid retrieval request</response>
        [HttpGet("GetPointAssignments")]
        public async Task<ActionResult<GetAllRecord<GetPointAssignmentDto>>> GetPointAssignments(int tenantId)
        {
            var result = await _pointAssignmentService.GetAllPointAssignmentsAsync(tenantId);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get point assignment by ID
        /// Retrieves specific point assignment details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Point assignment identifier</param>
        /// <returns>Point assignment details</returns>
        /// <response code="200">Successfully retrieved point assignment</response>
        /// <response code="404">Point assignment not found</response>
        [HttpGet("GetPointAssignmentById")]
        public async Task<ActionResult<GetSpecificRecord<GetPointAssignmentDto>>> GetPointAssignment(int tenantId, int id)
        {
            var result = await _pointAssignmentService.GetPointAssignmentByIdAsync(id, tenantId);
            return result.StatusCode == "200" ? Ok(result) : NotFound(result);
        }
    }
}