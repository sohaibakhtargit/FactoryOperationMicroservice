using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class SupportFeedbackService : ISupportFeedbackService
    {
        private readonly ISupportFeedbackRepository _feedbackRepo;

        public SupportFeedbackService(ISupportFeedbackRepository feedbackRepo)
        {
            _feedbackRepo = feedbackRepo;
        }

        public async Task<CommonResponseModel> AddFeedbackAsync(AddSupportFeedbackDto dto)
        {
            return await _feedbackRepo.AddFeedbackAsync(dto);
        }

        public async Task<CommonResponseModel> AcknowledgeFeedbackAsync(AcknowledgeFeedbackDto dto)
        {
            return await _feedbackRepo.AcknowledgeFeedbackAsync(dto);
        }

        public async Task<GetAllRecord<GetSupportFeedbackDto>> GetAllFeedbackByTenant(int tenantId)
        {
            return await _feedbackRepo.GetAllFeedbackByTenant(tenantId);
        }

        public async Task<FeedbackMetricDto> GetFeedbackMetricsAsync(int tenantId)
        {
            return await _feedbackRepo.GetFeedbackMetricsAsync(tenantId);
        }

        public async Task<CommonResponseModel> DeleteFeedbackAsync(DeleteSupportFeedbackDto dto)
        {
            return await _feedbackRepo.DeleteFeedbackAsync(dto);
        }

    }
}
