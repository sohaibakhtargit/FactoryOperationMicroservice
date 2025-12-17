using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Kafka_Notification;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.Common;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.EmailServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_NotificationService.FactoryOpsApp.Common.Models;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Kafka;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.Common;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.EmailService;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.NotificationServices;
using FactoryOperation_NotificationService.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_NotificationService.Middleware;
using FactoryOps.NotificationService.Infrastructure.Hubs;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])
        ),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/notificationHub") ||
                 path.StartsWithSegments("/notification/notificationHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);

// ======================= KAFKA =======================
builder.Services.AddOptions<KafkaSettings>()
    .Bind(builder.Configuration.GetSection("Kafka"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHostedService<KafkaConsumerWorker>();

// ======================= DB =======================
builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddScoped<TenantDbContextFactory>();

// ======================= SERVICES =======================
builder.Services.AddSingleton<INotificationDispatcher, NotificationDispatcher>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationProcessor, NotificationProcessor>();
builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
builder.Services.AddScoped<INotificationEmailSender, NotificationEmailSender>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IAppUserIdProvider, AppUserIdProvider>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddHealthChecks();

// ======================= API + CORS =======================
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", b =>
        b.SetIsOriginAllowed(_ => true)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// ======================= Logging =======================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ===========================================================
var app = builder.Build();

// ======================= Swagger =======================
if (envMode == "Local" || envMode == "Staging")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ======================= Middleware =======================
app.UseCors("CorsPolicy");
app.UseExceptionLogging();
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ======================= Map Routes =======================
app.MapHealthChecks("/health");

// 🔥 FINAL HUB ROUTE
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();
app.MapGet("/Check-Notification-MicroService", () => "Notification Microservice Running");

// ======================= RUN =======================
app.Run();
