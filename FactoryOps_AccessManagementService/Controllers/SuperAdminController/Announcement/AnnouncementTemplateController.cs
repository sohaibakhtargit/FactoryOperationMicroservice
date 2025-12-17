using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Announcements;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Announcement
{
    /// <summary>
    /// Announcement Template Management API
    /// Manages announcement templates for system-wide communications
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementTemplateController : ControllerBase
    {
        private readonly IAnnouncementTemplateService _templateService;

        public AnnouncementTemplateController(IAnnouncementTemplateService templateService)
        {
            _templateService = templateService;
        }

        /// <summary>
        /// Get all announcement templates
        /// Retrieves complete list of all announcement templates
        /// </summary>
        /// <returns>List of all announcement templates</returns>
        /// <response code="200">Successfully retrieved all templates</response>
        [HttpGet("get-AllTemplate")]
        public async Task<ActionResult<GetAllRecord<AnnouncementTemplate>>> GetAll()
            => Ok(await _templateService.GetAllAsync());

        /// <summary>
        /// Get template by ID
        /// Retrieves specific announcement template by its identifier
        /// </summary>
        /// <param name="id">Template identifier</param>
        /// <returns>Specific announcement template</returns>
        /// <response code="200">Successfully retrieved template</response>
        [HttpGet("get-TemplateById")]
        public async Task<ActionResult<GetSpecificRecord<AnnouncementTemplate>>> GetById(int id)
            => Ok(await _templateService.GetByIdAsync(id));

        /// <summary>
        /// Create announcement template
        /// Creates new announcement template for system communications
        /// </summary>
        /// <param name="dto">Template creation data</param>
        /// <returns>Template creation result</returns>
        /// <response code="200">Template successfully created</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("create-Template")]
        public async Task<ActionResult<CommonResponseModel>> Create([FromBody] AnnouncementTemplateCreateDto dto)
        {
            return Ok(await _templateService.CreateAsync(dto));
        }

        /// <summary>
        /// Update announcement template
        /// Modifies existing announcement template content and settings
        /// </summary>
        /// <param name="dto">Template update data</param>
        /// <returns>Template update result</returns>
        /// <response code="200">Template successfully updated</response>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("update-Template")]
        public async Task<ActionResult<CommonResponseModel>> Update([FromBody] AnnouncementTemplateUpdateDto dto)
        {
            return Ok(await _templateService.UpdateAsync(dto));
        }

        /// <summary>
        /// Delete announcement template
        /// Removes announcement template from the system
        /// </summary>
        /// <param name="id">Template identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Template successfully deleted</response>
        [HttpPost("delete-Template")]
        public async Task<ActionResult<CommonResponseModel>> Delete(int id)
            => Ok(await _templateService.DeleteAsync(id));
    }
}