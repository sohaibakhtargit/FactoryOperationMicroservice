using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
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
