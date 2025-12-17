using FactoryOperation_PreventiveMaintenance.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.PreventiveMaintenance;
using FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOpsApp.Common.CommonConstant;

namespace FactoryOpsApp.Infrastructure.Repository.TenantAdmin.PreventiveMaintenance
{
    public class AlertNotificationRepository : IAlertNotificationRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;
        public AlertNotificationRepository(TenantDbContextFactory tenantDbContext,
                                   IAuditLogService auditLogger,
                                   IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }
        public async Task<CommonResponseModel> AddAlertNotificationAsync(AlertNotificationDTO dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new AlertNotification
                {
                    TenantId = dto.TenantId,
                    AlertTitle = dto.AlertTitle,
                    AlertMessage = dto.AlertMessage,
                    AlertType= dto.AlertType,
                    Severity = dto.Severity,
                    Recipients = dto.Recipients,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.TenantId
                };

                await tenantDb.AlertNotifications.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Alert Notification '{entity.AlertTitle}'", dto.TenantId, "", "AddAlertNotificationAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AlertNotificationStatusMessage.CreateSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertRule-Module", "AddAlertNotificationAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AlertNotificationStatusMessage.CreateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> DeleteAlertNotificationAsync(int alertId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AlertNotifications
                    .FirstOrDefaultAsync(a => a.AlertId == alertId && !a.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = AlertNotificationStatusMessage.NotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Delete",
                    $"Deleted Alert Notification '{entity.AlertTitle}'",
                    tenantId,
                    "",
                    "DeleteAlertNotificationAsync"
                );

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AlertNotificationStatusMessage.DeleteSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertNotification-Module", "DeleteAlertNotificationAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AlertNotificationStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async  Task<GetSpecificRecord<AlertNotificationDTO>> GetAlertNotificationByIdAsync(int alertId, int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
           GetSpecificRecord<AlertNotificationDTO> response = new GetSpecificRecord<AlertNotificationDTO>();
            try
            {
                response.Data = await (from dto in tenantDb.AlertNotifications
                                       where dto.AlertId == alertId  && dto.IsActive && !dto.IsDeleted
                                             select new AlertNotificationDTO
                                             {
                                                 AlertId=dto.AlertId,
                                                 TenantId = dto.TenantId,
                                                 AlertTitle = dto.AlertTitle,
                                                 AlertMessage = dto.AlertMessage,
                                                 AlertType = dto.AlertType,
                                                 Severity = dto.Severity,
                                                 Recipients = dto.Recipients,
                                                 IsActive = true,
                                                 IsDeleted = false,
                                                 CreatedAt = DateTime.UtcNow,
                                                 CreatedBy = dto.TenantId,
                                                 UpdatedAt = dto.UpdatedAt,
                                                 UpdatedBy = dto.UpdatedBy
                                             }
                                      ).FirstOrDefaultAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AlertNotificationStatusMessage.FetchByIdSuccess;
                return response;


            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertNotifications-Module", "GetAlertNotificationById", tenantId, null);
                return new GetSpecificRecord<AlertNotificationDTO>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AlertNotificationStatusMessage.FetchByIdFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetAllRecord<AlertNotificationDTO>> GetAllAlertNotificationAsync(int tenantId)
        {
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            GetAllRecord<AlertNotificationDTO> response = new GetAllRecord<AlertNotificationDTO>();
            try
            {
                response.GetAllData =await (from dto in tenantDb.AlertNotifications
                                            where dto.IsActive && !dto.IsDeleted
                                            select new AlertNotificationDTO
                                       {
                                           AlertId=dto.AlertId,
                                           TenantId = dto.TenantId,
                                           AlertTitle = dto.AlertTitle,
                                           AlertMessage = dto.AlertMessage,
                                           AlertType = dto.AlertType,
                                           Severity = dto.Severity,
                                           Recipients = dto.Recipients,
                                           IsActive = true,
                                           IsDeleted = false,
                                           CreatedAt = DateTime.UtcNow,
                                           CreatedBy = dto.TenantId,
                                           UpdatedAt=dto.UpdatedAt,
                                           UpdatedBy=dto.UpdatedBy
                                       }
                                      ).ToListAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AlertNotificationStatusMessage.FetchAllSuccess;
                return response;


            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertNotifications-Module", "GetAllAlertNotification", tenantId, null);
                return new GetAllRecord<AlertNotificationDTO>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AlertNotificationStatusMessage.FetchAllFailed}: {ex.Message}"
                };
            }
        }
        public async Task<CommonResponseModel> UpdateAlertNotificationAsync(AlertNotificationDTO dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.AlertNotifications
                    .FirstOrDefaultAsync(a => a.AlertId == dto.AlertId && !a.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = AlertNotificationStatusMessage.NotFound };
                entity.AlertTitle = dto.AlertTitle;
                entity.AlertMessage = dto.AlertMessage;
                entity.AlertType = dto.AlertType;
                entity.Severity = dto.Severity;
                entity.Recipients = dto.Recipients;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.TenantId;

                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync("Update", $"Updated AlertNotification '{entity.AlertTitle}'", dto.TenantId, "", "UpdateAlertRuleAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AlertNotificationStatusMessage.UpdateSuccess };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertNotification-Module", "UpdateAlertNotificationAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AlertNotificationStatusMessage.UpdateFailed}: {ex.Message}" };
            }
        }
    }
}
