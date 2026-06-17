using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.AssetManagement;
using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.AssetManagement;
using FactoryOperation_Asset.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_Asset.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.AssetManagement;
using FactoryOperation_Asset.FactoryOpsApp.Infrastructure.Implementation.Services.TenantAdmin.AssetManagement;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static FactoryOpsApp.Infrastructure.Repository.TenantAdmin.AssetManagement.AssetLifecycleRepository;

namespace FactoryOperation_Asset.FactoryOpsApp.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ================= DB Contexts =================
            services.AddDbContext<FactoryOpsDBContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("TenantDbConnection")));

            services.AddDbContext<MasterFactoryOpsDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("MasterDbConnection")));

            services.AddScoped<TenantDbContextFactory>();

            // ================= Repositories =================
            services.AddScoped<IAssetManagementRepository, AssetManagementRepository>();
            services.AddScoped<IAssetBOMRepository, AssetBOMRepository>();
            services.AddScoped<IAssetTypeRepository, AssetTypeRepository>();
            services.AddScoped<IAssetDocumentRepository, AssetDocumentRepository>();
            services.AddScoped<IAssetLifecycleRepository, AssetLifecycleRepository>();
            services.AddScoped<IAssetFinancialAnalysisRepository, AssetFinancialAnalysisRepository>();
            services.AddScoped<IAssetDashboardReportRepository, AssetDashboardReportRepository>();
            services.AddScoped<IAsssetTrackingRepository, AssetTrackingRepository>();

            // ================= Services =================
            services.AddScoped<IAssetManagementService, AssetManagementService>();
            services.AddScoped<IAssetBOMService, AssetBOMService>();
            services.AddScoped<IAssetTypeService, AssetTypeService>();
            services.AddScoped<IAssetDocumentService, AssetDocumentService>();
            services.AddScoped<IAssetLifecycleService, AssetLifecycleService>();
            services.AddScoped<IAssetFinancialAnalysisService, AssetFinancialAnalysisService>();
            services.AddScoped<IAssetDashboardReportService, AssetDashboardReportService>();
            services.AddScoped<IAssetTrackingServices, AssetTrackingServices>();

            // ================= Cross-cutting =================
            services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IFileStorageService, FileStorageService>();

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
