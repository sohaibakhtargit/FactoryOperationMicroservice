using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOps_AccessManagementService.Controllers.TenantAdminController.TeamManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingModuleController : ControllerBase
    {
        private readonly ITrainingModuleService _traningModuleService;
        public TrainingModuleController(ITrainingModuleService traningModuleService)
        {
            _traningModuleService = traningModuleService;
        }

        [HttpPost("CreateTrainingModule")]
        public async Task<ActionResult<CommonResponseModel>> CreateTrainingModule([FromBody] AddTrainingModuleDto dto)
        {
            var result = await _traningModuleService.AddTrainingModuleAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
        [HttpPost("UpdateTrainingModule")]
        public async Task<ActionResult<CommonResponseModel>> UpdateTrainingModule([FromBody] AddTrainingModuleDto dto)
        {
            var result = await _traningModuleService.UpdateTrainingModuleAsync(dto);
            return StatusCode(int.Parse(result.StatusCode), result);
        }

        [HttpPost("DeleteTrainingModule")]
        public async Task<IActionResult> DeleteTrainingModule(int id, int tenantId)
        {
            var result = await _traningModuleService.DeleteTrainingModuleAsync(id, tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
        [HttpGet("GetAllTrainingModule")]
        public async Task<IActionResult> GetAllTrainingModule(int tenantId)
        {
            var result = await _traningModuleService.GetAllTrainingModuleAsync(tenantId);
            return StatusCode(int.Parse(result.StatusCode), result);
        }
    }
}
