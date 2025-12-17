namespace FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config
{
    public class MqttSimulatorSettings
    {
        public bool Enabled { get; set; } = false;
        public int IntervalMs { get; set; } = 5000;
        public int[]? TenantIds { get; set; } = Array.Empty<int>();
        public string[]? DeviceCodes { get; set; } = Array.Empty<string>();
        public int DeviceCount { get; set; } = 3;
    }
}