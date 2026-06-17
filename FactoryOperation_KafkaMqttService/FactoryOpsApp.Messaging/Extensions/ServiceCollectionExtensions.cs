using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Health;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Messaging.Services;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Health;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Services;
using FactoryOpsApp.Shared.Services;

namespace FactoryOpsApp.Messaging.Extensions
{
    public static class ServiceRegistration_Messaging
    {
        public static IServiceCollection AddMessagingServices(this IServiceCollection services, IConfiguration configuration)
        {
            var mqttSection = configuration.GetSection("Messaging:Mqtt");
            var kafkaSection = configuration.GetSection("Messaging:Kafka");
            var simulatorSection = configuration.GetSection("Messaging:Simulator");

            services.Configure<MqttSettings>(mqttSection);
            services.Configure<KafkaSettings>(kafkaSection);
            services.Configure<MqttSimulatorSettings>(simulatorSection);

            var mqttSettings = mqttSection.Get<MqttSettings>() ?? throw new InvalidOperationException("Missing configuration section: Messaging:Mqtt");
            if (string.IsNullOrWhiteSpace(mqttSettings.BrokerUrl))
                throw new InvalidOperationException("MQTT BrokerUrl is required in Messaging:Mqtt:BrokerUrl");

            var kafkaSettings = kafkaSection.Get<KafkaSettings>() ?? throw new InvalidOperationException("Missing configuration section: Messaging:Kafka");
            if (string.IsNullOrWhiteSpace(kafkaSettings.BootstrapServers))
                throw new InvalidOperationException("Kafka BootstrapServers is required in Messaging:Kafka:BootstrapServers");

            // Core messaging singletons
            services.AddSingleton<IMqttClientService, MqttClientService>();
            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
            //services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();

            services.AddSingleton<KafkaConsumerService>();
            services.AddSingleton<IKafkaConsumerService>(sp =>
                sp.GetRequiredService<KafkaConsumerService>());
            services.AddSingleton<IKafkaConsumerBridgeService>(sp =>
                sp.GetRequiredService<KafkaConsumerService>());

            // In-memory last telemetry store
            services.AddSingleton<ILastTelemetryStore, LastTelemetryStore>();

            // Dynamic settings provider (DB-backed)
            services.AddMemoryCache();
            services.AddSingleton<IMessagingSettingsProvider, DbMessagingSettingsProvider>();
            services.AddSingleton<ITenantResolver, DefaultTenantResolver>();

            // Hosted bridge: MQTT -> Kafka
            services.AddHostedService<MqttToKafkaBridgeService>();

            // Dev/test simulator (disabled by default in prod)
            var sim = simulatorSection.Get<MqttSimulatorSettings>() ?? new MqttSimulatorSettings();
            if (sim.Enabled)
            {
                services.AddHostedService<RandomMqttDataPublisherService>();
            }

            // Health checks
            services.AddHealthChecks()
                .AddCheck<MqttHealthCheck>("mqtt", tags: new[] { "messaging" })
                .AddCheck<KafkaHealthCheck>("kafka", tags: new[] { "messaging" });

            return services;
        }
    }
}