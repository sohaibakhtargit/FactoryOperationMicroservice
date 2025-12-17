using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.DTOs.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_KafkaMqttService.Controllers.KafkaMqttBridge
{
    [ApiController]
    [Route("api/[controller]")]
    public class KafkaConfigurationsController : ControllerBase
    {
        private readonly IKafkaConfigurationService _service;
        public KafkaConfigurationsController(IKafkaConfigurationService service) => _service = service;

        [HttpGet("get-all-kafka-configuration")]
        public async Task<IActionResult> GetAll(int tenantId)
            => Ok(await _service.GetAllAsync(tenantId));

        [HttpGet("get-kafka-configuration")]
        public async Task<IActionResult> GetById(int id, int tenantId)
            => Ok(await _service.GetByIdAsync(id, tenantId));

        [HttpPost("create-kafka-configuration")]
        public async Task<IActionResult> Create([FromBody] KafkaConfigurationCreateDto dto)
            => Ok(await _service.CreateAsync(dto));

        [HttpPost("update-kafka-configuration")]
        public async Task<IActionResult> Update([FromBody] KafkaConfigurationUpdateDto dto)
        {
            return Ok(await _service.UpdateAsync(dto));
        }

        [HttpPost("delete-kafka-configuration")]
        public async Task<IActionResult> Delete(int id, int tenantId)
            => Ok(await _service.DeleteAsync(id, tenantId));
    }
}
