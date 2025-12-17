using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;

namespace FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
{
    public interface IIoTDataSimulator
    {
        Task StartSimulationAsync();
        Task StopSimulationAsync();
        Task SimulateDeviceDataAsync(FactoryDevice device);
        Task SimulateDeviceStatusChangeAsync(FactoryDevice device);
        bool IsSimulationRunning { get; } // Add this property
    }

    public class DeviceSimulationData
    {
        public int DeviceId { get; set; }
        public int TenantId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceCode { get; set; }
        public DeviceStatusEnum Status { get; set; }
        public Dictionary<string, decimal> SensorData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}