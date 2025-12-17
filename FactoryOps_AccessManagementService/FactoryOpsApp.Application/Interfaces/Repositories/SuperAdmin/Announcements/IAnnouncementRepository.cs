using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Announcements
{
    public interface IAnnouncementRepository
    {
        public Task<CommonResponseModel> CreateAnnouncementAsync(CreateAnnouncementDto dto);
        public Task<GetAllRecord<AnnouncementResponseDto>> GetAllAnnouncementsAsync();
        public Task<GetAllRecord<AnnouncementResponseDto>> GetAllAnnouncementsByTenantIdAsync(int tenantId);
        public Task<CommonResponseModel> UpdateAnnouncementAsync(UpdateAnnouncementDto dto);
        public Task<CommonResponseModel> DeleteAnnouncementAsync(int announcementId);

    }
}
