using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Announcements
{
    public interface IAnnouncementTemplateRepository
    {
        Task<IEnumerable<AnnouncementTemplate>> GetAllAsync();
        Task<AnnouncementTemplate?> GetByIdAsync(int id);
        Task<AnnouncementTemplate> AddAsync(AnnouncementTemplate template);
        Task UpdateAsync(AnnouncementTemplate template);
        Task DeleteAsync(AnnouncementTemplate template);
    }
}
