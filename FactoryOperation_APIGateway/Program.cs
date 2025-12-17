using FactoryOperation_API_Gateway.FactoryOpsApp.Application.Models;
using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Implementations;
using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Implementations.ExceptionLogger;
using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces;
using FactoryOperation_API_Gateway.FactoryOpsApp.Infrastructure.Services.Interfaces.ExceptionLogger;
using FactoryOperation_API_Gateway.Middleware;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var envMode = builder.Configuration.GetValue<string>("EnvironmentMode");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FactoryOps API Gateway",
        Version = "v1",
        Description = "API Gateway with JWT Authentication and Reverse Proxy"
    });
});

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register services with correct lifetimes
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add HttpClient with resilience
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure JWT Authentication - FIXED: Don't resolve scoped services here
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Get configuration directly, don't resolve JwtService
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

        if (jwtSettings == null)
        {
            throw new InvalidOperationException("JwtSettings configuration is missing");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for: {context.Principal?.Identity?.Name}");
                return Task.CompletedTask;
            }
        };
    });

//builder.Services.AddSignalR();

builder.Services.AddDbContext<FactoryOpsDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TenantDbConnection")));

builder.Services.AddDbContext<MasterFactoryOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection")));

// Register the factory used by WorkOrderRepository
builder.Services.AddScoped<TenantDbContextFactory>();


builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();

builder.Services.AddAuthorization();

// Add logging
builder.Services.AddLogging();
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
app.UseHttpsRedirection();

// Add custom middleware - These will resolve scoped services correctly
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<JwtMiddleware>();
app.UseExceptionLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();

// Health check endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();