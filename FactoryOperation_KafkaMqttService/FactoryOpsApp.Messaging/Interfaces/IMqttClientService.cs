using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces
{
    public interface IMqttClientService : IAsyncDisposable
    {
        Task ConnectAsync(CancellationToken ct = default);
        Task DisconnectAsync(CancellationToken ct = default);
        Task SubscribeAsync(string topic, Func<MqttMessage, Task> handler, CancellationToken ct = default);
        Task UnsubscribeAsync(string topic, CancellationToken ct = default);
        Task PublishAsync(string topic, byte[] payload, int qos = 1, bool retain = false, CancellationToken ct = default);
        bool IsConnected { get; }
    }
}
