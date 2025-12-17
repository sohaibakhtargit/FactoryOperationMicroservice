namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.IOTDevices
{
    public interface IMqttService
    {
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
        Task PublishAsync(string topic, string payload, bool retain = false);
        Task SubscribeAsync(string topic);
        Task UnsubscribeAsync(string topic);
        bool IsConnected { get; }
        event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;
    }

    public class MqttMessageReceivedEventArgs : EventArgs
    {
        public string Topic { get; set; }
        public string Payload { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
