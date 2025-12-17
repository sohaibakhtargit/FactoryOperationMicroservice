using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces
{
    public interface ILastTelemetryStore
    {
        void Add(MqttMessage message);
        IReadOnlyList<MqttMessage> GetLastByDevice(string deviceId, int take = 5);
        IReadOnlyDictionary<string, IReadOnlyList<MqttMessage>> GetLastAllDevices(int take = 5);
    }
}
