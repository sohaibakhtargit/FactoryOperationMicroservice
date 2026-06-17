namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models
{
    public class TelemetryPayload
    {
        public int TenantId { get; set; }
        public string DeviceCode { get; set; }
        public string PayloadBase64 { get; set; }
    }

}
