using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.SuperAdminController.Configuration
{
    /// <summary>
    /// Global Dropdowns Management API
    /// Manages system-wide dropdown values and reference data across all modules
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GlobalDropdownsController : ControllerBase
    {
        private readonly IGlobalDropdownService _globalDropdownService;

        public GlobalDropdownsController(IGlobalDropdownService globalDropdownService)
        {
            _globalDropdownService = globalDropdownService;
        }

        /// <summary>
        /// Get all dropdown values
        /// Retrieves complete list of all dropdown values with optional filtering
        /// </summary>
        /// <param name="filter">Dropdown filter criteria</param>
        /// <returns>List of all dropdown values</returns>
        /// <response code="200">Successfully retrieved dropdown values</response>
        /// <response code="400">Invalid filter parameters provided</response>
        [HttpGet]
        public ActionResult<GetAllRecord<GetGlobalDropdownDto>> GetAllDropdowns([FromQuery] DropdownFilterDto? filter)
        {
            var result = _globalDropdownService.GetAllDropdowns(filter);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get dropdown values by module
        /// Retrieves dropdown values specific to a particular system module
        /// </summary>
        /// <param name="module">Module name</param>
        /// <returns>Module-specific dropdown values</returns>
        /// <response code="200">Successfully retrieved module dropdown values</response>
        /// <response code="400">Invalid module name provided</response>
        [HttpGet("module/{module}")]
        public ActionResult<GetAllRecord<GetGlobalDropdownDto>> GetDropdownsByModule(string module)
        {
            var result = _globalDropdownService.GetDropdownsByModule(module);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get dropdown values by module and submodule
        /// Retrieves dropdown values for specific module and submodule combination
        /// </summary>
        /// <param name="module">Module name</param>
        /// <param name="subModule">Submodule name</param>
        /// <returns>Module and submodule specific dropdown values</returns>
        /// <response code="200">Successfully retrieved submodule dropdown values</response>
        /// <response code="400">Invalid module or submodule provided</response>
        [HttpGet("module/{module}/submodule/{subModule}")]
        public ActionResult<GetAllRecord<GetGlobalDropdownDto>> GetDropdownsByModuleAndSubModule(string module, string subModule)
        {
            var result = _globalDropdownService.GetDropdownsByModuleAndSubModule(module, subModule);
            return result.StatusCode == "200" ? Ok(result) : BadRequest(result);
        }
    }
}