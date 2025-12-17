using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Announcements;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Announcements;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Announcements
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repository;

        public AnnouncementService(IAnnouncementRepository repository)
        {
            _repository = repository;
        }

        public async Task<CommonResponseModel> CreateAnnouncementAsync(CreateAnnouncementDto dto)
        {
            return await _repository.CreateAnnouncementAsync(dto);
        }
        public async Task<GetAllRecord<AnnouncementResponseDto>> GetAllAnnouncementsAsync()
        {
            return await _repository.GetAllAnnouncementsAsync();
        }
        public async Task<GetAllRecord<AnnouncementResponseDto>> GetAllAnnouncementsByTenantIdAsync(int tenantId)
        {
            return await _repository.GetAllAnnouncementsByTenantIdAsync(tenantId);
        }
        public async Task<CommonResponseModel> UpdateAnnouncementAsync(UpdateAnnouncementDto dto)
        {
            return await _repository.UpdateAnnouncementAsync(dto);
        }

        public async Task<CommonResponseModel> DeleteAnnouncementAsync(int announcementId)
        {
            return await _repository.DeleteAnnouncementAsync(announcementId);
        }
    }
}
