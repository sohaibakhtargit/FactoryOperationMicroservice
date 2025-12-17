using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Extension;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.SuperAdmin.AuditLogs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.Middleware;
using FactoryOps_IOTDeviceService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));



builder.Services.AddScoped<TenantDbContextFactory>();
// IOT Device Management
/*builder.Services.AddScoped<IFactoryGroupService, FactoryGroupService>();
builder.Services.AddScoped<IFactoryGroupRepository, FactoryGroupRepository>();*/
builder.Services.AddScoped<IAlertRuleService, AlertRuleService>();
builder.Services.AddScoped<IAlertRuleRepository, AlertRuleRepository>();

builder.Services.AddScoped<IDeviceConfigurationService, DeviceConfigurationService>();
builder.Services.AddScoped<IDeviceConfigurationRepository, DeviceConfigurationRepository>();

builder.Services.AddScoped<IFactoryDeviceService, FactoryDeviceService>();
builder.Services.AddScoped<IFactoryDeviceRepository, FactoryDeviceRepository>();

builder.Services.AddScoped<IFactoryMqttTopicService, FactoryMqttTopicService>();
builder.Services.AddScoped<IFactoryMqttTopicRepository, FactoryMqttTopicRepository>();

builder.Services.AddScoped<IReorderRuleService, ReorderRuleService>();
builder.Services.AddScoped<IReorderRuleRepository, ReorderRuleRepository>();

builder.Services.AddScoped<ISupplierManagementRepository, SupplierManagementRepository>();
builder.Services.AddScoped<ISupplierManagementService, SupplierManagementService>();

builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();

builder.Services.AddSingleton<IMqttMessageHandler, MqttMessageHandler>();
builder.Services.AddScoped<IDeviceStatusService, DeviceStatusService>();

builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// MQTT IOT SERVICES REGISTRATION

builder.Services.AddMqttServices(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
        builder
        .SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        );
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// ======================= Swagger =======================
if (envMode == "Local" || envMode == "Staging")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseExceptionLogging();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
