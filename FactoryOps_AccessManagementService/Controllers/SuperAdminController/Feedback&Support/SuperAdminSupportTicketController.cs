using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Feedback_Support;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Feedback_Support
{
    /// <summary>
    /// Super Admin Support Tickets API
    /// Manages and processes all support tickets across the system
    /// </summary>
    [Route("api/superadmin/support-tickets")]
    [ApiController]
    public class SuperAdminSupportTicketController : ControllerBase
    {
        private readonly ISuperAdminSupportTicketService _service;

        public SuperAdminSupportTicketController(ISuperAdminSupportTicketService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all support tickets
        /// Retrieves complete list of all support tickets from all tenants
        /// </summary>
        /// <returns>List of all support tickets</returns>
        /// <response code="200">Successfully retrieved all support tickets</response>
        [HttpGet]
        [Route("Get-AllSupportTickets")]
        public async Task<IActionResult> GetAllSupportTickets()
        {
            var data = await _service.GetAllSupportTicketsAsync();
            return Ok(data);
        }

        /// <summary>
        /// Update support ticket status and details
        /// Allows super admins to modify ticket status, assign resources, and update resolutions
        /// </summary>
        /// <param name="dto">Support ticket update data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Support ticket successfully updated</response>
        /// <response code="400">Invalid request data provided</response>
        /// <response code="500">Internal server error during update</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("update-SupportTicket")]
        public async Task<IActionResult> UpdateSupportTicket([FromBody] UpdateSupportTicketDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            var result = await _service.UpdateSupportTicketAsync(dto);

            if (result.StatusCode == "200")
                return Ok(result);
            else
                return StatusCode(500, result);
        }
    }
}