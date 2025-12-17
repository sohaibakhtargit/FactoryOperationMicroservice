using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.IOTDevices;
using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.IOTDevices
{
    public class MqttService : IMqttService, IDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;
        private readonly ILogger<MqttService> _logger;

        public bool IsConnected => _mqttClient?.IsConnected ?? false;
        public event EventHandler<MqttMessageReceivedEventArgs> MessageReceived;

        public MqttService(string brokerUrl, int port, string clientId, ILogger<MqttService> logger)
        {
            _logger = logger;

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                     .WithTcpServer(brokerUrl, port)
                     .WithClientId(clientId)
                     // .WithTls()
                     .WithCleanSession()
                     .Build();

            // Setup event handlers
            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation("Connected to MQTT broker");
            await SubscribeToDefaultTopics();
        }

        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("Disconnected from MQTT broker");
            await Task.Delay(TimeSpan.FromSeconds(5));

            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }
        }

        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            _logger.LogInformation($"Received message on topic {topic}: {payload}");

            MessageReceived?.Invoke(this, new MqttMessageReceivedEventArgs
            {
                Topic = topic,
                Payload = payload,
                Timestamp = DateTime.UtcNow
            });

            return Task.CompletedTask;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                var result = await _mqttClient.ConnectAsync(_options);
                _logger.LogInformation("MQTT connection result: {ResultCode}", result.ResultCode);
                return result.ResultCode == MqttClientConnectResultCode.Success;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            await _mqttClient.DisconnectAsync();
        }

        public async Task PublishAsync(string topic, string payload, bool retain = false)
        {
            if (!_mqttClient.IsConnected)
            {
                _logger.LogWarning("MQTT client not connected, attempting to reconnect...");
                await ConnectAsync();
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        public async Task SubscribeAsync(string topic)
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());
        }

        public async Task UnsubscribeAsync(string topic)
        {
            await _mqttClient.UnsubscribeAsync(topic);
        }

        private async Task SubscribeToDefaultTopics()
        {
            // Subscribe to device status topics
            await SubscribeAsync("factory/+/status");
            await SubscribeAsync("factory/+/sensor/data");
            await SubscribeAsync("factory/+/alerts");
        }

        public void Dispose()
        {
            // Remove event handlers
            _mqttClient.ConnectedAsync -= OnConnectedAsync;
            _mqttClient.DisconnectedAsync -= OnDisconnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync -= OnMessageReceivedAsync;

            _mqttClient?.Dispose();
        }
    }
}