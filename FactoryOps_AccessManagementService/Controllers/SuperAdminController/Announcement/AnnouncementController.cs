using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Announcements;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Announcement
{
    /// <summary>
    /// Announcement Management API
    /// Manages system announcements and notifications across tenants
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        /// <summary>
        /// Create announcement
        /// Creates new system announcement for targeted audience
        /// </summary>
        /// <param name="dto">Announcement creation data</param>
        /// <returns>Announcement creation result</returns>
        /// <response code="200">Announcement successfully created</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Create-Announcement")]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementDto dto)
        {
            var data = await _announcementService.CreateAnnouncementAsync(dto);
            return Ok(data);
        }

        /// <summary>
        /// Get all announcements
        /// Retrieves complete list of all system announcements
        /// </summary>
        /// <returns>List of all announcements</returns>
        /// <response code="200">Successfully retrieved all announcements</response>
        [HttpGet("All-Announcement")]
        public async Task<IActionResult> GetAllAnnouncements()
        {
            var result = await _announcementService.GetAllAnnouncementsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get announcements by tenant ID
        /// Retrieves announcements specific to a particular tenant
        /// </summary>
        /// <param name="TenantId">Tenant identifier</param>
        /// <returns>Tenant-specific announcements</returns>
        /// <response code="200">Successfully retrieved tenant announcements</response>
        [HttpGet("AnnouncementByTenantId")]
        public async Task<IActionResult> GetAllAnnouncementsByTenantId(int TenantId)
        {
            var result = await _announcementService.GetAllAnnouncementsByTenantIdAsync(TenantId);
            return Ok(result);
        }

        /// <summary>
        /// Update announcement
        /// Modifies existing announcement content and settings
        /// </summary>
        /// <param name="dto">Announcement update data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Announcement successfully updated</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Update-Announcement")]
        public async Task<IActionResult> UpdateAnnouncement([FromBody] UpdateAnnouncementDto dto)
        {
            var result = await _announcementService.UpdateAnnouncementAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Delete announcement
        /// Removes announcement from the system
        /// </summary>
        /// <param name="announcementId">Announcement identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Announcement successfully deleted</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("Delete-Announcement")]
        public async Task<IActionResult> DeleteAnnouncement(int announcementId)
        {
            var result = await _announcementService.DeleteAnnouncementAsync(announcementId);
            return Ok(result);
        }
    }
}