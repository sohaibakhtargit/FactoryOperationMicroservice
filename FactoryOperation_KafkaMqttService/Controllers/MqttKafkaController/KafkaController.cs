using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.Controllers.MqttKafkaController
{
    [Route("api/[controller]")]
    [ApiController]
    public class KafkaController : ControllerBase
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public KafkaController(IKafkaProducerService kafkaProducer, IHttpContextAccessor httpContextAccessor)
        {
            _kafkaProducer = kafkaProducer;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishEvent([FromBody] KafkaPublishRequest request)

        {
            var ctx = _httpContextAccessor.HttpContext;
            if (request.Headers == null ||
             !request.Headers.TryGetValue("tenant-id", out var tenantIdStr) ||
             !int.TryParse(tenantIdStr, out var tenantId))
            {
                return BadRequest("Missing or invalid tenant-id header");
            }
            string? eventType = null;

            if (request.Payload.TryGetProperty("EventType", out var eventTypeProp))
            {
                eventType = eventTypeProp.GetString();
            }

         
            var envelope = new KafkaMessageEnvelope
            {
                Key = request.Key,
               
                Payload = request.Payload,
                Source = request.Source,
                TenantId = tenantId,
                EventType = eventType,
                IpAddress = ctx?.Connection?.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow,
                Headers = request.Headers,
                CorrelationId = request.Headers?["correlation-id"]
            };

            await _kafkaProducer.ProduceEnvelopeAsync(request.Topic, envelope);
            return Ok(new { message = "Published successfully", topic = request.Topic });
        }

        

    }


}