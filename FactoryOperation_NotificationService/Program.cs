
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.Shared;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Models;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Kafka;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Queue;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.Common;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.EmailService;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.EventTrace;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Shared;
using FactoryOps.NotificationService.Infrastructure.Hubs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ======================= JWT & SIGNALR =======================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)
        ),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true
    };

    //options.Events = new JwtBearerEvents
    //{
    //    OnMessageReceived = context =>
    //    {
    //        var accessToken = context.Request.Query["access_token"];
    //        var path = context.HttpContext.Request.Path;

    //        if (!string.IsNullOrEmpty(accessToken) &&
    //            (path.StartsWithSegments("/notificationHub") ||
    //             path.StartsWithSegments("/notification/notificationHub")))
    //        {
    //            context.Token = accessToken;
    //        }
    //        return Task.CompletedTask;
    //    }
    //};

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

            Console.WriteLine($"Notification Service - Path: {path}, Token present: {!string.IsNullOrEmpty(accessToken)}");

            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Notification Auth Failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },

        OnTokenValidated = context =>
        {
            Console.WriteLine($"Notification Token Validated: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

//builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; 
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);  
});

//.AddStackExchangeRedis(builder.Configuration["Redis:Connection"], options =>
//{
//    options.Configuration.ChannelPrefix = "factoryops-signalr";
//});


// ======================= KAFKA =======================
builder.Services.AddOptions<KafkaSettings>()
    .Bind(builder.Configuration.GetSection("Kafka"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHostedService<KafkaConsumerWorker>();


builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddScoped<TenantDbContextFactory>();


builder.Services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationProcessor, NotificationProcessor>();
builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
builder.Services.AddScoped<INotificationEmailSender, NotificationEmailSender>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IUserIdProvider, AppUserIdProvider>();

//event trace logger    
// Event trace logger (same pattern as Audit service)
builder.Services.AddSingleton<EventTraceQueue>();
//builder.Services.AddSingleton<IEventTraceLogger, FileEventTraceLogger>();
builder.Services.AddScoped<IEventTraceLogger, DbEventTraceLogger>();

builder.Services.AddHostedService<EventTraceWorker>();
// once client provide Azure Blob Cred--
//builder.Services.AddSingleton<IExternalPayloadReader, AzureBlobPayloadReader>();

//--Local file storage get payload from the wwwroot folder
builder.Services.AddSingleton<IExternalPayloadReader, LocalFilePayloadReader>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddHealthChecks();

// ======================= API + CORS =======================
builder.Services.AddControllers();
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsPolicy", b =>
//        b.SetIsOriginAllowed(_ => true)
//         .AllowAnyHeader()
//         .AllowAnyMethod()
//         .AllowCredentials());
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

// ======================= Logging =======================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ===========================================================


var app = builder.Build();

if (envMode == "Local" || envMode == "Staging")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(15)
});

app.UseCors("CorsPolicy");

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

//app.MapHub<NotificationHub>("/notification/notificationHub");
app.MapHub<NotificationHub>("/notificationHub");


app.MapControllers();

app.MapGet("/Check-Notification-MicroService",
    () => "Notification Microservice Running");

app.Run();
