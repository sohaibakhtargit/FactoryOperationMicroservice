using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_WorkOrder.Controllers.TenantAdminController.WorkOrderManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;

        public ServiceRequestController(IServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        /// <summary>
        /// Create service request
        /// Creates new service request for maintenance or support
        /// </summary>
        /// <param name="dto">Service request data</param>
        /// <returns>Request creation result</returns>
        /// <response code="200">Service request successfully created</response>
        /// <response code="400">Invalid request data provided</response>
        [HttpPost("Create-ServiceRequest")]
        public async Task<IActionResult> CreateServiceRequestAsync([FromBody] ServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _serviceRequestService.CreateServiceRequestAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Update service request
        /// Modifies existing service request details and information
        /// </summary>
        /// <param name="dto">Updated service request data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Service request successfully updated</response>
        /// <response code="400">Invalid request data provided</response>
        [HttpPost("Update-ServiceRequest")]
        public async Task<IActionResult> UpdateServiceRequestAsync([FromBody] ServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _serviceRequestService.UpdateServiceRequestAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete service request
        /// Removes service request from system
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="serviceRequestId">Service request identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Service request successfully deleted</response>
        [HttpPost("Delete-ServiceRequest")]
        public async Task<IActionResult> DeleteServiceRequestAsync(int tenantId, int serviceRequestId)
        {
            var result = await _serviceRequestService.DeleteServiceRequestAsync(serviceRequestId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Update service request status
        /// Updates service request status and progress tracking
        /// </summary>
        /// <param name="dto">Service request status update data</param>
        /// <returns>Status update result</returns>
        /// <response code="200">Service request status successfully updated</response>
        /// <response code="400">Invalid status data provided</response>
        [HttpPost("Update-ServiceRequestStatus")]
        public async Task<IActionResult> UpdateServiceRequestStatusAsync([FromBody] ServiceRequestStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _serviceRequestService.UpdateServiceRequestStatusAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get all service requests
        /// Retrieves complete list of all service requests for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of service requests</returns>
        /// <response code="200">Successfully retrieved all service requests</response>
        [HttpGet("Get-AllServiceRequests")]
        public async Task<IActionResult> GetAllServiceRequestsAsync(int tenantId)
        {
            var result = await _serviceRequestService.GetAllServiceRequestsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get service request by ID
        /// Retrieves specific service request details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="serviceRequestId">Service request identifier</param>
        /// <returns>Service request details</returns>
        /// <response code="200">Successfully retrieved service request</response>
        [HttpGet("Get-ServiceRequestById")]
        public async Task<IActionResult> GetServiceRequestByIdAsync(int tenantId, int serviceRequestId)
        {
            var result = await _serviceRequestService.GetServiceRequestByIdAsync(serviceRequestId, tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get service requests by status
        /// Retrieves service requests filtered by specific status
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="status">Service request status filter</param>
        /// <returns>Status-filtered service requests</returns>
        /// <response code="200">Successfully retrieved service requests by status</response>
        [HttpGet("Get-ServiceRequestsByStatus")]
        public async Task<IActionResult> GetServiceRequestsByStatusAsync(int tenantId, ServiceRequestStatus status)
        {
            var result = await _serviceRequestService.GetServiceRequestsByStatusAsync(tenantId, status);
            return Ok(result);
        }

        /// <summary>
        /// Get overdue service requests
        /// Retrieves service requests that are past their due date
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of overdue service requests</returns>
        /// <response code="200">Successfully retrieved overdue service requests</response>
        [HttpGet("Get-OverdueServiceRequests")]
        public async Task<IActionResult> GetOverdueServiceRequestsAsync(int tenantId)
        {
            var result = await _serviceRequestService.GetOverdueServiceRequestsAsync(tenantId);
            return Ok(result);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve(ApproveServiceRequestDto dto)
        {
            var result = await _serviceRequestService.ApproveServiceRequestAsync(dto);
            return Ok(result);
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject(RejectServiceRequestDto dto)
        {
            var result = await _serviceRequestService.RejectServiceRequestAsync(dto);
            return Ok(result);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> Assign(AssignServiceRequestDto dto)
        {
            var result = await _serviceRequestService.AssignServiceRequestAsync(dto);
            return Ok(result);
        }

        [HttpPost("reopen")]
        public async Task<IActionResult> Reopen(ServiceRequestDto dto)
        {
            var result = await _serviceRequestService.ReopenServiceRequestAsync(dto);
            return Ok(result);
        }
        [HttpPost("service-request-media")]
        public async Task<IActionResult> UploadServiceRequestMediaAsync([FromForm] ServiceRequestMediaDto dto)
        {
            var result = await _serviceRequestService.UploadServiceRequestMediaAsync(dto);

            return StatusCode(int.Parse(result.StatusCode!), result);
        }

    }
}
