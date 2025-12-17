using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Common;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.IOTDevices;
using FactoryOperation_IOTDevices.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOps_IOTDeviceService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOperation_IOTDevices.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_IOTDevices.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.IOTDevices
{
    public class AlertRuleRepository : IAlertRuleRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public AlertRuleRepository(TenantDbContextFactory tenantDbContext,
                                   IAuditLogService auditLogger,
                                   IExceptionLoggerService exceptionLogger)
        {
            _tenantDbContext = tenantDbContext;
            _auditLogger = auditLogger;
            _exceptionLogger = exceptionLogger;
        }

        public async Task<CommonResponseModel> AddAlertRuleAsync(AlertRuleDto dto)
        {
            try
            {

                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = new AlertRule
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    RuleCondition = dto.RuleCondition,
                    Severity = dto.Severity,
                    Status = dto.Status,
                    DevicesCount = 0,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.TenantId
                };

                await tenantDb.AlertRules.AddAsync(entity);
                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync("Create", $"Created Alert Rule '{entity.Name}'", dto.TenantId, "", "AddAlertRuleAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AlertRuleStatusMessage.Added };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertRule-Module", "AddAlertRuleAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AlertRuleStatusMessage.AddFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> UpdateAlertRuleAsync(AlertRuleDto dto)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);

                var entity = await tenantDb.AlertRules
                    .FirstOrDefaultAsync(a => a.AlertRuleId == dto.AlertRuleId && !a.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = AlertRuleStatusMessage.NotFound };

                entity.Name = dto.Name;
                entity.RuleCondition = dto.RuleCondition;
                entity.Severity = dto.Severity;
                entity.Status = dto.Status;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = dto.TenantId;

                await tenantDb.SaveChangesAsync();
                await _auditLogger.LogAuditAsync("Update", $"Updated Alert Rule '{entity.Name}'", dto.TenantId, "", "UpdateAlertRuleAsync");

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AlertRuleStatusMessage.Updated };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertRule-Module", "UpdateAlertRuleAsync", dto.TenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AlertRuleStatusMessage.UpdateFailed}: {ex.Message}" };
            }
        }
        public async Task<CommonResponseModel> DeleteAlertRuleAsync(int alertRuleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AlertRules
                    .FirstOrDefaultAsync(a => a.AlertRuleId == alertRuleId && !a.IsDeleted);

                if (entity == null)
                    return new CommonResponseModel { StatusCode = StatusCode.NotFound, StatusMessage = AlertRuleStatusMessage.NotFound };

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                entity.DeletedBy = tenantId;

                await tenantDb.SaveChangesAsync();

                await _auditLogger.LogAuditAsync(
                    "Delete",
                    $"Deleted Alert Rule '{entity.Name}'",
                    tenantId,
                    "",
                    "DeleteAlertRuleAsync"
                );

                return new CommonResponseModel { StatusCode = StatusCode.Success, StatusMessage = AlertRuleStatusMessage.Deleted };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertRule-Module", "DeleteAlertRuleAsync", tenantId, null);
                return new CommonResponseModel { StatusCode = StatusCode.Error, StatusMessage = $"{AlertRuleStatusMessage.DeleteFailed}: {ex.Message}" };
            }
        }
        public async Task<GetAllRecord<GetAlertRuleDto>> GetAllAlertRulesAsync(int tenantId, AlertStatusEnum? statusFilter = null)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var query = tenantDb.AlertRules
                    .Where(a => a.TenantId == tenantId && !a.IsDeleted)
                    .AsQueryable();

                if (statusFilter.HasValue)
                {
                    query = query.Where(a => a.Status == statusFilter.Value);
                }

                var entities = await query.ToListAsync();

                var dtoList = entities.Select(a => new GetAlertRuleDto
                {
                    AlertRuleId = a.AlertRuleId,
                    TenantId = a.TenantId,
                    Name = a.Name,
                    RuleCondition = a.RuleCondition,
                    Severity = a.Severity,
                    Status = a.Status,
                    LastTriggered = a.LastTriggered,
                    DevicesCount = a.DevicesCount,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    LastTriggeredFormatted = a.LastTriggered.HasValue ?
                        GetTimeSinceText(a.LastTriggered.Value) : "Never"
                }).ToList();

                return new GetAllRecord<GetAlertRuleDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AlertRuleStatusMessage.FetchAllSuccess,
                    GetAllData = dtoList
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertRule-Module", "GetAllAlertRulesAsync", tenantId, null);
                return new GetAllRecord<GetAlertRuleDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AlertRuleStatusMessage.FetchAllFailed}: {ex.Message}"
                };
            }
        }
        public async Task<GetSpecificRecord<GetAlertRuleDto>> GetAlertRuleByIdAsync(int alertRuleId, int tenantId)
        {
            try
            {
                using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);

                var entity = await tenantDb.AlertRules
                    .FirstOrDefaultAsync(a => a.AlertRuleId == alertRuleId && !a.IsDeleted);

                if (entity == null)
                    return new GetSpecificRecord<GetAlertRuleDto>
                    {
                        StatusCode = StatusCode.NotFound,
                        StatusMessage = AlertRuleStatusMessage.NotFound
                    };

                var dto = new GetAlertRuleDto
                {
                    AlertRuleId = entity.AlertRuleId,
                    TenantId = entity.TenantId,
                    Name = entity.Name,
                    RuleCondition = entity.RuleCondition,
                    Severity = entity.Severity,
                    Status = entity.Status,
                    LastTriggered = entity.LastTriggered,
                    DevicesCount = entity.DevicesCount,
                    IsActive = entity.IsActive,
                    IsDeleted = entity.IsDeleted,
                    CreatedAt = entity.CreatedAt,
                    UpdatedAt = entity.UpdatedAt,
                    LastTriggeredFormatted = entity.LastTriggered.HasValue ?
                        GetTimeSinceText(entity.LastTriggered.Value) : "Never"
                };

                return new GetSpecificRecord<GetAlertRuleDto>
                {
                    StatusCode = StatusCode.Success,
                    StatusMessage = AlertRuleStatusMessage.FetchByIdSuccess,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(ex, "AlertRule-Module", "GetAlertRuleByIdAsync", tenantId, null);
                return new GetSpecificRecord<GetAlertRuleDto>
                {
                    StatusCode = StatusCode.Error,
                    StatusMessage = $"{AlertRuleStatusMessage.FetchByIdFailed}: {ex.Message}"
                };
            }
        }
        private string GetTimeSinceText(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";

            return $"{(int)timeSpan.TotalDays} days ago";
        }
    }
}
