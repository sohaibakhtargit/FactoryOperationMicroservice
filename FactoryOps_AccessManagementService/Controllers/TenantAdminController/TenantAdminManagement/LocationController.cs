using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TenantAdminManagement
{
    /// <summary>
    /// Location Management API
    /// Manages factory locations, location hierarchy, and spatial organization
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly IFactoryLocationService _service;

        public LocationController(IFactoryLocationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Add location
        /// Creates new factory location in the system
        /// </summary>
        /// <param name="dto">Location data</param>
        /// <returns>Location creation result</returns>
        /// <response code="200">Location successfully added</response>
        [HttpPost("Add-Location")]
        public async Task<IActionResult> Add(LocationDto dto) =>
           Ok(await _service.AddLocationAsync(dto));

        /// <summary>
        /// Update location
        /// Modifies existing factory location information
        /// </summary>
        /// <param name="dto">Updated location data</param>
        /// <returns>Update operation result</returns>
        /// <response code="200">Location successfully updated</response>
        [HttpPost("Update-Location")]
        public async Task<IActionResult> Update(LocationDto dto) =>
            Ok(await _service.UpdateLocationAsync(dto));

        /// <summary>
        /// Delete location
        /// Removes factory location from the system
        /// </summary>
        /// <param name="TenantId">Tenant identifier</param>
        /// <param name="LocationId">Location identifier</param>
        /// <returns>Deletion operation result</returns>
        /// <response code="200">Location successfully deleted</response>
        [HttpPost("Delete-Location")]
        public async Task<IActionResult> Delete(int TenantId, int LocationId) =>
            Ok(await _service.DeleteLocationAsync(TenantId, LocationId));

        /// <summary>
        /// Get location by ID
        /// Retrieves specific factory location details
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="id">Location identifier</param>
        /// <returns>Location details</returns>
        /// <response code="200">Successfully retrieved location</response>
        [HttpGet("Get-Location_ById")]
        public async Task<IActionResult> GetById(int tenantId, int id) =>
            Ok(await _service.GetLocationByIdAsync(tenantId, id));

        /// <summary>
        /// Get all locations
        /// Retrieves complete list of all factory locations
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <returns>List of all locations</returns>
        /// <response code="200">Successfully retrieved all locations</response>
        [HttpGet("GetAll-Locations")]
        public async Task<IActionResult> GetAll(int tenantId) =>
            Ok(await _service.GetAllLocationsAsync(tenantId));

        /// <summary>
        /// Get all locations by parent ID
        /// Retrieves location hierarchy with children locations
        /// </summary>
        /// <param name="tenantId">Tenant identifier</param>
        /// <param name="selectedLocationId">Parent location identifier</param>
        /// <returns>Location hierarchy with children</returns>
        /// <response code="200">Successfully retrieved location hierarchy</response>
        [HttpGet("GetAll-Locations-ByParentId")]
        public async Task<IActionResult> GetAll(int tenantId, int selectedLocationId) =>
            Ok(await _service.GetLocationWithChildrenAsync(tenantId, selectedLocationId));
    }
}