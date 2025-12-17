using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminDashboard
{
    /// <summary>
    /// Support Ticket Management API
    /// Manages support tickets, ticket tracking, and customer support operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService;

        public SupportTicketController(ISupportTicketService supportTicketService)
        {
            _supportTicketService = supportTicketService;
        }

        /// <summary>
        /// Create support ticket
        /// Creates new support ticket for customer assistance
        /// </summary>
        /// <param name="dto">Support ticket data</param>
        /// <returns>Ticket creation result</returns>
        /// <response code="200">Support ticket successfully created</response>
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AddSupportTicketDto dto)
        {
            var result = await _supportTicketService.AddSupportTicket(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update support ticket
        /// Modifies existing support ticket information and details
        /// </summary>
        /// <param name="ticketId">Ticket identifier</param>
        /// <param name="dto">Updated ticket data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Support ticket successfully updated</response>
        [Authorize]
        [HttpPost("update/{ticketId}")]
        public async Task<IActionResult> Update(int ticketId, [FromBody] AddSupportTicketDto dto)
        {
            var result = await _supportTicketService.UpdateSupportTicket(ticketId, dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete support ticket
        /// Removes support ticket from tracking system
        /// </summary>
        /// <param name="ticketId">Ticket identifier</param>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Support ticket successfully deleted</response>
        [Authorize]
        [HttpPost("delete/{ticketId}")]
        public async Task<IActionResult> Delete(int ticketId, int TenantId)
        {
            var result = await _supportTicketService.DeleteSupportTicket(ticketId, TenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get all tickets by tenant
        /// Retrieves all support tickets for specific tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of support tickets</returns>
        /// <response code="200">Successfully retrieved all support tickets</response>
        [HttpGet("all/{tenantId}")]
        public IActionResult GetAllByTenant(int tenantId)
        {
            var result = _supportTicketService.GetAllTicketsByTenant(tenantId);
            return Ok(result);
        }
    }
}