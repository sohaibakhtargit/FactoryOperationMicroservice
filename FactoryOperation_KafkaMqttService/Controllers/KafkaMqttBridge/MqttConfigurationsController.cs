using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_KafkaMqttService.Controllers.KafkaMqttBridge
{
    [ApiController]
    [Route("api/[controller]")]
    public class MqttConfigurationsController : ControllerBase
    {
        private readonly IMqttConfigurationService _service;
        public MqttConfigurationsController(IMqttConfigurationService service) => _service = service;

        [HttpGet("get-all-mqtt-configuration")]
        public async Task<IActionResult> GetAll(int tenantId)
            => Ok(await _service.GetAllAsync(tenantId));

        [HttpGet("get-mqtt-configuration")]
        public async Task<IActionResult> GetById(int id, int tenantId)
            => Ok(await _service.GetByIdAsync(id, tenantId));

        [HttpPost("create-mqtt-configuration")]
        public async Task<IActionResult> Create([FromBody] MqttConfigurationCreateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPost("update-mqtt-configuration")]
        public async Task<IActionResult> Update([FromBody] MqttConfigurationUpdateDto dto)
        {
            return Ok(await _service.UpdateAsync(dto));
        }

        [HttpPost("delete-mqtt-configuration")]
        public async Task<IActionResult> Delete(int id, int tenantId)
            => Ok(await _service.DeleteAsync(id, tenantId));
    }
}
