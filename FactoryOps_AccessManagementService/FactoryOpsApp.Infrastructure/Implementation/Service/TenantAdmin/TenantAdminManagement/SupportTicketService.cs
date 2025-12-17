using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement;
using FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement;
using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Infrastructure.Implementation.Service.TenantAdmin.TenantAdminManagement
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _ticketRepo;

        public SupportTicketService(ISupportTicketRepository ticketRepo)
        {
            _ticketRepo = ticketRepo;
        }

        public async Task<CommonResponseModel> AddSupportTicket(AddSupportTicketDto dto)
        {
            return await _ticketRepo.AddSupportTicket(dto);
        }

        public async Task<CommonResponseModel> UpdateSupportTicket(int ticketId, AddSupportTicketDto dto)
        {
            return await _ticketRepo.UpdateSupportTicket(ticketId, dto);
        }

        public async Task<CommonResponseModel> DeleteSupportTicket(int ticketId, int TenantId)
        {
            return await _ticketRepo.DeleteSupportTicket(ticketId, TenantId);
        }

        public GetAllRecord<GetSupportTicketDto> GetAllTicketsByTenant(int tenantId)
        {
            return _ticketRepo.GetAllTicketsByTenant(tenantId);
        }
    }
}
