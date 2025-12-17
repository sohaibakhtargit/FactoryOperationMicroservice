using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_KafkaMqttService.Controllers.KafkaMqttBridge
{
    [ApiController]
    [Route("api/[controller]")]
    public class BridgeConfigurationsController : ControllerBase
    {
        private readonly IBridgeConfigurationService _service;
        public BridgeConfigurationsController(IBridgeConfigurationService service) => _service = service;

        [HttpGet]
        [Route("get-all-bridge-configuration")]
        public async Task<IActionResult> GetAll(int tenantId)
            => Ok(await _service.GetAllAsync(tenantId));

        [HttpGet("get-bridge-configuration")]
        public async Task<IActionResult> GetById(int id, int tenantId)
            => Ok(await _service.GetByIdAsync(id, tenantId));

        [HttpPost("create-bridge-configuration")]
        public async Task<IActionResult> Create([FromBody] BridgeConfigurationCreateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPost("update-bridge-configuration")]
        public async Task<IActionResult> Update([FromBody] BridgeConfigurationUpdateDto dto)
        {
            return Ok(await _service.UpdateAsync(dto));
        }

        [HttpPost("delete-bridge-configuration")]
        public async Task<IActionResult> Delete(int id, int tenantId)
            => Ok(await _service.DeleteAsync(id, tenantId));
    }
}
