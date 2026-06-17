using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.IsolationControl;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Monitoring_Health;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.RestoreBackup;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.UserManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Analytic_Reports;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Authentication;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Notification;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminDashboard;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.IsolationControl;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Monitoring_Health;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.RestoreBackup;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.UserManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Authentication;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Notification;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminDashboard;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.IsolationControl;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Monitoring_Health;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.RestoreBackup;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.UserManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Analytic_Reports;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Authentication;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Notification;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminDashboard;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Configuration;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.IsolationControl;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Monitoring_Health;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.RestoreBackup;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.TenantManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.UserManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Analytic_Reports;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Authentication;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.ExceptionLogger;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.Notification;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TeamManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminDashboard;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.DTOs;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.Analytic_Reports;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Analytic_Reports;
using FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Analytics_Reports;
using FactoryOps_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.Analytic_Reports;
using FactoryOpsApp.Infrastructure.DBContext;
using FactoryOpsApp.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.InfrastructureServices
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

            services.AddScoped<IFactoryUserService, FactoryUserService>();
            services.AddScoped<IFactoryUserRepository, FactoryUserRepository>();

            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<ITenantRepository, TenantRepository>();

            services.AddScoped<IRoleService, FactoryRoleService>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            services.AddScoped<IPermissionService, FactoryPermissionService>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();

            services.AddScoped<IRolePermissionMappingService, RolePermissionMappingService>();
            services.AddScoped<IRolePermissionMappingRepository, RolePermissionMappingRepository>();

            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ITeamService, TeamService>();

            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IAuditService, AuditService>();

            services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
            services.AddScoped<ISupportTicketService, SupportTicketService>();

            services.AddScoped<ISystemMetricService, SystemMetricService>();
            services.AddScoped<ISystemMetricRepository, SystemMetricRepository>();

            services.AddScoped<ITenantIsolationService, TenantIsolationService>();
            services.AddScoped<ITenantIsolationRepository, TenantIsolationRepository>();

            services.AddScoped<IGlobalUserService, GlobalUserService>();
            services.AddScoped<IGlobalUserRepository, GlobalUserRepository>();

            services.AddScoped<ITenantAdminDashboardService, TenantAdminDashboardService>();
            services.AddScoped<ITenantAdminDashboardRepository, TenantAdminDashboardRepository>();

            services.AddScoped<ISuperAdminDashboardService, SuperAdminDashboardService>();
            services.AddScoped<ISuperAdminDashboardRepository, SuperAdminDashboardRepository>();

            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
            services.AddScoped<TenantDbContextFactory>();

            services.AddScoped<IAnnouncementTemplateService, AnnouncementTemplateService>();
            services.AddScoped<IAnnouncementTemplateRepository, AnnouncementTemplateRepository>();

            services.AddScoped<ICompanyBrandingInfoService, CompanyBrandingInfoService>();
            services.AddScoped<ICompanyBrandingInfoRepository, CompanyBrandingInfoRepository>();

            // Super Admin Support Ticket Services
            services.AddScoped<ISuperAdminSupportTicketService, SuperAdminSupportTicketService>();
            services.AddScoped<ISuperAdminSupportTicketRepository, SuperAdminSupportTicketRepository>();

            services.AddScoped<IFactoryNotificationRulesService, FactoryNotificationRulesService>();
            services.AddScoped<IFactoryNotificationRulesRepository, FactoryNotificationRulesRepository>();

            // Super Admin - Team API
            services.AddScoped<ISuperAdminTeamService, SuperAdminTeamService>();
            services.AddScoped<ISuperAdminTeamRepository, SuperAdminTeamRepository>();

            services.AddScoped<IFactoryAuthenticationService, FactoryAuthenticationService>();
            services.AddScoped<IFactoryAuthenticationRepository, FactoryAuthenticationRepository>();

            services.AddScoped<ISupportFeedbackService, SupportFeedbackService>();
            services.AddScoped<ISupportFeedbackRepository, SupportFeedbackRepository>();

            services.AddScoped<ISuperAdminSupportFeedbackService, SuperAdminSupportFeedbackService>();
            services.AddScoped<ISuperAdminSupportFeedbackRepository, SuperAdminSupportFeedbackRepository>();

            services.AddScoped<IMonitoringHealthRepository, MonitoringHealthRepository>();
            services.AddScoped<IMonitoringHealthService, MonitoringHealthService>();

            services.AddScoped<IReportsAnalyticsService, ReportsAnalyticsService>();
            services.AddScoped<IReportsAnalyticsRepository, ReportsAnalyticsRepository>();

            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IBackupRepository, BackupRepository>();

            services.AddScoped<ITenantMigrationService, TenantMigrationService>();
            services.AddScoped<ITenantMigrationRepository, TenantMigrationRepository>();

            services.AddScoped<IGlobalDropdownService, GlobalDropdownService>();
            services.AddScoped<IGlobalDropdownRepository, GlobalDropdownRepository>();


            services.AddScoped<IFactoryLocationService, FactoryLocationService>();
            services.AddScoped<IFactoryLocationRepository, FactoryLocationRepository>();

            services.AddScoped<IAnalyticsAndReportsRepository, AnalyticsAndReportsRepository>();
            services.AddScoped<IAnalyticsAndReportsServices, AnalyticsAndReportsService>();

            services.AddScoped<IIntegrationSettingsRepository, IntegrationSettingsRepository>();
            services.AddScoped<IIntegrationSettingsService, IntegrationSettingsService>();
                     

            services.AddScoped<IGamificationRepository, GamificationRepository>();
            services.AddScoped<IGamificationService, GamificationService>();

            services.AddScoped<INotificationQueryService, NotificationQueryService>();
            services.AddScoped<INotificationQueryRepository, NotificationQueryRepository>();

            // IOT Device Management
            services.AddScoped<IFactoryGroupService, FactoryGroupService>();
            services.AddScoped<IFactoryGroupRepository, FactoryGroupRepository>();

            // Team Management
            services.AddScoped<IPointAssignmentService, PointAssignmentService>();
            services.AddScoped<IPointAssignmentRepository, PointAssignmentRepository>();

            services.AddScoped<IBadgeService, BadgeService>();
            services.AddScoped<IBadgeRepository, BadgeRepository>();

            services.AddScoped<IUserBadgeService, UserBadgeService>();
            services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();

            services.AddScoped<IChallengesRepository, ChallengesRepository>();
            services.AddScoped<IChallengesService, ChallengesService>();

            services.AddScoped<ITeamsOverviewRepository, TeamsOverviewRepository>();
            services.AddScoped<ITeamsOverviewServices, TeamsOverviewService>();

            services.AddScoped<ITeamAlertNotificationRepository, TeamAlertNotificationRepository>();
            services.AddScoped<ITeamAlertNotificationService, TeamAlertNotificationService>();

            // Training Module
            services.AddScoped<ITrainingModuleService, TrainingModuleService>();
            services.AddScoped<ITrainingModuleRepository, TrainingModuleRepository>();

            // File Storage Service
            services.AddScoped<IFileStorageService, FileStorageService>();

            // Exception Logger Service
            services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();

            // Audit Log Service
            services.AddScoped<IAuditLogService, AuditLogService>();

            // SMTP
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddScoped<IEmailService, EmailService>();

            //Backup&Restore Setting
            services.Configure<BackupSettings>(configuration.GetSection("BackupSettings"));

            // SignalR
            services.AddSignalR(options => options.EnableDetailedErrors = true);
            services.AddScoped<INotificationService, NotificationService>();

            services.AddScoped<ITechnicianDashboardService, TechnicianDashboardService>();
            services.AddScoped<ITechnicianDashboardRepository, TechnicianDashboardRepository>();

            // HttpContext accessor
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
