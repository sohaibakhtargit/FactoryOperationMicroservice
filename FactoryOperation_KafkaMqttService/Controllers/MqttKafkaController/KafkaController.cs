using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FactoryOperation_KafkaMqttService.Controllers.MqttKafkaController
{
    [Route("api/[controller]")]
    [ApiController]
    public class KafkaController : ControllerBase
    {
        private readonly IKafkaProducerService _kafkaProducer;

        public KafkaController(IKafkaProducerService kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishEvent([FromBody] KafkaPublishRequest request)
        {
            var envelope = new KafkaMessageEnvelope
            {
                Key = request.Key,
                Payload = request.Payload,
                Source = request.Source,
                Timestamp = DateTime.UtcNow,
                Headers = request.Headers
            };

            await _kafkaProducer.ProduceEnvelopeAsync(request.Topic, envelope);
            return Ok(new { message = "Published successfully", topic = request.Topic });
        }
    }

    public class KafkaPublishRequest
    {
        public string Topic { get; set; }
        public string Key { get; set; }
        public object Payload { get; set; }
        public string Source { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}