using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models
{
    public class DeviceSimulationData
    {
        public int DeviceId { get; set; }
        public int TenantId { get; set; }
        public string DeviceName { get; set; } = default!;
        public string DeviceCode { get; set; }  = default!;
        public DeviceStatusEnum Status { get; set; }
        public Dictionary<string, decimal> SensorData { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }
}
