using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.SuperAdmin.Feedback_Support;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.SuperAdmin.Feedback_Support;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.SuperAdmin.Feedback_Support
{
    public class SuperAdminSupportTicketService : ISuperAdminSupportTicketService
    {
        private readonly ISuperAdminSupportTicketRepository _repository;

        public SuperAdminSupportTicketService(
            ISuperAdminSupportTicketRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetAllRecord<SuperAdminSupportTicketDto>> GetAllSupportTicketsAsync()
        {
            return await _repository.GetAllSupportTicketsAsync();
        }

        public async Task<CommonResponseModel> UpdateSupportTicketAsync(UpdateSupportTicketDto dto)
        {
            return await _repository.UpdateSupportTicketAsync(dto);
        }
    }
}
