using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers
{
    public interface IMqttMessageHandler
    {
        Task HandleMessageAsync(MqttMessageReceivedEventArgs e);
        Task HandleDeviceStatusMessageAsync(string topic, string payload);
        Task HandleSensorDataMessageAsync(string topic, string payload);
        Task HandleAlertMessageAsync(string topic, string payload);
    }
}
