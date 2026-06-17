
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.SuperAdmin.AuditLogs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.EntityFrameworkCore;
namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<FactoryOpsDBContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("TenantDbConnection")));

            services.AddDbContext<MasterFactoryOpsDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("MasterDbConnection")));
      
            services.AddScoped<IFactoryMqttTopicService, FactoryMqttTopicService>();
            services.AddScoped<IFactoryMqttTopicRepository, FactoryMqttTopicRepository>();

            services.AddScoped<IFactoryDeviceService, FactoryDeviceService>();
            services.AddScoped<IFactoryDeviceRepository, FactoryDeviceRepository>();

            services.AddScoped<IAlertRuleService, AlertRuleService>();
            services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();

            services.AddScoped<ITelemetryRepository, TelemetryRepository>();
          //  services.AddSingleton<IMqttMessageHandler, MqttMessageHandler>();
            services.AddScoped<IDeviceStatusService, DeviceStatusService>();

            services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
            services.AddScoped<IDeviceConfigurationRepository, DeviceConfigurationRepository>();

          

            // Audit Log Service
            services.AddScoped<IAuditLogService, AuditLogService>();

         
            // HttpContext accessor
            services.AddHttpContextAccessor();

            return services;
        }
    }
}