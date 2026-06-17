using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.GlobalFilters;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.GlobalFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.GlobalFilters
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalFilterController : ControllerBase
    {
        private readonly IGlobalFiltersServices _globalFiltersService;

        public GlobalFilterController(IGlobalFiltersServices globalFiltersService)
        {
            _globalFiltersService = globalFiltersService;
        }
        [HttpPost("CreateGlobalFilters")]
        public async Task<IActionResult> AddAsync(CreateFilterConfigurationDto dto)
        {
            var result = await _globalFiltersService.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet("GetGlobalFilters")]
        public async Task<IActionResult> GetAllAsync(int? roleId, int? tenantId, string? module, string? submodule)
        {
            var result = await _globalFiltersService.GetAllAsync(roleId, tenantId, module, submodule);
            return Ok(result);
        }

        [HttpPost("DeleteAsync")]
        public async Task<IActionResult> DeleteAsync(int pId, int tenantId)
        {
            var result = await _globalFiltersService.DeleteAsync(pId, tenantId);
            return Ok(result);
        }
    }
}
