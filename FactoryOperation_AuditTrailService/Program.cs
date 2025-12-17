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

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

builder.Services.AddScoped<TenantDbContextFactory>();

builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();

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
