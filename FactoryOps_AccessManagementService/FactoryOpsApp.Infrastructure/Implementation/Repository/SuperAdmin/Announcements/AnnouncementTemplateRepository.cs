using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.AuditLogs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;
using FactoryOpsApp.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Repository.SuperAdmin.Announcements
{
    public class AnnouncementTemplateRepository : IAnnouncementTemplateRepository
    {
        private readonly MasterFactoryOpsDbContext _masterDbContext;
        private readonly IAuditLogService _auditLogger;

        public AnnouncementTemplateRepository(MasterFactoryOpsDbContext masterDbContext,
            IAuditLogService auditLogger)
        {
            _masterDbContext = masterDbContext;
            _auditLogger = auditLogger;
        }

        public async Task<IEnumerable<AnnouncementTemplate>> GetAllAsync()
        {
            return await _masterDbContext.AnnouncementTemplates
                                 .Where(t => !t.IsDeleted)
                                 .AsNoTracking()
                                 .ToListAsync();
        }
        public async Task<AnnouncementTemplate?> GetByIdAsync(int id)
        {
            return await _masterDbContext.AnnouncementTemplates
                                 .FirstOrDefaultAsync(t => t.TemplateId == id && !t.IsDeleted);
        }
        public async Task<AnnouncementTemplate> AddAsync(AnnouncementTemplate template)
        {
            _masterDbContext.AnnouncementTemplates.Add(template);

            await _auditLogger.LogAuditAsync(
                 action: "Create",
                 details: $"Created new announcement template: {template.Name}",
                 tenantId: null,
                 email: "",
                 eventType: "CreateTemplate"
 );
            await _masterDbContext.SaveChangesAsync();

            return template;
        }
        public async Task UpdateAsync(AnnouncementTemplate template)
        {
            await _auditLogger.LogAuditAsync(
                action: "Update",
                details: $"Updated announcement template: {template.Name} (ID: {template.TemplateId})",
                tenantId: null,
                email: "",
                eventType: "Modify"
            );
            await _masterDbContext.SaveChangesAsync();

        }
        public async Task DeleteAsync(AnnouncementTemplate template)
        {
            template.IsDeleted = true;
            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;

            await _masterDbContext.SaveChangesAsync();
            await _auditLogger.LogAuditAsync(
                 action: "Delete",
                 details: $"Soft-deleted announcement template: {template.Name} (ID: {template.TemplateId})",
                 tenantId: null,
                 email: "",
                 eventType: "Remove"
             );
        }
    }
}
