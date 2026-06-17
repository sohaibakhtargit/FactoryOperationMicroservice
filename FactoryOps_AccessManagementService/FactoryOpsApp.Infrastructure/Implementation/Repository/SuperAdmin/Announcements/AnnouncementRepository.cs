using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.Common;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.ExceptionLogger;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using static FactoryOps_AccessManagementService.FactoryOpsApp.Common.CommonConstant;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Announcements
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly IEmailService _iEmailService;
        private readonly IAuditLogService _auditLogger;
        private readonly IExceptionLoggerService _exceptionLogger;

        public AnnouncementRepository(MasterFactoryOpsDbContext masterDbContext, IEmailService iEmailService,
            IExceptionLoggerService exceptionLogger, IAuditLogService auditLogger)
        {
            _masterDbContext = masterDbContext;
            _iEmailService = iEmailService;
            _exceptionLogger = exceptionLogger;
            _auditLogger = auditLogger;
        }

        public async Task<CommonResponseModel> CreateAnnouncementAsync(CreateAnnouncementDto dto)
        {
            CommonResponseModel response = new();
            try
            {
                var announcement = new Announcement
                {
                    Title = dto.Title,
                    Type = dto.Type,
                    Message = dto.Message,
                    TemplateId = dto.TemplateId,
                    ScheduledTime = dto.SendImmediately ? null : dto.ScheduledTime,
                    Status = dto.IsDraft
                     ? "draft"
                     : dto.SendImmediately ? "sent" : "scheduled",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                };

                await _masterDbContext.Announcements.AddAsync(announcement);
                await _masterDbContext.SaveChangesAsync();

                var channels = dto.Channels.Select(channel => new AnnouncementChannel
                {
                    ChannelType = channel,
                    AnnouncementId = announcement.AnnouncementId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _masterDbContext.AnnouncementChannels.AddRangeAsync(channels);

                var tenants = dto.TenantIds.Select(tenantId => new AnnouncementTenant
                {
                    TenantId = tenantId,
                    AnnouncementId = announcement.AnnouncementId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _masterDbContext.AnnouncementTenants.AddRangeAsync(tenants);
                await _masterDbContext.SaveChangesAsync();

                if (dto.Channels.Any(c => c.Equals("email", StringComparison.OrdinalIgnoreCase)) && dto.TemplateId.HasValue)
                {
                    var template = await _masterDbContext.AnnouncementTemplates
                                        .FirstOrDefaultAsync(t => t.TemplateId == dto.TemplateId && !t.IsDeleted);

                    if (template != null)
                    {
                        string emailSubject = template.TitleTemplate;
                        string emailBodyTemplate = template.MessageTemplate;

                        foreach (var tenantId in dto.TenantIds)
                        {
                            var tenant = await _masterDbContext.FactoryTenants
                                .FirstOrDefaultAsync(t =>
                                    t.TenantId == tenantId &&
                                    t.IsActive &&
                                    !t.IsDeleted);

                            if (tenant == null || string.IsNullOrWhiteSpace(tenant.AdminEmail))
                                continue;

                            string processedSubject = emailSubject;
                            string processedBody = emailBodyTemplate;

                            processedBody = processedBody
                                .Replace("{{UserName}}", $"<b>{tenant.TenantName ?? "Tenant User"}</b>")
                                .Replace("{{DateTime}}", $"<b>{DateTime.UtcNow:yyyy-MM-dd HH:mm}</b>");

                            processedSubject = processedSubject
                                .Replace("{{UserName}}", tenant.TenantName ?? "Tenant User")
                                .Replace("{{DateTime}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));

                            if (dto.TemplatePlaceholders != null)
                            {
                                foreach (var placeholder in dto.TemplatePlaceholders)
                                {
                                    string boldValue = $"<b>{placeholder.Value}</b>";

                                    processedBody = processedBody.Replace(
                                        $"{{{{{placeholder.Key}}}}}",
                                        boldValue);

                                    processedSubject = processedSubject.Replace(
                                        $"{{{{{placeholder.Key}}}}}",
                                        placeholder.Value);
                                }
                            }

                            var emailDto = new EmailDTO
                            {
                                From = "shoaibmaliklenovo@gmail.com",
                                To = tenant.AdminEmail,
                                Subject = processedSubject,
                                Body = $@"<html><body>{processedBody}</body></html>"
                            };

                            await _iEmailService.SendEmailAsync(emailDto);
                        }

                    }
                }

                await _auditLogger.LogAuditAsync(
                           action: "Create",
                           details: $"Created new announcement with ID {announcement.AnnouncementId}",
                           tenantId: null,
                           email: "",
                           eventType: "CreateAnnouncement"
                       );
                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AnnouncementStatusMessage.AnnouncementAdded;
                return response;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                        ex,
                        sourceModule: "AnnouncementModule",
                        apiName: "Create-Announcement",
                        tenantId: null,
                        userId: null
                    );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to create announcement: {ex.Message}";
                return response;
            }
        }
        public async Task<GetAllRecord<AnnouncementResponseDto>> GetAllAnnouncementsAsync()
        {
            var response = new GetAllRecord<AnnouncementResponseDto>();
            try
            {
                var announcements = await _masterDbContext.Announcements
                                        .Where(a => !a.IsDeleted)
                                        .OrderByDescending(a => a.AnnouncementId)
                                        .Select(a => new AnnouncementResponseDto
                                        {
                                            AnnouncementId = a.AnnouncementId,
                                            Title = a.Title,
                                            Message = a.Message,
                                            Type = a.Type,
                                            Status = a.Status,
                                            ScheduledTime = a.ScheduledTime,
                                            Channels = a.Channels
                                                        .Where(c => !c.IsDeleted)
                                                        .Select(c => c.ChannelType)
                                                        .ToList(),
                                            Tenants = a.Tenants
                                                        .Where(t => !t.IsDeleted && t.Tenant != null)
                                                        .Select(t => new TenantDto
                                                        {
                                                            TenantId = t.Tenant.TenantId,
                                                            TenantName = t.Tenant.TenantName
                                                        })
                                                        .ToList()
                                        })
                                        .ToListAsync();

                response.GetAllData = announcements;
                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AnnouncementStatusMessage.AnnouncementFetched;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementModule",
                    apiName: "All-Announcement",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Internal Server Error: {ex.Message}";
            }

            return response;
        }
        public async Task<GetAllRecord<AnnouncementResponseDto>> GetAllAnnouncementsByTenantIdAsync(int tenantId)
        {
            var response = new GetAllRecord<AnnouncementResponseDto>();

            try
            {
                var announcements = await _masterDbContext.Announcements
                    .Where(a => !a.IsDeleted && a.Tenants.Any(t => t.TenantId == tenantId && !t.IsDeleted))
                    .Include(a => a.Channels.Where(c => !c.IsDeleted))
                    .Include(a => a.Tenants.Where(t => !t.IsDeleted))
                        .ThenInclude(at => at.Tenant)
                    .OrderByDescending(a => a.AnnouncementId)
                    .ToListAsync();

                response.GetAllData = announcements.Select(a => new AnnouncementResponseDto
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Type = a.Type,
                    Message = a.Message,
                    Status = a.Status,
                    ScheduledTime = a.ScheduledTime,
                    Channels = a.Channels
                        .Select(c => c.ChannelType)
                        .ToList(),

                    Tenants = a.Tenants
                        .Where(t => t.Tenant != null)
                        .Select(t => new TenantDto
                        {
                            TenantId = t.Tenant.TenantId,
                            TenantName = t.Tenant.TenantName
                        })
                        .ToList()
                }).ToList();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AnnouncementStatusMessage.AnnouncementFetched;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementModule",
                    apiName: "AnnouncementByTenantId",
                    tenantId: tenantId,
                    userId: null
                );

                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Internal Server Error: {ex.Message}";
            }

            return response;
        }
        public async Task<CommonResponseModel> UpdateAnnouncementAsync(UpdateAnnouncementDto dto)
        {
            var response = new CommonResponseModel();
            try
            {
                var announcement = await _masterDbContext.Announcements
                    .Include(a => a.Channels)
                    .Include(a => a.Tenants)
                    .FirstOrDefaultAsync(a => a.AnnouncementId == dto.AnnouncementId && !a.IsDeleted);

                if (announcement == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AnnouncementStatusMessage.NoRecordsFound;
                    return response;
                }

                announcement.Title = dto.Title;
                announcement.Type = dto.Type;
                announcement.Message = dto.Message;
                announcement.TemplateId = dto.TemplateId;
                announcement.ScheduledTime = dto.SendImmediately ? null : dto.ScheduledTime;
                announcement.Status = dto.SendImmediately ? "sent" : "scheduled";
                announcement.Status = dto.IsDraft
                   ? "draft" : dto.SendImmediately ? "sent" : "scheduled";
                announcement.UpdatedAt = DateTime.UtcNow;
                announcement.UpdatedBy = 1;

                foreach (var ch in announcement.Channels.Where(c => !c.IsDeleted))
                {
                    ch.IsDeleted = true;
                    ch.IsActive = false;
                    ch.DeletedAt = DateTime.UtcNow;
                    ch.DeletedBy = 1;
                }

                var newChannels = dto.Channels.Distinct().Select(channel => new AnnouncementChannel
                {
                    AnnouncementId = announcement.AnnouncementId,
                    ChannelType = channel,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                }).ToList();
                await _masterDbContext.AnnouncementChannels.AddRangeAsync(newChannels);

                foreach (var tn in announcement.Tenants.Where(t => !t.IsDeleted))
                {
                    tn.IsDeleted = true;
                    tn.IsActive = false;
                    tn.DeletedAt = DateTime.UtcNow;
                    tn.DeletedBy = 1;
                }

                var newTenants = dto.TenantIds.Distinct().Select(tenantId => new AnnouncementTenant
                {
                    AnnouncementId = announcement.AnnouncementId,
                    TenantId = tenantId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                }).ToList();
                await _masterDbContext.AnnouncementTenants.AddRangeAsync(newTenants);

                if (dto.Channels.Any(c => c.Equals("email", StringComparison.OrdinalIgnoreCase)) && dto.TemplateId.HasValue)
                {
                    var template = await _masterDbContext.AnnouncementTemplates
                                        .FirstOrDefaultAsync(t => t.TemplateId == dto.TemplateId && !t.IsDeleted);

                    if (template != null)
                    {
                        foreach (var tenantId in dto.TenantIds)
                        {
                            var tenant = await _masterDbContext.FactoryTenants
                                .FirstOrDefaultAsync(t => t.TenantId == tenantId && t.IsActive && !t.IsDeleted);

                            if (tenant == null) continue;

                            string processedBody = template.MessageTemplate
                                .Replace("{{UserName}}", $"<b>{tenant.TenantName ?? "Tenant User"}</b>")
                                .Replace("{{DateTime}}", $"<b>{DateTime.UtcNow:yyyy-MM-dd HH:mm}</b>");

                            string processedSubject = template.TitleTemplate
                                .Replace("{{UserName}}", tenant.TenantName ?? "Tenant User")
                                .Replace("{{DateTime}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));

                            if (dto.TemplatePlaceholders != null)
                            {
                                foreach (var placeholder in dto.TemplatePlaceholders)
                                {
                                    string boldValue = $"<b>{placeholder.Value}</b>";
                                    processedBody = processedBody.Replace($"{{{{{placeholder.Key}}}}}", boldValue);
                                    processedSubject = processedSubject.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
                                }
                            }

                            var emailDto = new EmailDTO
                            {
                                From = "shoaibmaliklenovo@gmail.com",
                                To = "factory.operation@yopmail.com", 
                                Subject = processedSubject,
                                Body = $@"<html><body>{processedBody}</body></html>"
                            };

                            await _iEmailService.SendEmailAsync(emailDto);
                        }
                    }
                }

                await _auditLogger.LogAuditAsync(
                        action: "Update",
                        details: $"Updated announcement with ID {announcement.AnnouncementId}",
                        tenantId: null,
                        email: "",
                        eventType: "UpdateAnnouncement"
                    );

                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AnnouncementStatusMessage.AnnouncementUpdated;
                return response;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementModule",
                    apiName: "Update-Announcement",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Failed to update announcement: {ex.Message}";
                return response;
            }
        }
        public async Task<CommonResponseModel> DeleteAnnouncementAsync(int announcementId)
        {
            CommonResponseModel response = new();

            try
            {
                var announcement = await _masterDbContext.Announcements
                    .Include(a => a.Channels)
                    .Include(a => a.Tenants)
                    .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId && !a.IsDeleted);

                if (announcement == null)
                {
                    response.StatusCode = StatusCode.NotFound;
                    response.StatusMessage = AnnouncementStatusMessage.NoRecordsFound;
                    return response;
                }

                announcement.IsDeleted = true;
                announcement.IsActive = false;
                announcement.DeletedAt = DateTime.UtcNow;


                foreach (var channel in announcement.Channels)
                {
                    channel.IsDeleted = true;
                    channel.IsActive = false;
                    channel.DeletedAt = DateTime.UtcNow;
                }

                foreach (var tenant in announcement.Tenants)
                {
                    tenant.IsDeleted = true;
                    tenant.IsActive = false;
                    tenant.DeletedAt = DateTime.UtcNow;
                }
                await _auditLogger.LogAuditAsync(
                    action: "Delete",
                    details: $"Deleted announcement with ID {announcement.AnnouncementId}",
                    tenantId: null,
                    email: "",
                    eventType: "DeleteAnnouncement"
                );

                await _masterDbContext.SaveChangesAsync();

                response.StatusCode = StatusCode.Success;
                response.StatusMessage = AnnouncementStatusMessage.AnnouncementDeleted;
            }
            catch (Exception ex)
            {
                await _exceptionLogger.LogExceptionAsync(
                    ex,
                    sourceModule: "AnnouncementModule",
                    apiName: "Delete-Announcement",
                    tenantId: null,
                    userId: null
                );
                response.StatusCode = StatusCode.Error;
                response.StatusMessage = $"Internal Server Error: {ex.Message}";
            }

            return response;
        }

    }
}

