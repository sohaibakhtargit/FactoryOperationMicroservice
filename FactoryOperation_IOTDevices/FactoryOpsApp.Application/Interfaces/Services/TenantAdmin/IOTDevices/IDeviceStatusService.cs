using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices
{
    public interface IDeviceStatusService
    {
        Task LogDeviceEventAsync(int tenantId, int deviceId, string eventType, string remarks);
    }
}
