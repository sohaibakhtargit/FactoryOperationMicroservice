using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminManagement
{
    /// <summary>
    /// Factory Group Management API
    /// Manages factory groups, group organization, and group operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FactoryGroupController : ControllerBase
    {
        private readonly IFactoryGroupService _groupService;

        public FactoryGroupController(IFactoryGroupService groupService)
        {
            _groupService = groupService;
        }

        /// <summary>
        /// Get all groups
        /// Retrieves all factory groups for a tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of factory groups</returns>
        /// <response code="200">Successfully retrieved all groups</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("GetAllGroups")]
        public async Task<IActionResult> GetAllGroups(int tenantId)
        {
            var result = await _groupService.GetAllGroupsAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }

        /// <summary>
        /// Get group by ID
        /// Retrieves specific factory group details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="groupId">Group identifier</param>
        /// <returns>Factory group details</returns>
        /// <response code="200">Successfully retrieved group</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("GetGroupById")]
        public async Task<IActionResult> GetGroupById(int tenantId, int groupId)
        {
            var result = await _groupService.GetGroupByIdAsync(tenantId, groupId);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }

        /// <summary>
        /// Add group
        /// Creates new factory group in the system
        /// </summary>
        /// <param name="dto">Factory group data</param>
        /// <returns>Group creation result</returns>
        /// <response code="200">Group successfully added</response>
        /// <response code="400">Invalid group data provided</response>
        /// <response code="500">Internal server error during creation</response>
        [HttpPost("AddGroup")]
        public async Task<IActionResult> AddGroup([FromBody] FactoryGroupDto dto)
        {
            if (dto == null)
                return BadRequest(new CommonResponseModel { StatusCode = "400", StatusMessage = "Invalid request body" });

            var result = await _groupService.AddGroupAsync(dto);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }

        /// <summary>
        /// Update group
        /// Modifies existing factory group information
        /// </summary>
        /// <param name="dto">Updated group data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Group successfully updated</response>
        /// <response code="400">Invalid group data provided</response>
        /// <response code="500">Internal server error during update</response>
        [HttpPost("UpdateGroup")]
        public async Task<IActionResult> UpdateGroup([FromBody] FactoryGroupDto dto)
        {
            if (dto == null || dto.GroupId == 0)
                return BadRequest(new CommonResponseModel { StatusCode = "400", StatusMessage = "Invalid request body or missing GroupId" });

            var result = await _groupService.UpdateGroupAsync(dto);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }

        /// <summary>
        /// Delete group
        /// Removes factory group from the system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="groupId">Group identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Group successfully deleted</response>
        /// <response code="500">Internal server error during deletion</response>
        [HttpPost("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup(int tenantId, int groupId)
        {
            var result = await _groupService.DeleteGroupAsync(tenantId, groupId);
            return StatusCode(int.Parse(result.StatusCode ?? "500"), result);
        }
    }
}