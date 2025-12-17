using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Feedback_Support
{
    public interface ISuperAdminSupportTicketRepository
    {
        public Task<GetAllRecord<SuperAdminSupportTicketDto>> GetAllSupportTicketsAsync();
        Task<CommonResponseModel> UpdateSupportTicketAsync(UpdateSupportTicketDto dto);
    }
}
