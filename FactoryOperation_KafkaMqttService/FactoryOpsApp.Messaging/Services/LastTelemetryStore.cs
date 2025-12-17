using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Models;
using System.Collections.Concurrent;

namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services
{
    public class LastTelemetryStore : ILastTelemetryStore
    {
        private const int MaxPerDevice = 50;
        private readonly ConcurrentDictionary<string, ConcurrentQueue<MqttMessage>> _byDevice =
            new(StringComparer.OrdinalIgnoreCase);

        public void Add(MqttMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Topic)) return;

            var deviceId = ExtractDeviceId(message.Topic);
            if (string.IsNullOrEmpty(deviceId)) return;

            var q = _byDevice.GetOrAdd(deviceId, _ => new ConcurrentQueue<MqttMessage>());
            q.Enqueue(message);

            while (q.Count > MaxPerDevice && q.TryDequeue(out _)) { }
        }

        public IReadOnlyList<MqttMessage> GetLastByDevice(string deviceId, int take = 5)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return Array.Empty<MqttMessage>();
            take = Math.Clamp(take, 1, MaxPerDevice);

            if (!_byDevice.TryGetValue(deviceId, out var q)) return Array.Empty<MqttMessage>();

            var arr = q.ToArray();
            if (arr.Length == 0) return Array.Empty<MqttMessage>();

            Array.Sort(arr, static (a, b) => Nullable.Compare<DateTimeOffset>(b.ReceivedAt, a.ReceivedAt));
            if (arr.Length > take) Array.Resize(ref arr, take);
            return arr;
        }

        public IReadOnlyDictionary<string, IReadOnlyList<MqttMessage>> GetLastAllDevices(int take = 5)
        {
            take = Math.Clamp(take, 1, MaxPerDevice);

            var result = new Dictionary<string, IReadOnlyList<MqttMessage>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _byDevice)
            {
                var arr = kv.Value.ToArray();
                Array.Sort(arr, static (a, b) => Nullable.Compare<DateTimeOffset>(b.ReceivedAt, a.ReceivedAt));
                if (arr.Length > take) Array.Resize(ref arr, take);
                result[kv.Key] = arr;
            }
            return result;
        }

        private static string? ExtractDeviceId(string topic)
        {
            // Matches .../devices/{deviceId}/... or .../device/{deviceId}/...
            var segments = topic.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].Equals("devices", StringComparison.OrdinalIgnoreCase) ||
                    segments[i].Equals("device", StringComparison.OrdinalIgnoreCase))
                {
                    return segments[i + 1];
                }
            }
            return null;
        }
    }
}
