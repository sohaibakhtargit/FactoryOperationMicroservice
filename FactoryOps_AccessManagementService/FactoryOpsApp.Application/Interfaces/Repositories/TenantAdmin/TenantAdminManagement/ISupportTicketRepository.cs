using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Repositories.TenantAdmin.TenantAdminManagement
{
    public interface ISupportTicketRepository
    {
        Task<CommonResponseModel> AddSupportTicket(AddSupportTicketDto dto);
        Task<CommonResponseModel> UpdateSupportTicket(int ticketId, AddSupportTicketDto dto);
        Task<CommonResponseModel> DeleteSupportTicket(int ticketId, int TenantId);
        GetAllRecord<GetSupportTicketDto> GetAllTicketsByTenant(int tenantId);
    }
}
