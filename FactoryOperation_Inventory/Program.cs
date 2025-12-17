using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_Inventory.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.InventoryManagement;
using FactoryOperation_Inventory.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_Inventory.Middleware;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Notification;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp.Infrastructure.Hubs;
using FactoryOpsApp.Infrastructure.Repository.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Infrastructure.Repository.TenantAdmin.Notification;
using FactoryOpsApp.Infrastructure.Service;
using FactoryOpsApp.Infrastructure.Service.SuperAdmin.AuditLogs;
using FactoryOpsApp.Infrastructure.Service.TenantAdmin.Common;
using FactoryOpsApp.Infrastructure.Service.TenantAdmin.InventoryManagement;
using FactoryOpsApp.Infrastructure.Service.TenantAdmin.Notification;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FactoryOps API - Inventory",
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
             Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
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

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddScoped<TenantDbContextFactory>();

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();

builder.Services.AddScoped<IInventoryCostIntegrationService, InventoryCostIntegrationService>();
builder.Services.AddScoped<IInventoryCostIntegrationRepository, InventoryCostIntegrationRepository>();

builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationQueryRepository, NotificationQueryRepository>();
builder.Services.AddScoped<IPurchaseRequisitionRepository, PurchaseRequisitionRepository>();
builder.Services.AddScoped<IPurchaseRequisitionService, PurchaseRequisitionService>();


builder.Services.AddHttpContextAccessor();
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

builder.Services.AddSignalR();

var app = builder.Build();


// ======================= Swagger =======================
if (envMode == "Local" || envMode == "Staging")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("CorsPolicy");
app.UseExceptionLogging();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");
app.MapControllers();

app.UseHttpsRedirection();

app.Run();
