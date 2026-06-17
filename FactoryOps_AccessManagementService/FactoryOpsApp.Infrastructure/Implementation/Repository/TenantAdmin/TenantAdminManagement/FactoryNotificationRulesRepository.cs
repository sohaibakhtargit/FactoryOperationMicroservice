using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.FactoryOpsTenants;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.TenantAdmin.TenantAdminManagement
{
    public class FactoryNotificationRulesRepository : IFactoryNotificationRulesRepository
    {
        private readonly TenantDbContextFactory _tenantDbContext;
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly IExceptionLoggerService _exceptionLogger;
        private readonly IAuditLogService _auditLogger;
        public FactoryNotificationRulesRepository(IExceptionLoggerService exceptionLogger,
            TenantDbContextFactory tenantDbContext,
            MasterFactoryOpsDbContext masterDbContext,
            IAuditLogService auditLogger)
        {
            _tenantDbContext = tenantDbContext;
            _masterDbContext = masterDbContext;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> CreateAsync(NotificationRuleDto dto)
        {
            CommonResponseModel response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                var isDuplicate = await tenantDb.FactoryNotificationRules.AnyAsync(x =>
                    x.TenantId == dto.TenantId &&
                    x.TriggerEvent == dto.TriggerEvent &&
                    x.DeliveryMethod == dto.DeliveryMethod &&
                    x.RecipientType == dto.RecipientType &&
                    x.RecipientId == dto.RecipientId &&
                    !x.IsDeleted
                );

                if (isDuplicate)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = "Notification rule already exists."
                    };
                }
                var entity = new FactoryNotificationRules
                {
                    TenantId = dto.TenantId,
                    Name = dto.Name,
                    TriggerEvent = dto.TriggerEvent,
                    DeliveryMethod = dto.DeliveryMethod,
                    RecipientType = dto.RecipientType,
                    RecipientId = dto.RecipientId ?? "string",
                    EscalationTime = dto.EscalationTime,
                    EscalationRecipient = dto.EscalationRecipient,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = dto.TenantId
                };

                tenantDb.FactoryNotificationRules.Add(entity);

                await _auditLogger.LogAuditAsync(
                    action: "Create",
                    details: "Create Notification Rule",
                    tenantId: dto.TenantId,
                    email: "",
                    eventType: "NotificationRule"
                );

                await tenantDb.SaveChangesAsync();
                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = NotificationRuleMessage.Created;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "NotificationRulesModule",
                    apiName: "Add-Notification-Rules",
                    tenantId: dto.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{NotificationRuleMessage.CreateFailled}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetAllRecord<NotificationRuleDto>> GetAllAsync(int tenantId)
        {
            var response = new GetAllRecord<NotificationRuleDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            try
            {
                var rules = await tenantDb.FactoryNotificationRules
                    .Where(x => x.TenantId == tenantId && !x.IsDeleted).OrderByDescending(x => x.RuleId)
                    .ToListAsync();

                response.GetAllData = rules.Select(r => new NotificationRuleDto
                {
                    RuleId = r.RuleId,
                    TenantId = r.TenantId,
                    Name = r.Name,
                    TriggerEvent = r.TriggerEvent,
                    DeliveryMethod = r.DeliveryMethod,
                    RecipientType = r.RecipientType,
                    RecipientId = r.RecipientId,
                    EscalationTime = r.EscalationTime,
                    EscalationRecipient = r.EscalationRecipient,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy
                }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = NotificationRuleMessage.FetchedAll;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "NotificationRulesModule",
                    apiName: "getAll-notification",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{NotificationRuleMessage.FetchFailled}: {ex.Message}";
            }

            return response;
        }
        public async Task<GetSpecificRecord<NotificationRuleDto>> GetByIdAsync(int id, int tenantId)
        {
            var response = new GetSpecificRecord<NotificationRuleDto>();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            try
            {
                var rule = await tenantDb.FactoryNotificationRules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RuleId == id && x.TenantId == tenantId && !x.IsDeleted);

                if (rule == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = NotificationRuleMessage.NotFound;
                    return response;
                }

                response.Data = new NotificationRuleDto
                {
                    RuleId = rule.RuleId,
                    TenantId = rule.TenantId,
                    Name = rule.Name,
                    TriggerEvent = rule.TriggerEvent,
                    DeliveryMethod = rule.DeliveryMethod,
                    RecipientType = rule.RecipientType,
                    RecipientId = rule.RecipientId,
                    EscalationTime = rule.EscalationTime,
                    EscalationRecipient = rule.EscalationRecipient,
                    IsActive = rule.IsActive,
                    CreatedAt = rule.CreatedAt,
                    CreatedBy = rule.CreatedBy
                };

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = NotificationRuleMessage.FetchedSingle;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                   ex,
                   sourceModule: "NotificationRulesModule",
                   apiName: "getNotificationBy_Id",
                   tenantId: tenantId,
                   userId: null
               );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{NotificationRuleMessage.FetchFailled}: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateAsync(NotificationRuleDto dto)
        {
            CommonResponseModel response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(dto.TenantId);
            try
            {
                var existing = await tenantDb.FactoryNotificationRules
                    .FirstOrDefaultAsync(x => x.RuleId == dto.RuleId && x.TenantId == dto.TenantId && !x.IsDeleted);

                if (existing == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = NotificationRuleMessage.NotFound;
                    return response;
                }

                var isDuplicate = await tenantDb.FactoryNotificationRules.AnyAsync(x =>
                    x.RuleId != dto.RuleId &&   
                    x.TenantId == dto.TenantId &&
                    x.TriggerEvent == dto.TriggerEvent &&
                    x.DeliveryMethod == dto.DeliveryMethod &&
                    x.RecipientType == dto.RecipientType &&
                    x.RecipientId == dto.RecipientId &&
                    !x.IsDeleted
                );

                if (isDuplicate)
                {
                    return new CommonResponseModel
                    {
                        StatusCode = StatusCode.BadRequest,
                        StatusMessage = "Notification rule already exists."
                    };
                }

                existing.Name = dto.Name;
                existing.TriggerEvent = dto.TriggerEvent;
                existing.DeliveryMethod = dto.DeliveryMethod;
                existing.RecipientType = dto.RecipientType;
                existing.RecipientId = dto.RecipientId!;
                existing.EscalationTime = dto.EscalationTime;
                existing.EscalationRecipient = dto.EscalationRecipient;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = dto.TenantId;

                await _auditLogger.LogAuditAsync(
                    action: "Update",
                    details: "Update Notification Rule",
                    tenantId: dto.TenantId,
                    email: "",
                    eventType: "NotificationRule"
                );

                await tenantDb.SaveChangesAsync();
                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = NotificationRuleMessage.Updated;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "NotificationRulesModule",
                    apiName: "Update-Notification-Rules",
                    tenantId: dto.TenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{NotificationRuleMessage.UpdateFailled} : : {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> DeleteAsync(int id, int tenantId)
        {
            CommonResponseModel response = new CommonResponseModel();
            using var tenantDb = _tenantDbContext.GetTenantDbContext(tenantId);
            try
            {
                var existing = await tenantDb.FactoryNotificationRules
                    .FirstOrDefaultAsync(x => x.RuleId == id && x.TenantId == tenantId && !x.IsDeleted);

                if (existing == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = NotificationRuleMessage.NotFound;
                    return response;
                }

                existing.IsDeleted = true;
                existing.IsActive = false;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = tenantId;

                await _auditLogger.LogAuditAsync(
                    action: "Delete",
                    details: "Delete Notification Rule",
                    tenantId: tenantId,
                    email: "",
                    eventType: "NotificationRule"
                );
                await _masterDbContext.SaveChangesAsync();

                await tenantDb.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = NotificationRuleMessage.Deleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "NotificationRulesModule",
                    apiName: "Delete-Notification",
                    tenantId: tenantId,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"{NotificationRuleMessage.DeleteFailled}: {ex.Message}";
            }

            return response;
        }

    }
}
