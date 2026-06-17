using System.Text.Json;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Models
{
    public class KafkaPublishRequest
    {
        public string Topic { get; set; }
        public string Key { get; set; }
       //ublic object Payload { get; set; }
        public JsonElement Payload { get; set; }   

        public string Source { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}
