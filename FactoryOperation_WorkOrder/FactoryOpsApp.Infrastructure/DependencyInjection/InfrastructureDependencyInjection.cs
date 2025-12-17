using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AuditLogs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOperation_WorkOrder.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderServices;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.WorkOrderManagement;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.Common;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.AuditLogs;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.Notification;
using FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.WorkOrderManagement;
using FactoryOpsApp.Infrastructure.Repository.TenantAdmin.WorkOrderManagement;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace FactoryOperation_WorkOrder.FactoryOpsApp.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<FactoryOpsDBContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("TenantDbConnection")));

            services.AddDbContext<MasterFactoryOpsDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("MasterDbConnection")));

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var cfg = ConfigurationOptions.Parse("localhost:6379", true);
                cfg.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(cfg);
            });

            services.AddScoped<TenantDbContextFactory>();
            services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
            services.AddScoped<IWorkOrderService, WorkOrderService>();

            services.AddScoped<IWorkOrderSubTaskRepository, WorkOrderSubTaskRepository>();
            services.AddScoped<IWorkOrderSubTaskService, WorkOrderSubTaskService>();

            services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
            services.AddScoped<IServiceRequestService, ServiceRequestService>();

            services.AddScoped<ITechnicianAssignmentService, TechnicianAssignmentService>();
            services.AddScoped<ITechnicianAssignmentRepository, TechnicianAssignmentRepository>();

            services.AddScoped<IGamificationRepository, GamificationRepository>();
            services.AddScoped<IGamificationService, GamificationService>();

            services.AddScoped<INotificationQueryService, NotificationQueryService>();
            services.AddScoped<INotificationQueryRepository, NotificationQueryRepository>();

            // Exception Logger Service
            services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();

            // Audit Log Service
            services.AddScoped<IAuditLogService, AuditLogService>();

            // SignalR
            services.AddSignalR(options => options.EnableDetailedErrors = true);
            services.AddScoped<INotificationService, NotificationService>();

            services.AddSingleton<IUserIdProviders, CustomUserIdProvider>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            // HttpContext accessor
            services.AddHttpContextAccessor();

            return services;
        }
    }
}