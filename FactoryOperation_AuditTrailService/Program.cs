using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.EventTrace;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Interfaces.Services.Shared;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Application.Models;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Infrastructure.Implementation.EventTrace;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Infrastructure.Implementation.Queue;
using FactoryOperation_AuditTrailService.FactoryOpsApp.Infrastructure.Implementation.Shared;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.Middleware;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FactoryOps API - AuditTrailService",
        Version = "v1",
        Description = "Factory Operations Management System API",
        Contact = new OpenApiContact
        {
            Name = "FactoryOps Support",
            Email = "support@factoryops.com"
        }
    });
});

// ======================= KAFKA =======================
builder.Services.AddOptions<KafkaSettings>()
    .Bind(builder.Configuration.GetSection("Kafka"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHostedService<KafkaAuditConsumer>();

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddScoped<TenantDbContextFactory>();

builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();

//event trace logger   
// once client provide Azure Blob Cred--
//builder.Services.AddSingleton<IExternalPayloadReader, AzureBlobPayloadReader>();

//--Local file storage get payload from the wwwroot folder

builder.Services.AddSingleton<EventTraceQueue>();
//builder.Services.AddSingleton<IEventTraceLogger, FileEventTraceLogger>();
builder.Services.AddScoped<IEventTraceLogger, DbEventTraceLogger>();

builder.Services.AddHostedService<EventTraceWorker>();

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
