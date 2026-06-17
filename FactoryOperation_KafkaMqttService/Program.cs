using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.DBContext;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Repositories.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.EventTrace;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.KafkaMqttBridge;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Infrastructure.Implementations.Services.Queue;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Config;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Interfaces;
using FactoryOperation_KafkaMqttService.FactoryOpsApp.Shared.Services;
using FactoryOperation_KafkaMqttService.Middleware;
using FactoryOpsApp.Messaging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");

// Keep the app alive if a BackgroundService throws
builder.Services.Configure<HostOptions>(o =>
{
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Separate flags for legacy Infra MQTT and new Messaging stack
//var infraMqttEnabled = builder.Configuration.GetValue<bool>("Mqtt:Enabled", true);
var messagingEnabled = builder.Configuration.GetValue<bool>("Messaging:Enabled", true);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FactoryOps API - Kafka",
        Version = "v1",
        Description = "Factory Operations Management System API",
        Contact = new OpenApiContact
        {
            Name = "FactoryOps Support",
            Email = "support@factoryops.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer {token}')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
         ValidAudience = builder.Configuration["JwtSettings:Audience"],
         IssuerSigningKey = new SymmetricSecurityKey(
             Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
     };

     options.Events = new JwtBearerEvents
     {
         OnMessageReceived = context =>
         {
             var accessToken = context.Request.Query["access_token"];
             var path = context.HttpContext.Request.Path;

             if (!string.IsNullOrEmpty(accessToken) &&
                 path.StartsWithSegments("/notificationHub"))
             {
                 context.Token = accessToken;
             }

             return Task.CompletedTask;
         }
     };
 });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddScoped<TenantDbContextFactory>();

// New: Configuration repositories and services
builder.Services.AddScoped<IKafkaConfigurationRepository, KafkaConfigurationRepository>();
builder.Services.AddScoped<IMqttConfigurationRepository, MqttConfigurationRepository>();
builder.Services.AddScoped<IBridgeConfigurationRepository, BridgeConfigurationRepository>();

builder.Services.AddScoped<IKafkaConfigurationService, KafkaConfigurationService>();
builder.Services.AddScoped<IMqttConfigurationService, MqttConfigurationService>();
builder.Services.AddScoped<IBridgeConfigurationService, BridgeConfigurationService>();
builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();

//event trace logger    
builder.Services.AddSingleton<EventTraceQueue>();
//builder.Services.AddSingleton<IEventTraceLogger, FileEventTraceLogger>();
builder.Services.AddScoped<IEventTraceLogger, DbEventTraceLogger>();

builder.Services.AddHostedService<EventTraceWorker>();

// once client provide Azure Blob Cred--
//builder.Services.AddSingleton<IExternalPayloadStore, AzureBlobPayloadStore>();

//--Local file storage in wwwroot folder
builder.Services.AddSingleton<IExternalPayloadStore, LocalFilePayloadStore>();


if (messagingEnabled)
{
    // New Messaging stack (IMqttClientService + bridge to Kafka + ILastTelemetryStore)
    builder.Services.AddMessagingServices(builder.Configuration);
}

//builder.Services.AddOpenTelemetry()
//    .WithTracing(t =>
//    {
//        t.AddAspNetCoreInstrumentation();
//        t.AddHttpClientInstrumentation();
//        t.AddSource("FactoryOpsTelemetry"); // your Telemetry.Activity source
//        // Add exporter via config, e.g., OTLP
//    })
//    .WithMetrics(m =>
//    {
//        m.AddAspNetCoreInstrumentation();
//        m.AddHttpClientInstrumentation();
//    });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("FactoryOps.Messaging"))
    .WithMetrics(metrics =>
    {
        metrics
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddMeter("FactoryOps.Messaging")
            .AddPrometheusExporter()

            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://44.211.113.36:4317");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
    })
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddSource("FactoryOps.Messaging")
            .AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://44.211.113.36:4317");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
    });


//Kafka

builder.Services.AddLogging();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.Configure<BlobStorageSettings>(
    builder.Configuration.GetSection("BlobStorage"));


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsPolicy", builder =>
//        builder
//        .SetIsOriginAllowed(_ => true)
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .AllowCredentials()
//        );
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

builder.Services.AddHttpContextAccessor();

var app = builder.Build();


// ======================= Swagger =======================
if (envMode == "Local" || envMode == "Staging")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.UseCors("CorsPolicy");

app.UseExceptionLogging();

app.UseHttpsRedirection();

app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();
app.Run();
