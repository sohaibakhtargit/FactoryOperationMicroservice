using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Feedback_Support;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Feedback_Support
{
    /// <summary>
    /// Super Admin Support Feedback API
    /// Manages customer support feedback and service quality monitoring
    /// </summary>
    [ApiController]
    [Route("api/superadmin/[controller]")]
    public class SuperAdminSupportFeedbackController : ControllerBase
    {
        private readonly ISuperAdminSupportFeedbackService _feedbackService;

        public SuperAdminSupportFeedbackController(ISuperAdminSupportFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Get all support feedback entries
        /// Retrieves complete collection of customer feedback for service quality analysis
        /// </summary>
        /// <returns>List of all support feedback entries</returns>
        /// <response code="200">Successfully retrieved all support feedback</response>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSupportFeedbacks()
        {
            var response = await _feedbackService.GetAllSupportFeedbackAsync();
            return Ok(response);
        }
    }
}