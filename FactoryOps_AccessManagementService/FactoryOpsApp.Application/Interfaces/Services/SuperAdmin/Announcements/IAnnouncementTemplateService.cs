using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;
using FactoryOpsApp.Domain.Entities.MasterTenantsAdmin;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Announcements
{
    public interface IAnnouncementTemplateService
    {
        Task<GetAllRecord<AnnouncementTemplate>> GetAllAsync();
        Task<GetSpecificRecord<AnnouncementTemplate>> GetByIdAsync(int id);
        Task<CommonResponseModel> CreateAsync(AnnouncementTemplateCreateDto dto);
        Task<CommonResponseModel> UpdateAsync(AnnouncementTemplateUpdateDto dto);
        Task<CommonResponseModel> DeleteAsync(int id);
    }
}
