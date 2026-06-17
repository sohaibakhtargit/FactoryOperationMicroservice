using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.EventTrace;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Shared;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Models;
//using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Extension;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Handlers;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.EventTrace;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Queue;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.SuperAdmin.AuditLogs;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Shared;
using FactoryOperation_IOTDevices.Middleware;
using FactoryOps_IOTDeviceService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.IOTDevices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");

// =======================
// CORE WEB
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// =======================
// DATABASES
// =======================
builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddScoped<TenantDbContextFactory>();

// =======================
// IOT DEVICE MANAGEMENT
// =======================
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

// =======================
// HANDLERS
// =======================
// Telemetry handler
builder.Services.AddScoped<IKafkaTelemetryHandler, KafkaTelemetryHandler>();
builder.Services.AddScoped<IDeviceStatusService, DeviceStatusService>();

builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();


// =======================
// KAFKA TELEMETRY PIPELINE (NEW, SAFE)
// =======================

// Bind Kafka settings (DB-driven fallback supported)
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Messaging:Kafka"));


// Event trace logger (same pattern as Audit service)

builder.Services.AddSingleton<EventTraceQueue>();
//builder.Services.AddSingleton<IEventTraceLogger, FileEventTraceLogger>();
builder.Services.AddScoped<IEventTraceLogger, DbEventTraceLogger>();

builder.Services.AddHostedService<EventTraceWorker>();

// External payload reader (local storage safe default)
builder.Services.AddSingleton<IExternalPayloadReader, LocalFilePayloadReader>();

// Background Kafka → Telemetry → DB → UI worker
builder.Services.AddHostedService<KafkaTelemetryConsumer>();

// =======================
// CORS
// =======================
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsPolicy", policy =>
//        policy
//            .SetIsOriginAllowed(_ => true)
//            .AllowAnyMethod()
//            .AllowAnyHeader()
//            .AllowCredentials());
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "https://ms.stagingsdei.com:8108"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// =======================
// APP PIPELINE
// =======================
var app = builder.Build();

// =======================
// SWAGGER
// =======================
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
