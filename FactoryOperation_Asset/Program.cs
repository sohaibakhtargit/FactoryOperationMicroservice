using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_Asset.Middleware;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp.Infrastructure.Repository.TenantAdmin.AssetManagement;
using FactoryOpsApp.Infrastructure.Service;
using FactoryOpsApp.Infrastructure.Service.SuperAdmin.AuditLogs;
using FactoryOpsApp.Infrastructure.Service.TenantAdmin.AssetManagement;
using FactoryOpsApp.Infrastructure.Service.TenantAdmin.ExceptionLogger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FactoryOps API- Asset",
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
builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

// Register the factory used by WorkOrderRepository
builder.Services.AddScoped<TenantDbContextFactory>();

builder.Services.AddScoped<IAssetDashboardReportRepository, AssetDashboardReportRepository>();
builder.Services.AddScoped<IAssetDashboardReportService, AssetDashboardReportService>();

builder.Services.AddScoped<IAssetDocumentRepository, AssetDocumentRepository>();
builder.Services.AddScoped<IAssetDocumentService, AssetDocumentService>();


builder.Services.AddScoped<IAssetLifecycleRepository, AssetLifecycleRepository>();
builder.Services.AddScoped<IAssetLifecycleService, AssetLifecycleService>();

builder.Services.AddScoped<IAssetFinancialAnalysisRepository, AssetFinancialAnalysisRepository>();
builder.Services.AddScoped<IAssetFinancialAnalysisService, AssetFinancialAnalysisService>();

builder.Services.AddScoped<IAssetManagementRepository, AssetManagementRepository>();
builder.Services.AddScoped<IAssetManagementService, AssetManagementService>();


builder.Services.AddScoped<IAssetTypeRepository, AssetTypeRepository>();
builder.Services.AddScoped<IAssetTypeService, AssetTypeService>();


builder.Services.AddScoped<IAsssetTrackingRepository, AssetTrackingRepository>();
builder.Services.AddScoped<IAssetTrackingServices, AssetTrackingServices>();


builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
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
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
