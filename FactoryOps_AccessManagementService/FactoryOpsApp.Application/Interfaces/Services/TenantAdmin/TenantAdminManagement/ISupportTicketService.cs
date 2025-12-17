using FactoryOps_AccessManagementService.FactoryOpsApp.Application.Common;
using FactoryOpsApp.Application.DTOs;

namespace FactoryOperation_AccessManagementService.FactoryOpsApp.Application.Interfaces.Services.TenantAdmin.TenantAdminManagement
{
    public interface ISupportTicketService
    {
        Task<CommonResponseModel> AddSupportTicket(AddSupportTicketDto dto);
        Task<CommonResponseModel> UpdateSupportTicket(int ticketId, AddSupportTicketDto dto);
        Task<CommonResponseModel> DeleteSupportTicket(int ticketId, int TenandId);
        GetAllRecord<GetSupportTicketDto> GetAllTicketsByTenant(int TenantId);

    }
}
