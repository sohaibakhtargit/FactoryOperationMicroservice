using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.DTOs
{
    public class DeviceConfigurationDto
    {
        public int ConfigId { get; set; } // for update/get
        public int TenantId { get; set; }
        public int DeviceId { get; set; }
        public int SamplingRate { get; set; }
        public string FirmwareVersion { get; set; }
        public string DataFormat { get; set; } = "JSON";
        public string Protocol { get; set; } = "MQTT";
    }

}
