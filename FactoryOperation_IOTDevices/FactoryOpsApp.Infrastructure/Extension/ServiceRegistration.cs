using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Extension
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddMqttServices(this IServiceCollection services, IConfiguration configuration)
        {
            // MQTT Configuration
            var mqttConfig = configuration.GetSection("Mqtt");
            services.Configure<MqttSettings>(mqttConfig);

            // MQTT Service
            services.AddSingleton<IMqttService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<MqttService>>();
                var settings = mqttConfig.Get<MqttSettings>();

                return new MqttService(
                    settings.BrokerUrl,
                    settings.Port,
                    settings.ClientId,
                    logger
                );
            });

            // Message Handler
            services.AddScoped<IMqttMessageHandler, MqttMessageHandler>();

            // IoT Simulator
            services.AddSingleton<IIoTDataSimulator, IoTDataSimulator>();
            services.AddHostedService(provider => provider.GetRequiredService<IIoTDataSimulator>() as IoTDataSimulator);

            return services;
        }
    }

    public class MqttSettings
    {
        public string BrokerUrl { get; set; } = "test.mosquitto.org";
        public int Port { get; set; } = 1883;
        public string ClientId { get; set; } = "factory-ops-app-test-12348203479237492639-3938er9fdh";
    }
}
