using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class DeviceTelemetryDto
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceCode { get; set; }
        public Dictionary<string, object> SensorData { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DeviceStatusUpdateDto
    {
        public int DeviceId { get; set; }
        public string Status { get; set; }
        public DateTime LastSeen { get; set; }
    }

    public class MqttPublishRequest
    {
        public string Topic { get; set; }
        public string Message { get; set; }
        public bool Retain { get; set; } = false;
    }

    public class MqttSubscribeRequest
    {
        public string Topic { get; set; }
    }

    public class TelemetryResponseDto
    {
        public DeviceTelemetryRequestDto Device { get; set; }  
        public List<TelemetryDto> TelemetryRecords { get; set; }
        public List<DeviceStatusLog> StatusLogs { get; set; } = new();
    }

    public class DeviceTelemetryRequestDto
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceCode { get; set; }
        public string Status { get; set; }
    }

    public class TelemetryDto
    {
        public int TelemetryId { get; set; }
        public int DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public string SensorDataJson { get; set; }
    }
}
