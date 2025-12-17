using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Feedback_Support
{
    public interface ISuperAdminSupportFeedbackRepository
    {
        Task<GetAllRecord<SuperAdminSupportFeedbackDto>> GetAllSupportFeedbackAsync();
    }
}
