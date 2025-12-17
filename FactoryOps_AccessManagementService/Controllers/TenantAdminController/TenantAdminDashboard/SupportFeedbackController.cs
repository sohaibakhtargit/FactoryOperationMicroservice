using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminDashboard
{
    /// <summary>
    /// Support Feedback Management API
    /// Manages customer support feedback, feedback processing, and satisfaction metrics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SupportFeedbackController : ControllerBase
    {
        private readonly ISupportFeedbackService _feedbackService;

        public SupportFeedbackController(ISupportFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Add support feedback
        /// Creates new customer support feedback entry
        /// </summary>
        /// <param name="dto">Support feedback data</param>
        /// <returns>Feedback creation result</returns>
        /// <response code="200">Support feedback successfully added</response>
        [HttpPost]
        [Route("add-support-feedback")]
        public async Task<IActionResult> AddFeedback([FromBody] AddSupportFeedbackDto dto)
        {
            var result = await _feedbackService.AddFeedbackAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Acknowledge feedback
        /// Acknowledges receipt and processing of customer feedback
        /// </summary>
        /// <param name="dto">Feedback acknowledgment data</param>
        /// <returns>Acknowledgment operation result</returns>
        /// <response code="200">Feedback successfully acknowledged</response>
        [HttpPost("acknowledge-feedback")]
        public async Task<IActionResult> AcknowledgeFeedback([FromBody] AcknowledgeFeedbackDto dto)
        {
            var result = await _feedbackService.AcknowledgeFeedbackAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Get all feedback
        /// Retrieves all support feedback entries for tenant
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of support feedback</returns>
        /// <response code="200">Successfully retrieved all feedback</response>
        [HttpGet("get-all-feedback")]
        public IActionResult GetAllFeedbacks(int tenantId)
        {
            var result = _feedbackService.GetAllFeedbackByTenant(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Get feedback metrics
        /// Retrieves feedback analytics and satisfaction metrics
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>Feedback metrics and analytics</returns>
        /// <response code="200">Successfully retrieved feedback metrics</response>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetFeedbackMetrics(int tenantId)
        {
            var result = await _feedbackService.GetFeedbackMetricsAsync(tenantId);
            return Ok(result);
        }

        /// <summary>
        /// Delete support feedback
        /// Removes support feedback entry from system
        /// </summary>
        /// <param name="dto">Feedback deletion data</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Support feedback successfully deleted</response>
        [HttpPost("delete-support-feedback")]
        public async Task<IActionResult> DeleteFeedback([FromBody] DeleteSupportFeedbackDto dto)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(dto);
            return Ok(result);
        }
    }
}