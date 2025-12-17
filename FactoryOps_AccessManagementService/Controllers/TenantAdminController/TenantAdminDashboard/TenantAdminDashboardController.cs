using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminDashboard;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminDashboard
{
    /// <summary>
    /// Tenant Admin Dashboard API
    /// Provides dashboard metrics and analytics for tenant administrators
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TenantAdminDashboardController : ControllerBase
    {
        private readonly ITenantAdminDashboardService _Service;

        public TenantAdminDashboardController(ITenantAdminDashboardService Service)
        {
            _Service = Service;
        }

        /// <summary>
        /// Get users count
        /// Retrieves total number of users in tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>User count data</returns>
        /// <response code="200">Successfully retrieved user count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("count")]
        public IActionResult GetUsersCount([FromQuery] int tenantId)
        {
            var response = _Service.GetUsersCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get users count this month
        /// Retrieves number of active users in current month
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Monthly active user count</returns>
        /// <response code="200">Successfully retrieved monthly user count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("userCount-thisMonth")]
        public IActionResult GetUsersCountThisMonth([FromQuery] int tenantId)
        {
            var response = _Service.GetActiveUsersCountPresentMonth(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get open support ticket count
        /// Retrieves number of active support tickets
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Open support ticket count</returns>
        /// <response code="200">Successfully retrieved support ticket count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("Open-Support-ticketCount")]
        public IActionResult GetSupportTicketCount([FromQuery] int tenantId)
        {
            var response = _Service.GetActiveSupportTicketCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get team count
        /// Retrieves total number of teams in tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Team count data</returns>
        /// <response code="200">Successfully retrieved team count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("team-Count")]
        public IActionResult GetTeams([FromQuery] int tenantId)
        {
            var response = _Service.GetTotalTeamCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get suspended user count
        /// Retrieves number of suspended users in tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Suspended user count</returns>
        /// <response code="200">Successfully retrieved suspended user count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("suspended-User-Count")]
        public IActionResult GetSuspendUsers([FromQuery] int tenantId)
        {
            var response = _Service.GetSuspendUsersCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get new ticket count
        /// Retrieves number of new support tickets
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>New ticket count</returns>
        /// <response code="200">Successfully retrieved new ticket count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("newTicket-count")]
        public IActionResult GetNewTicket([FromQuery] int tenantId)
        {
            var response = _Service.GetNewTicketCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get ticket in progress count
        /// Retrieves number of tickets currently in progress
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>In-progress ticket count</returns>
        /// <response code="200">Successfully retrieved in-progress ticket count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("Ticket-inprogress-count")]
        public IActionResult GetTicketInProgress([FromQuery] int tenantId)
        {
            var response = _Service.GetTicketInProgressCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get ticket resolved today count
        /// Retrieves number of tickets resolved today
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Today's resolved ticket count</returns>
        /// <response code="200">Successfully retrieved resolved ticket count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("Ticket-Resolvetoday-count")]
        public IActionResult GetTicketResolved([FromQuery] int tenantId)
        {
            var response = _Service.GetTicketResolvedTodayCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get critical priority count
        /// Retrieves number of tickets with critical priority
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Critical priority ticket count</returns>
        /// <response code="200">Successfully retrieved critical priority count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("Ticket-Criticalpriority-count")]
        public IActionResult GetCriticalPriority([FromQuery] int tenantId)
        {
            var response = _Service.GetCriticalPriorityCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get admin role user count
        /// Retrieves number of users with admin role
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Admin user count</returns>
        /// <response code="200">Successfully retrieved admin user count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("user-role-adminCount")]
        public IActionResult GetUserByAdmin([FromQuery] int tenantId)
        {
            var response = _Service.GetUserByAdminCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get supervisor role user count
        /// Retrieves number of users with supervisor role
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Supervisor user count</returns>
        /// <response code="200">Successfully retrieved supervisor user count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("user-role-supervisorCount")]
        public IActionResult GetUserBySupervisor([FromQuery] int tenantId)
        {
            var response = _Service.GetUserBySupervisorCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get technician role user count
        /// Retrieves number of users with technician role
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Technician user count</returns>
        /// <response code="200">Successfully retrieved technician user count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("user-role-technicianCount")]
        public IActionResult GetUserByTechnician([FromQuery] int tenantId)
        {
            var response = _Service.GetUserByTechnicianCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get operator role user count
        /// Retrieves number of users with operator role
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Operator user count</returns>
        /// <response code="200">Successfully retrieved operator user count</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("user-role-operatorCount")]
        public IActionResult GetUserByOperator([FromQuery] int tenantId)
        {
            var response = _Service.GetUserByOperatorCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        /// <summary>
        /// Get all user role count
        /// Retrieves comprehensive user count across all roles
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Complete user role count data</returns>
        /// <response code="200">Successfully retrieved all user role counts</response>
        /// <response code="500">Internal server error during retrieval</response>
        [HttpGet("user-role-Count")]
        public IActionResult GetUserCount([FromQuery] int tenantId)
        {
            var response = _Service.GetAllUserCount(tenantId);
            if (response.StatusCode == "200")
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }
    }
}