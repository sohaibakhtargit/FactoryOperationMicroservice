using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement
{
    public interface ISupportFeedbackRepository
    {
        Task<CommonResponseModel> AddFeedbackAsync(AddSupportFeedbackDto dto);
        Task<CommonResponseModel> AcknowledgeFeedbackAsync(AcknowledgeFeedbackDto dto);
        Task<GetAllRecord<GetSupportFeedbackDto>> GetAllFeedbackByTenant(int tenantId);
        Task<FeedbackMetricDto> GetFeedbackMetricsAsync(int tenantId);
        Task<CommonResponseModel> DeleteFeedbackAsync(DeleteSupportFeedbackDto dto);

    }
}

