using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Feedback_Support
{
    public interface ISuperAdminSupportFeedbackService
    {
        Task<GetAllRecord<SuperAdminSupportFeedbackDto>> GetAllSupportFeedbackAsync();
    }
}
