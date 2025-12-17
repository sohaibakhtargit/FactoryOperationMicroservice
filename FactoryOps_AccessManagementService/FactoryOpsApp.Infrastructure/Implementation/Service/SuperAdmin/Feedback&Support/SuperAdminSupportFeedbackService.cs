using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Feedback_Support;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Feedback_Support
{
    public class SuperAdminSupportFeedbackService : ISuperAdminSupportFeedbackService
    {
        private readonly ISuperAdminSupportFeedbackRepository _repository;

        public SuperAdminSupportFeedbackService(ISuperAdminSupportFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetAllRecord<SuperAdminSupportFeedbackDto>> GetAllSupportFeedbackAsync()
        {
            return await _repository.GetAllSupportFeedbackAsync();
        }
    }

}
